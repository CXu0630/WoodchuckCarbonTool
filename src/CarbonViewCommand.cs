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
                
            }
        }
    }
}