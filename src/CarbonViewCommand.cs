using System;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using System.Drawing;
using System.Collections.Generic;
using Rhino.Input;
using System.Linq;

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

        private Color o_BkgColor = new Color();
        private Dictionary<Guid, Color> o_ObjColors = new Dictionary<Guid, Color>();
        private Dictionary<Guid, ObjectColorSource> o_ObjColorSource = new Dictionary<Guid, ObjectColorSource>();
        private double MaxGwp = -1;
        private double MinGwp = -1;
        // Due to possibility to include biogen carbon, and for both Min and Max to 
        // be any negative number (bummer), it seems necessary to include this 
        // indicator...
        private bool MinMaxCalculated = false;

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Reset();
            SaveCurrentColors(doc);

            try
            {
                AssignCarbonColors(doc);
                string out_str = null;
                RhinoGet.GetString("Press <Enter> to continue", true, ref out_str);
            }
            catch (Exception)
            {
                RhinoApp.WriteLine("Could not switch to carbon view");
                RevertOriginalColors(doc);
                return Result.Failure;
            }

            RevertOriginalColors(doc);
            return Result.Success;
        }

        protected void Reset()
        {
            o_BkgColor = new Color();
            o_ObjColors = new Dictionary<Guid, Color>();
            o_ObjColorSource = new Dictionary<Guid, ObjectColorSource>();
            MaxGwp = -1;
            MinGwp = -1;
            MinMaxCalculated = false;
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
            Rhino.ApplicationSettings.AppearanceSettings.ViewportBackgroundColor = Color.White;

            foreach (RhinoObject obj in doc.Objects)
            {
                Color carbonColor = GetCarbonColor(doc, obj);
                obj.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                obj.Attributes.ObjectColor = carbonColor;
                obj.CommitChanges();
            }

            doc.Views.Redraw();
        }

        protected void RevertOriginalColors(RhinoDoc doc)
        {
            Rhino.ApplicationSettings.AppearanceSettings.ViewportBackgroundColor = o_BkgColor;

            foreach (RhinoObject obj in doc.Objects)
            {
                obj.Attributes.ColorSource = o_ObjColorSource[obj.Id];
                obj.Attributes.ObjectColor = o_ObjColors[obj.Id];
                obj.CommitChanges();
            }

            doc.Views.Redraw();
        }

        protected Color GetCarbonColor(RhinoDoc doc, RhinoObject obj)
        {
            if (!MinMaxCalculated) PopulateMinMaxGwp(doc);

            EPD epd = EPDManager.Get(new ObjRef(obj));
            if (epd == null) { return Color.Gray; }
            double gwp = epd.GetGwpPerSystemUnit(doc).Value;
            
            if (MaxGwp == MinGwp) { return Color.Yellow; }

            // Normalize the GWP value
            double normalizedGwp = (gwp - MinGwp) / (MaxGwp - MinGwp);

            Color[] colors = {
                Color.LightBlue,
                Color.Yellow,
                Color.Orange,
                Color.DarkRed
            };


            // Determine the segment and interpolate
            int segmentCount = colors.Length - 1;
            double segmentSize = 1.0 / segmentCount;
            int segment = (int)Math.Floor(normalizedGwp / segmentSize);
            double segmentFraction = (normalizedGwp - segment * segmentSize) / segmentSize;

            // Interpolate between the colors of the relevant segment
            Color startColor = colors[segment];
            Color endColor = startColor;
            if (segment != segmentCount) endColor = colors[segment + 1];

            int r = (int)(startColor.R + segmentFraction * (endColor.R - startColor.R));
            int g = (int)(startColor.G + segmentFraction * (endColor.G - startColor.G));
            int b = (int)(startColor.B + segmentFraction * (endColor.B - startColor.B));

            return Color.FromArgb(r, g, b);
        }

        protected void PopulateMinMaxGwp(RhinoDoc doc)
        {
            List<double> gwps = new List<double>();

            foreach (RhinoObject obj in doc.Objects)
            {
                EPD epd = EPDManager.Get(new ObjRef(obj));
                if (epd == null) { continue; }

                double currentGwp = epd.GetGwpPerSystemUnit(doc).Value;

                gwps.Add(currentGwp);
            }

            MaxGwp = gwps.Max();
            MinGwp = gwps.Min();

            MinMaxCalculated = true;
        }
    }
}