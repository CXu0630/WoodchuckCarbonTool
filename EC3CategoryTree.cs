using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using Rhino.Input;
using System.Reflection;
using UnitsNet;

namespace EC3CarbonCalculator
{
    internal sealed class EC3CategoryTree
    {
        public List<string> names = new List<string>();
        public List<string> masterformats = new List<string>();
        public List<string> ids = new List<string>();
        public List<int> dimensions = new List<int>();
        
        string filePath;

        private static readonly Lazy<EC3CategoryTree> _categoryTreeInstance = 
            new Lazy<EC3CategoryTree>(() => new EC3CategoryTree());

        public static EC3CategoryTree Instance => _categoryTreeInstance.Value;

        private EC3CategoryTree() 
        { 
            // update csv path to be in the same folder as the dll
            string rhpLocation = IOTools.GetRhpLocation();
            string[] locationSplit = rhpLocation.Split('/');
            locationSplit[locationSplit.Length - 1] = "EC3Categories.csv";
            filePath = string.Join("/", locationSplit);

            if (!ReadCategoryCSV())
            {
                UpdateEC3CategoriesToFile();
            }
        }

        public bool ReadCategoryCSV()
        {
            if (!File.Exists(filePath)) return false;
            var allLines = File.ReadAllLines(filePath).ToList();
            if (allLines.Count == 0) return false;
            foreach(string line in allLines)
            {
                string[] catData = line.Split(',');
                if (catData.Length != 4) continue;
                this.names.Add(catData[0]);
                this.masterformats.Add(catData[1]);
                this.ids.Add(catData[2]);
                this.dimensions.Add(int.Parse(catData[3]));
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
            string treeStr = EC3Request.GetCategoryTree();
            JObject rootTree = JObject.Parse(treeStr);

            // reinitialize all data stored
            this.names = new List<string>();
            this.masterformats = new List<string>();
            this.ids = new List<string>();
            this.dimensions = new List<int>();

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
                string[] cats = { this.names[i], this.masterformats[i], this.ids[i], 
                    this.dimensions[i].ToString() };
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
            string declaredUnit = catObj["declared_unit"]?.ToString();

            this.names.Add(name);
            this.masterformats.Add(masterformat);
            this.ids.Add(id);

            IQuantity unitMaterial = EC3MaterialParser.ParseQuantity(declaredUnit, out bool valid);

            if (unitMaterial.GetType() == typeof(Length))
            {
                this.dimensions.Add(1);
            }
            else if (unitMaterial.GetType() == typeof(Area))
            {
                this.dimensions.Add(2);
            }
            else { this.dimensions.Add(3); }

            JArray subcategories = (JArray)catObj["subcategories"];
            if (subcategories == null || subcategories.Count == 0) { return; }

            foreach (JObject subcategory in subcategories)
            {
                this.ParseCategory(subcategory);
            }
        }

        public int GetCategoryIdx(string category)
        {
            if (this.names.Contains(category))
            {
                return this.names.IndexOf(category);
            }
            else if (this.masterformats.Contains(category))
            {
                return this.masterformats.IndexOf(category);
            }
            else if (this.ids.Contains(category))
            {
                return this.ids.IndexOf(category);
            }
            return -1;
        }

        public void SetFilePath(string newPath) { this.filePath = newPath; }

        public string GetFilePath() { return this.filePath; }
    }
}
