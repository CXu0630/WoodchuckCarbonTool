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

        public MaterialQuantityOptionsForm() 
        {
            WindowStyle = WindowStyle.Default;
            Maximizable = false;
            Minimizable = false;
            Padding = new Padding(5);
            Resizable = false;
            ShowInTaskbar = true;
            Title = "Material Quantity Options";
            MinimumSize = new Size(300, 200);

            DynamicLayout layout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };

            Label lbl = new Label
            {
                Text = "Percentage of solid material\n" +
                "in modeled object: "
            };

            Slider sldr = new Slider
            {
                MinValue = 0,
                MaxValue = 100,
                Orientation = Orientation.Horizontal,
                Value = 100,
                Width = lbl.Width
            };

            Button confirm = new Button
            {
                Text = "Confirm"
            };
            confirm.Click += (sender, e) =>
            {
                PercentageEvent.Invoke(confirm, new PercentageEventArgs(sldr.Value));
            };

            layout.Add(new Panel());
            layout.Add(lbl);
            layout.Add(sldr);
            layout.Add(confirm);

            this.Content = layout;
        }

        internal class PercentageEventArgs : EventArgs
        {
            public double percentage;
            public PercentageEventArgs(double percentage)
            {
                this.percentage = percentage;
            }
        }
    }
}
