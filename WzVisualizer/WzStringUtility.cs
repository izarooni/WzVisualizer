using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WzVisualizer
{
    class WzStringUtility
    {

        private readonly WzImage EqpImage;
        private readonly WzImage EtcImage;
        private readonly WzImage CashImage;
        private readonly WzImage InsImage;
        private readonly WzImage ConsumeImage;

        public WzStringUtility(WzFile StringWZ)
        {
            EqpImage = StringWZ.WzDirectory.GetImageByName("Eqp.img");
            EtcImage = StringWZ.WzDirectory.GetImageByName("Etc.img");
            CashImage = StringWZ.WzDirectory.GetImageByName("Cash.img");
            InsImage = StringWZ.WzDirectory.GetImageByName("Ins.img");
            ConsumeImage = StringWZ.WzDirectory.GetImageByName("Consume.img");
        }

        private static string LeftPadding(char pad, string input, int count)
        {
            if (input.Length < count)
            {
                string padded = "";
                for (int i = 0; i < count - input.Length; i++)
                    padded += pad;
                padded += input;
                return padded;
            }
            return input;
        }

        private static string GetEqpCategory(int ID)
        {
            switch (ID / 10000)
            {
                default: return null;
                case 2: return "Face";
                case 3: return "Hair";
                case 100: return "Cap";
                case 101: return "Accessory";
                case 104: return "Coat";
                case 105: return "Longcoat";
                case 106: return "Pants";
                case 107: return "Shoes";
                case 108: return "Glove";
                case 109: return "Shield";
                case 110: return "Cape";
                case 111: return "Ring";
                case 130: return "Weapon";
                case 180: return "PetEquip";
                case 190: return "Taming";
            }
        }

        private void SetParsed(WzImage image)
        {
            if (image.Parsed) return;
            image.ParseImage();
        }

        public string GetEqp(int ID)
        {
            SetParsed(EqpImage);
            string category = GetEqpCategory(ID);
            WzImageProperty imgProperty = EqpImage.GetFromPath(string.Format("Eqp/{0}/{1}/name", category, ID));
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetEtc(int ID)
        {
            SetParsed(EtcImage);
            WzImageProperty imgProperty = EtcImage.GetFromPath(string.Format("Etc/{0}/name", ID));
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetCash(int ID)
        {
            SetParsed(CashImage);
            WzImageProperty imgProperty = CashImage.GetFromPath(string.Format("{0}/name", ID));
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetChair(int ID)
        {
            SetParsed(InsImage);
            WzImageProperty imgProperty = InsImage.GetFromPath(string.Format("{0}/name", ID));
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetConsume(int ID)
        {
            SetParsed(ConsumeImage);
            WzImageProperty imgProperty = ConsumeImage.GetFromPath(string.Format("{0}/name", ID));
            return ((WzStringProperty)imgProperty)?.Value;
        }
    }
}
