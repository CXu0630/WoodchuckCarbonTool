using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using static WoodchuckCarbonTool.src.UI.SearchForm;

namespace WoodchuckCarbonTool.src.UI
{
    internal class MaterialQuantityOptionsForm : Form
    {
        public delegate void PercentageEventHandler(object sender, PercentageEventArgs e);
        public event PercentageEventHandler PercentageEvent;

        public Panel errorPanel;

        public MaterialQuantityOptionsForm() 
        {
            WindowStyle = WindowStyle.Default;
            Maximizable = false;
            Minimizable = false;
            Padding = new Padding(5);
            Resizable = false;
            ShowInTaskbar = true;
            Title = "Material Quantity Options";
            MinimumSize = new Size(300, 250);

            errorPanel = new Panel();

            DynamicLayout layout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };

            Label lbl = new Label
            {
                Text = "Percentage of solid material in modeled object: ",
                Width = 100
            };

            TextBox textBox = new TextBox
            {
                PlaceholderText = "100",
                Width = 60
            };

            Label noteLbl = new Label
            {
                Text = "The percentage you enter will be applied to objects selected" +
                "/nwhen calculating their carbon. You may enter a number larger" +
                "/nthan 100.",
                Width = 100,
                Font = new Eto.Drawing.Font(SystemFonts.Default().FamilyName, 6)
            };

            Button confirm = new Button
            {
                Text = "Confirm",
                Width = 60
            };
            confirm.Click += (sender, e) =>
            {
                int percentage = CheckPercentage(textBox.Text);

                if (percentage != -1)
                {
                    PercentageEvent.Invoke(confirm, new PercentageEventArgs(percentage));
                }
            };

            layout.Add(null);
            layout.BeginHorizontal();
            layout.Add(lbl);
            layout.Add(textBox);
            layout.EndBeginHorizontal();
            layout.Add(noteLbl);
            layout.Add(null);
            layout.EndBeginHorizontal();
            layout.Add(null);
            layout.Add(new Panel());
            layout.EndHorizontal();
            layout.Add(null);
            layout.BeginHorizontal();
            layout.Add(errorPanel);
            layout.Add(confirm);
            layout.EndHorizontal();

            this.Content = layout;
        }

        private int CheckPercentage(string text)
        {
            if ( text == "" )
            {
                return 100;
            }
            if (int.TryParse(text, out var percentage))
            {
                if (percentage > 0) return percentage;
                RepopulateErrorPanel("Please enter a positive number.");
                return -1;
            }
            RepopulateErrorPanel("Please enter a number.");
            return -1;
        }

        private void RepopulateErrorPanel (string errormsg)
        {
            DynamicLayout lyt = new DynamicLayout();

            Label error = new Label
            {
                Text = errormsg,
                TextColor = Colors.Red,
                Width = 90
            };

            lyt.Add(error);

            this.errorPanel.Content = lyt;
        }

        internal class PercentageEventArgs : EventArgs
        {
            public int pctgSolid;
            public PercentageEventArgs(int pctgSolid)
            {
                this.pctgSolid = pctgSolid;
            }
        }
    }
}
