using System;
using System.IO;

using MapleLib.WzLib;

using WzVisualizer.Properties;

namespace WzVisualizer.Util {
    public static class WzExt {
        private static readonly WzFile[] WzCache = new WzFile[Enum.GetNames(typeof(Wz)).Length];

        /// <summary>
        /// Gets the cached WZ file
        /// </summary>
        public static WzFile GetFile(this Wz wz) {
            return WzCache[(int) wz];
        }

        /// <summary>
        /// Disposes then nulls the cached WZ file
        /// </summary>
        public static void Dispose(this Wz wz) {
            var file = WzCache[(int) wz];
            file?.Dispose();
            WzCache[(int) wz] = null;

            if (wz == Wz.String) {
                StringWz.Dispose();
            }
        }

        /// <summary>
        /// Loads data given the path of a file or directory
        /// </summary>
        public static WzFile LoadFile(this Wz wz, string filePath, WzMapleVersion encryption, bool force = true) {
            WzFile file = null;
            if (!force) {
                if ((file = GetFile(wz)) != null) return file;
            }

            if (File.Exists(filePath)) {
                file = new WzFile(filePath, encryption);
                file.ParseWzFile();
            } else if (File.Exists(filePath + Resources.FileExtensionWZ)) {
                file = new WzFile(filePath + Resources.FileExtensionWZ, encryption);
                file.ParseWzFile();
            } else if (Directory.Exists(filePath)) {
                file = new WzFile(filePath, encryption);
                WzDirectory dir = new WzDirectory(filePath, file);
                file.WzDirectory = dir;
                LoadFilesImg(dir, filePath, encryption);
            }

            if (wz == Wz.String) {
                StringWz.Load(file);
            }

            return WzCache[(int) wz] = file;
        }

        /// <summary>
        /// Loads data using a directory type file system
        /// </summary>
        private static void LoadFilesImg(WzDirectory dir, string directoryPath, WzMapleVersion mapleVersion) {
            if (!Directory.Exists(directoryPath)) return;
            string[] files = Directory.GetFiles(directoryPath);
            foreach (string file in files) {
                FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                WzImage img = new WzImage(Path.GetFileName(file), stream, mapleVersion);
                dir.AddImage(img);
            }

            files = Directory.GetDirectories(directoryPath);
            foreach (string sub in files) {
                WzDirectory subDir = new WzDirectory(Path.GetFileNameWithoutExtension(sub));
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