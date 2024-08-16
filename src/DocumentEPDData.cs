using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;

namespace WoodchuckCarbonTool.src
{
    [Serializable]
    [Guid("847b706e-f578-47eb-a8f0-b440584e5e2d")]

    /// This class is specific to implementing EPD IO in Rhino. On file save, the data
    /// from the EPD is serialized and stored, and on file load, is is read and
    /// reconstructed into an EPD object.
    public class DocumentEpdData
    {
        private const int MAJOR = 1;
        private const int MINOR = 0;

        public EPD epd { get; set; }

        public DocumentEpdData() { }

        public DocumentEpdData(EPD epd) 
        { 
            this.epd = epd; 
            AssignNewId();
        }

        public string AssignNewId()
        {
            string newId;
            do
            {
                newId = GenerateId();
            } while(WoodchuckCarbonToolPlugin.Instance.DocumentEPDs.ContainsKey(newId));

            epd.id = newId;

            return newId;
        }

        static string GenerateId()
        {
            Random random = new Random();
            return new string(Enumerable.Range(0, 16).Select(_ =>
            {
                int num = random.Next(0, 62);
                if (num < 10) return (char)('0' + num);       // 0-9
                else if (num < 36) return (char)('A' + num - 10); // A-Z
                else return (char)('a' + num - 36);           // a-z
            }).ToArray());
        }

        public bool Write(BinaryArchiveWriter archive)
        {
            if (epd == null) return false;

            archive.Write3dmChunkVersion(MAJOR, MINOR);

            // Create a new dictionary to store your data
            var dict = new Rhino.Collections.ArchivableDictionary(1, epd.id);

            if (epd.mf != null)
            {
                dict.Set("MF Category", epd.mf.categoryName);
                dict.Set("MF Country", epd.mf.country);
                dict.Set("MF State", epd.mf.state);
                dict.Set("MF Date", epd.mf.expirationDate);
            }
            else
            {
                dict.Set("MF Category", (string)null);
                dict.Set("MF Country", (string)null);
                dict.Set("MF State", (string)null);
                dict.Set("MF Date", (string)null);
            }

            UnitManager.ParseDoubleWithUnit(epd.unitMaterial.ToString(), out string unit);
            UnitManager.ParseDoubleWithUnit(epd.density.ToString(), out string densityUnit);
            // Add your data to the dictionary
            dict.Set("EPD Name", epd.name);
            dict.Set("EPD Id", epd.id);
            dict.Set("EPD Category", epd.category);
            dict.Set("EPD Manufacturer", epd.manufacturer);
            dict.Set("EPD Dimension", epd.dimension);
            dict.Set("EPD GWP", epd.GetGwpPerUnit(epd.unitMaterial.Unit));
            dict.Set("EPD Unit", unit);
            dict.Set("EPD Density Val", epd.density.Value);
            dict.Set("EPD Density Unit", densityUnit);
            dict.Set("EPD EC3Id", epd.ec3id);
            dict.Set("EPD Description", epd.description);

            // Write the dictionary to the archive
            archive.WriteDictionary(dict);
            return true; // Return true to indicate successful write
        }

        public bool Read(BinaryArchiveReader archive)
        {
            try
            {
                archive.Read3dmChunkVersion(out var major, out var minor);
                if (major != MAJOR) { return false; }
                // Attempt to read the dictionary from the archive
                Rhino.Collections.ArchivableDictionary dict = archive.ReadDictionary();
                if (dict != null)
                {
                    MaterialFilter mf = new MaterialFilter();

                    // I hate this too :(
                    if (dict.ContainsKey("MF Category"))
                        mf.SetEC3Category(dict["MF Category"] as string);
                    if (dict.ContainsKey("MF Country"))
                        mf.SetEC3Country(dict["MF Country"] as string);
                    if (dict.ContainsKey("MF State"))
                        mf.SetEC3State(dict["MF State"] as string);
                    if (dict.ContainsKey("MF Date"))
                        mf.SetEC3FormattedExporationDate(dict["MF Date"] as string);

                    string name = null;
                    string id = null;
                    string category = null;
                    string manufacturer = null;
                    int dimension = 0;
                    double gwp = 0;
                    string unit = null;
                    double densityVal = 0;
                    string densityUnit = null;
                    string ec3id = null;
                    string description = null;

                    if (dict.ContainsKey("EPD Name"))
                        name = dict["EPD Name"] as string;
                    if (dict.ContainsKey("EPD Id"))
                        id = dict["EPD Id"] as string;
                    if (dict.ContainsKey("EPD Category"))
                        category = dict["EPD Category"] as string;
                    if (dict.ContainsKey("EPD Manufacturer"))
                        manufacturer = dict["EPD Manufacturer"] as string;
                    if (dict.ContainsKey("EPD Dimension"))
                        dimension = (int)dict["EPD Dimension"];
                    if (dict.ContainsKey("EPD GWP"))
                        gwp = (double)dict["EPD GWP"];
                    if (dict.ContainsKey("EPD Unit"))
                        unit = dict["EPD Unit"] as string;
                    if (dict.ContainsKey("EPD Density Val"))
                        densityVal = (double)dict["EPD Density Val"];
                    if (dict.ContainsKey("EPD Density Unit"))
                        densityUnit = dict["EPD Density Unit"] as string;
                    if (dict.ContainsKey("EPD EC3Id"))
                        ec3id = dict["EPD EC3Id"] as string;
                    if (dict.ContainsKey("EPD Description"))
                        description = dict["EPD Description"] as string;

                    epd = new EPD(name, gwp, unit, densityVal, densityUnit, category,
                        dimension, mf, manufacturer, ec3id);
                    epd.description = description;
                    epd.id = id;

                    return true; // Successfully read all data
                }
            } 
            catch (Exception)
            {
                return false;
            }

            return false; // Return false if the dictionary is null
        }
    }
}
