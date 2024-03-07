using Rhino.DocObjects.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC3CarbonCalculator
{
    internal class UserEPD : UserData
    {
        public EPD epd {  get; set; }
        public UserEPD( EPD epd ) 
        { 
            this.epd = epd;
        }
    }
}
