using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.UI;
using Rhino.DocObjects;
using System.Data;
using Eto.Forms;

namespace WoodchuckCarbonTool.src.UI
{
    internal class WCKObjectProperties: ObjectPropertiesPage
    {
        WCKObjectPropertiesControl control;

        public override string EnglishPageTitle => "Embodied Carbon Material";
        public override object PageControl => control ?? (control = new WCKObjectPropertiesControl());

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            return e.ObjectCount > 0;
        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {
            EPD uniqueEpd = null;
            bool hasUniqueEpd = true;
            int numAssignedObjs = 0;

            List<ObjRef> objRefs = new List<ObjRef>();

            foreach (RhinoObject obj in e.Objects)
            {
                objRefs.Add(new ObjRef(obj));

                EPD epd = null;
                epd = EPDManager.Get(new ObjRef(obj));
                if (epd == null) { continue; }

                numAssignedObjs++;
                if (uniqueEpd == null) { uniqueEpd = epd; }
                else if (!epd.Equals(uniqueEpd)) { hasUniqueEpd = false; break; }
            }

            double totalGwp = GWPCalculator.GetTotalGwp(e.Document, objRefs.ToArray());

            control.PopulateControl(numAssignedObjs, totalGwp, "EC3", uniqueEpd, 25);
        }
    }
}
