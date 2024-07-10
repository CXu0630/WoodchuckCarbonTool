using System;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using System.Drawing;
using System.Collections.Generic;

namespace WoodchuckCarbonTool.src
{
    public class CarbonViewCommand : Command
    {
        public CarbonViewCommand()
        {
            Instance = this;
        }

        ///<summary>The only instance of the MyCommand command.</summary>
        public static CarbonViewCommand Instance { get; private set; }

        public override string EnglishName => "WoodchuckCarbonView";

        private Color o_BkgColor;
        private Dictionary<Guid, Color> o_ObjColors;
        private Dictionary<Guid, ObjectColorSource> o_ObjColorSource;
        private double MaxGwp = -1;
        private double MinGwp = -1;
        // Due to possibility to include biogen carbon, and for both Min and Max to 
        // be any negative number (bummer), it seems necessary to include this 
        // indicator...
        private bool MinMaxCalculated = false;

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: complete command.
            return Result.Success;
        }

        protected void SaveCurrentColors(RhinoDoc doc)
        {
            o_BkgColor = Rhino.ApplicationSettings.AppearanceSettings.ViewportBackgroundColor;

            foreach (RhinoObject obj in doc.Objects)
            {
                o_ObjColorSource.Add(obj.Id, obj.Attributes.ColorSource);
                o_ObjColors.Add(obj.Id, obj.Attributes.ObjectColor);
            }
        }

        protected void AssignCarbonColors(RhinoDoc doc)
        {

        }

        protected Color GetCarbonColor(RhinoDoc doc, RhinoObject obj)
        {
            if (!MinMaxCalculated) PopulateMinMaxGwp(doc);

            // Define the colors for the highest and lowest GWP values
            Color lowColor = Color.LightBlue; // Corresponds to minimum GWP
            Color highColor = Color.DarkRed;  // Corresponds to maximum GWP

            EPD epd = EPDManager.Get(new ObjRef(obj));
            if (epd == null) { return Color.Gray; }
            double gwp = epd.GetGwpPerSystemUnit(doc).Value;
            
            // Normalize the GWP value
            double normalizedGwp = (gwp - MinGwp) / (MaxGwp - MinGwp);

            // Interpolate between the two colors
            int r = (int)(lowColor.R + normalizedGwp * (highColor.R - lowColor.R));
            int g = (int)(lowColor.G + normalizedGwp * (highColor.G - lowColor.G));
            int b = (int)(lowColor.B + normalizedGwp * (highColor.B - lowColor.B));

            return Color.FromArgb(r, g, b);
        }

        protected void PopulateMinMaxGwp(RhinoDoc doc)
        {
            foreach (RhinoObject obj in doc.Objects)
            {
                EPD epd = EPDManager.Get(new ObjRef(obj));
                if (epd == null) { continue; }

                double currentGwp = epd.GetGwpPerSystemUnit(doc).Value;

                if (currentGwp < MinGwp) MinGwp = currentGwp;
                if (currentGwp > MaxGwp) MaxGwp = currentGwp;
            }

            MinMaxCalculated = true;
        }
    }
}