using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using System.Runtime.CompilerServices;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino;
using System.Runtime.Remoting.Channels;

namespace WoodchuckCarbonTool.src.UI
{
    internal class WCKObjectPropertiesControl : Panel
    {
        public void PopulateControl(RhinoDoc doc, int numAssignedObjs, double totalGwp,
            string database, EPD uniqueEpd, int percentageSolid, ObjRef[] selectedObjs)
        {
            RhinoStyleTable propertiesTable = new RhinoStyleTable
            {
                Spacing = new Size(10, 4),
                Width = this.Width
            };

            propertiesTable.AddTitle("Carbon Properties");

            Control numAssignedTxt = new Label { Text = "Selected Objects with EPDs" };
            Control numAssignedRsp = new Label { Text = numAssignedObjs.ToString() };

            Control totalGwpTxt = new Label { Text = "Total GWP" };
            Control totalGwpRsp = new Label { Text = totalGwp.ToString() };

            if (numAssignedObjs == 0) { database = "None"; }

            Control databaseTxt = new Label { Text = "Source Database" };
            Control databaseRsp = new Label { Text = database };

            Control[] texts = new Control[] { numAssignedTxt, totalGwpTxt, databaseTxt };
            Control[] rsps = new Control[] { numAssignedRsp, totalGwpRsp, databaseRsp };

            Control[][] controlColumns = new Control[][] { texts, rsps };

            propertiesTable.AddSubtable(controlColumns);

            propertiesTable.AddTitle("Assigned EPD");

            if (numAssignedObjs == 0)
            {
                Label noEpdLabel = new Label { Text = "No EPDs assigned." };
                Button searchEpdButton = new Button { Text = "Search EPDs" };
                searchEpdButton.Click += (sender, e) =>
                {
                    SearchEPDCommand.Instance.SearchEPD(doc);
                };

                propertiesTable.AddSubtable(new Control[][] { new Control[] { noEpdLabel }, new Control[] { searchEpdButton } });
            }
            else
            {
                EPDPanel uniqueEpdPanel = new EPDPanel(doc, uniqueEpd, this.Width - 20);
                propertiesTable.AddSubtable(new Control[][] { new Control[] { uniqueEpdPanel } });
            }

            propertiesTable.AddTitle("Material Quantity");

            Label percentageSolidLabel = new Label { Text = "Percentage of solid material" };
            TextBox percentageSolidTextBox = new TextBox { };
            if (percentageSolid > 0) percentageSolidTextBox.Text = percentageSolid.ToString();
            else percentageSolidTextBox.Text = "100";

            propertiesTable.Add(null);

            this.Content = propertiesTable;
        }

    }
}
