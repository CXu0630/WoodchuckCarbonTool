using Rhino.DocObjects.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC3CarbonCalculator
{
    /// <summary>
    /// Wrapper class used to assign EPD data to Rhino objects.
    /// 
    /// TODO: implement serialization of this class, ensure the encoding of serialized
    /// information to and from EPD objects. Currently, EPD data is lost every time a file
    /// is opened and closed.
    /// </summary>
    public class EPDData : UserData
    {
        public EPD epd;
        public EPDData() { }
    }
}
