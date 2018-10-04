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

        private FolderBrowserDialog FB = new FolderBrowserDialog();

        public MainForm()
        {
            InitializeComponent();

            this.TextWzPath.Text = Settings.Default.PathCache;
            this.ComboLoadType.SelectedIndex = 0;
            this.ComboEncType.SelectedIndex = 0;
        }

        private void TextWzPath_Click(object sender, EventArgs e)
        {
            if (FB.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(FB.SelectedPath))
            {
                string[] files = Directory.GetFiles(FB.SelectedPath);
                TextWzPath.Text = FB.SelectedPath;
            }
        }

        private void BtnWzLoad_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            string folderPath = TextWzPath.Text;
            if (folderPath.Length > 0)
            {
                Settings.Default.PathCache = folderPath;
                Settings.Default.Save();
                WzMapleVersion mapleVersion = (WzMapleVersion)ComboEncType.SelectedIndex;
                LoadWzData(mapleVersion, folderPath);
            }
            Cursor = Cursors.Default;
        }

        private void ComboLoadType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboLoadType.SelectedIndex == 1)
            {
                TextWzPath.Enabled = true;
                BtnWzLoad.Enabled = true;
            }
        }

        private void AddGridRow(DataGridView grid, object wzObject)
        {
            if (wzObject is WzImage image)
            {
                string ID = image.Name;
                WzCanvasProperty imgIcon = (WzCanvasProperty)image.GetFromPath("info/icon");
                grid.Rows.Add(new object[] { ID, imgIcon?.GetBitmap() });
            } else if (wzObject is WzSubProperty subProperty)
            {
                string ID = subProperty.Name;
                WzImageProperty imgProperty = subProperty.GetFromPath("info/icon");
                if (imgProperty is WzUOLProperty ufo)
                {
                    WzCanvasProperty imgIcon = (WzCanvasProperty)ufo.LinkValue;
                    grid.Rows.Add(new object[] { ID, imgIcon?.GetBitmap() });
                } else
                {
                    WzCanvasProperty imgIcon = (WzCanvasProperty)imgProperty;
                    grid.Rows.Add(new object[] { ID, imgIcon?.GetBitmap() });
                }
            }
        }

        private void LoadWzData(WzMapleVersion mapleVersion, string mapleDirectory)
        {
            switch (TabControlMain.SelectedIndex)
            {
                default:
                    Console.WriteLine("Unable to load WZ data unhandled selected index: " + TabControlMain.SelectedIndex);
                    break;
                case 0: // Equips
                    {
                        
                        WzFile f = new WzFile(mapleDirectory + "/Character.wz", 83, mapleVersion);
                        f.ParseWzFile();
                        List<WzImage> children = f.WzDirectory.GetChildImages();
                        children.Sort((a, b) => a.Name.CompareTo(b.Name));
                        for (int i = 0; i < f.WzDirectory.CountImages(); i++)
                        {
                            WzImage image = children[i];
                            string name = Path.GetFileNameWithoutExtension(image.Name);
                            if (int.TryParse(name, out int equip_id))
                            {
                                int bodyPart = equip_id / 10000;
                                switch (bodyPart)
                                {
                                    default:
                                        if (bodyPart >= 130 && bodyPart <= 170)
                                        {
                                            image.ParseImage();
                                            AddGridRow(GridEWeapons, image);
                                        } else
                                            Console.WriteLine("Unhandled body part equip: " + equip_id + " // " + (bodyPart));
                                        break;
                                    case 100: // Caps
                                        image.ParseImage();
                                        AddGridRow(GridECaps, image);
                                        break;
                                    case 101: // Accessory
                                        // image.ParseImage();
                                        
                                        break;
                                    case 110: // Cape
                                        image.ParseImage();
                                        AddGridRow(GridECapes, image);
                                        break;
                                    case 104: // Coat
                                        image.ParseImage();
                                        AddGridRow(GridETops, image);
                                        break;
                                    case 108: // Glove
                                        image.ParseImage();
                                        AddGridRow(GridEGloves, image);
                                        break;
                                    case 105: // Longcoat
                                        image.ParseImage();
                                        AddGridRow(GridELongcoats, image);
                                        break;
                                    case 106: // Pants
                                        image.ParseImage();
                                        AddGridRow(GridEBottoms, image);
                                        break;
                                    case 180: // Pet Equips
                                        // image.ParseImage();
                                        break;
                                    case 111: // Rings
                                        image.ParseImage();
                                        break;
                                    case 109: // Shield
                                        image.ParseImage();
                                        AddGridRow(GridEShields, image);
                                        break;
                                    case 107: // Shoes
                                        image.ParseImage();
                                        AddGridRow(GridEShoes, image);
                                        break;
                                    case 190:
                                    case 191:
                                    case 193: // Taming Mob
                                        // image.ParseImage();
                                        break;
                                }
                                //int category = equip_id / 100000;
                                //if (category >= 13 && category <= 17)
                                    //GridEWeapons.Rows.Add(new object[] { name, ((WzCanvasProperty)image.GetFromPath("info/icon")).GetBitmap() });
                            }
                        }
                        break;
                    }
                case 1: // Use
                    {
                        WzFile f = new WzFile(mapleDirectory + "/Item.wz", 83, mapleVersion);
                        f.ParseWzFile();
                        List<WzImage> children = f.WzDirectory.GetChildImages();
                        children.Sort((a, b) => a.Name.CompareTo(b.Name));
                        for (int i = 0; i < f.WzDirectory.CountImages(); i++)
                        {
                            WzImage image = children[i];
                            string name = Path.GetFileNameWithoutExtension(image.Name);
                            if (int.TryParse(name, out int item_id))
                            {
                                if (item_id >= 200 && item_id <= 245) // consumes
                                {
                                    image.ParseImage();
                                    List<WzImageProperty> imgProperties = image.WzProperties;
                                    foreach (WzImageProperty property in imgProperties)
                                    {
                                        AddGridRow(GridUse, property);
                                    }
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }
}
