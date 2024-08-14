using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;
using static WoodchuckCarbonTool.src.UI.SearchForm;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace WoodchuckCarbonTool.src.UI
{
    internal class EPDPanel : Panel
    {
        public delegate void AssignEventHandler(object sender, AssignEventArgs e);
        public event AssignEventHandler AssignEvent;
        private MaterialQuantityOptionsForm qForm { get; set; }
        private ObjRef[] AssignTargets { get; set; }

        public EPDPanel(RhinoDoc doc, EPD epd, int width = -1, Form parent = null)
        {
            if(width > 0) this.Width = width;

            // this should only occur when trying to access a unique EPD from the
            // properties panel but retreives multiple different epds.
            if (epd == null)
            {
                this.Content = EPDVariesLayout();
                return;
            }

            string gwpText = epd.GetGwpConverted(UnitManager.GetSystemUnit(doc, epd.dimension))
                + "CO2e/" + UnitManager.GetSystemUnitStr(doc, epd.dimension);
            string[] gwpSplitText = gwpText.Split(' ');

            Label gwp = new Label
            {
                Text = gwpSplitText[0],
                Font = new Eto.Drawing.Font(SystemFonts.Default().FamilyName, 12)
            };
            Label unit = new Label
            {
                Text = gwpSplitText[1],
            };
            DynamicLayout gwpLayout = new DynamicLayout { Spacing = new Size(5, 5) };
            gwpLayout.Add(new Panel { Content = gwp });
            gwpLayout.Add(new Panel { Content = unit });
            gwpLayout.Add(null);


            Label epdName = new Label
            {
                Text = epd.name,
                Font = new Eto.Drawing.Font(SystemFonts.Default().FamilyName, 12)
            };
            if (epd.tooltip != null && epd.tooltip != "")
            {
                epdName.ToolTip = epd.tooltip;
            }
            Label manufacturer = new Label
            {
                Text = "Manufacturer: " + epd.manufacturer,
                Font = new Eto.Drawing.Font(SystemFonts.Default().FamilyName, 10),
                TextColor = Colors.DarkSlateGray
            };
            Label description = new Label
            {
                Text = epd.description,
                Font = new Eto.Drawing.Font(SystemFonts.Default().FamilyName, 8)
            };
            DynamicLayout descriptionLayout = new DynamicLayout
            {
                Spacing = new Size(5, 5),
                Width = this.Width - gwpLayout.Width - 10
            };
            descriptionLayout.Add(new Panel { Content = epdName });
            if (epd.manufacturer != null && epd.manufacturer != "")
                descriptionLayout.Add(new Panel { Content = manufacturer });
            if (epd.description != null && epd.description != "")
                descriptionLayout.Add(new Panel { Content = description });

            // Container for display information
            DynamicLayout infoLayout = new DynamicLayout
            {
                Padding = new Padding(10),
                Spacing = new Size (10, 5)
            };
            infoLayout.BeginHorizontal();
            infoLayout.Add(descriptionLayout);
            infoLayout.Add(gwpLayout);
            infoLayout.EndBeginHorizontal();
            infoLayout.Add(null);
            infoLayout.EndHorizontal();


            Button browserView = new Button { Text = "View in Browser" };
            Button assignButton = new Button { Text = "Assign to Object" };

            // Makes a system call to open the EPD's page on EC3 in the default browser
            // window. This is nice because it does not interfere with Rhino and also
            // opens a tab on an existing window if there is one already.
            browserView.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start("https://buildingtransparency.org/ec3/epds/" + epd.ec3id);
            };

            // The assign button is pressed. Non-UI events are not delt with in this class.
            assignButton.Click += (s, e) =>
            {
                AssignEvent?.Invoke(this, new AssignEventArgs(epd));

                if (parent != null) { parent.WindowState = WindowState.Minimized; }

                if (qForm == null)
                {
                    qForm = new MaterialQuantityOptionsForm(epd) { Owner = RhinoEtoApp.MainWindow };
                    qForm.Closed += OnQFormClosed;
                    qForm.Show();
                }

                qForm.PercentageEvent += (s2, e2) =>
                {
                    qForm.Close();

                    if (AssignTargets == null || AssignTargets.Length == 0)
                    {
                        WCKSelector geoSelector = new WCKSelector(e2.epd.dimension);
                        AssignTargets = geoSelector.GetSelection();
                    }
                    
                    Result rslt = EPDManager.Assign(AssignTargets, e2.epd);
                    if (rslt != Result.Success)
                    {
                        RhinoApp.WriteLine("Assignment canceled, No objects selected");
                    }
                    if (parent != null) { parent.WindowState = WindowState.Normal; }
                };
            };

            // General container
            DynamicLayout epdLayout = new DynamicLayout();

            // Maybe this layout should be put in a separate method to encapsulate it?
            DynamicLayout buttonLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };
            buttonLayout.BeginHorizontal();
            buttonLayout.Add(null);
            if (epd.ec3id != null)
            {
                buttonLayout.Add(browserView);
            }
            buttonLayout.Add(assignButton);

            epdLayout.Add(infoLayout);
            epdLayout.Add(null);
            epdLayout.Add(buttonLayout);

            this.Content = UICommonElements.TransparentCell(epdLayout, Colors.White, 0.4f);
        }

        private DynamicLayout EPDVariesLayout()
        {
            Label label = new Label { Text = "Varies" };
            DynamicLayout dl = new DynamicLayout();
            dl.Add(null);
            dl.AddRow(new Control[] { null, label, null });
            dl.Add(null);

            return dl;
        }

        private void OnQFormClosed(object sender, EventArgs e)
        {
            qForm.Dispose();
            qForm = null;
        }

        internal class AssignEventArgs : EventArgs
        {
            public EPD epd { get; set; }
            public AssignEventArgs(EPD epd)
            {
                this.epd = epd;
            }
        }
    }
}
