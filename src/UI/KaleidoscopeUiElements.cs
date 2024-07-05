using Eto.Forms;
using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoodchuckCarbonTool.src.Kaleidoscope;

namespace WoodchuckCarbonTool.src.UI
{
    internal class KaleidoscopeUiElements
    {
        public delegate void CategoryChangeHandler(object sender, CategoryChangeEventArgs e);
        public event CategoryChangeHandler CategoryChangeEvent;

        public KaleidoscopeUiElements() { }

        public DynamicLayout KaleidoscopeLayout(MaterialFilter mf)
        {
            List<ListItem> catOptions = new List<ListItem>();
            for (int i = 0; i < KaleidoscopeCategoryTree.categoryNames.Count(); i++)
            {
                catOptions.Add(new ListItem
                {
                    Text = KaleidoscopeCategoryTree.categoryNames[i],
                    Key = KaleidoscopeCategoryTree.categoryNames[i]
                });
            }
            DropDown catDD = new DropDown();
            catDD.DataStore = catOptions;
            // set default values
            int defaultIdx = 0;
            catDD.SelectedIndex = defaultIdx;
            mf.SetKaleidoscopeCategory(catDD.SelectedKey);
            CategoryChangeEvent.Invoke(catDD, new CategoryChangeEventArgs(KaleidoscopeCategoryTree.categoryNames[defaultIdx]));
            // set listener
            catDD.SelectedValueChanged += (sender, e) =>
            {
                mf.SetKaleidoscopeCategory(catDD.SelectedKey);
                CategoryChangeEvent.Invoke(this, new CategoryChangeEventArgs(catDD.SelectedKey));
            };

            Label catLabel = new Label { Text = "Category" };

            DynamicLayout dl = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5)
            };

            dl.BeginHorizontal();
            dl.Add(catLabel);
            dl.Add(null);
            dl.Add(catDD);
            dl.EndHorizontal();

            return dl;
        }

        public DynamicLayout scopeLayout (MaterialFilter mf)
        {
            CheckBox biogenCheckBox = new CheckBox { 
                ThreeState = false, 
                Checked = false,
                ToolTip = "Biogenic carbon refers to carbon removed from the atmosphere " +
                "by photosynthesis and is stored in biofuels. Biogenic carbon is generally " +
                "eventually released back into the atmosphere due to degredation."
            };
            biogenCheckBox.CheckedChanged += (sender, e) =>
            {
                mf.includeBiogen = (bool)biogenCheckBox.Checked;
            };

            CheckBox dCheckBox = new CheckBox { 
                ThreeState = false, 
                Checked = false,
                ToolTip = "Stage D of the embodied carbon cycle refers to material " +
                "reuse, recycling, and recovery. Stage D is generally more speculative " +
                "than other carbon stages."
            };
            dCheckBox.Enabled = false;
            dCheckBox.CheckedChanged += (sender, e) =>
            {
                mf.includeD = (bool)dCheckBox.Checked;
            };

            CheckBox bcCheckBox = new CheckBox { 
                ThreeState = false, 
                Checked = false,
                ToolTip = "Stages B and C refer to the use and end of life of building " +
                "materials."
            };
            bcCheckBox.CheckedChanged += (sender, e) =>
            {
                mf.includeBC = (bool)bcCheckBox.Checked;
                if ((bool)bcCheckBox.Checked)
                {
                    dCheckBox.Enabled = true;
                } else
                {
                    dCheckBox.Enabled = false;
                }
            };

            Label biogenLabel = new Label { Text = "Include Biogenic Carbon" };
            Label dLabel = new Label { Text = "Include Stage D" };
            Label bcLabel = new Label { Text = "Include Stages B and C" };

            DynamicLayout dl = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5)
            };

            dl.BeginHorizontal();
            dl.Add(biogenCheckBox);
            dl.Add(biogenLabel);
            dl.EndBeginHorizontal();
            dl.Add(bcCheckBox);
            dl.Add(bcLabel);
            dl.EndBeginHorizontal();
            dl.Add(dCheckBox);
            dl.Add(dLabel);
            dl.EndHorizontal();

            return dl;
        }

        internal class CategoryChangeEventArgs : EventArgs
        {
            public string category;
            public CategoryChangeEventArgs(string cat)
            {
                category = cat;
            }
        }
    }
}
