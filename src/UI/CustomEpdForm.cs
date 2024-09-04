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
using Rhino.Geometry;
using Rhino.UI;

namespace WoodchuckCarbonTool.src.UI
{
    internal class CustomEpdForm : Form
    {
        private MaterialQuantityOptionsForm qForm { get; set; }

        EpdDetailsControl infoPanel;
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
            infoPanel = new EpdDetailsControl(doc, this.Width);

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

            assignButton.Click += (s, e) =>
            {
                EPD epd = infoPanel.GetInfillEpd();
                if (epd == null)
                {
                    Label errorMsg = new Label { Text = "Please fill out mandatory sections.", TextColor = Colors.Red };
                    errorPanel.Content = errorMsg;
                    return;
                }

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
        }

        private void OnQFormClosed(object sender, EventArgs e)
        {
            qForm.Dispose();
            qForm = null;
        }
    }
}
