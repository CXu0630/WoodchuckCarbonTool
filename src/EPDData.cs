using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using System;
using System.Runtime.InteropServices;

namespace EC3CarbonCalculator.src
{
    [Serializable]
    [Guid("847b706e-f578-47eb-a8f0-b440584e5e2d")]
    public class EPDData : UserData
    {
        public EPD epd { get; set; }

        public EPDData() { }

        public EPDData(EPD epd) { this.epd = epd; }

        protected override bool Write(BinaryArchiveWriter archive)
        {
            if (epd == null) return false;

            // Create a new dictionary to store your data
            var dict = new Rhino.Collections.ArchivableDictionary(1, "EPDUserData");

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
            dict.Set("EPD Category", epd.category);
            dict.Set("EPD Manufacturer", epd.manufacturer);
            dict.Set("EPD Dimension", epd.dimension);
            dict.Set("EPD GWP", epd.GetGwpPerUnit(epd.unitMaterial.Unit));
            dict.Set("EPD Unit", unit);
            dict.Set("EPD Density Val", epd.density.Value);
            dict.Set("EPD Density Unit", densityUnit);
            dict.Set("EPD Id", epd.id);

            // Write the dictionary to the archive
            archive.WriteDictionary(dict);
            return true; // Return true to indicate successful write
        }

        protected override bool Read(BinaryArchiveReader archive)
        {
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
                string category = null;
                string manufacturer = null;
                int dimension = 0;
                double gwp = 0;
                string unit = null;
                double densityVal = 0;
                string densityUnit = null;
                string id = null;

                if (dict.ContainsKey("EPD Name"))
                    name = dict["EPD Name"] as string;
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
                if (dict.ContainsKey("EPD Id"))
                    id = dict["EPD Id"] as string;

                epd = new EPD(name, gwp, unit, densityVal, densityUnit, category,
                    dimension, mf, manufacturer, id);

                return true; // Successfully read all data
            }
            return false; // Return false if the dictionary is null
        }

        // Provide a unique type description for your custom user data
        public override string Description => "EPD User Data";

        // Ensure that Rhino knows to write this data when saving the document
        public override bool ShouldWrite { get { return true; } }
    }
}
