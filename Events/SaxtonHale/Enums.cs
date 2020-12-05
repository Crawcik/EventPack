using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaxtonHale
{
    public enum Class
    {
        SAXTON,
        RIPPER,
        DEMOMAN,
        FLASH,
        MINIMIKE
    }

    public enum Abbility
    {
        RAGE = Smod2.API.ItemType.KEYCARD_JANITOR,
        TAUNT = Smod2.API.ItemType.KEYCARD_NTF_COMMANDER,
        SPECIAL = Smod2.API.ItemType.KEYCARD_O5
    }
}
