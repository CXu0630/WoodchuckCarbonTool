using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;
using System;
using System.ComponentModel;

namespace WoodchuckCarbonTool.src.UI
{
    /// <summary>
    /// ETO form to display EPD calculation results in. For now, it's just a very simple
    /// window to display one line of text.
    /// </summary>
    internal class ResultForm : Form
    {
        public ResultForm(string rsltStr)
        {
            WindowStyle = WindowStyle.Default;
            Maximizable = false;
            Minimizable = false;
            Padding = new Padding(5);
            Resizable = false;
            ShowInTaskbar = true;
            Title = "GWP Result";
            MinimumSize = new Size(200, 150);

            DynamicLayout layout = new DynamicLayout
            {
                DefaultSpacing = new Size(5, 5),
                Padding = new Padding(10)
            };

            Label lbl = new Label
            {
                Text = rsltStr
            };

            layout.BeginHorizontal();
            layout.Add(new Panel());
            layout.Add(lbl);
            layout.Add(new Panel());
            layout.EndHorizontal();

            Content = layout;
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
            this.RestorePosition();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.SavePosition();
            base.OnClosing(e);
        }
    }
}
