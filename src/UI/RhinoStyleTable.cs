using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using System.Runtime.CompilerServices;

namespace WoodchuckCarbonTool.src.UI
{
    internal class RhinoStyleTable : DynamicLayout
    {
        public int leftSpacerWidth = 15;

        public void AddTitle (string title)
        {
            Label titleLabel = new Label
            {
                Text = title,
                Font = new Eto.Drawing.Font(SystemFonts.Default().FamilyName, 10)
            };

            // Add spacer before a title if there is already content in the dynamic
            // layout
            if (this.Rows.Count > 0)
            {
                this.Add(UICommonElements.SeparationLine(Colors.Gray, 0.8f));
            }
            this.Add(titleLabel);
            this.Add(UICommonElements.SeparationLine(Colors.Gray, 0.8f));
        }

        public void AddSubtable(Control[][] subtableControls, double[] distribution = null)
        {
            DynamicLayout subtableLayout = new DynamicLayout();
            Panel leftSpacer = new Panel { Width = this.leftSpacerWidth };
            RhinoStyleSubtable subtable = new RhinoStyleSubtable
            {
                Spacing = this.Spacing
            };

            if (distribution == null)
            {
                double width = (double) 1 / subtableControls.Length;
                distribution = new double[subtableControls.Length];
                for(int i = 0; i < subtableControls.Length; i++)
                {
                    distribution[i] = width;
                }
            }
            subtable.PopulateTable(subtableControls, distribution, this.Width - this.leftSpacerWidth);
            subtableLayout.BeginHorizontal();
            subtableLayout.Add(leftSpacer);
            subtableLayout.Add(subtable);
            subtableLayout.EndHorizontal();

            this.Add(subtableLayout);
        }

        public void AddBlankRow()
        {
            DynamicLayout blankRowLayout = new DynamicLayout();
            Panel panel = new Panel { Height = 20 };
            blankRowLayout.Add(panel);
            this.Add(blankRowLayout);
        }
    }
}
