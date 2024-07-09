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
        private ObjectPropertiesPageEventArgs prevEventArgs;

        public override string EnglishPageTitle => "Embodied Carbon Material";
        public override object PageControl => control ?? (control = new WCKObjectPropertiesControl());

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            return e.ObjectCount > 0;
        }

        public override void OnSizeParent(int width, int height)
        {
            this.control.Size = new Eto.Drawing.Size(width, height);
            this.UpdatePage(prevEventArgs);
        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {
            prevEventArgs = e;

            EPD uniqueEpd = null;
            bool hasUniqueEpd = true;

            string uniqueSource = null;
            bool hasUniqueSource = true;

            int uniquePercentSolid = -1;
            bool hasUniquePercentSolid = true;

            int numAssignedObjs = 0;

            List<ObjRef> objRefs = new List<ObjRef>();
            List<EPD> epds = new List<EPD>();

            foreach (RhinoObject obj in e.Objects)
            {
                objRefs.Add(new ObjRef(obj));

                EPD epd = null;
                epd = EPDManager.Get(new ObjRef(obj));
                if (epd == null) { continue; }
                epds.Add(epd);

                numAssignedObjs++;

                if (uniqueSource == null) { uniqueSource = epd.mf.dataBase; }
                else if (!epd.mf.dataBase.Equals(uniqueSource)) { hasUniqueSource = false; }

                if (uniqueEpd == null) { uniqueEpd = epd; }
                else if (!epd.Equals(uniqueEpd)) { hasUniqueEpd = false; }

                if (uniquePercentSolid == -1) { uniquePercentSolid = epd.percentageSolid; }
                else if (!(epd.percentageSolid == uniquePercentSolid)) { hasUniquePercentSolid = false; }
            }

            if (!hasUniqueEpd) { uniqueEpd = null; }
            if (!hasUniqueSource) { uniqueSource = null; }
            if (!hasUniquePercentSolid) { uniquePercentSolid = -1; }

            double totalGwp = GWPCalculator.GetTotalGwp(e.Document, objRefs.ToArray());

            control.SetParent(this);
            control.SetEventArgs(e);
            control.PopulateControl(e.Document, numAssignedObjs, totalGwp, 
                uniqueSource, uniqueEpd, uniquePercentSolid, objRefs);
        }
    }
}
