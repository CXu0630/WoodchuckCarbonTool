using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC3CarbonCalculator
{
    internal class EPD
    {
        JObject JObj;
        public string name { get; set; }
        public float gwp { get; set; }
        public string unit { get; set; }
        public float density { get; set; }
        public string densityUnit { get; set; }
        public string id { get; }
        public string uuid { get; }
        public string category { get; }

        public EC3MaterialFilter searchParameters { get; }

        public EPD(JObject obj, EC3MaterialFilter searchPar = null)
        {
            this.JObj = obj;

            this.name = obj["name"]?.ToString();

            this.gwp = EC3MaterialParser.ParseFloatWithUnit(obj, "gwp");
            float unitMultiplier = EC3MaterialParser.ParseFloatWithUnit(obj, "unit", out string unit);
            this.unit = unit;
            if (unitMultiplier != 1) { this.gwp /= unitMultiplier; }

            this.density = EC3MaterialParser.ParseFloatWithUnit(obj, "density", out string densityUnit);
            this.densityUnit = densityUnit;

            this.id = obj["id"]?.ToString();
            this.uuid = obj["open_xpd_uuid"]?.ToString();
            this.category = obj["category"]["name"]?.ToString();
        }

        public EPD(string name, float gwp, string unit, float density, string densityUnit, 
            string category, EC3MaterialFilter searchPar = null)
        {
            this.name = name;
            this.gwp = gwp;
            this.unit = unit;
            this.density = density;
            this.densityUnit = densityUnit;
            this.category = category;
            this.searchParameters = searchPar;
        }

        public static float AverageGwp(List<EPD> epds)
        {
            List<float> gwps = new List<float>();
            foreach(EPD epd in epds)
            {
                if (epd.gwp != 0) { gwps.Add(epd.gwp); }
            }
            return gwps.Average();
        }
    }
}
