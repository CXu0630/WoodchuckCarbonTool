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
    /// <summary>
    /// A singleton class that contains EC3 category information. It stores the category
    /// information in a csv file at the location of the executable rhp (is this a good 
    /// move? Maybe they should be stored in embeded resources and refreshed at request?).
    /// If there is no csv file containing category information, it creates one and infils
    /// that csv with category info from a request to EC3. This class also stores lists
    /// containing category names, masterformats, ids, and dimensions for other classes
    /// to retreive.
    /// 
    /// Warning: Usability of this class is in question... Other than good for populating
    /// the selection dropdown (which would probably function better with hand-picked 
    /// categories instead of using every single category from EC3) and for mapping values
    /// to category names, there isn't much incentive to use this class... Using it to 
    /// find dimensions of materials seems unreliable as the declared dimension for the
    /// category and each specific EPD could be at odds. The way in which the csv is
    /// accessed and populated is also not the most efficient. There is a high possibility
    /// that the csv will remain unupdated for some time and be out of date.
    /// </summary>
    internal sealed class EC3CategoryTree
    {
        // category properties to be accessed by other classes
        public List<string> names = new List<string>();
        public List<string> masterformats = new List<string>();
        public List<string> ids = new List<string>();
        public List<int> dimensions = new List<int>();
        
        string filePath;

        // lazy singleton: there is only ever once instance of the EC3CategoryTree that
        // is created when it is first needed
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

            // if there is currently no csv containing the information, retreive current
            // category information from EC3 and populate a csv with that info
            if (!ReadCategoryCSV())
            {
                UpdateEC3CategoriesToFile();
            }
        }

        /// <summary>
        /// Parses the csv containing category information and populates the lists
        /// </summary>
        /// <returns></returns>
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
        public void ParseCategory(JObject catObj)
        {
            string name = catObj["name"]?.ToString();
            string masterformat = catObj["masterformat"]?.ToString();
            string id = catObj["id"]?.ToString();
            string declaredUnit = catObj["declared_unit"]?.ToString();

            this.names.Add(name);
            this.masterformats.Add(masterformat);
            this.ids.Add(id);

            if (declaredUnit.Contains("tkm"))
            { this.dimensions.Add(0); }
            else
            {
                IQuantity unitMaterial = UnitManager.ParseQuantity(declaredUnit, out bool valid);

                if (unitMaterial.GetType() == typeof(Length))
                {
                    this.dimensions.Add(1);
                }
                else if (unitMaterial.GetType() == typeof(Area))
                {
                    this.dimensions.Add(2);
                }
                else if (unitMaterial.GetType() == typeof(Volume) || 
                    unitMaterial.GetType() == typeof(Mass))
                {
                    this.dimensions.Add(3);
                }
                else { this.dimensions.Add(0); }
            }

            JArray subcategories = (JArray)catObj["subcategories"];
            if (subcategories == null || subcategories.Count == 0) { return; }

            foreach (JObject subcategory in subcategories)
            {
                this.ParseCategory(subcategory);
            }
        }

        /// <summary>
        /// Retreives the index of a category based on an identifier provided. The index
        /// of the category is the same accross all lists.
        /// </summary>
        /// <returns>Index of a category within all stored category information lists 
        /// in this class</returns>
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
            // failed to find the category
            return -1;
        }

        /// <summary>
        /// Gets the dimension of a category based on category identifier (name, 
        /// masterformat code, etc.)
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public int GetCategoryDimension(string category)
        {
            return this.dimensions[GetCategoryIdx(category)];
        }

        public void SetFilePath(string newPath) { this.filePath = newPath; }

        public string GetFilePath() { return this.filePath; }
    }
}
