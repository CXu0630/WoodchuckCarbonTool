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
    /// <summary>
    /// This class is used to store search criterea for materials in EC3. EC3 uses a 
    /// custom format for search criterea called MaterialFilter. This class also formats
    /// input criterea into MaterialFilter format which can be understood by EC3. 
    /// 
    /// The hope is that in future, this can be further expanded to include properties
    /// specific to different categories.
    /// </summary>
    public class EC3MaterialFilter
    {
        // CategoryTree and GeographyCodes classes used to ensure validity of input params
        EC3CategoryTree categoryTree = EC3CategoryTree.Instance;
        GeographyCodes geoCodes = GeographyCodes.Instance;

        // Stored search criterea
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

        /// <summary>
        /// Method used only to recreate Material Filters. Do not use other than for
        /// serialization.
        /// </summary>
        public bool SetFormattedExporationDate(string date)
        {
            this.expirationDate = date;
            return true;
        }

        private static string CompileDate(DateTime date)
        {
            return date.Year.ToString() + "-" + date.Month.ToString() + "-" + 
                date.Day.ToString();
        }

        /// <summary>
        /// Formats information in this class into MaterialFilter fomat readable by EC3's
        /// servers.
        /// </summary>
        /// <returns> A string containing the search criterea information stored in this
        /// object in MaterialFilter format </returns>
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

        /// <summary>
        /// Primarily used for debugging. Retreives the infromation stored in this class
        /// as strings to be printed or otherwise displayed.
        /// </summary>
        /// <returns> An array of strings containing search parameters stored in this 
        /// object. </returns>
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
