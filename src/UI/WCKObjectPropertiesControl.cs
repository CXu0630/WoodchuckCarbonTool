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
using Rhino.UI;

namespace WoodchuckCarbonTool.src.UI
{
    internal class WckObjectPropertiesControl : Panel
    {
        private ObjectPropertiesPage parent;
        private ObjectPropertiesPageEventArgs eventArgs;

        public void PopulateControl(RhinoDoc doc, int numAssignedObjs, double totalGwp,
            string database, EPD uniqueEpd, int percentageSolid, List<ObjRef> selectedObjs)
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
            Control totalGwpRsp = new Label { Text = GwpCalculator.FormatDoubleWithLengthLimit(totalGwp, 12) };

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
                    SearchEpdCommand.Instance.SearchEPD(doc);
                };

                propertiesTable.AddSubtable(new Control[][] { new Control[] { noEpdLabel }, new Control[] { searchEpdButton } });
            }
            else
            {
                EpdPanel uniqueEpdPanel = new EpdPanel(doc, uniqueEpd, this.Width - 20);
                propertiesTable.AddSubtable(new Control[][] { new Control[] { uniqueEpdPanel } });
            }

            propertiesTable.AddTitle("Material Quantity");

            Label percentageSolidLabel = new Label { Text = "Percentage solid" };
            TextBox percentageSolidTextBox = new TextBox { };
            if (percentageSolid > 0) percentageSolidTextBox.Text = percentageSolid.ToString();
            else if (numAssignedObjs > 0) percentageSolidTextBox.Text = "";
            else percentageSolidTextBox.Text = "100";

            string previousText = percentageSolidTextBox.Text;
            percentageSolidTextBox.TextChanged += (sender, e) =>
            {
                int val;
                if (percentageSolidTextBox.Text == "")
                {
                    previousText = percentageSolidTextBox.Text;
                }
                else if (!int.TryParse(percentageSolidTextBox.Text, out val) ||
                val < 0 || val > 100)
                {
                    percentageSolidTextBox.Text = previousText;
                }
                else
                {
                    previousText = percentageSolidTextBox.Text;
                    foreach (ObjRef objRef in selectedObjs)
                    {
                        EpdManager.UpdatePercentSolid(objRef, val);
                    }
                }
            };

            propertiesTable.AddSubtable(new Control[][] { new Control[] { percentageSolidLabel }, new Control[] { percentageSolidTextBox } });
            propertiesTable.AddBlankRow();

            Button updateButton = new Button { Text = "Update" };
            updateButton.Click += (sender, e) =>
            {
                this.parent.UpdatePage(this.eventArgs);
            };

            propertiesTable.AddSubtable(new Control[][] { new Control[] { new Panel() }, new Control[] { updateButton } });

            propertiesTable.Add(null);
            this.Content = propertiesTable;
        }

        public void SetParent (ObjectPropertiesPage parent)
        {
            this.parent = parent;
        }

        public void SetEventArgs (ObjectPropertiesPageEventArgs eventArgs)
        {
            this.eventArgs = eventArgs;
        }
    }
}
