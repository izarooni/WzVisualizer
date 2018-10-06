using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WzVisualizer
{
    class ItemConstants
    {

        public static bool IsConsume(int ID)
        {
            int section = ID / 10000;
            return (ID >= 200 && ID <= 245) || (section >= 200 && section <= 245);
        }

        public static bool IsChair(int ID)
        {
            return ID == 301 || ID / 10000 == 301;
        }

        public static bool IsCash(int ID)
        {
            int section = ID / 10000;
            return (ID >= 501 && ID <= 599) || (section >= 501 && section <= 599);
        }

        public static bool IsEtc(int ID)
        {
            int section = ID / 10000;
            return (ID >= 400 && ID <= 431) || (section >= 400 && section <= 431);
        }

        public static bool IsEquip(int ID)
        {
            int category = ID / 10000;
            return category == 3 || category == 2 || (category >= 100 && category <= 190);
        }
    }
}
