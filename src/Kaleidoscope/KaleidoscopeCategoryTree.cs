using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodchuckCarbonTool.src.Kaleidoscope
{
    internal class KaleidoscopeCategoryTree
    {
        public static string[] categoryNames = new string[] {"Wall", "Ceiling", 
            "Flooring", "Partitions", "Envelopes"};

        public static int GetDimension(string categoryName)
        {
            if (categoryNames.Contains(categoryName))
            {
                return 2;
            }
            return 0;
        }
    }
}
