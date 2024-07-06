using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using System.Runtime.CompilerServices;
using Rhino.Geometry;

namespace WoodchuckCarbonTool.src.UI
{
    internal class WCKObjectPropertiesControl : Panel
    {
        public WCKObjectPropertiesControl()
        {
            
        }

        public void PopulateControl (int numAssignedObjs, double totalGwp, 
            string database, EPD uniqueEpd, int percentageSolid, EPD epds)
        {
            Control propertiesTxt = new Label 
            { 
                Text = "Carbon Properties", 
                Font = new Font(SystemFonts.Default().FamilyName, 10)
            };

            Control numAssignedTxt = new Label { Text = "Selected Objects with EPDs" };
            Control numAssignedRsp = new Label { Text = numAssignedObjs.ToString() };

            Control totalGwpTxt = new Label { Text = "Total GWP" };
            Control totalGwpRsp = new Label { Text = totalGwp.ToString() };

            Control databaseTxt = new Label { Text = "Source Database" };
            Control databaseRsp = new Label { Text = database };

            Control[] texts = new Control[] {numAssignedTxt, totalGwpTxt, databaseTxt};
            Control[] rsps = new Control[] {numAssignedRsp, totalGwpRsp, databaseRsp};

            DynamicLayout propertiesDl = TwoColumnLeftSpacedTable(texts, rsps);

            DynamicLayout mainDl = new DynamicLayout
            {
                Spacing = new Size(3, 4)
            };

            mainDl.Add(propertiesTxt);
            mainDl.Add(SeparationLine(Colors.Gray));
            mainDl.Add(propertiesDl);
            mainDl.Add(null);

            this.Content = mainDl;
        }

        private DynamicLayout TwoColumnLeftSpacedTable(Control[] ctrls1, Control[] ctrls2)
        {
            DynamicLayout twoColumnTable = TwoColumnTable(ctrls1, ctrls2);
            Panel leftSpacer = new Panel { Width = 15 };
            DynamicLayout leftSpacedTable = new DynamicLayout();

            leftSpacedTable.AddRow(new Control[] {leftSpacer, twoColumnTable});

            return leftSpacedTable;
        }

        private DynamicLayout TwoColumnTable(Control[] ctrls1, Control[] ctrls2)
        {
            DynamicLayout dl = new DynamicLayout 
            { 
                Spacing = new Size (3, 4) 
            };

            if (ctrls1.Length != ctrls2.Length) { return null; }

            for (int i = 0; i < ctrls1.Length; i++)
            {
                DynamicLayout subDl = TwoColumnRow(ctrls1[i], ctrls2[i]);
                dl.Add(subDl);
                if (i < ctrls1.Length - 1)
                {
                    dl.Add(SeparationLine(Colors.Gray));
                }
            }

            return dl;
        }

        private DynamicLayout TwoColumnRow(Control ctrl1, Control ctrl2)
        {
            DynamicLayout dl = new DynamicLayout { DefaultSpacing = new Size(10, 2) };
            dl.AddRow(new Control[] { FixedWidthCell(ctrl1), FixedWidthCell(ctrl2) });

            return dl;
        }

        private Control SeparationLine(Color clr)
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
                var semiTransparentColor = new Color(clr, 0.8f); // Semi-transparent color
                g.FillRectangle(semiTransparentColor, rect);
                //g.DrawRectangle(Colors.Gray, rect); // Draw the border
            };

            return drawable;
        }

        private Control FixedWidthCell(Control control)
        {
            if (control == null) { return null; }
            Panel panel = new Panel { Width = (this.Width - 20)/2 };
            panel.Content = control;
            return panel;
        }

        private Control TransparentCell(Control control, Color clr, int width = -1)
        {
            var drawable = new Drawable
            {
                Padding = new Padding(3),
                BackgroundColor = Colors.Transparent
            };

            drawable.Paint += (sender, e) =>
            {
                var g = e.Graphics;
                var rect = new Rectangle(drawable.Size);
                var semiTransparentColor = new Color(clr, 0.2f); // Semi-transparent color
                g.FillRectangle(semiTransparentColor, rect);
                //g.DrawRectangle(Colors.Gray, rect); // Draw the border
            };

            drawable.Content = control;
            return drawable;
        }
    }
}
