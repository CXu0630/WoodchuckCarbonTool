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

            foreach(var epdData in this.Values)
            {
                epdData.Write(archive);
            }
            return true;
        }

        public bool ReadDocument(BinaryArchiveReader archive)
        {
            bool rc = false;


        }
    }
}
