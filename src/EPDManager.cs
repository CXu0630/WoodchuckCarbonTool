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
    internal class EpdManager
    {
        public static Result Assign(ObjRef objRef, EPD epd, int pctg)
        {
            if (objRef == null) { return Result.Failure; }
            RhinoObject obj = objRef.Object();
            if (epd == null) { return Result.Failure; }

            string id;
            bool exists = CheckEPDExists(epd, out id);
            if (!exists)
            {
                DocumentEpdData epdData = new DocumentEpdData(epd);
                id = epdData.epd.id;
                WoodchuckCarbonToolPlugin.Instance.DocumentEPDs.Add(id, epdData);
            }

            obj.Attributes.UserDictionary["WCK_EPD_ID"] = id;

            if (pctg <= 0) { pctg = 100; }
            obj.Attributes.UserDictionary["WCK_PCTG"] = pctg.ToString();

            obj.CommitChanges();

            return Result.Success;
        }

        /// <summary>
        /// Assigns an EPD to an array of Object References
        /// </summary>
        public static Result Assign(ObjRef[] objRefs, EPD epd, int pctg)
        {
            Result finalRslt = Result.Success;

            if (objRefs == null) { return Result.Cancel; }
            if (objRefs.Length == 0) { return Result.Cancel; }

            foreach (ObjRef objRef in objRefs)
            {
                if (objRef == null) continue;

                Result rslt = Assign(objRef, epd, pctg);
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

            DocumentEpdData epdData = WoodchuckCarbonToolPlugin.Instance.DocumentEPDs[id];

            EPD epd = epdData.epd;

            return epd;
        }

        public static bool UpdatePercentSolid(ObjRef objRef, int newPercent) 
        {
            RhinoObject obj = objRef.Object();
            obj.Attributes.UserDictionary["WCK_PCTG"] = newPercent.ToString();

            return true;
        }

        public static int GetPercentSolid(ObjRef objRef)
        {
            if (objRef == null) { return -1; }
            RhinoObject obj = objRef.Object();

            if (obj.Attributes.UserDictionary.Keys.Contains("WCK_PCTG"))
            {
                int pctg = -1;
                int.TryParse((string)obj.Attributes.UserDictionary["WCK_PCTG"], out pctg);
                return pctg;
            }

            return -1;
        }

        public static bool CheckEPDExists(EPD epd, out string id)
        {
            id = "";
            if (epd == null) { return true; }
            foreach(DocumentEpdData epdData in WoodchuckCarbonToolPlugin.Instance.DocumentEPDs.Values)
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