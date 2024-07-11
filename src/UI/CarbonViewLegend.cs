using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.Display;
using Rhino.Geometry;

namespace WoodchuckCarbonTool.src.UI
{
    internal class CarbonViewLegend: DisplayConduit
    {
        int xGap = 60;
        int yGap = 80;
        Color[] Colors;
        double[] Values;

        public CarbonViewLegend(Color[] colors, double[] values)
        {
            this.Colors = colors;
            this.Values = values;
        }

        protected override void DrawForeground(DrawEventArgs e)
        {
            int left, right, top, bottom, near, far;
            int width;
            int height;
            if (e.Viewport.GetScreenPort(out left, out right, out bottom, out top, out near, out far))
            {
                width = right - left;
                height = bottom - top;
            }
            else
            {
                return;
            }

            int legendBoxWidth = 40;
            int spacing = 20;
            int legendHeight = Colors.Length * legendBoxWidth;

            if (height < legendHeight) return;

            string titleText = "Global Waming Potential Per Unit Material";

            var titleRect = e.Display.Measure2dText(titleText, new Point2d(0, 0), false, 0.0, 12, "Arial");
            var titleHeight = Math.Abs(titleRect.Height);
            var titleLoc = new Point2d(left + xGap, top + yGap);
            e.Display.Draw2dText(titleText, Color.Black, titleLoc, false, 12, "Arial");

            string unitText = "kgCO2e/" + UnitManager.GetSystemUnitStr(e.RhinoDoc, 3);

            var unitRect = e.Display.Measure2dText(unitText, new Point2d(0, 0), false, 0.0, 8, "Arial");
            var unitHeight = Math.Abs(unitRect.Height);
            var unitLoc = new Point2d(left + xGap, top + yGap + titleHeight + spacing / 2);
            e.Display.Draw2dText(unitText, Color.Black, unitLoc, false, 8, "Arial");

            for(int i = 0; i < Values.Length; i++)
            {

            }

            for (int i = 0; i < Colors.Length; i++)
            {
                System.Drawing.Point boxLoc = new System.Drawing.Point(left + legendBoxWidth + xGap, top + yGap + 
                    (Colors.Length - i - 1)*(legendBoxWidth * 2));
                Size boxSize = new Size(legendBoxWidth, legendBoxWidth * 2);
                Rectangle box = new Rectangle(boxLoc, boxSize);

                e.Display.Draw2dRectangle(box, Color.Transparent, 0, Colors[i]);
            }


        }
    }
}
