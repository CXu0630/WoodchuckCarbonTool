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
    /// <summary>
    /// This class is used to assign EPDs to Rhino objects and retreive those EPDs for
    /// calculations
    /// </summary>
    internal class EPDManager
    {
        /// <summary>
        /// Assigns an EPD to an array of Object References
        /// </summary>
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
        
        /// <summary>
        /// Requests the user select Rhino geometry and assigns an EPD to each of these 
        /// objects.
        /// </summary>
        public static Result SelectAssign(EPD epd)
        {
            EC3Selector geoSelector = new EC3Selector(epd.dimension);
            ObjRef[] objRefs = geoSelector.GetSelection();

            return Assign(objRefs, epd);
        }

        /// <summary>
        /// Retreives the EPD assigned to a Rhino object.
        /// </summary>
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