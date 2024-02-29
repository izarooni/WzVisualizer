﻿/*  MapleLib - A general-purpose MapleStory library
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
using MapleLib.WzLib.Util;
using System;
using System.Diagnostics;
using MapleLib.PacketLib;
using MapleLib.WzLib.WzStructure.Enums;
using MapleLib.WzLib.WzProperties;
using System.Linq;

namespace MapleLib.WzLib
{
    /// <summary>
    /// A directory in the wz file, which may contain sub directories or wz images
    /// </summary>
    public class WzDirectory : WzObject
    {
        #region Fields
        private List<WzImage> images = new List<WzImage>();
        internal List<WzDirectory> subDirs = new List<WzDirectory>();
        internal WzBinaryReader reader;
        internal uint offset = 0;
        internal string name;
        internal uint hash;
        internal int size, checksum, offsetSize;
        internal byte[] WzIv;
        internal WzObject parent;
        internal WzFile wzFile;
        #endregion

        #region Inherited Members
        /// <summary>  
        /// The parent of the object
        /// </summary>
        public override WzObject Parent { get { return parent; } internal set { parent = value; } }
        /// <summary>
        /// The name of the directory
        /// </summary>
        public override string Name { get { return name; } set { name = value; } }
        /// <summary>
        /// The WzObjectType of the directory
        /// </summary>
        public override WzObjectType ObjectType { get { return WzObjectType.Directory; } }

        public override WzFile WzFileParent
        {
            get { return wzFile; }
        }

        /// <summary>
        /// Disposes the obejct
        /// </summary>
        public override void Dispose()
        {
            name = null;
            reader = null;
            foreach (WzImage img in images)
                img.Dispose();
            foreach (WzDirectory dir in subDirs)
                dir.Dispose();
            images.Clear();
            subDirs.Clear();
            images = null;
            subDirs = null;
        }
        #endregion

        #region Custom Members
        /// <summary>
        /// The size of the directory in the wz file
        /// </summary>
        public int BlockSize { get { return size; } set { size = value; } }
        /// <summary>
        /// The directory's chceksum
        /// </summary>
        public int Checksum { get { return checksum; } set { checksum = value; } }
        /// <summary>
        /// The wz images contained in the directory
        /// </summary>
        public List<WzImage> WzImages { get { return images; } private set { } }
        /// <summary>
        /// The sub directories contained in the directory
        /// </summary>
        public List<WzDirectory> WzDirectories { get { return subDirs; } }
        /// <summary>
        /// Offset of the folder
        /// </summary>
        public uint Offset { get { return offset; } set { offset = value; } }
        /// <summary>
        /// Returns a WzImage or a WzDirectory with the given name
        /// </summary>
        /// <param name="name">The name of the img or dir to find</param>
        /// <returns>A WzImage or WzDirectory</returns>
        public new WzObject this[string name]
        {
            get
            {
                string nameLower = name.ToLower();

                foreach (WzImage i in images)
                    if (i.Name.ToLower() == nameLower)
                        return i;
                foreach (WzDirectory d in subDirs)
                    if (d.Name.ToLower() == nameLower)
                        return d;

                //throw new KeyNotFoundException("No wz image or directory was found with the specified name");
                return null;
            }
            set
            {
                if (value != null)
                {
                    value.Name = name;
                    if (value is WzDirectory directory)
                        AddDirectory(directory);
                    else if (value is WzImage image)
                        AddImage(image);
                    else
                        throw new ArgumentException("Value must be a Directory or Image");
                }
            }
        }



        /// <summary>
        /// Creates a blank WzDirectory
        /// </summary>
        public WzDirectory() { }
        /// <summary>
        /// Creates a WzDirectory with the given name
        /// </summary>
        /// <param name="dirName">The name of the directory</param>
        public WzDirectory(string dirName)
        {
            this.name = dirName;
        }

        public WzDirectory(string dirName, WzFile parentWzFileIvVerHashCloneSource)
        {
            this.name = dirName;
            this.hash = parentWzFileIvVerHashCloneSource.versionHash;
            this.WzIv = parentWzFileIvVerHashCloneSource.WzIv;
            this.wzFile = parentWzFileIvVerHashCloneSource;
        }

        /// <summary>
        /// Creates a WzDirectory
        /// </summary>
        /// <param name="reader">The BinaryReader that is currently reading the wz file</param>
        /// <param name="blockStart">The start of the data block</param>
        /// <param name="parentname">The name of the directory</param>
        /// <param name="wzFile">The parent Wz File</param>
        internal WzDirectory(WzBinaryReader reader, string dirName, uint verHash, byte[] WzIv, WzFile wzFile)
        {
            this.reader = reader;
            this.name = dirName;
            this.hash = verHash;
            this.WzIv = WzIv;
            this.wzFile = wzFile;
        }

        public override string ToString() {
            return $"WzDirectory(Name={Name})";
        }

        /// <summary>
        /// Parses the WzDirectory
        /// <paramref name="lazyParse">Only parses the first directory</paramref>
        /// </summary>
        internal void ParseDirectory(bool lazyParse = false)
        {
            //reader.PrintHexBytes(20);
            long available = reader.Available();
            if (available == 0)
                return;

            int entryCount = reader.ReadCompressedInt();
            if (entryCount < 0 || entryCount > 100000) // probably nothing > 100k folders for now.
                throw new Exception("Invalid wz version used for decryption, try parsing other version numbers.");

            for (int i = 0; i < entryCount; i++)
            {
                byte type = reader.ReadByte(); // see WzBinaryWriter.WriteWzObjectValue
                string fname = null;
                int fsize;
                int checksum;
                uint offset;

                long rememberPos = 0;
                switch (type)
                {
                    case (byte) WzDirectoryType.UnknownType_1:  //01 XX 00 00 00 00 00 OFFSET (4 bytes) 
                        {
                            int unknown = reader.ReadInt32();
                            reader.ReadInt16();
                            uint offs = reader.ReadOffset();
                            continue;
                        }
                    case (byte) WzDirectoryType.RetrieveStringFromOffset_2:
                        {
                            int stringOffset = reader.ReadInt32();
                            rememberPos = reader.BaseStream.Position;
                            reader.BaseStream.Position = reader.Header.FStart + stringOffset;

                            type = reader.ReadByte();
                            fname = reader.ReadString();
                            break;
                        }
                    case (byte) WzDirectoryType.WzDirectory_3:
                    case (byte) WzDirectoryType.WzImage_4:
                        {
                            fname = reader.ReadString();
                            rememberPos = reader.BaseStream.Position;
                            break;
                        }
                    default:
                        {
                            reader.PrintHexBytes(20);
                            throw new Exception("[WzDirectory] Unknown directory. type = " + type);
                        }
                }
                reader.BaseStream.Position = rememberPos;
                fsize = reader.ReadCompressedInt();
                checksum = reader.ReadCompressedInt();
                offset = reader.ReadOffset(); // IWzArchive::Getposition(pArchive)

                if (type == (byte) WzDirectoryType.WzDirectory_3)
                {
                    WzDirectory subDir = new WzDirectory(reader, fname, hash, WzIv, wzFile)
                    {
                        BlockSize = fsize,
                        Checksum = checksum,
                        Offset = offset,
                        Parent = this
                    };
                    subDirs.Add(subDir);

                    if (lazyParse)
                        break;
                }
                else
                {
                    WzImage img = new WzImage(fname, reader, checksum)
                    {
                        BlockSize = fsize,
                        Offset = offset,
                        Parent = this
                    };
                    images.Add(img);

                    if (lazyParse)
                        break;
                }
            }

            foreach (WzDirectory subdir in subDirs)
            {
                if (subdir.Checksum != 0)
                {
                    reader.BaseStream.Position = subdir.offset;
                    subdir.ParseDirectory();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wzWriter"></param>
        /// <param name="fs"></param>
        internal void SaveImages(BinaryWriter wzWriter, FileStream fs)
        {
            // List<string> wzImageNameTracking = new List<string>(); // Check for duplicate WZ image name that could cause errors later on.

            foreach (WzImage img in images)
            {
                // Check for duplicate WZ image name that could cause errors later on.
                // this will only be warning to the user, but it'll still save it fine
                //if (wzImageNameTracking.Contains(img.Name))
                //Debug.WriteLine("Duplicate img detected. Parent: {0}, Name: {1}", img.Parent.Name, img.Name);
                // else
                //wzImageNameTracking.Add(img.Name);

                // Write 
                if (img.Changed)
                {
                    fs.Position = img.tempFileStart;

                    //byte[] buffer = new byte[size];
                    //fs.Read(buffer, 0, size);
                    byte[] buffer = new byte[img.size];
                    fs.Read(buffer, 0, img.size);

                    wzWriter.Write(buffer);
                }
                else
                {
                    //reader.BaseStream.Position = img.tempFileStart;
                    //wzWriter.Write(reader.ReadBytes((int)(img.tempFileEnd - img.tempFileStart)));
                    img.reader.BaseStream.Position = img.tempFileStart;
                    wzWriter.Write(img.reader.ReadBytes((int)(img.tempFileEnd - img.tempFileStart)));
                }
            }
            foreach (WzDirectory dir in subDirs)
            {
                dir.SaveImages(wzWriter, fs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="useIv">The IV to use while generating the data file. If null, it'll use the WzDirectory default</param>
        /// <param name="bIsWzUserKeyDefault">Uses the default MapleStory UserKey or a custom key.</param>
        /// <param name="prevOpenedStream">The previously opened file stream</param>
        /// <returns></returns>
        internal int GenerateDataFile(byte[] useIv, bool bIsWzUserKeyDefault, FileStream prevOpenedStream)
        {
            bool useCustomIv = useIv != null; // whole shit gonna be re-written if its a custom IV specified

            size = 0;
            int entryCount = subDirs.Count + images.Count;
            if (entryCount == 0)
            {
                offsetSize = 1;
                return (size = 0);
            }
            size = WzTool.GetCompressedIntLength(entryCount);
            offsetSize = WzTool.GetCompressedIntLength(entryCount);

            foreach (WzImage img in images)
            {
                if (useCustomIv ||// everything needs to be re-written when a custom IV is used.
                    !bIsWzUserKeyDefault || //  everything needs to be re-written when a custom UserKey is used too
                    img.bIsImageChanged) // or when an image is changed
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        using (WzBinaryWriter imgWriter = new WzBinaryWriter(memStream, useCustomIv ? useIv : this.WzIv))
                        {
                            img.SaveImage(imgWriter, bIsWzUserKeyDefault, useCustomIv);

                            img.CalculateAndSetImageChecksum(memStream.ToArray()); // checksum

                            img.tempFileStart = prevOpenedStream.Position;
                            prevOpenedStream.Write(memStream.ToArray(), 0, (int)memStream.Length);
                            img.tempFileEnd = prevOpenedStream.Position;
                        }
                    }
                }
                else
                {
                    img.tempFileStart = img.offset;
                    img.tempFileEnd = img.offset + img.size;
                }
                img.UnparseImage();

                int nameLen = WzTool.GetWzObjectValueLength(img.name, 4);
                size += nameLen;
                int imgLen = img.size;
                size += WzTool.GetCompressedIntLength(imgLen);
                size += imgLen;
                size += WzTool.GetCompressedIntLength(img.Checksum);
                size += 4;
                offsetSize += nameLen;
                offsetSize += WzTool.GetCompressedIntLength(imgLen);
                offsetSize += WzTool.GetCompressedIntLength(img.Checksum);
                offsetSize += 4;

                // otherwise Item.wz (300MB) probably uses > 4GB
                if (useCustomIv || !bIsWzUserKeyDefault) // when using custom IV, or changing IVs, all images have to be re-read and re-written..
                {
                    GC.Collect(); // GC slows down writing of maps in HaCreator
                    GC.WaitForPendingFinalizers();
                }

                //Debug.WriteLine("Writing image :" + img.FullPath);
            }

            foreach (WzDirectory dir in subDirs)
            {
                int nameLen = WzTool.GetWzObjectValueLength(dir.name, 3);
                size += nameLen;
                size += dir.GenerateDataFile(useIv, bIsWzUserKeyDefault, prevOpenedStream);
                size += WzTool.GetCompressedIntLength(dir.size);
                size += WzTool.GetCompressedIntLength(dir.Checksum);
                size += 4;
                offsetSize += nameLen;
                offsetSize += WzTool.GetCompressedIntLength(dir.size);
                offsetSize += WzTool.GetCompressedIntLength(dir.Checksum);
                offsetSize += 4;

                //Debug.WriteLine("Writing dir :" + dir.FullPath);
            }
            return size;
        }
        internal void SaveDirectory(WzBinaryWriter writer)
        {
            offset = (uint)writer.BaseStream.Position;
            int entryCount = subDirs.Count + images.Count;
            if (entryCount == 0)
            {
                BlockSize = 0;
                return;
            }
            writer.WriteCompressedInt(entryCount);
            foreach (WzImage img in images)
            {
                if (!writer.WriteWzObjectValue(img.name, WzDirectoryType.WzImage_4))  // true if written as an offset
                {
                }
                writer.WriteCompressedInt(img.BlockSize);
                writer.WriteCompressedInt(img.Checksum);
                writer.WriteOffset(img.Offset);
            }
            foreach (WzDirectory dir in subDirs)
            {
                if (!writer.WriteWzObjectValue(dir.name, WzDirectoryType.WzDirectory_3)) // true if written as an offset
                {
                }
                writer.WriteCompressedInt(dir.BlockSize);
                writer.WriteCompressedInt(dir.Checksum);
                writer.WriteOffset(dir.Offset);
            }
            foreach (WzDirectory dir in subDirs)
                if (dir.BlockSize > 0)
                    dir.SaveDirectory(writer);
                else
                    writer.Write((byte)0);
        }
        internal uint GetOffsets(uint curOffset)
        {
            offset = curOffset;
            curOffset += (uint)offsetSize;
            foreach (WzDirectory dir in subDirs)
            {
                curOffset = dir.GetOffsets(curOffset);
            }
            return curOffset;
        }
        internal uint GetImgOffsets(uint curOffset)
        {
            foreach (WzImage img in images)
            {
                img.Offset = curOffset;
                curOffset += (uint)img.BlockSize;
            }
            foreach (WzDirectory dir in subDirs)
            {
                curOffset = dir.GetImgOffsets(curOffset);
            }
            return curOffset;
        }
        internal void ExportXml(StreamWriter writer, bool oneFile, int level, bool isDirectory)
        {
            if (oneFile)
            {
                if (isDirectory)
                {
                    writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.OpenNamedTag("WzDirectory", this.name, true));
                }
                foreach (WzDirectory subDir in WzDirectories)
                {
                    subDir.ExportXml(writer, oneFile, level + 1, isDirectory);
                }
                foreach (WzImage subImg in WzImages)
                {
                    subImg.ExportXml(writer, oneFile, level + 1);
                }
                if (isDirectory)
                {
                    writer.WriteLine(XmlUtil.Indentation(level) + XmlUtil.CloseTag("WzDirectory"));
                }
            }
        }
        /// <summary>
        /// Parses the wz images
        /// </summary>
        public void ParseImages()
        {
            foreach (WzImage img in images)
            {
                if (reader.BaseStream.Position != img.Offset)
                {
                    reader.BaseStream.Position = img.Offset;
                }
                img.ParseImage();
            }
            foreach (WzDirectory subdir in subDirs)
            {
                if (reader.BaseStream.Position != subdir.Offset)
                {
                    reader.BaseStream.Position = subdir.Offset;
                }
                subdir.ParseImages();
            }
        }

        /// <summary>
        /// Sets the version hash of the directory (see WzFile.CreateVersionHash() )
        /// </summary>
        /// <param name="newHash"></param>
        internal void SetVersionHash(uint newHash)
        {
            this.hash = newHash;
            foreach (WzDirectory dir in subDirs)
                dir.SetVersionHash(newHash);
        }

        /// <summary>
        /// Adds a WzImage to the list of wz images
        /// </summary>
        /// <param name="img">The WzImage to add</param>
        public void AddImage(WzImage img)
        {
            images.Add(img);
            img.Parent = this;
        }
        /// <summary>
        /// Adds a WzDirectory to the list of sub directories
        /// </summary>
        /// <param name="dir">The WzDirectory to add</param>
        public void AddDirectory(WzDirectory dir)
        {
            subDirs.Add(dir);
            dir.wzFile = wzFile;
            dir.Parent = this;
        }
        /// <summary>
        /// Clears the list of images
        /// </summary>
        public void ClearImages()
        {
            foreach (WzImage img in images) 
                img.Parent = null;
            images.Clear();
        }

        /// <summary>
        /// Clears the list of sub directories
        /// </summary>
        public void ClearDirectories()
        {
            foreach (WzDirectory dir in subDirs) 
                dir.Parent = null;
            subDirs.Clear();
        }

        /// <summary>
        /// Gets an image in the list of images by it's name
        /// </summary>
        /// <param name="name">The name of the image</param>
        /// <returns>The wz image that has the specified name or null if none was found</returns>
        public WzImage GetImageByName(string name)
        {
            // Find the first WzImage with a matching name (case-insensitive)
            return images.FirstOrDefault(wzI => wzI.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Gets a sub directory in the list of directories by it's name
        /// </summary>
        /// <param name="name">The name of the directory</param>
        /// <returns>The wz directory that has the specified name or null if none was found</returns>
        public WzDirectory GetDirectoryByName(string name)
        {
            // Find the first WzDirectory with a matching name (case-insensitive)
            return subDirs.FirstOrDefault(dir => dir.Name.ToLower() == name.ToLower());
        }

        /// <summary>
        /// Removes an image from the list
        /// </summary>
        /// <param name="image">The image to remove</param>
        public void RemoveImage(WzImage image)
        {
            images.Remove(image);
            image.Parent = null;
        }
        /// <summary>
        /// Removes a sub directory from the list
        /// </summary>
        /// <param name="name">The sub directory to remove</param>
        public void RemoveDirectory(WzDirectory dir)
        {
            subDirs.Remove(dir);
            dir.Parent = null;
        }

        public WzDirectory DeepClone()
        {
            WzDirectory result = (WzDirectory)MemberwiseClone();
            result.WzDirectories.Clear();
            result.WzImages.Clear();
            foreach (WzDirectory dir in WzDirectories)
                result.WzDirectories.Add(dir.DeepClone());
            foreach (WzImage img in WzImages)
                result.WzImages.Add(img.DeepClone());
            return result;
        }

        public int CountImages()
        {
            int result = images.Count;
            foreach (WzDirectory subdir in WzDirectories)
                result += subdir.CountImages();
            return result;
        }
        #endregion

        public override void Remove()
        {
            ((WzDirectory)Parent).RemoveDirectory(this);
        }
    }
}