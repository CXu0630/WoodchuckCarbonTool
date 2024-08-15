using Rhino.Commands;
using Rhino.DocObjects;
using System.Linq;

namespace WoodchuckCarbonTool.src
{
    /// <summary>
    /// This class is used to assign EPDs to Rhino objects and retreive those EPDs for
    /// calculations. It makes use of EPDData as the wrapper class for EPD objects
    /// so that EPDs can be read and written with the Rhino file.
    /// </summary>
    internal class EPDManager
    {
        public static Result Assign(ObjRef objRef, EPD epd)
        {
            if (objRef == null) { return Result.Failure; }
            RhinoObject obj = objRef.Object();
            if (epd == null) { return Result.Failure; }

            string id;
            bool exists = CheckEPDExists(epd, out id);
            if (!exists)
            {
                EPDData epdData = new EPDData(epd);
                id = epdData.epd.id;
                WoodchuckCarbonToolPlugin.Instance.DocumentEPDs.Add(id, epdData);
            }

            obj.Attributes.UserDictionary["WCK_EPD_ID"] = id;

            obj.CommitChanges();

            return Result.Success;
        }

        /// <summary>
        /// Assigns an EPD to an array of Object References
        /// </summary>
        public static Result Assign(ObjRef[] objRefs, EPD epd)
        {
            Result finalRslt = Result.Success;

            if (objRefs == null) { return Result.Cancel; }
            if (objRefs.Length == 0) { return Result.Cancel; }

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

            if (!obj.Attributes.UserDictionary.Keys.Contains("WCK_EPD_ID"))
            {
                return null;
            }

            string id = (string)obj.Attributes.UserDictionary["WCK_EPD_ID"];
            
            if (!WoodchuckCarbonToolPlugin.Instance.DocumentEPDs.Keys.Contains(id))
            {
                return null;
            }

            EPDData epdData = WoodchuckCarbonToolPlugin.Instance.DocumentEPDs[id];

            EPD epd = epdData.epd;

            return epd;
        }

        public static bool UpdatePercentSolid(ObjRef objRef, int newPercent) 
        {
            EPD epd = Get(objRef);
            if (epd == null) return false;

            epd.percentageSolid = newPercent;
            return true;
        }

        public static bool CheckEPDExists(EPD epd, out string id)
        {
            id = "";
            if (epd == null) { return true; }
            foreach(EPDData epdData in WoodchuckCarbonToolPlugin.Instance.DocumentEPDs.Values)
            {
                if (epdData.epd.Equals(epd))
                {
                    id = epdData.epd.id;
                    return true;
                }
            }
            return false;
        }
    }
}