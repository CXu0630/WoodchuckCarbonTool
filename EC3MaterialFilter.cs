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
        GeographyCodes geoCodes = GeographyCodes.Instance;
        public string categoryName { get; private set; }
        public string countryCode { get; private set; }
        public string stateCode { get; private set; }
        public string expirationDate { get; private set; }

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
            if(geoCodes.CountryCodes.Contains(countryCode))
            {
                this.countryCode = countryCode;
                return true;
            } 
            else
            {
                this.countryCode = null;
            }
            return false;
        }

        public bool SetState(string stateCode)
        {
            if (geoCodes.StateCodes.Contains(stateCode))
            {
                this.stateCode = stateCode;
                return true;
            }
            else
            {
                this.stateCode = null;
            }
            return false;
        }

        public bool SetExpirationDate(DateTime date)
        {
            this.expirationDate = CompileDate(date);
            return true;
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
