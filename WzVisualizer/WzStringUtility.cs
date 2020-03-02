using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WzVisualizer {
    internal class WzStringUtility {

        private readonly WzImage EqpImage;
        private readonly WzImage EtcImage;
        private readonly WzImage CashImage;
        private readonly WzImage InsImage;
        private readonly WzImage ConsumeImage;
        private readonly WzImage MapImage;
        private readonly WzImage MobImage;
        private readonly WzImage SkillImage;
        private readonly WzImage NPCImage;
        private readonly WzImage PetImage;

        public WzStringUtility(WzFile StringWZ) {
            EqpImage = StringWZ.WzDirectory.GetImageByName("Eqp.img");
            EtcImage = StringWZ.WzDirectory.GetImageByName("Etc.img");
            CashImage = StringWZ.WzDirectory.GetImageByName("Cash.img");
            InsImage = StringWZ.WzDirectory.GetImageByName("Ins.img");
            ConsumeImage = StringWZ.WzDirectory.GetImageByName("Consume.img");
            MapImage = StringWZ.WzDirectory.GetImageByName("Map.img");
            MobImage = StringWZ.WzDirectory.GetImageByName("Mob.img");
            SkillImage = StringWZ.WzDirectory.GetImageByName("Skill.img");
            NPCImage = StringWZ.WzDirectory.GetImageByName("Npc.img");
            PetImage = StringWZ.WzDirectory.GetImageByName("Pet.img");
        }

        private static string LeftPadding(char pad, string input, int count) {
            if (input.Length < count) {
                string padded = "";
                for (int i = 0; i < count - input.Length; i++)
                    padded += pad;
                padded += input;
                return padded;
            }
            return input;
        }

        private static string GetEqpCategory(int ID) {
            switch (ID / 10000) {
                default: return null;
                case 2: return "Face";
                case 3: return "Hair";
                case 100: return "Cap";
                case 101:
                case 102:
                case 103:
                case 112:
                case 113:
                case 114: return "Accessory";
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

        public string GetFieldFullName(int map_id) {
            string path;
            int section = map_id / 10000000;

            if (section <= 9) path = "maple";
            else if (section >= 10 && section <= 19) path = "victoria";
            else if (section >= 20 && section <= 28) path = "ossyria";
            else if (section >= 50 && section <= 55) path = "singapore";
            else if (section >= 60 && section <= 61) path = "MasteriaGL";
            else if (section >= 67 && section <= 68) path = "weddingGL";
            else path = "etc";
            path += "/" + map_id;
            WzSubProperty subProperty = (WzSubProperty)MapImage.GetFromPath(path);
            Console.WriteLine((subProperty != null) + " / " + path);
            if (subProperty != null) {
                string retName = "";
                WzStringProperty mapName = (WzStringProperty)subProperty.GetFromPath("mapName");
                WzStringProperty streetName = (WzStringProperty)subProperty.GetFromPath("streetName");
                if (mapName != null) retName += mapName.Value;
                if (mapName != null && streetName != null) retName += " - "; // x fucking d
                if (streetName != null) retName += streetName.Value;
                return retName;
            }
            return "NO-NAME";
        }

        private void SetParsed(WzImage image) {
            if (image.Parsed) return;
            image.ParseImage();
        }

        public string GetNPC(int ID) {
            SetParsed(NPCImage);
            WzImageProperty imgProperty = (WzStringProperty)NPCImage.GetFromPath($"{ID}/name");
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetSkill(string ID) {
            SetParsed(SkillImage);
            WzImageProperty imgProperty = (WzStringProperty)SkillImage.GetFromPath($"{ID}/name");
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetMob(int ID) {
            SetParsed(MobImage);
            WzImageProperty imgProperty = (WzStringProperty)MobImage.GetFromPath($"{ID}/name");
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetEqp(int ID) {
            SetParsed(EqpImage);
            string category = GetEqpCategory(ID);
            WzImageProperty imgProperty = EqpImage.GetFromPath($"Eqp/{category}/{ID}/name");
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetEtc(int ID) {
            SetParsed(EtcImage);
            WzImageProperty imgProperty = EtcImage.GetFromPath($"Etc/{ID}/name");
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetCash(int ID) {
            SetParsed(CashImage);
            WzImageProperty imgProperty = CashImage.GetFromPath($"{ID}/name");
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetChair(int ID) {
            SetParsed(InsImage);
            WzImageProperty imgProperty = InsImage.GetFromPath($"{ID}/name");
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetConsume(int ID) {
            SetParsed(ConsumeImage);
            WzImageProperty imgProperty = ConsumeImage.GetFromPath($"{ID}/name");
            return ((WzStringProperty)imgProperty)?.Value;
        }

        public string GetPet(int ID) {
            SetParsed(PetImage);
            WzImageProperty imgProperty = PetImage.GetFromPath($"{ID}/name");
            return ((WzStringProperty)imgProperty)?.Value;
        }
    }
}
