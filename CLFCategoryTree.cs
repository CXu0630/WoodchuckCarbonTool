using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC3CarbonCalculator
{
    internal class CLFCategoryTree
    {
        public static string[] categoryNames = new string[] {"US Regional Ready-Mix Concrete", 
            "Concrete", "Masonry", "Steel", "Aluminum", "Wood and Composites", "Insulation",
            "Cladding and Roofing", "Openings", "Finishes"};
        public static int[] dimensions = new int[] { 3, 3, 3, 3, 3, 3, 2, 2, 3, 2 };

        public static int GetDimension(string categoryName)
        {
            if (categoryNames.Contains(categoryName))
            {
                return dimensions[Array.IndexOf(categoryNames, categoryName)];
            }
            return 0;
        }
    }
}
