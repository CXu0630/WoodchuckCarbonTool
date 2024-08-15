using Rhino.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace WoodchuckCarbonTool.src
{
    public class DocumentEPDData : Dictionary<string, EPDData>
    {
        private const int MAJOR = 1;
        private const int MINOR = 0;

        public DocumentEPDData() { }

        public bool WriteDocument(BinaryArchiveWriter archive)
        {
            bool rc = false;

            archive.WriteInt(Keys.Count);
            foreach(var epdData in this.Values)
            {
                epdData.Write(archive);
            }

            rc = archive.WriteErrorOccured;
            return !rc;
        }

        public bool ReadDocument(BinaryArchiveReader archive)
        {
            bool rc = false;

            try
            {
                var count = archive.ReadInt();
                for (var i = 0; i < count; i++)
                {
                    var data = new EPDData();
                    if (data.Read(archive)) { Add(data.epd.id, data); }

                }
                rc = archive.ReadErrorOccured;

                return !rc;
            } catch (Exception)
            {
                return false;
            }
        }
    }
}
