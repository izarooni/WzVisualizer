using System;
using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using WzVisualizer.Util;

namespace WzVisualizer {
    public static class StringWz {
        public static void Dispose() {
            // disposed when Wz.String.Dispose() is called,
            // when the WzFile is disposed, disposing each img unnecessary
            // we can just null to remove the reference
            _eqp = null;
            _etc = null;
            _cash = null;
            _ins = null;
            _consume = null;
            _map = null;
            _mob = null;
            _skill = null;
            _npc = null;
            _pet = null;
        }

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

        private static void SetParsed(WzImage image) {
            if (image.Parsed) return;
            image.ParseImage();
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
                case 104:                             return "Coat";
                case 105:                             return "Longcoat";
                case 106:                             return "Pants";
                case 107:                             return "Shoes";
                case 108:                             return "Glove";
                case 109:                             return "Shield";
                case 110:                             return "Cape";
                case 111:                             return "Ring";
                case int n when n >= 130 && n <= 170: return "Weapon";
                case 180:                             return "PetEquip";
                case 190:                             return "Taming";
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
            if (subProperty == null) return "NO-NAME";

            string retName = "";
            WzStringProperty mapName = (WzStringProperty) subProperty.GetFromPath("mapName");
            WzStringProperty streetName = (WzStringProperty) subProperty.GetFromPath("streetName");
            if (mapName != null) retName += mapName.Value;
            if (mapName != null && streetName != null) retName += " - "; // x fucking d
            if (streetName != null) retName += streetName.Value;
            return retName;

        }

        private static string GetStringValue(WzImageProperty img) {
            return img != null && img is WzStringProperty str ? str.Value : "NO-NAME";
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