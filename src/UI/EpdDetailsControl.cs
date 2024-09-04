using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;
using Eto.Drawing;
using Eto.Forms;
using Eto.Forms.ThemedControls;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;

namespace WoodchuckCarbonTool.src.UI
{
    internal class EpdDetailsControl : Panel
    {
        EPD epd;
        RhinoDoc doc;

        TextBox nameTb;
        TextBox gwpTb;
        DropDown dimensionDd;

        public EpdDetailsControl(RhinoDoc doc, int width, EPD epd = null)
        {
            this.epd = epd;
            this.doc = doc;

            this.Width = width;

            InitializeControl();
            if (epd != null) { PopulateValues(); }
        }

        public void InitializeControl()
        {
            RhinoStyleTable infoTable = new RhinoStyleTable();
            if (this.Width < 0) { infoTable.Width = this.Width - 20; }
            else { infoTable.Width = this.Width; }
            infoTable.AddTitle("Basic Information");

            // --------------------------------------------------------------------
            Control nameLbl = new Label { Text = "Name" };
            nameTb = new TextBox { ShowBorder = false };

            // --------------------------------------------------------------------
            Control gwpLbl = UICommonElements.RequiredInfilLabel("GWP");
            // this is a text box for inputing gwp values followed by the unit text label
            Panel gwpUnitPanel = new Panel();
            Label gwpUnitLabel = new Label { Text = "KgCO2e / " + UnitManager.GetSystemUnitStr(doc, 3) };
            gwpUnitPanel.Content = gwpUnitLabel;
            gwpTb = new TextBox { ShowBorder = false, Width = (this.Width - 40) / 2 - 60 };

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

            // --------------------------------------------------------------------
            // Dropdown for dimension
            Control dimensionLbl = UICommonElements.RequiredInfilLabel("Dimension");
            // Setup dropdown values
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
            dimensionDd = new DropDown
            {
                ShowBorder = false,
                BackgroundColor = Colors.White
            };
            dimensionDd.DataStore = dimensionOptions;

            // Setup gwp units to correspond to changes in dimension
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

            // --------------------------------------------------------------------
            Control[] labelCtrl = new Control[] { nameLbl, gwpLbl, dimensionLbl };
            Control[] inputCtrl = new Control[] { nameTb, gwpLayout, dimensionDd };

            Control[][] infoSubtableCtrls = new Control[][] { labelCtrl, inputCtrl };
            infoTable.AddSubtable(infoSubtableCtrls, new double[] { 0.3, 0.7 });

            this.Content = infoTable;
        }

        public void PopulateValues()
        {
            nameTb.Text = epd.name;
            gwpTb.Text = epd.GetGwpPerSystemUnit(doc).Value.ToString();
            dimensionDd.SelectedKey = epd.dimension.ToString();
        }

        public EPD GetInfillEpd()
        {
            // necessary information not given, unable to complete
            if (nameTb.Text == null || gwpTb.Text == null)
            {
                return null;
            }

            double gwp = 0;
            double.TryParse(gwpTb.Text, out gwp);
            int dimension = 3;
            int.TryParse(dimensionDd.SelectedKey, out dimension);

            EPD newEpd = new EPD(nameTb.Text, gwp, UnitManager.GetSystemUnitStr(doc, dimension), 
                0, null, null, dimension, new MaterialFilter(), null);
            return newEpd;
        }
    }
}
