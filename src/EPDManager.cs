using Rhino.Commands;
using Rhino.DocObjects;

namespace WoodchuckCarbonTool.src
{
    /// <summary>
    /// This class is used to assign EPDs to Rhino objects and retreive those EPDs for
    /// calculations
    /// </summary>
    internal class EPDManager
    {
        public static Result Assign(ObjRef objRef, EPD epd)
        {
            if (objRef == null) { return Result.Failure; }
            RhinoObject obj = objRef.Object();
            if (epd == null) { return Result.Failure; }

            bool reassign = false;
            EPDData epdData = obj.Geometry.UserData.Find(typeof(EPDData)) as EPDData;
            if (epdData == null) { epdData = new EPDData(epd); reassign = true; }
            else { epdData.epd = epd; }

            if (reassign) obj.Geometry.UserData.Add(epdData);

            obj.CommitChanges();

            return Result.Success;
        }

        /// <summary>
        /// Assigns an EPD to an array of Object References
        /// </summary>
        public static Result Assign(ObjRef[] objRefs, EPD epd)
        {
            Result finalRslt = Result.Success;


            foreach (ObjRef objRef in objRefs)
            {
                if (objRef == null) continue;

                Result rslt = Assign(objRef, epd);
                if (rslt != Result.Success) finalRslt = rslt;
            }

            return finalRslt;
        }

        /// <summary>
        /// Retreives the EPD assigned to a Rhino object.
        /// </summary>
        public static EPD Get(ObjRef objRef)
        {
            if (objRef == null) return null;

            RhinoObject obj = objRef.Object();

            EPDData epdData = obj.Geometry.UserData.Find(typeof(EPDData)) as EPDData;
            if (epdData == null) return null;

            EPD epd = epdData.epd;

            return epd;
        }
    }
}