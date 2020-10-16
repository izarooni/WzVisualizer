/*  MapleLib - A general-purpose MapleStory library
 * Copyright (C) 2009, 2010, 2015 Snow and haha01haha01
   
 * This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

 * This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

 * You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;

namespace MapleLib.WzLib {
    /// <summary>
    /// A class that contains all the information of a wz file
    /// </summary>
    public class WzFile : WzObject {
        #region Fields
        private short version = 0;
        private uint versionHash = 0;
        private byte[] WzIv;
        #endregion

        /// <summary>
        /// The parsed IWzDir after having called ParseWzDirectory(), this can either be a WzDirectory or a WzListDirectory
        /// </summary>
        public WzDirectory WzDirectory { get; set; }

        /// <summary>
        /// Name of the WzFile
        /// </summary>
        public override string Name { get; set; }

        /// <summary>
        /// The WzObjectType of the file
        /// </summary>
        public override WzObjectType ObjectType { get { return WzObjectType.File; } }

        /// <summary>
        /// Returns WzDirectory[name]
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>WzDirectory[name]</returns>
        public new WzObject this[string name] { get { return WzDirectory[name]; } }

        public WzHeader Header { get; set; }

        public short FileVersion { get; set; }

        public string FilePath { get; private set; }

        public WzMapleVersion MapleVersion { get; set; }

        public override WzObject Parent { get { return null; } internal set { } }

        public override WzFile WzFileParent { get { return this; } }

        public override void Dispose() {
            if (WzDirectory == null || WzDirectory.reader == null) return;
            WzDirectory.reader.Close();
            Header = null;
            FilePath = null;
            Name = null;
            WzDirectory.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public WzFile(short gameVersion, WzMapleVersion version) {
            WzDirectory = new WzDirectory();
            this.Header = WzHeader.GetDefault();
            FileVersion = gameVersion;
            MapleVersion = version;
            WzIv = version.EncryptionKey();
            WzDirectory.WzIv = WzIv;
        }

        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        public WzFile(string filePath, WzMapleVersion version) {
            Name = Path.GetFileName(filePath);
            FilePath = filePath;
            FileVersion = -1;
            MapleVersion = version;
            if (version == WzMapleVersion.GETFROMZLZ) {
                FileStream zlzStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(filePath), "ZLZ.dll"));
                WzIv = Util.WzKeyGenerator.GetIvFromZlz(zlzStream);
                zlzStream.Close();
            } else WzIv = version.EncryptionKey();
        }

        /// <summary>
        /// Open a wz file from a file on the disk
        /// </summary>
        /// <param name="filePath">Path to the wz file</param>
        public WzFile(string filePath, short gameVersion, WzMapleVersion version) {
            Name = Path.GetFileName(filePath);
            FilePath = filePath;
            FileVersion = gameVersion;
            MapleVersion = version;
            if (version == WzMapleVersion.GETFROMZLZ) {
                FileStream zlzStream = File.OpenRead(Path.Combine(Path.GetDirectoryName(filePath), "ZLZ.dll"));
                WzIv = Util.WzKeyGenerator.GetIvFromZlz(zlzStream);
                zlzStream.Close();
            } else WzIv = version.EncryptionKey();
        }

        /// <summary>
        /// Parses the wz file, if the wz file is a list.wz file, WzDirectory will be a WzListDirectory, if not, it'll simply be a WzDirectory
        /// </summary>
        public void ParseWzFile() {
            if (MapleVersion == WzMapleVersion.GENERATE)
                throw new InvalidOperationException("Cannot call ParseWzFile() if WZ file type is GENERATE");
            ParseMainWzDirectory();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ParseWzFile(byte[] WzIv) {
            if (MapleVersion != WzMapleVersion.GENERATE)
                throw new InvalidOperationException(
                    "Cannot call ParseWzFile(byte[] generateKey) if WZ file type is not GENERATE");
            this.WzIv = WzIv;
            ParseMainWzDirectory();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        internal void ParseMainWzDirectory() {
            if (FilePath == null) {
                Helpers.ErrorLogger.Log(Helpers.ErrorLevel.Critical, "[Error] Path is null");
                return;
            }

            WzBinaryReader reader = new WzBinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read), WzIv);

            this.Header = new WzHeader();
            this.Header.Ident = reader.ReadString(4);
            this.Header.FSize = reader.ReadUInt64();
            this.Header.FStart = reader.ReadUInt32();
            this.Header.Copyright = reader.ReadNullTerminatedString();
            reader.ReadBytes((int)(Header.FStart - reader.BaseStream.Position));
            reader.Header = this.Header;
            this.version = reader.ReadInt16();
            if (FileVersion == -1) {
                for (int j = 0; j < short.MaxValue; j++) {
                    this.FileVersion = (short)j;
                    this.versionHash = GetVersionHash(version, FileVersion);
                    if (this.versionHash != 0) {
                        reader.Hash = this.versionHash;
                        long position = reader.BaseStream.Position;
                        WzDirectory testDirectory = null;
                        try {
                            testDirectory = new WzDirectory(reader, this.Name, this.versionHash, this.WzIv, this);
                            testDirectory.ParseDirectory();
                        } catch {
                            reader.BaseStream.Position = position;
                            continue;
                        }
                        WzImage testImage = testDirectory.GetChildImages()[0];

                        try {
                            reader.BaseStream.Position = testImage.Offset;
                            byte checkByte = reader.ReadByte();
                            reader.BaseStream.Position = position;
                            testDirectory.Dispose();
                            switch (checkByte) {
                                case 0x73:
                                case 0x1b: {
                                    WzDirectory directory = new WzDirectory(reader, this.Name, this.versionHash, this.WzIv, this);
                                    directory.ParseDirectory();
                                    this.WzDirectory = directory;
                                    return;
                                }
                            }
                            reader.BaseStream.Position = position;
                        } catch {
                            reader.BaseStream.Position = position;
                        }
                    }
                }
                throw new Exception("Error with game version hash : The specified game version is incorrect and WzLib was unable to determine the version itself");
            } else {
                this.versionHash = GetVersionHash(version, FileVersion);
                reader.Hash = this.versionHash;
                WzDirectory directory = new WzDirectory(reader, this.Name, this.versionHash, this.WzIv, this);
                directory.ParseDirectory();
                this.WzDirectory = directory;
            }
        }

        private uint GetVersionHash(int encver, int realver) {
            int EncryptedVersionNumber = encver;
            int VersionNumber = realver;
            int VersionHash = 0;
            int DecryptedVersionNumber = 0;
            string VersionNumberStr;
            int a = 0, b = 0, c = 0, d = 0, l = 0;

            VersionNumberStr = VersionNumber.ToString();

            l = VersionNumberStr.Length;
            for (int i = 0; i < l; i++) {
                VersionHash = (32 * VersionHash) + (int)VersionNumberStr[i] + 1;
            }
            a = (VersionHash >> 24) & 0xFF;
            b = (VersionHash >> 16) & 0xFF;
            c = (VersionHash >> 8) & 0xFF;
            d = VersionHash & 0xFF;
            DecryptedVersionNumber = (0xff ^ a ^ b ^ c ^ d);

            if (EncryptedVersionNumber == DecryptedVersionNumber) {
                return Convert.ToUInt32(VersionHash);
            } else {
                return 0;
            }
        }

        private void CreateVersionHash() {
            versionHash = 0;
            foreach (char ch in FileVersion.ToString()) {
                versionHash = (versionHash * 32) + (byte)ch + 1;
            }
            uint a = (versionHash >> 24) & 0xFF,
                b = (versionHash >> 16) & 0xFF,
                c = (versionHash >> 8) & 0xFF,
                d = versionHash & 0xFF;
            version = (byte)~(a ^ b ^ c ^ d);
        }

        /// <summary>
        /// Saves a wz file to the disk, AKA repacking.
        /// </summary>
        /// <param name="path">Path to the output wz file</param>
        public void SaveToDisk(string path) {
            WzIv = MapleVersion.EncryptionKey();
            CreateVersionHash();
            WzDirectory.SetHash(versionHash);
            string tempFile = Path.GetFileNameWithoutExtension(path) + ".TEMP";
            File.Create(tempFile).Close();
            WzDirectory.GenerateDataFile(tempFile);
            WzTool.StringCache.Clear();
            uint totalLen = WzDirectory.GetImgOffsets(WzDirectory.GetOffsets(Header.FStart + 2));
            WzBinaryWriter wzWriter = new WzBinaryWriter(File.Create(path), WzIv);
            wzWriter.Hash = (uint)versionHash;
            Header.FSize = totalLen - Header.FStart;
            for (int i = 0; i < 4; i++)
                wzWriter.Write((byte)Header.Ident[i]);
            wzWriter.Write((long)Header.FSize);
            wzWriter.Write(Header.FStart);
            wzWriter.WriteNullTerminatedString(Header.Copyright);
            long extraHeaderLength = Header.FStart - wzWriter.BaseStream.Position;
            if (extraHeaderLength > 0) {
                wzWriter.Write(new byte[(int)extraHeaderLength]);
            }
            wzWriter.Write(version);
            wzWriter.Header = Header;
            WzDirectory.SaveDirectory(wzWriter);
            wzWriter.StringCache.Clear();
            FileStream fs = File.OpenRead(tempFile);
            WzDirectory.SaveImages(wzWriter, fs);
            fs.Close();
            File.Delete(tempFile);
            wzWriter.StringCache.Clear();
            wzWriter.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public void ExportXml(string path, bool oneFile) {
            if (oneFile) {
                FileStream fs = File.Create(path + "/" + this.Name + ".xml");
                StreamWriter writer = new StreamWriter(fs);

                int level = 0;
                writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzFile", this.Name, true));
                this.WzDirectory.ExportXml(writer, oneFile, level, false);
                writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzFile"));

                writer.Close();
            } else {
                throw new Exception("Under Construction");
            }
        }

        /// <summary>
        /// Returns an array of objects from a given path. Wild cards are supported
        /// For example :
        /// GetObjectsFromPath("Map.wz/Map0/*");
        /// Would return all the objects (in this case images) from the sub directory Map0
        /// </summary>
        /// <param name="path">The path to the object(s)</param>
        /// <returns>An array of IWzObjects containing the found objects</returns>
        public List<WzObject> GetObjectsFromWildcardPath(string path) {
            if (path.ToLower() == Name.ToLower())
                return new List<WzObject> { WzDirectory };
            else if (path == "*") {
                List<WzObject> fullList = new List<WzObject>();
                fullList.Add(WzDirectory);
                fullList.AddRange(GetObjectsFromDirectory(WzDirectory));
                return fullList;
            } else if (!path.Contains("*"))
                return new List<WzObject> { GetObjectFromPath(path) };
            string[] seperatedNames = path.Split("/".ToCharArray());
            if (seperatedNames.Length == 2 && seperatedNames[1] == "*")
                return GetObjectsFromDirectory(WzDirectory);
            List<WzObject> objList = new List<WzObject>();
            foreach (WzImage img in WzDirectory.WzImages)
                foreach (string spath in GetPathsFromImage(img, Name + "/" + img.Name))
                    if (strMatch(path, spath))
                        objList.Add(GetObjectFromPath(spath));
            foreach (WzDirectory dir in WzDirectory.WzDirectories)
                foreach (string spath in GetPathsFromDirectory(dir, Name + "/" + dir.Name))
                    if (strMatch(path, spath))
                        objList.Add(GetObjectFromPath(spath));
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList;
        }

        public List<WzObject> GetObjectsFromRegexPath(string path) {
            if (path.ToLower() == Name.ToLower())
                return new List<WzObject> { WzDirectory };
            List<WzObject> objList = new List<WzObject>();
            foreach (WzImage img in WzDirectory.WzImages)
                foreach (string spath in GetPathsFromImage(img, Name + "/" + img.Name))
                    if (Regex.Match(spath, path).Success)
                        objList.Add(GetObjectFromPath(spath));
            foreach (WzDirectory dir in WzDirectory.WzDirectories)
                foreach (string spath in GetPathsFromDirectory(dir, Name + "/" + dir.Name))
                    if (Regex.Match(spath, path).Success)
                        objList.Add(GetObjectFromPath(spath));
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return objList;
        }

        public List<WzObject> GetObjectsFromDirectory(WzDirectory dir) {
            List<WzObject> objList = new List<WzObject>();
            foreach (WzImage img in dir.WzImages) {
                objList.Add(img);
                objList.AddRange(GetObjectsFromImage(img));
            }
            foreach (WzDirectory subdir in dir.WzDirectories) {
                objList.Add(subdir);
                objList.AddRange(GetObjectsFromDirectory(subdir));
            }
            return objList;
        }

        public List<WzObject> GetObjectsFromImage(WzImage img) {
            List<WzObject> objList = new List<WzObject>();
            foreach (WzImageProperty prop in img.WzProperties) {
                objList.Add(prop);
                objList.AddRange(GetObjectsFromProperty(prop));
            }
            return objList;
        }

        public List<WzObject> GetObjectsFromProperty(WzImageProperty prop) {
            List<WzObject> objList = new List<WzObject>();
            switch (prop.PropertyType) {
                case WzPropertyType.Canvas:
                    foreach (WzImageProperty canvasProp in ((WzCanvasProperty)prop).WzProperties)
                        objList.AddRange(GetObjectsFromProperty(canvasProp));
                    objList.Add(((WzCanvasProperty)prop).PngProperty);
                    break;
                case WzPropertyType.Convex:
                    foreach (WzImageProperty exProp in ((WzConvexProperty)prop).WzProperties)
                        objList.AddRange(GetObjectsFromProperty(exProp));
                    break;
                case WzPropertyType.SubProperty:
                    foreach (WzImageProperty subProp in ((WzSubProperty)prop).WzProperties)
                        objList.AddRange(GetObjectsFromProperty(subProp));
                    break;
                case WzPropertyType.Vector:
                    objList.Add(((WzVectorProperty)prop).X);
                    objList.Add(((WzVectorProperty)prop).Y);
                    break;
            }
            return objList;
        }

        internal List<string> GetPathsFromDirectory(WzDirectory dir, string curPath) {
            List<string> objList = new List<string>();
            foreach (WzImage img in dir.WzImages) {
                objList.Add(curPath + "/" + img.Name);

                objList.AddRange(GetPathsFromImage(img, curPath + "/" + img.Name));
            }
            foreach (WzDirectory subdir in dir.WzDirectories) {
                objList.Add(curPath + "/" + subdir.Name);
                objList.AddRange(GetPathsFromDirectory(subdir, curPath + "/" + subdir.Name));
            }
            return objList;
        }

        internal List<string> GetPathsFromImage(WzImage img, string curPath) {
            List<string> objList = new List<string>();
            foreach (WzImageProperty prop in img.WzProperties) {
                objList.Add(curPath + "/" + prop.Name);
                objList.AddRange(GetPathsFromProperty(prop, curPath + "/" + prop.Name));
            }
            return objList;
        }

        internal List<string> GetPathsFromProperty(WzImageProperty prop, string curPath) {
            List<string> objList = new List<string>();
            switch (prop.PropertyType) {
                case WzPropertyType.Canvas:
                    foreach (WzImageProperty canvasProp in ((WzCanvasProperty)prop).WzProperties) {
                        objList.Add(curPath + "/" + canvasProp.Name);
                        objList.AddRange(GetPathsFromProperty(canvasProp, curPath + "/" + canvasProp.Name));
                    }
                    objList.Add(curPath + "/PNG");
                    break;
                case WzPropertyType.Convex:
                    foreach (WzImageProperty exProp in ((WzConvexProperty)prop).WzProperties) {
                        objList.Add(curPath + "/" + exProp.Name);
                        objList.AddRange(GetPathsFromProperty(exProp, curPath + "/" + exProp.Name));
                    }
                    break;
                case WzPropertyType.SubProperty:
                    foreach (WzImageProperty subProp in ((WzSubProperty)prop).WzProperties) {
                        objList.Add(curPath + "/" + subProp.Name);
                        objList.AddRange(GetPathsFromProperty(subProp, curPath + "/" + subProp.Name));
                    }
                    break;
                case WzPropertyType.Vector:
                    objList.Add(curPath + "/X");
                    objList.Add(curPath + "/Y");
                    break;
            }
            return objList;
        }

        public WzObject GetObjectFromPath(string path) {
            string[] seperatedPath = path.Split("/".ToCharArray());
            if (seperatedPath[0].ToLower() != WzDirectory.name.ToLower() && seperatedPath[0].ToLower() != WzDirectory.name.Substring(0, WzDirectory.name.Length - 3).ToLower())
                return null;
            if (seperatedPath.Length == 1)
                return WzDirectory;
            WzObject curObj = WzDirectory;
            for (int i = 1; i < seperatedPath.Length; i++) {
                if (curObj == null) {
                    return null;
                }
                switch (curObj.ObjectType) {
                    case WzObjectType.Directory:
                        curObj = ((WzDirectory)curObj)[seperatedPath[i]];
                        continue;
                    case WzObjectType.Image:
                        curObj = ((WzImage)curObj)[seperatedPath[i]];
                        continue;
                    case WzObjectType.Property:
                        switch (((WzImageProperty)curObj).PropertyType) {
                            case WzPropertyType.Canvas:
                                curObj = ((WzCanvasProperty)curObj)[seperatedPath[i]];
                                continue;
                            case WzPropertyType.Convex:
                                curObj = ((WzConvexProperty)curObj)[seperatedPath[i]];
                                continue;
                            case WzPropertyType.SubProperty:
                                curObj = ((WzSubProperty)curObj)[seperatedPath[i]];
                                continue;
                            case WzPropertyType.Vector:
                                if (seperatedPath[i] == "X")
                                    return ((WzVectorProperty)curObj).X;
                                else if (seperatedPath[i] == "Y")
                                    return ((WzVectorProperty)curObj).Y;
                                else
                                    return null;
                            default: // Wut?
                                return null;
                        }
                }
            }
            return curObj;
        }

        internal bool strMatch(string strWildCard, string strCompare) {
            if (strWildCard.Length == 0) return strCompare.Length == 0;
            if (strCompare.Length == 0) return false;
            if (strWildCard[0] == '*' && strWildCard.Length > 1)
                for (int index = 0; index < strCompare.Length; index++) {
                    if (strMatch(strWildCard.Substring(1), strCompare.Substring(index)))
                        return true;
                }
            else if (strWildCard[0] == '*')
                return true;
            else if (strWildCard[0] == strCompare[0])
                return strMatch(strWildCard.Substring(1), strCompare.Substring(1));
            return false;
        }
    }
}