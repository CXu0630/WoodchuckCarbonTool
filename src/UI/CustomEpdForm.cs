using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;

namespace WoodchuckCarbonTool.src.UI
{
    internal class CustomEpdForm : Form
    {
        private MaterialQuantityOptionsForm qForm { get; set; }

        Panel infoPanel;
        Panel buttonPanel;
        Panel errorPanel;

        Button assignButton = new Button { Text = "Assign", Width = 100};
        private ObjRef[] AssignTargets { get; set; }

        RhinoDoc doc;

        public CustomEpdForm(RhinoDoc doc)
        {
            this.doc = doc;

            WindowStyle = WindowStyle.Default;
            Maximizable = true;
            Minimizable = true;
            Padding = new Padding(5);
            Resizable = false;
            ShowInTaskbar = true;
            Title = "Woodchuck Custom Material";
            MinimumSize = new Size(400, 500);
        }

        public void PopulateForm()
        {
            infoPanel = new Panel();
            infoPanel.Content = infoLayout();

            buttonPanel = new Panel();
            DynamicLayout buttonLayout = new DynamicLayout();

            errorPanel = new Panel { Width = 200 };

            buttonLayout.BeginHorizontal();
            buttonLayout.Add(null);
            buttonLayout.Add(errorPanel);
            buttonLayout.Add(assignButton);
            buttonLayout.EndHorizontal();

            buttonPanel.Content = buttonLayout;

            DynamicLayout formLayout = new DynamicLayout();
            formLayout.Add(infoPanel);
            formLayout.Add(null);
            formLayout.Add(buttonPanel);

            this.Content = formLayout;
        }

        private DynamicLayout infoLayout()
        {
            RhinoStyleTable infoTable = new RhinoStyleTable();
            if (this.Width < 0) { infoTable.Width = this.Width - 20; }
            else { infoTable.Width = this.Width;}
            infoTable.AddTitle("Basic Information");
            
            Control nameLbl = new Label { Text = "Name"};
            Control gwpLbl = UICommonElements.RequiredInfilLabel("GWP");
            Control dimensionLbl = UICommonElements.RequiredInfilLabel("Dimension");
            Control[] labelCtrl = new Control[] { nameLbl, gwpLbl, dimensionLbl };

            TextBox nameTb = new TextBox{ShowBorder = false};

            // this is a text box for inputing gwp values followed by the unit text label
            Panel gwpUnitPanel = new Panel();
            Label gwpUnitLabel = new Label { Text = "KgCO2e / " + UnitManager.GetSystemUnitStr(doc, 3) };
            gwpUnitPanel.Content = gwpUnitLabel;
            TextBox gwpTb = new TextBox{ShowBorder = false, Width = (this.Width - 40) / 2 - 60};

            DynamicLayout gwpLayout = new DynamicLayout { DefaultSpacing = new Size(5, 5) };
            gwpLayout.BeginHorizontal();
            gwpLayout.Add(gwpTb);
            gwpLayout.Add(gwpUnitPanel);
            gwpLayout.EndHorizontal();

            string previousText = gwpTb.Text;

            // ensure that the inputs are double values that can be used as GWP
            gwpTb.TextChanged += (sender, e) =>
            {
                double val;
                if (gwpTb.Text == "")
                {
                    previousText = gwpTb.Text;
                }
                else if (!double.TryParse(gwpTb.Text, out val))
                {
                    gwpTb.Text = previousText;
                }
                else
                {
                    previousText = gwpTb.Text;
                }
            };

            // Dropdown for dimension
            List<ListItem> dimensionOptions = new List<ListItem>
            {
                new ListItem
                {
                    Text = "Volume",
                    Key = "3"
                },
                new ListItem
                {
                    Text = "Area",
                    Key = "2"
                },
                new ListItem
                {
                    Text = "Length",
                    Key = "1"
                }
            };
            DropDown dimensionDd = new DropDown { 
                ShowBorder = false, 
                BackgroundColor = Colors.White};
            dimensionDd.DataStore = dimensionOptions;
            dimensionDd.SelectedIndex = 0;
            dimensionDd.SelectedValueChanged += (s, e) =>
            {
                int selectedDimension;
                int.TryParse(dimensionDd.SelectedKey, out selectedDimension);
                Label newGwpUnitLabel = new Label
                {
                    Text = "KgCO2e / " + UnitManager.GetSystemUnitStr(doc, selectedDimension)
                };
                gwpUnitPanel.Content = newGwpUnitLabel;
            };

            assignButton.Click += (s, e) =>
            {
                if (nameTb.Text == null || gwpTb.Text == null)
                {
                    Label errorMsg = new Label { Text = "Please fill out mandatory sections.", TextColor = Colors.Red };
                    errorPanel.Content = errorMsg;
                }
                double gwp = 0;
                double.TryParse(gwpTb.Text, out gwp);
                int dimension = 3;
                int.TryParse(dimensionDd.SelectedKey, out dimension);

                EPD epd = new EPD(nameTb.Text, gwp, UnitManager.GetSystemUnitStr(doc, dimension), 0, null, null, dimension, new MaterialFilter(), null);

                this.WindowState = WindowState.Minimized;

                if (qForm == null)
                {
                    qForm = new MaterialQuantityOptionsForm() { Owner = RhinoEtoApp.MainWindow };
                    qForm.Closed += OnQFormClosed;
                    qForm.Show();
                }

                qForm.PercentageEvent += (s2, e2) =>
                {
                    qForm.Close();

                    if (AssignTargets == null || AssignTargets.Length == 0)
                    {
                        WckSelector geoSelector = new WckSelector(epd.dimension);
                        AssignTargets = geoSelector.GetSelection();
                    }

                    Result rslt = EpdManager.Assign(AssignTargets, epd, e2.pctgSolid);
                    if (rslt != Result.Success)
                    {
                        RhinoApp.WriteLine("Assignment canceled, No objects selected");
                    }
                };

                this.Close();
            };

            Control[] inputCtrl = new Control[] { nameTb, gwpLayout, dimensionDd };

            Control[][] infoSubtableCtrls = new Control[][] { labelCtrl, inputCtrl };
            infoTable.AddSubtable(infoSubtableCtrls, new double[] {0.3, 0.7});

            return infoTable;
        }
        private void OnQFormClosed(object sender, EventArgs e)
        {
            qForm.Dispose();
            qForm = null;
        }
    }
}
