using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.UI;
using Rhino.DocObjects;

namespace WoodchuckCarbonTool.src.UI
{
    internal class WCKObjectProperties: ObjectPropertiesPage
    {
        public override string EnglishPageTitle => "Embodied Carbon Material";

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            return e.ObjectCount > 0;
        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {
            foreach (RhinoObject obj in e.Objects)
            {
                if (obj.HasUserData)
                {

                }
            }
        }
    }
}
