using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using UnitsNet;
using UnitsNet.Units;

namespace EC3CarbonCalculator
{
    internal class EPDManager
    {
        public static Result Assign(ObjRef[] objRefs, EPD epd)
        {
            foreach (ObjRef objRef in objRefs)
            {
                if(objRef == null) continue;

                RhinoObject obj = objRef.Object();
                EPDData epdData = new EPDData();
                epdData.epd = epd;
                obj.UserData.Add(epdData);
            }

            return Result.Success;
        }
        
        public static Result SelectAssign(EPD epd)
        {
            EC3Selector geoSelector = new EC3Selector(epd.dimension);
            ObjRef[] objRefs = geoSelector.GetSelection();

            return Assign(objRefs, epd);
        }

        public static EPD Get(ObjRef objRef)
        {
            if (objRef == null) return null;

            RhinoObject obj = objRef.Object();
            EPDData data = obj.UserData.Find(typeof(EPDData)) as EPDData;

            if (data != null) { return data.epd; }

            return null;
        }
    }
}