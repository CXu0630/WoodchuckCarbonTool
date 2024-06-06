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

        public MaterialQuantityOptionsForm(EPD epd) 
        {
            WindowStyle = WindowStyle.Default;
            Maximizable = false;
            Minimizable = false;
            Padding = new Padding(5);
            Resizable = false;
            ShowInTaskbar = true;
            Title = "Material Quantity Options";
            MinimumSize = new Size(300, 200);

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
                    epd.percentageSolid = percentage;
                    PercentageEvent.Invoke(confirm, new PercentageEventArgs(epd));
                }
            };

            layout.Add(null);
            layout.BeginHorizontal();
            layout.Add(lbl);
            layout.Add(textBox);
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
                if (percentage > 0 && percentage <= 100) return percentage;
                RepopulateErrorPanel("Please enter a number between 1 to 100.");
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
            public EPD epd;
            public PercentageEventArgs(EPD epd)
            {
                this.epd = epd;
            }
        }
    }
}
