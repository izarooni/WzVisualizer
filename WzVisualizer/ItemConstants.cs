using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WzVisualizer
{
    class ItemConstants
    {

        public static bool IsEtc(int itemID)
        {
            int section = itemID / 10000;
            return section >= 400 && section <= 431;
        }

        public static bool IsEquip(int itemID)
        {
            int category = itemID / 10000;
            return category == 3 || category == 2 || (category >= 100 && category <= 190);
        }
    }
}
