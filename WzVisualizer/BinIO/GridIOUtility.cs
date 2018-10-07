using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WzVisualizer
{
    class GridIOUtility
    {

        private const string ExportFolder = "exports";

        internal static void ImportGrid(string file, DataGridView grid)
        {
            string path = string.Format("{0}/{1}", ExportFolder, file);
            if (!File.Exists(path))
                return;
            using (BinaryReader breader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                int rows = breader.ReadInt32();
                for (int i = 0; i < rows; i++)
                {
                    BinData binData = new BinData();
                    binData.ID = breader.ReadInt32();
                    binData.Name = breader.ReadString();
                    int propCount = breader.ReadInt32();
                    for (int p = 0; p < propCount; p++)
                        binData.properties.Add(breader.ReadString());
                    if (breader.ReadBoolean())
                    {
                        int bufferLength = breader.ReadInt32();
                        byte[] buffer = breader.ReadBytes(bufferLength);
                        using (MemoryStream memStream = new MemoryStream(buffer))
                            binData.image = new Bitmap(memStream);
                    }
                    string allProperties = "";
                    foreach (string prop in binData.properties)
                        allProperties += prop + "\r\n";
                    grid.Rows.Add(new object[] { binData.ID, binData?.image, binData.Name, allProperties });
                }
            }
        }

        internal static void ExportGrid(DataGridView grid, string folder)
        {
            string directory = string.Format("{0}/{1}", ExportFolder, folder);
            Directory.CreateDirectory(directory);
            string path = string.Format("{0}/{2}.bin", directory, folder, ((TabPage)grid.Parent).Text);
            using (BinaryWriter bwriter = new BinaryWriter(new FileStream(path, FileMode.Create)))
            {
                var rows = grid.Rows;
                bwriter.Write(rows.Count);
                for (int a = 0; a < rows.Count; a++)
                {
                    var cells = rows[a].Cells;
                    BinData binData = new BinData();
                    for (int b = 0; b < cells.Count; b++)
                    {
                        var cell = cells[b];
                        string ownColumnName = cell.OwningColumn.Name;
                        if (ownColumnName.EndsWith("ID")) binData.ID = (int)cell.Value;
                        else if (ownColumnName.EndsWith("Image")) binData.image = (Bitmap)cell.Value;
                        else if (ownColumnName.EndsWith("Name")) binData.Name = (string)cell.Value;
                        else if (ownColumnName.EndsWith("Properties"))
                        {
                            string[] properties = ((string)cell.Value).Split(new[] { "\r\n" }, StringSplitOptions.None);
                            foreach (string prop in properties)
                                binData.properties.Add(prop);
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Unhandled column '{0}'", ownColumnName));
                            continue;
                        }
                    }
                    bwriter.Write(binData.ID);
                    bwriter.Write(binData.Name);
                    var propCount = binData.properties.Count;
                    bwriter.Write(propCount);
                    foreach (string prop in binData.properties)
                        bwriter.Write(prop);
                    bwriter.Write(binData.image != null);
                    if (binData.image != null)
                    {
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            binData.image.Save(memStream, ImageFormat.Png);
                            byte[] buffer = memStream.GetBuffer();
                            bwriter.Write(buffer.Length);
                            bwriter.Write(buffer);
                        }
                    }
                }
            }
        }
    }
}
