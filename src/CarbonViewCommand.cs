using System;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using System.Drawing;
using System.Collections.Generic;
using Rhino.Input;
using System.Linq;
using WoodchuckCarbonTool.src.UI;
using Rhino.Input.Custom;
using System.Runtime.Remoting;

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

        private static Color[] colors = {
                Color.LightBlue,
                Color.Yellow,
                Color.Orange,
                Color.DarkRed
            };

        double[] values = new double[colors.Length + 1];

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Reset();
            SaveCurrentColors(doc);
            CarbonViewLegend legend;

            RhinoApp.WriteLine("NOTE: only objects with volume will be colored in this view.");

            try
            {
                PopulateMinMaxGwp(doc);

                var minMaxOptions = new MinMaxGwpOption(((int)this.MinGwp), ((int)this.MaxGwp));
                Result res = minMaxOptions.GetMinMax(out int intMin, out int intMax);

                if (res == Result.Cancel) { return Result.Cancel; }

                this.MinGwp = intMin;
                this.MaxGwp = intMax;

                AssignCarbonColors(doc);
                for(int i = 0; i < colors.Length + 1; i++)
                {
                    values[i] = Math.Round(MinGwp + (MaxGwp - MinGwp)/colors.Length * i, 4);
                }
                legend = new CarbonViewLegend(colors, values);
                legend.Enabled = true;
                doc.Views.Redraw();
                string out_str = null;
                RhinoGet.GetString("Press <Enter> to continue", true, ref out_str);
            }
            catch (Exception)
            {
                RhinoApp.WriteLine("Could not switch to carbon view, obejcts may not be assigned" +
                    "carbon materials yet.");
                RevertOriginalColors(doc);
                return Result.Failure;
            }

            legend.Enabled = false;
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
            if (!MinMaxCalculated) PopulateMinMaxGwp(doc);

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
            EPD epd = EpdManager.Get(new Rhino.DocObjects.ObjRef(obj));
            if (epd == null) { return Color.Gray; }

            double gwp = epd.GetGwpPerSystemUnit(doc).Value;

            Rhino.DocObjects.ObjRef objRef = new Rhino.DocObjects.ObjRef(obj);
            double volume = GeometryProcessor.GetDimensionalInfo(objRef, 3);
            if (volume == 0 || volume == -1) { return Color.DarkGray; }

            if (epd.dimension == 2 || epd.dimension == 1)
            {
                double objGwp = GwpCalculator.GetTotalGwp(doc, 
                    new Rhino.DocObjects.ObjRef[] { new Rhino.DocObjects.ObjRef(obj) });
                gwp = objGwp / volume;
            }
            
            if (MaxGwp == MinGwp) { return Color.Yellow; }

            if (gwp <= MinGwp) { return colors[0]; }
            if (gwp >= MaxGwp) { return colors[colors.Length - 1]; }

            // Normalize the GWP value
            double normalizedGwp = (gwp - MinGwp) / (MaxGwp - MinGwp);

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
                EPD epd = EpdManager.Get(new Rhino.DocObjects.ObjRef(obj));
                if (epd == null) { continue; }

                double currentGwp = epd.GetGwpPerSystemUnit(doc).Value;

                gwps.Add(currentGwp);
            }

            if (gwps.Count > 0)
            {
                MaxGwp = gwps.Max();
                MinGwp = gwps.Min();
            } else
            {
                MaxGwp = 0;
                MinGwp = 0;
            }

            MinMaxCalculated = true;
        }
    }

    internal class MinMaxGwpOption
    {
        int max;
        int min;

        OptionInteger optMin;
        OptionInteger optMax;

        public MinMaxGwpOption(int defaultMin, int defaultMax)
        {
            min = defaultMin;
            max = defaultMax;
        }

        public Result GetMinMax(out int min, out int max)
        {
            GetOption getMinMax = new GetOption();
            getMinMax.SetCommandPrompt("Set a custom minimum and maximum GWP for your view");

            optMin = new OptionInteger(this.min);
            optMax = new OptionInteger(this.max);

            getMinMax.AddOptionInteger("Legend_Min", ref optMin, "Set Legend Min");
            getMinMax.AddOptionInteger("Legend_Max", ref optMax, "Set Legend Max");

            for (; ; )
            {
                GetResult res = getMinMax.Get();
                if (res == GetResult.Option)
                {
                    this.min = optMin.CurrentValue;
                    this.max = optMax.CurrentValue;
                    continue;
                }
                //else if (res == GetResult.Cancel)
                //{
                //    min = this.min;
                //    max = this.max;
                //    return Result.Cancel;
                //}
                // This does not work as intended: the result is cancel regardless of if 
                // esc or enter was pressed.
                break;
            }

            min = this.min;
            max = this.max;
            return Result.Success;
        }
    }
}