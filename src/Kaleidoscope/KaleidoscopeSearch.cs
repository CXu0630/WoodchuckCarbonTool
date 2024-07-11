using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodchuckCarbonTool.src.Kaleidoscope
{
    internal class KaleidoscopeSearch
    {
        private static readonly Lazy<KaleidoscopeSearch> _ksSearch = 
            new Lazy<KaleidoscopeSearch>(() => new KaleidoscopeSearch());

        public static KaleidoscopeSearch Instance => _ksSearch.Value;

        public List<List<string>> ksMaterials;

        public KaleidoscopeSearch()
        {
            ksMaterials = IOTools.ReadCSVFromEmbedded("2023_Kaleidoscope_AssemblyDatabase");
        }

        public List<EPD> Search(MaterialFilter mf)
        {
            List<EPD> epds = new List<EPD>();

            int gwpIdx = -1;
            if (mf.includeD && mf.includeBiogen) gwpIdx = 8;
            else if (mf.includeD && !mf.includeBiogen) gwpIdx = 7;
            else if (mf.includeBC && mf.includeBiogen) gwpIdx = 6;
            else if (mf.includeBC && !mf.includeBiogen) gwpIdx = 5;
            else if (mf.includeBiogen) gwpIdx = 4;
            else gwpIdx = 3;

            foreach (List<string> material in ksMaterials)
            {
                if (material[0] != mf.categoryName) continue;
                double qUnit = UnitManager.ParseDoubleWithUnit(material[9], out string unit);
                EPD epd;
                try
                {
                    double.TryParse(material[gwpIdx], out double gwp);

                    epd = new EPD(material[1] + " - " + material[2], gwp / qUnit, 
                        unit, -1, null, mf.categoryName, 2, mf, null);

                    epds.Add(epd);

                    epd.description = material[10];
                    epd.tooltip = material[11];
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                    continue;
                }
            }
            return epds;
        }
    }
}
