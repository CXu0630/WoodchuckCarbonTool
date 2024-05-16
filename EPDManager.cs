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
                obj.UserData.Add(new EPDData(epd));
            }

            return Result.Success;
        }
        
        public static Result SelectAssign(EPD epd)
        {
            EC3Selector geoSelector = new EC3Selector(epd.dimension);
            ObjRef[] objRefs = geoSelector.GetSelection();

            return Assign(objRefs, epd);
        }

        internal class EPDData: UserData
        {
            public EPD epd;
            public EPDData(EPD epd) 
            {
                this.epd = epd;
            }
        }
    }
}