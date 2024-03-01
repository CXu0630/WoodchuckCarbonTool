using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC3CarbonCalculator
{
    internal class EC3MaterialFilter
    {
        EC3CategoryTree categoryTree = EC3CategoryTree.Instance;
        string category = "ConstructionMaterials";

        public EC3MaterialFilter() { }

        public bool SetCategory(string category)
        {
            int idx = categoryTree.GetCategoryIdx(category);
            if (idx == -1) { return false; }

            this.category = categoryTree.names[idx];
            return true;
        }
    }
}
