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
    internal class WCKObjectPropertiesControl : Panel
    {
        public WCKObjectPropertiesControl()
        {
            
        }

        public void PopulateControl (int numAssignedObjs, double totalGwp, 
            string database, EPD uniqueEpd, int percentageSolid)
        {
            DynamicLayout propertiesDl = new DynamicLayout
            {
                Padding = new Padding(10),
                DefaultSpacing = new Size(3, 2)
            };

            int keyWidth = 50;

            Panel numAssignedTxt = CreateCell(new Label { Text = "Objects with EPDs" }, Colors.White);
            Panel numAssignedRsp = CreateCell(new Label { Text = numAssignedObjs.ToString() }, Colors.White);

            Panel totalGwpTxt = CreateCell(new Label { Text = "Total GWP" }, Colors.White);
            Panel totalGwpRsp = CreateCell(new Label { Text = totalGwp.ToString() }, Colors.White);

            Panel databaseTxt = CreateCell(new Label { Text = "Source Database" }, Colors.White);
            Panel databaseRsp = CreateCell(new Label { Text = database }, Colors.White);

            propertiesDl.AddRow(new Control[] { numAssignedTxt, numAssignedRsp });
            propertiesDl.AddRow(new Control[] { totalGwpTxt, totalGwpRsp });
            propertiesDl.AddRow(new Control[] { databaseTxt, databaseRsp });

            DynamicLayout mainDl = new DynamicLayout();

            mainDl.Add(propertiesDl);
            mainDl.Add(null);

            this.Content = mainDl;
        }

        private Panel CreateCell(Control control, Color clr, int width = -1)
        {
            var panel = new Panel
            {
                Padding = new Padding(4),
                Content = control,
                BackgroundColor = clr, 
            };

            if (width > 0) { panel.Width = width; }

            return panel;
        }
    }
}
