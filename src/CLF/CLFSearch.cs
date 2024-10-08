using System;
using System.Collections.Generic;

namespace WoodchuckCarbonTool.src
{
    /// <summary>
    /// Counterpart of EC3Request
    /// </summary>
    internal sealed class CLFSearch
    {
        // Singleton class so that the CSVs only have to be read once. There isn't that 
        // much information so not with the IO overhead.
        private static readonly Lazy<CLFSearch> _clfSearch =
            new Lazy<CLFSearch>(() => new CLFSearch());

        public static CLFSearch Instance => _clfSearch.Value;

        public List<List<string>> generalMaterials;
        public List<List<string>> readyMixes;

        public CLFSearch()
        {
            generalMaterials = IOTools.ReadCSVFromEmbedded("2023_04_CLF_CLFMaterialBaselines");
            readyMixes = IOTools.ReadCSVFromEmbedded("2023_04_CLF_USAReadyMixedConcreteRegional");
        }

        public List<EPD> Search(MaterialFilter mf)
        {
            if (mf.categoryName == "US Regional Ready-Mix Concrete") return SearchReadyMix(mf);
            return SearchGeneralMaterial(mf);
        }

        public List<EPD> SearchGeneralMaterial(MaterialFilter mf)
        {
            List<EPD> epds = new List<EPD>();
            foreach (List<string> material in generalMaterials)
            {
                if (material[0] != mf.categoryName) continue;
                double qUnit = UnitManager.ParseDoubleWithUnit(material[4], out string unit);
                EPD epd;
                try
                {
                    double.TryParse(material[3], out double gwp);
                    double.TryParse(material[9], out double density);

                    epd = new EPD(material[1], gwp / qUnit, unit,
                    density, material[10], mf.categoryName,
                    int.Parse(material[8]), mf, null);
                    epds.Add(epd);

                    epd.description = material[2];
                    epd.tooltip = material[7];
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                    continue;
                }
            }
            return epds;
        }

        public List<EPD> SearchReadyMix(MaterialFilter mf)
        {
            List<EPD> epds = new List<EPD>();
            List<string> psi = null;
            for (int i = 0; i < readyMixes.Count; i++)
            {
                List<string> material = readyMixes[i];
                if (i == 0) { psi = material; continue; }
                if (material[0] != mf.state) continue;

                for (int j = 1; j < material.Count; j++)
                {
                    EPD epd;
                    try
                    {
                        epd = new EPD(psi[j], double.Parse(material[j]), "m3",
                        2300, "kg/m3", mf.categoryName, 3, mf, null);
                        epds.Add(epd);
                    }
                    catch (Exception) { continue; }
                }
            }
            return epds;
        }
    }
}
