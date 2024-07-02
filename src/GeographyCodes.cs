using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace WoodchuckCarbonTool.src
{
    internal class GeographyCodes
    {
        public List<string> CountryCodes;
        public List<string> CountryNames;

        public List<string> StateCodes;
        public List<string> StateNames;

        public List<string> USRegions = new List<string> { "Pacific Southwest",
            "Pacific Northwest", "Rocky Mountains", "South Central", "North Central",
            "Southeastern", "Great Lakes", "Eastern", "National" };

        private static readonly Lazy<GeographyCodes> _geoCodeInstance =
            new Lazy<GeographyCodes>(() => new GeographyCodes());

        public static GeographyCodes Instance => _geoCodeInstance.Value;

        private GeographyCodes()
        {
            PopulateAreaCodeLists("ISOCountryCodes", out CountryNames, out CountryCodes);
            PopulateAreaCodeLists("USStateCodes", out StateNames, out StateCodes);
        }

        private void PopulateAreaCodeLists(string csvName, out List<string> names, out List<string> codes)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WoodchuckCarbonTool.EmbeddedResources." + csvName + ".csv";

            List<string> areaNames = new List<string>();
            List<string> areaCodes = new List<string>();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split('"');
                    if (parts.Length > 1)
                    {
                        areaNames.Add(parts[1]);
                        areaCodes.Add(parts[2].Trim(','));
                    }
                    else
                    {
                        parts = line.Split(',');
                        areaNames.Add(parts[0]);
                        areaCodes.Add(parts[1]);
                    }
                }
            }

            names = areaNames;
            codes = areaCodes;
        }

        public string GetCountryName(string code)
        {
            int idx = CountryCodes.FindIndex(s => s.Equals(code));
            if (idx != -1)
            {
                return CountryNames[idx];
            }
            else
            {
                return null;
            }
        }

        public string GetCountryCode(string name)
        {
            int idx = CountryNames.FindIndex(s => s.Equals(name));
            if (idx != -1)
            {
                return CountryCodes[idx];
            }
            else
            {
                return null;
            }
        }

        public string GetStateName(string code)
        {
            int idx = StateCodes.FindIndex(s => s.Equals(code));
            if (idx != -1)
            {
                return StateNames[idx];
            }
            else
            {
                return null;
            }
        }

        public string GetStateCode(string name)
        {
            int idx = StateNames.FindIndex(s => s.Equals(name));
            if (idx != -1)
            {
                return StateCodes[idx];
            }
            else
            {
                return null;
            }
        }
    }
}
