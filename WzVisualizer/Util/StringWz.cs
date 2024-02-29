using System;
using System.Collections.Generic;

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

            _fieldNames.Clear();
        }

        public static void Load(List<WzFile> files) {
            foreach (var file in files) {
                file.ParseWzFile();

                _eqp ??= file.WzDirectory.GetImageByName("Eqp.img");
                _etc ??= file.WzDirectory.GetImageByName("Etc.img");
                _cash ??= file.WzDirectory.GetImageByName("Cash.img");
                _ins ??= file.WzDirectory.GetImageByName("Ins.img");
                _consume ??= file.WzDirectory.GetImageByName("Consume.img");
                _map ??= file.WzDirectory.GetImageByName("Map.img");
                _mob ??= file.WzDirectory.GetImageByName("Mob.img");
                _skill ??= file.WzDirectory.GetImageByName("Skill.img");
                _npc ??= file.WzDirectory.GetImageByName("Npc.img");
                _pet ??= file.WzDirectory.GetImageByName("Pet.img");
            }
        }

        private static Dictionary<int, string> _fieldNames = new();
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
                case 4:
                case 6: return "Hair";
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
                case int n when n >= 130 && n <= 170: return "Weapon";
                case 180: return "PetEquip";
                case 190: return "Taming";
            }
        }

        public static string GetFieldFullName(int mapId) {

            var files = Wz.String.GetFiles();

            if (_fieldNames.Count == 0 && files.Count > 0) {
                foreach (var file in files) {
                    var map = file.WzDirectory.GetImageByName("Map.img");
                    if (map == null) continue;

                    foreach (var region in map.WzProperties) {
                        foreach (var id in region.WzProperties) {
                            var parsedId = int.Parse(id.Name);

                            var retName = "";
                            var mapName = id.GetFromPath("mapName") as WzStringProperty;
                            var streetName = id.GetFromPath("streetName") as WzStringProperty;

                            if (mapName != null) retName += mapName.Value;
                            if (mapName != null && streetName != null) retName += " - "; // x fucking d
                            if (streetName != null) retName += streetName.Value;

                            if (!_fieldNames.ContainsKey(parsedId)) _fieldNames.Add(parsedId, retName);
                        }
                    }
                }
            }

            if (_fieldNames.TryGetValue(mapId, out var name)) {
                return name;
            }

            return "NO-NAME";

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