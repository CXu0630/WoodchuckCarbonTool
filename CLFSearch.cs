using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC3CarbonCalculator
{
    /// <summary>
    /// Counterpart of EC3Request
    /// </summary>
    internal sealed class CLFSearch
    {
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

        public List<EPD> Search (MaterialFilter mf)
        {
            if (mf.categoryName == "US Regional Ready-Mix Concrete") return this.SearchReadyMix(mf);
            return this.SearchGeneralMaterial(mf);
        }

        public List<EPD> SearchGeneralMaterial (MaterialFilter mf)
        {
            List<EPD> epds = new List<EPD>();
            foreach(List<string> material in generalMaterials)
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
                }
                catch (Exception e) 
                { 
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
