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
        public void PopulateTable(Control[][] controlColumns, int width = -1)
        {
            if (width > 0) this.Width = width;
            int cellWidth = Math.DivRem(this.Width, controlColumns.Length, out int a);

            int maxLength = controlColumns.Max(column => column.Length);

            for (int i = 0; i < maxLength; i++)
            {
                Control[] controlRow = new Control[controlColumns.Length];
                for (int j = 0; j < controlColumns.Length; j++)
                {
                    if (i < controlColumns[j].Length)
                    {
                        controlRow[j] = UICommonElements.FixedWidthCell(controlColumns[j][i], cellWidth);
                    }
                    else
                    {
                        controlRow[j] = UICommonElements.FixedWidthCell(null, cellWidth);
                    }
                }
                DynamicLayout rowLayout = RhinoStyleRow(controlRow);
                this.Add(rowLayout);

                if (i != maxLength - 1)
                {
                    this.Add(UICommonElements.SeparationLine(Colors.Gray, 0.8f));
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
