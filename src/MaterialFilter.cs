using System;
using System.Linq;
using EC3CarbonCalculator.src.EC3;

namespace EC3CarbonCalculator.src
{
    /// <summary>
    /// This class is used to store search criterea for materials in EC3. EC3 uses a 
    /// custom format for search criterea called MaterialFilter. This class also formats
    /// input criterea into MaterialFilter format which can be understood by EC3. 
    /// 
    /// The hope is that in future, this can be further expanded to include properties
    /// specific to different categories.
    /// </summary>
    public class MaterialFilter
    {
        // CategoryTree and GeographyCodes classes used to ensure validity of input params
        EC3CategoryTree ec3CategoryTree = EC3CategoryTree.Instance;
        GeographyCodes geoCodes = GeographyCodes.Instance;

        // Stored search criterea
        public string dataBase { get; set; }
        public string categoryName { get; private set; }
        public string country { get; private set; }
        public string state { get; private set; }
        public string expirationDate { get; private set; }

        public MaterialFilter() { }

        public bool SetCLFCategory(string category)
        {
            if (CLFCategoryTree.categoryNames.Contains(category))
            {
                categoryName = category;
                return true;
            }
            return false;
        }

        public bool SetCLFRegion(string region)
        {
            if (GeographyCodes.Instance.USRegions.Contains(region))
            {
                state = region;
                return true;
            }
            return false;
        }

        public bool SetEC3Category(string category)
        {
            int idx = ec3CategoryTree.GetCategoryIdx(category);
            if (idx == -1) { return false; }

            categoryName = ec3CategoryTree.names[idx];
            return true;
        }

        public bool SetEC3Country(string countryCode)
        {
            if (geoCodes.CountryCodes.Contains(countryCode))
            {
                country = countryCode;
                return true;
            }
            else
            {
                country = null;
            }
            return false;
        }

        public bool SetEC3State(string stateCode)
        {
            if (geoCodes.StateCodes.Contains(stateCode))
            {
                state = stateCode;
                return true;
            }
            else
            {
                state = null;
            }
            return false;
        }

        public bool SetEC3ExpirationDate(DateTime date)
        {
            expirationDate = CompileEC3Date(date);
            return true;
        }

        /// <summary>
        /// Method used only to recreate Material Filters. Do not use other than for
        /// serialization.
        /// </summary>
        public bool SetEC3FormattedExporationDate(string date)
        {
            expirationDate = date;
            return true;
        }

        private static string CompileEC3Date(DateTime date)
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
        public string GetEC3MaterialFilter()
        {
            string pragma = "!pragma eMF(\"2.0/1\"), lcia(\"TRACI 2.1\")";
            string category = $"category:\"{categoryName}\"";
            string date = $"epd__date_validity_ends:>\"{expirationDate}\"";
            string jurisdiction = null;
            if (country != null)
            {
                string jurisdictionCode = country;
                if (state != null && country == "US")
                {
                    jurisdictionCode += $"-{state}";
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
            string category = $"category: {categoryName}";
            string date = $"epd validity ends after: {expirationDate}";
            string jurisdiction = null;
            if (country != null)
            {
                string jurisdictionCode = country;
                if (state != null && country == "US")
                {
                    jurisdictionCode += $"-{state}";
                }
                jurisdiction = $"produced in: {jurisdictionCode}";
            }
            string[] mfArray = new string[] { category, date, jurisdiction };
            return mfArray;
        }
    }
}
