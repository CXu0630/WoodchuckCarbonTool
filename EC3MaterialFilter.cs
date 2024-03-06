using Eto.Forms;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EC3CarbonCalculator
{
    internal class EC3MaterialFilter
    {
        EC3CategoryTree categoryTree = EC3CategoryTree.Instance;
        string categoryName = "ConstructionMaterials";
        string countryCode = null;
        string stateCode = null;
        string expirationDate = CompileDate(DateTime.Now);

        public static string[] countryCodes = new string[]
        {
            "AF", "AX", "AL", "DZ", "AS", "AD", "AO", "AI", "AQ", "AG",
            "AR", "AM", "AW", "AU", "AT", "AZ", "BS", "BH", "BD", "BB",
            "BY", "BE", "BZ", "BJ", "BM", "BT", "BO", "BQ", "BA", "BW",
            "BV", "BR", "IO", "BN", "BG", "BF", "BI", "KH", "CM", "CA",
            "CV", "KY", "CF", "TD", "CL", "CN", "CX", "CC", "CO", "KM",
            "CG", "CD", "CK", "CR", "CI", "HR", "CU", "CW", "CY", "CZ",
            "DK", "DJ", "DM", "DO", "EC", "EG", "SV", "GQ", "ER", "EE",
            "ET", "FK", "FO", "FJ", "FI", "FR", "GF", "PF", "TF", "GA",
            "GM", "GE", "DE", "GH", "GI", "GR", "GL", "GD", "GP", "GU",
            "GT", "GG", "GN", "GW", "GY", "HT", "HM", "VA", "HN", "HK",
            "HU", "IS", "IN", "ID", "IR", "IQ", "IE", "IM", "IL", "IT",
            "JM", "JP", "JE", "JO", "KZ", "KE", "KI", "KP", "KR", "KW",
            "KG", "LA", "LV", "LB", "LS", "LR", "LY", "LI", "LT", "LU",
            "MO", "MK", "MG", "MW", "MY", "MV", "ML", "MT", "MH", "MQ",
            "MR", "MU", "YT", "MX", "FM", "MD", "MC", "MN", "ME", "MS",
            "MA", "MZ", "MM", "NA", "NR", "NP", "NL", "NC", "NZ", "NI",
            "NE", "NG", "NU", "NF", "MP", "NO", "OM", "PK", "PW", "PS",
            "PA", "PG", "PY", "PE", "PH", "PN", "PL", "PT", "PR", "QA",
            "RE", "RO", "RU", "RW", "BL", "SH", "KN", "LC", "MF", "PM",
            "VC", "WS", "SM", "ST", "SA", "SN", "RS", "SC", "SL", "SG",
            "SX", "SK", "SI", "SB", "SO", "ZA", "GS", "SS", "ES", "LK",
            "SD", "SR", "SJ", "SZ", "SE", "CH", "SY", "TW", "TJ", "TZ",
            "TH", "TL", "TG", "TK", "TO", "TT", "TN", "TR", "TM", "TC",
            "TV", "UG", "UA", "AE", "GB", "US", "UM", "UY", "UZ", "VU",
            "VE", "VN", "VG", "VI", "WF", "EH", "YE", "ZM", "ZW"
        };

        public static string[] stateCodes = new string[]
        {
            "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
            "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
            "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
            "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
            "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY"
        };

        public EC3MaterialFilter() { }

        public bool SetCategory(string category)
        {
            int idx = categoryTree.GetCategoryIdx(category);
            if (idx == -1) { return false; }

            this.categoryName = categoryTree.names[idx];
            return true;
        }

        public bool SetCountry(string countryCode)
        {
            if(countryCodes.Contains(countryCode))
            {
                this.countryCode = countryCode;
                return true;
            }
            return false;
        }

        public bool SetState(string stateCode)
        {


            if (stateCodes.Contains(stateCode))
            {
                this.stateCode = stateCode;
                return true;
            }
            return false;
        }

        public bool SetExpirationDate(string expirationDate)
        {
            string[] dateFormats = { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", 
                "yyyy/MM/dd" };
            if (DateTime.TryParseExact(expirationDate, dateFormats, null, 
                System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                this.expirationDate = CompileDate(parsedDate);
                return true;
            }
            return false;
        }

        private static string CompileDate(DateTime date)
        {
            return date.Year.ToString() + "-" + date.Month.ToString() + "-" + 
                date.Day.ToString();
        }

        public string GetMaterialFilter()
        {
            string pragma = "!pragma eMF(\"2.0/1\"), lcia(\"TRACI 2.1\")";
            string category = $"category:\"{this.categoryName}\"";
            string date = $"epd__date_validity_ends:>\"{this.expirationDate}\"";
            string jurisdiction = null;
            if (this.countryCode != null)
            {
                string jurisdictionCode = this.countryCode;
                if (this.stateCode != null && this.countryCode == "US")
                {
                    jurisdictionCode += $"-{this.stateCode}";
                }
                jurisdiction = $"jurisdiction:IN(\"{jurisdictionCode}\")";
            }
            string[] mfArray = new string[] { pragma, category, date };
            string mf = string.Join(" ", mfArray);
            if (jurisdiction != null)
            {
                mf += $" {jurisdiction}";
            }
            return mf;
        }

        public string[] GetPrintableData()
        {
            string category = $"category: {this.categoryName}";
            string date = $"epd validity ends after: {this.expirationDate}";
            string jurisdiction = null;
            if (this.countryCode != null)
            {
                string jurisdictionCode = this.countryCode;
                if (this.stateCode != null && this.countryCode == "US")
                {
                    jurisdictionCode += $"-{this.stateCode}";
                }
                jurisdiction = $"produced in: {jurisdictionCode}";
            }
            string[] mfArray = new string[] {category, date, jurisdiction };
            return mfArray;
        }
    }
}
