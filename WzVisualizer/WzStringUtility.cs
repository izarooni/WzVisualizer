using System;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;

namespace WzVisualizer {
    public static class WzStringUtility {
        public static void Load(WzFile wz) {
            _eqp = wz.WzDirectory.GetImageByName("Eqp.img");
            _etc = wz.WzDirectory.GetImageByName("Etc.img");
            _cash = wz.WzDirectory.GetImageByName("Cash.img");
            _ins = wz.WzDirectory.GetImageByName("Ins.img");
            _consume = wz.WzDirectory.GetImageByName("Consume.img");
            _map = wz.WzDirectory.GetImageByName("Map.img");
            _mob = wz.WzDirectory.GetImageByName("Mob.img");
            _skill = wz.WzDirectory.GetImageByName("Skill.img");
            _npc = wz.WzDirectory.GetImageByName("Npc.img");
            _pet = wz.WzDirectory.GetImageByName("Pet.img");
        }

        private static WzImage _eqp;
        private static WzImage _etc;
        private static WzImage _cash;
        private static WzImage _ins;
        private static WzImage _consume;
        private static WzImage _map;
        private static WzImage _mob;
        private static WzImage _skill;
        private static WzImage _npc;
        private static WzImage _pet;

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

        private static string GetEqpCategory(int id) {
            switch (id / 10000) {
                default: return null;
                case 2:
                case 5: return "Face";
                case 3:
                case 4: return "Hair";
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

        public static string GetFieldFullName(int mapId) {
            string path;
            int section = mapId / 10000000;

            if (section <= 9) path = "maple";
            else if (section >= 10 && section <= 19) path = "victoria";
            else if (section >= 20 && section <= 28) path = "ossyria";
            else if (section >= 50 && section <= 55) path = "singapore";
            else if (section >= 60 && section <= 61) path = "MasteriaGL";
            else if (section >= 67 && section <= 68) path = "weddingGL";
            else path = "etc";
            path += "/" + mapId;
            WzSubProperty subProperty = (WzSubProperty) _map.GetFromPath(path);
            Console.WriteLine((subProperty != null) + " / " + path);
            if (subProperty != null) {
                string retName = "";
                WzStringProperty mapName = (WzStringProperty) subProperty.GetFromPath("mapName");
                WzStringProperty streetName = (WzStringProperty) subProperty.GetFromPath("streetName");
                if (mapName != null) retName += mapName.Value;
                if (mapName != null && streetName != null) retName += " - "; // x fucking d
                if (streetName != null) retName += streetName.Value;
                return retName;
            }

            return "NO-NAME";
        }

        private static void SetParsed(WzImage image) {
            if (image.Parsed) return;
            image.ParseImage();
        }

        private static string GetStringValue(WzImageProperty img) {
            return img != null && img is WzStringProperty str ? str.Value : null;
        }

        public static string GetNpc(int id) {
            SetParsed(_npc);
            return GetStringValue(_npc.GetFromPath($"{id}/name"));
        }

        public static string GetSkill(string id) {
            SetParsed(_skill);
            return GetStringValue(_skill.GetFromPath($"{id}/name"));
        }

        public static string GetMob(int id) {
            SetParsed(_mob);
            return GetStringValue(_mob.GetFromPath($"{id}/name"));
        }

        public static string GetEqp(int id) {
            SetParsed(_eqp);
            string category = GetEqpCategory(id);
            return GetStringValue(_eqp.GetFromPath($"Eqp/{category}/{id}/name"));
        }

        public static string GetEtc(int id) {
            SetParsed(_etc);
            return GetStringValue(_etc.GetFromPath($"Etc/{id}/name"));
        }

        public static string GetCash(int id) {
            SetParsed(_cash);
            return GetStringValue(_cash.GetFromPath($"{id}/name"));
        }

        public static string GetChair(int id) {
            SetParsed(_ins);
            return GetStringValue(_ins.GetFromPath($"{id}/name"));
        }

        public static string GetConsume(int id) {
            SetParsed(_consume);
            return GetStringValue(_consume.GetFromPath($"{id}/name"));
        }

        public static string GetPet(int id) {
            SetParsed(_pet);
            return GetStringValue(_pet.GetFromPath($"{id}/name"));
        }
    }
}