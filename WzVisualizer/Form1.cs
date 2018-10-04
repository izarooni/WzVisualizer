using MapleLib.WzLib;
using MapleLib.WzLib.WzProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WzVisualizer.Properties;

namespace WzVisualizer
{
    public partial class MainForm : Form
    {

        private OpenFileDialog FD = new OpenFileDialog();

        public MainForm()
        {
            InitializeComponent();

            FD.Filter = "WZ Files (*.wz)|*.wz";
            FD.RestoreDirectory = true;

            this.ComboLoadType.SelectedIndex = 0;
            this.ComboEncType.SelectedIndex = 0;
        }

        private void TextWzPath_Click(object sender, EventArgs e)
        {
            if (FD.ShowDialog() == DialogResult.OK)
            {
                string filePath = FD.FileName;
                TextWzPath.Text = filePath;
            }
        }

        private void BtnWzLoad_Click(object sender, EventArgs e)
        {
            string filePath = TextWzPath.Text;
            if (filePath.Length > 0)
            {
                WzMapleVersion mapleVersion = (WzMapleVersion)ComboEncType.SelectedIndex;
                Console.WriteLine(string.Format("loading wz file '{0}' with encryption type '{1}'", filePath, mapleVersion));
                WzFile f = new WzFile(filePath, 83, mapleVersion);
                f.ParseWzFile();
                List<WzImage> children = f.WzDirectory.GetChildImages();
                children.Sort((a, b) => a.Name.CompareTo(b.Name));
                for (int i = 0; i < f.WzDirectory.CountImages(); i++)
                {
                    WzImage image = children[i];
                    image.ParseImage();
                    string name = Path.GetFileNameWithoutExtension(image.Name);
                    if (int.TryParse(name, out int equip_id))
                    {
                        int category = equip_id / 100000;
                        if (category >= 13 && category <= 17)
                        {
                            GridEWeapons.Rows.Add(new object[] { name, ((WzCanvasProperty)image.GetFromPath("info/icon")).GetBitmap() });
                        }
                    }
                }
            }
        }

        private void ComboLoadType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboLoadType.SelectedIndex == 1)
            {
                TextWzPath.Enabled = true;
                BtnWzLoad.Enabled = true;
            }
        }
    }
}
