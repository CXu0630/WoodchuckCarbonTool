using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using Rhino.Input;
using System.Reflection;

namespace EC3CarbonCalculator
{
    internal class EC3CategoryTree
    {
        List<string> names = new List<string>();
        List<string> masterformats = new List<string>();
        List<string> ids = new List<string>();
        
        string apiKey;
        string filePath;

        public EC3CategoryTree(string apiKey) 
        { 
            this.apiKey = apiKey; 

            // update csv path to be in the same folder as the dll
            string rhpLocation = IOTools.GetRhpLocation();
            string[] locationSplit = rhpLocation.Split('/');
            locationSplit[locationSplit.Length - 1] = "EC3Categories.csv";
            filePath = string.Join("/", locationSplit);
        }

        public bool ReadCategoryCSV()
        {
            if (!File.Exists(filePath)) return false;
            var allLines = File.ReadAllLines(filePath).ToList();
            if (allLines.Count == 0) return false;
            foreach(string line in allLines)
            {
                string[] catData = line.Split(',');
                if (catData.Length != 3) continue;
                this.names.Add(catData[0]);
                this.masterformats.Add(catData[1]);
                this.ids.Add(catData[2]);
            }
            return true;
        }

        /// <summary>
        /// Updates category data stored within this object with the most recent data
        /// from EC3.
        /// </summary>
        public void UpdateEC3Categories()
        {
            // get category tree from API request
            EC3Request treeReq = new EC3Request(apiKey);
            string treeStr = treeReq.GetCategoryTree();
            JObject rootTree = JObject.Parse(treeStr);

            // reinitialize all data stored
            this.names = new List<string>();
            this.masterformats = new List<string>();
            this.ids = new List<string>();

            this.ParseCategory(rootTree);
        }

        /// <summary>
        /// Updates category data stored within thsi object as well as category data
        /// stored within the csv at the indicated file location with the most recent
        /// data from EC3.
        /// </summary>
        public void UpdateEC3CategoriesToFile()
        {
            this.UpdateEC3Categories();
            List<string> allLines = new List<string>();

            for (int i = 0; i < this.names.Count; i++)
            {
                string[] cats = { this.names[i], this.masterformats[i], this.ids[i] };
                string catCSV = string.Join(",", cats);

                allLines.Add(catCSV);
            }

            File.WriteAllLines(filePath, allLines);
        }

        /// <summary>
        /// Recursive method that reads nested category data from a category tree API
        /// request to EC3.
        /// </summary>
        /// <param name="catObj"></param>
        public void ParseCategory(JObject catObj)
        {
            string name = catObj["name"]?.ToString();
            string masterformat = catObj["masterformat"]?.ToString();
            string id = catObj["id"]?.ToString();

            this.names.Add(name);
            this.masterformats.Add(masterformat);
            this.ids.Add(id);

            JArray subcategories = (JArray)catObj["subcategories"];
            if (subcategories == null || subcategories.Count == 0) { return; }

            foreach (JObject subcategory in subcategories)
            {
                this.ParseCategory(subcategory);
            }
        }

        public void SetFilePath(string newPath) { this.filePath = newPath; }

        public string GetFilePath() { return this.filePath; }
    }
}
