using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoodchuckCarbonTool.src
{
    // TODO: this class can be expanded in future to make object EPD data more robust
    internal class ObjectEpdData : UserData
    {
        public ObjectEpdData() { }

        protected override bool Read(BinaryArchiveReader archive)
        {
            return base.Read(archive);
        }

        protected override bool Write(BinaryArchiveWriter archive)
        {
            return base.Write(archive);
        }
    }
}
