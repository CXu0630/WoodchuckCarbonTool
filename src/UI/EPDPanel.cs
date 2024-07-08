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

namespace WoodchuckCarbonTool.src.UI
{
    internal class EPDPanel : Panel
    {
        public delegate void AssignEventHandler(object sender, AssignEventArgs e);
        public event AssignEventHandler AssignEvent;
        private MaterialQuantityOptionsForm qForm { get; set; }

        public EPDPanel(RhinoDoc doc, EPD epd, int width, Form parent = null)
        {
            this.Width = width;
            this.BackgroundColor = Colors.WhiteSmoke;

            Label gwp = new Label
            {
                Text = epd.GetGwpConverted(UnitManager.GetSystemUnit(doc, epd.dimension))
                + "CO2e/" + UnitManager.GetSystemUnitStr(doc, epd.dimension),
                Font = new Eto.Drawing.Font(SystemFonts.Default().FamilyName, 12)
            };
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
                Font = new Eto.Drawing.Font(SystemFonts.Default().FamilyName, 8),
                TextColor = Colors.DarkSlateGray
            };

            Button browserView = new Button { Text = "View in Browser" };
            Button assignButton = new Button { Text = "Assign to Object" };

            // Makes a system call to open the EPD's page on EC3 in the default browser
            // window. This is nice because it does not interfere with Rhino and also
            // opens a tab on an existing window if there is one already.
            browserView.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start("https://buildingtransparency.org/ec3/epds/" + epd.id);
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
                    WCKSelector geoSelector = new WCKSelector(e2.epd.dimension);
                    ObjRef[] objRefs = geoSelector.GetSelection();

                    Result rslt = EPDManager.Assign(objRefs, e2.epd);
                    if (rslt != Result.Success)
                    {
                        RhinoApp.WriteLine("Assignment canceled, No objects selected");
                    }
                    if (parent != null) { parent.WindowState = WindowState.Normal; }
                };
            };

            // General container
            DynamicLayout epdLayout = new DynamicLayout();

            // Container for display information
            DynamicLayout infoLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10),
                Size = new Size(-1, -1)
            };
            infoLayout.BeginHorizontal();
            infoLayout.Add(epdName);
            infoLayout.Add(gwp);
            infoLayout.EndBeginHorizontal();
            if (epd.manufacturer != null && epd.manufacturer != "")
                infoLayout.Add(manufacturer);
            infoLayout.EndBeginHorizontal();
            if (epd.description != null && epd.description != "")
                infoLayout.Add(description);
            infoLayout.EndBeginHorizontal();
            // These spacer lines are to make sure that the EPD name does not extend off
            // the screen... These are the times when I hate frontend.
            infoLayout.Add(null);
            infoLayout.Add(new Panel());

            // Maybe this layout should be put in a separate method to encapsulate it?
            DynamicLayout buttonLayout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };
            buttonLayout.BeginHorizontal();
            buttonLayout.Add(null);
            if (epd.id != null)
            {
                buttonLayout.Add(browserView);
            }
            buttonLayout.Add(assignButton);

            epdLayout.Add(infoLayout);
            epdLayout.Add(null);
            epdLayout.Add(buttonLayout);

            this.Content = epdLayout;
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
