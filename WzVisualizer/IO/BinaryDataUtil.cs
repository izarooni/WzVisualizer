using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using WzVisualizer.GUI;
using WzVisualizer.GUI.Controls;

namespace WzVisualizer.IO {
    public static class BinaryDataUtil {
        private const string ExportFolder = "exports";
        private const string ImagesFolder = "images";

        private static List<BinData> ParseBinaryFile(string path) {
            using FileStream fs = new FileStream(path, FileMode.Open);
            using BinaryReader br = new BinaryReader(fs);

            var rowCount = br.ReadInt32();
            var bins = new List<BinData>(rowCount);
            for (int i = 0; i < rowCount; i++) {
                var bin = new BinData {
                    ID = br.ReadInt32(),
                    Name = br.ReadString()
                };
                int propCount = br.ReadInt32();
                for (int p = 0; p < propCount; p++) {
                    bin.Properties.Add(br.ReadString());
                }

                if (br.ReadBoolean()) {
                    int bufferLength = br.ReadInt32();
                    byte[] buffer = br.ReadBytes(bufferLength);
                    using MemoryStream ms = new MemoryStream(buffer);
                    Image image = Image.FromStream(ms);
                    Bitmap bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format16bppRgb555);
                    bmp.MakeTransparent();
                    using (var g = Graphics.FromImage(bmp)) {
                        g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
                    }

                    image.Dispose();
                    bin.Image = bmp;
                }

                bins.Add(bin);
            }

            bins.Sort((a, b) => a.ID.CompareTo(b.ID));

            return bins;
        }

        internal static void ImportGrid(string file, DataViewport dv, AddGridRowCallBack callbackTask) {
            var grid = dv.GridView;
            var data = dv.Data;

            if (data.Count == 0) {
                // load binary files
                var path = $"{ExportFolder}/{file}";
                if (!File.Exists(path)) return;
                dv.Data.AddRange(ParseBinaryFile(path));
            }

            if (callbackTask == null) return;
            grid.SuspendLayout();
            dv.Data.ForEach(e => callbackTask(grid, e));
            grid.ResumeLayout();
        }

        public static void ExportPictures(TabPage tab, string folder) {
            if (tab.Controls[0] is TabControl ctrl) {
                foreach (TabPage subTab in ctrl.TabPages) {
                    ExportPictures(subTab, folder);
                }

                return;
            }

            var directory = $"{ImagesFolder}/{folder}";
            Directory.CreateDirectory(directory);

            var dv = (DataViewport)tab.Controls[0];
            foreach (BinData bin in dv.Data) {
                var bitmap = bin.Image;
                string fileName = $"{bin.ID}";
                if (bitmap == null || string.IsNullOrEmpty(fileName)) continue;
                bitmap.Save($"{directory}/{fileName}.png", ImageFormat.Png);
            }
        }

        public static void ExportBinary(TabPage tab, string folder) {
            if (tab.Controls[0] is TabControl ctrl) {
                foreach (TabPage subTab in ctrl.TabPages) {
                    ExportBinary(subTab, folder);
                }
                return;
            }

            var dv = (DataViewport)tab.Controls[0];
            var data = dv.Data;

            if (data.Count == 0) return;

            string directory = $"{ExportFolder}/{folder}";
            string filePath = $"{directory}/{tab.Text}.bin";
            Directory.CreateDirectory(directory);

            using BinaryWriter bw = new BinaryWriter(new FileStream(filePath, FileMode.Create));
            bw.Write(data.Count);
            foreach (BinData bin in data) {
                bw.Write(bin.ID);
                bw.Write(bin.Name);
                bw.Write(bin.Properties.Count);
                foreach (string prop in bin.Properties) {
                    bw.Write(prop);
                }

                bw.Write(bin.Image != null);
                if (bin.Image != null) {
                    using MemoryStream ms = new MemoryStream();
                    bin.Image.Save(ms, ImageFormat.Png);
                    byte[] buffer = ms.GetBuffer();
                    bw.Write(buffer.Length);
                    bw.Write(buffer);
                }
            }
            bw.Flush();
        }
    }
}