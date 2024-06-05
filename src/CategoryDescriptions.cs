using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodchuckCarbonTool.src
{
    internal class CategoryDescriptions
    {
        private static readonly Lazy<CategoryDescriptions> _categoryDescriptionsInstance =
            new Lazy<CategoryDescriptions>(() => new CategoryDescriptions());

        public static CategoryDescriptions Instance => _categoryDescriptionsInstance.Value;

        List<string> descriptions = new List<string>();
        Dictionary<string, int> categoryToDescriptionIdx = new Dictionary<string, int>();

        public CategoryDescriptions() 
        {
            List<List<string>> csvData = IOTools.ReadCSVFromEmbedded(
                "2024_05_CategoryDescriptions_v001");
            for(int i = 0; i < csvData.Count; i++)
            {
                List<string> data = csvData[i];
                string categories = data[0];
                string description = data[1];

                if (description == "" | description == null) 
                {
                    descriptions.Add("");
                    continue; 
                }

                descriptions.Add(description);

                string[] categoryArray = categories.Split(',');
                for (int j = 0; j < categoryArray.Length; j++)
                {
                    string trimmed = categoryArray[j].Trim();
                    categoryToDescriptionIdx[trimmed] = i;
                }
            }
        }

        public string GetCategoryDescription(string category)
        {
            if (!categoryToDescriptionIdx.ContainsKey(category)) { return null; }

            int idx = categoryToDescriptionIdx[category];
            return descriptions[idx];
        }
    }
}
