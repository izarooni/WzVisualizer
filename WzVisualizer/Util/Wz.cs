using System;
using System.Collections.Generic;
using System.IO;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using WzVisualizer.Properties;

namespace WzVisualizer.Util {
    public static class WzExt {

        private static readonly Dictionary<Wz, List<WzFile>> _wzCache = new ();
        static WzExt() {
            foreach (Wz wz in Enum.GetValues(typeof(Wz))) {
                _wzCache[wz] = new List<WzFile>();
            }
        }

        public static List<WzFile> GetFiles(this Wz wz) {
            return _wzCache[wz];
        }

        /// <summary>
        /// Disposes then nulls the cached WZ file
        /// </summary>
        public static void Dispose(this Wz wz) {
            var list = _wzCache[wz];
            if (list == null) return;

            foreach (var file in list) {
                file.Dispose();
            }
            list.Clear();

            if (wz == Wz.String) {
                StringWz.Dispose();
            }
        }

        /// <summary>
        /// Loads data given the path of a file or directory
        /// </summary>
        public static List<WzFile> Load(this Wz wz, string path, bool lazyParse = true) {
            var list = _wzCache[wz];

            if (list == null) list = new List<WzFile>();
            var ext = Path.GetExtension(path);

            if (ext == Resources.FileExtensionWZ) {
                var encryption = WzTool.DetectMapleVersion(path, out var version);
                var file = new WzFile(path, version, encryption);
                file.ParseWzFile(lazyParse: lazyParse);
                list.Add(file);
            }

            if (Directory.Exists(path)) {
                var encryption = (WzMapleVersion)WzTool.DetectMapleVersion(path, "Cash.img");
                var file = new WzFile(path, encryption);
                var dir = file.WzDirectory = new WzDirectory(path, file);
                LoadFilesImg(dir, path, encryption);
                list.Add(file);
            }

            if (wz == Wz.String) {
                StringWz.Load(list);
            }

            _wzCache[wz] = list;
            return list;
        }

        public static void Parse(this Wz wz) {

        }

        /// <summary>
        /// We need to programmatically re-create the file structure using MapleLib,
        /// meaning WzDirectory need to contain WzDirectory and other sub-properties, recursively.
        ///
        /// Using SearchOption.AllDirectories would not be ideal as sub-directories and sub-properties would
        /// all be contained at the root level
        /// </summary>
        private static void LoadFilesImg(WzDirectory dir, string directoryPath, WzMapleVersion mapleVersion) {
            if (!Directory.Exists(directoryPath)) return;

            var files = Directory.GetFiles(directoryPath);
            foreach (string file in files) {
                var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                var img = new WzImage(Path.GetFileName(file), stream, mapleVersion);
                dir.AddImage(img);
            }

            files = Directory.GetDirectories(directoryPath);
            foreach (string sub in files) {
                var subDir = new WzDirectory(Path.GetFileNameWithoutExtension(sub));
                LoadFilesImg(subDir, sub, mapleVersion);
                dir.AddDirectory(subDir);
            }
        }
    }

    public enum Wz {
        Base,
        Character,
        Effect,
        Etc,
        Item,
        List,
        Map,
        Mob,
        Morph,
        Npc,
        Quest,
        Reactor,
        Skill,
        Sound,
        String,
        TamingMob,
        UI
    }
}