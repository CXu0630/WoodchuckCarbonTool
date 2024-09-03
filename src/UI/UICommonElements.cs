using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodchuckCarbonTool.src.UI
{
    internal class UICommonElements
    {
        public static Control FixedWidthCell(Control control, int width)
        {
            if (control == null) { return null; }
            Panel panel = new Panel { Width = width };
            panel.Content = control;
            return panel;
        }

        public static Control TransparentCell(Control control, Color clr, float alpha)
        {
            var drawable = new Drawable
            {
                BackgroundColor = Colors.Transparent
            };

            drawable.Paint += (sender, e) =>
            {
                var g = e.Graphics;
                var rect = new Rectangle(drawable.Size);
                var semiTransparentColor = new Color(clr, alpha); // Semi-transparent color
                g.FillRectangle(semiTransparentColor, rect);
                //g.DrawRectangle(Colors.Gray, rect); // Draw the border
            };

            drawable.Content = control;
            return drawable;
        }

        public static Control SeparationLine(Color clr, float alpha)
        {
            var drawable = new Drawable
            {
                Height = 1,
                BackgroundColor = Colors.Transparent
            };

            drawable.Paint += (sender, e) =>
            {
                var g = e.Graphics;
                var rect = new Rectangle(drawable.Size);
                var semiTransparentColor = new Color(clr, alpha); // Semi-transparent color
                g.FillRectangle(semiTransparentColor, rect);
                //g.DrawRectangle(Colors.Gray, rect); // Draw the border
            };

            return drawable;
        }

        public static Control SpacerLine(Color clr, float alpha, int spacerHeight)
        {
            DynamicLayout spacer = new DynamicLayout();

            Panel blank1 = new Panel { Height = spacerHeight };
            Panel blank2 = new Panel { Height = spacerHeight };

            spacer.Add(blank1);
            spacer.Add(SeparationLine(clr, alpha));
            spacer.Add(blank2);

            return spacer;
        }

        public static Control RequiredInfilLabel(string text)
        {
            DynamicLayout dl = new DynamicLayout();
            Label lbl = new Label { Text = text };
            Label astrisk = new Label { Text = "*", TextColor = Colors.Red };
            dl.AddRow(new Control[] { lbl, astrisk });
            return dl;
        }
    }
}
