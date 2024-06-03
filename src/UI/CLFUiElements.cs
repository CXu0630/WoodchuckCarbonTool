using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;

namespace WoodchuckCarbonTool.src.UI
{
    internal class CLFUiElements
    {
        public Panel clfPanel;

        public CLFUiElements()
        {
            clfPanel = new Panel();
        }

        private void RepopulateCLFPanel(MaterialFilter mf)
        {
            DynamicLayout dl = new DynamicLayout();

            if (mf.categoryName == "US Regional Ready-Mix Concrete")
            {
                dl.Add(SearchForm.Spacer(Colors.DarkGray));
                dl.Add(USRegionLayout(mf));
            }

            clfPanel.Content = dl;
        }

        public DynamicLayout CLFCategoryLayout(MaterialFilter mf)
        {
            List<ListItem> catOptions = new List<ListItem>();
            for (int i = 0; i < CLFCategoryTree.categoryNames.Count(); i++)
            {
                catOptions.Add(new ListItem
                {
                    Text = CLFCategoryTree.categoryNames[i],
                    Key = CLFCategoryTree.categoryNames[i]
                });
            }
            DropDown catDD = new DropDown();
            catDD.DataStore = catOptions;
            // set default values
            catDD.SelectedIndex = 1;
            mf.SetCLFCategory(catDD.SelectedKey);
            // set listener
            catDD.SelectedValueChanged += (sender, e) =>
            {
                mf.SetCLFCategory(catDD.SelectedKey);
                if (catDD.SelectedIndex != 0)
                {
                    mf.SetCLFRegion(null);
                }
                RepopulateCLFPanel(mf);
            };

            Label catLabel = new Label { Text = "Category" };

            DynamicLayout catLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
            };

            catLayout.BeginHorizontal();
            catLayout.Add(catLabel);
            catLayout.Add(null);
            catLayout.Add(catDD);
            catLayout.EndHorizontal();

            return catLayout;
        }

        public DynamicLayout USRegionLayout(MaterialFilter mf)
        {
            DynamicLayout dl = new DynamicLayout();

            List<ListItem> regionOptions = new List<ListItem>();
            for (int i = 0; i < GeographyCodes.Instance.USRegions.Count(); i++)
            {
                regionOptions.Add(new ListItem
                {
                    Text = GeographyCodes.Instance.USRegions[i],
                    Key = GeographyCodes.Instance.USRegions[i]
                });
            }
            DropDown regionDD = new DropDown();
            regionDD.DataStore = regionOptions;
            // set default values
            regionDD.SelectedIndex = 0;
            mf.SetCLFRegion(regionDD.SelectedKey);
            // set listener
            regionDD.SelectedValueChanged += (sender, e) =>
            {
                mf.SetCLFRegion(regionDD.SelectedKey);
            };

            Label regionLabel = new Label { Text = "US Region" };

            dl.BeginHorizontal();
            dl.Add(regionLabel);
            dl.Add(null);
            dl.Add(regionDD);
            dl.EndHorizontal();

            return dl;
        }
    }
}
