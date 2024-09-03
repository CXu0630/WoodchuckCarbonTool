using Eto.Forms;
using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodchuckCarbonTool.src.UI
{
    internal class RhinoStyleSubtable : DynamicLayout
    {
        public void PopulateTable(Control[][] controlColumns, double[] distribution, int width = -1)
        {
            if (width > 0) this.Width = width;
            if (distribution.Length < controlColumns.Length)
            {
                double[] newDistribution = new double[controlColumns.Length];
                Array.Copy(distribution, newDistribution, distribution.Length);
                distribution = newDistribution;
            }
            //int cellWidth = Math.DivRem(this.Width, controlColumns.Length, out int a);

            int maxLength = controlColumns.Max(column => column.Length);

            for (int i = 0; i < maxLength; i++)
            {
                Control[] controlRow = new Control[controlColumns.Length];
                for (int j = 0; j < controlColumns.Length; j++)
                {
                    if (i < controlColumns[j].Length)
                    {
                        controlRow[j] = UICommonElements.FixedWidthCell(controlColumns[j][i], (int)Math.Round(distribution[j] * this.Width));
                    }
                    else
                    {
                        controlRow[j] = UICommonElements.FixedWidthCell(null, (int)Math.Round(distribution[j] * this.Width));
                    }
                }
                DynamicLayout rowLayout = RhinoStyleRow(controlRow);
                this.Add(rowLayout);

                if (i != maxLength - 1)
                {
                    this.Add(UICommonElements.SpacerLine(Colors.Gray, 0.8f, 3));
                }
            }
        }

        private DynamicLayout RhinoStyleRow(Control[] controls)
        {
            DynamicLayout row = new DynamicLayout { Spacing = this.Spacing };
            row.AddRow(controls);
            return row;
        }
    }
}
