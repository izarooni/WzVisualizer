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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WzVisualizer.Properties;

namespace WzVisualizer
{

    internal delegate void AddGridRowCallBack(DataGridView grid, BinData binData);

    public partial class MainForm : Form
    {
        private PropertiesViewer viewer = new PropertiesViewer();
        private FolderBrowserDialog FB = new FolderBrowserDialog();
        private WzStringUtility StringUtility = null;
        private WzFile _StringWZ = null;
        private WzFile _ItemWZ = null;
        private WzFile _CharacterWZ = null;
        private WzFile _MapWZ = null;
        private WzFile _MobWZ = null;
        private WzFile _SkillWZ = null;
        private WzFile _NpcWZ = null;

        public MainForm()
        {
            InitializeComponent();

            string[] names = Enum.GetNames(typeof(WzMapleVersion));
            for (int i = 0; i < names.Length; i++)
            {
                if (i == (int) WzMapleVersion.GENERATE) break;
                ComboEncType.Items.Add(names[i]);
            }

            // Obtain the last used WZ root directory
            this.TextWzPath.Text = Settings.Default.PathCache;
            this.MapleVersion.Value = Settings.Default.MapleVersion;
            // Set default values for the ComboBoxes
            this.ComboLoadType.SelectedIndex = 0;
            this.ComboEncType.SelectedIndex = 0;

            #region grid view cell events

            #region cell double clicked
            this.GridEHairs.CellDoubleClick += Grid_CellDoubleClick;
            this.GridEFaces.CellDoubleClick += Grid_CellDoubleClick;
            this.GridEWeapons.CellDoubleClick += Grid_CellDoubleClick;
            this.GridEAccessory.CellDoubleClick += Grid_CellDoubleClick;
            this.GridECaps.CellDoubleClick += Grid_CellDoubleClick;
            this.GridELongcoats.CellDoubleClick += Grid_CellDoubleClick;
            this.GridETops.CellDoubleClick += Grid_CellDoubleClick;
            this.GridEBottoms.CellDoubleClick += Grid_CellDoubleClick;
            this.GridEShoes.CellDoubleClick += Grid_CellDoubleClick;
            this.GridECapes.CellDoubleClick += Grid_CellDoubleClick;
            this.GridEGloves.CellDoubleClick += Grid_CellDoubleClick;
            this.GridERings.CellDoubleClick += Grid_CellDoubleClick;
            this.GridEShields.CellDoubleClick += Grid_CellDoubleClick;
            this.GridETames.CellDoubleClick += Grid_CellDoubleClick;

            this.GridUConsumes.CellDoubleClick += Grid_CellDoubleClick;
            this.GridUScrolls.CellDoubleClick += Grid_CellDoubleClick;
            this.GridUProjectiles.CellDoubleClick += Grid_CellDoubleClick;

            this.GridSChairs.CellDoubleClick += Grid_CellDoubleClick;
            this.GridSOthers.CellDoubleClick += Grid_CellDoubleClick;
            this.GridEtc.CellDoubleClick += Grid_CellDoubleClick;
            this.GridCash.CellDoubleClick += Grid_CellDoubleClick;

            this.GridMaps.CellDoubleClick += Grid_CellDoubleClick;

            this.GridMobs.CellDoubleClick += Grid_CellDoubleClick;

            this.GridSkills.CellDoubleClick += Grid_CellDoubleClick;

            this.GridNPCs.CellDoubleClick += Grid_CellDoubleClick;
            #endregion

            #region state change (cell selection)
            this.GridEHairs.CellStateChanged += Grid_RowStateChanged;
            this.GridEFaces.CellStateChanged += Grid_RowStateChanged;
            this.GridEWeapons.CellStateChanged += Grid_RowStateChanged;
            this.GridEAccessory.CellStateChanged += Grid_RowStateChanged;
            this.GridECaps.CellStateChanged += Grid_RowStateChanged;
            this.GridELongcoats.CellStateChanged += Grid_RowStateChanged;
            this.GridETops.CellStateChanged += Grid_RowStateChanged;
            this.GridEBottoms.CellStateChanged += Grid_RowStateChanged;
            this.GridEShoes.CellStateChanged += Grid_RowStateChanged;
            this.GridECapes.CellStateChanged += Grid_RowStateChanged;
            this.GridEGloves.CellStateChanged += Grid_RowStateChanged;
            this.GridERings.CellStateChanged += Grid_RowStateChanged;
            this.GridEShields.CellStateChanged += Grid_RowStateChanged;
            this.GridETames.CellStateChanged += Grid_RowStateChanged;

            this.GridUConsumes.CellStateChanged += Grid_RowStateChanged;
            this.GridUScrolls.CellStateChanged += Grid_RowStateChanged;
            this.GridUProjectiles.CellStateChanged += Grid_RowStateChanged;

            this.GridSChairs.CellStateChanged += Grid_RowStateChanged;
            this.GridSOthers.CellStateChanged += Grid_RowStateChanged;
            this.GridEtc.CellStateChanged += Grid_RowStateChanged;
            this.GridCash.CellStateChanged += Grid_RowStateChanged;

            this.GridMaps.CellStateChanged += Grid_RowStateChanged;

            this.GridMobs.CellStateChanged += Grid_RowStateChanged;

            this.GridSkills.CellStateChanged += Grid_RowStateChanged;
            
            this.GridNPCs.CellStateChanged += Grid_RowStateChanged;
            #endregion

            #endregion
        }

        #region row append
        private void AddFaceRow(WzImage image)
        {
            string imgName = Path.GetFileNameWithoutExtension(image.Name);
            int ID = int.Parse(imgName);
            WzObject wzObject = image.GetFromPath("blink/0/face");
            WzCanvasProperty icon;
            if (wzObject is WzUOLProperty ufo) icon = (WzCanvasProperty)ufo.LinkValue;
            else icon = (WzCanvasProperty)wzObject;
            string name = StringUtility.GetEqp(ID);
            Bitmap bitmap = null;
            try { bitmap = icon?.GetBitmap(); } catch { }
            GridEFaces.Rows.Add(new object[] { ID, bitmap, name });
        }

        private void AddHairRow(WzImage image)
        {
            string imgName = Path.GetFileNameWithoutExtension(image.Name);
            int ID = int.Parse(imgName);
            WzCanvasProperty icon = (WzCanvasProperty)image.GetFromPath("default/hairOverHead");
            if (icon == null)
            {
                icon = (WzCanvasProperty)image.GetFromPath("default/hair");
            }
            string name = StringUtility.GetEqp(ID);
            GridEHairs.Rows.Add(new object[] { ID, icon?.GetBitmap(), name });
        }

        private void AddGridRow(DataGridView grid, object wzObject)
        {
            int ID;
            string properties = "";
            string name = null;
            WzCanvasProperty icon = null;

            if (wzObject is WzImage image)
            {
                if (image.WzFileParent.Name.StartsWith("Npc"))
                {
                    // NPC icon breadcrumb like: '{ID}/stand/0'
                    // and also sometimes contains a link STRING property instead of using UOL
                    image.ParseImage();
                    string imgName = Path.GetFileNameWithoutExtension(image.Name);
                    properties = BuildProperties(image) ?? "";
                    ID = int.Parse(imgName);
                    name = StringUtility.GetNPC(ID);

                    WzImageProperty linkProperty = image.GetFromPath("info/link");
                    if (linkProperty != null)
                    {
                        string linkName = ((WzStringProperty)linkProperty).Value;
                        image = ((WzDirectory)image.Parent).GetChildImages().Find(p => p.Name.Equals(linkName + ".img"));
                        if (image == null) return;
                    }
                    icon = (WzCanvasProperty)image.GetFromPath("stand/0");
                } else if (image.WzFileParent.Name.StartsWith("Mob"))
                {
                    // Mob icon breadcrumb like: '{ID}/(move|stand)/0'
                    // where the 'move' or 'stand' sub property may not exist
                    image.ParseImage();
                    string imgName = Path.GetFileNameWithoutExtension(image.Name);
                    properties = BuildProperties(image);
                    ID = int.Parse(imgName);
                    name = StringUtility.GetMob(ID);

                    WzStringProperty linkProperty = (WzStringProperty)image.GetFromPath("info/link");
                    if (linkProperty != null)
                    {
                        string linkName = ((WzStringProperty)linkProperty).Value;
                        image = ((WzDirectory)image.Parent).GetChildImages().Find(p => p.Name.Equals(linkName + ".img"));
                        if (image == null) return;
                    }

                    // attempt to get image of the monster
                    WzImageProperty temp = image.GetFromPath("stand/0");
                    if (temp == null) temp = image.GetFromPath("fly/0");
                    if (temp == null) temp = image.GetFromPath("move/0");
                    if (temp is WzUOLProperty uol) icon = (WzCanvasProperty)uol.LinkValue;
                    else icon = (WzCanvasProperty)temp;
                }
                else
                {  // for breadcrumb like: '{ID}.img/info/icon' (character.wz)
                    image.ParseImage();
                    string imgName = Path.GetFileNameWithoutExtension(image.Name);
                    properties = BuildProperties(image);
                    ID = int.Parse(imgName);
                    if (ItemConstants.IsEquip(ID)) name = StringUtility.GetEqp(ID);

                    icon = (WzCanvasProperty)image.GetFromPath("info/icon");
                }
            } else if (wzObject is WzSubProperty subProperty)
            {
                if (subProperty.WzFileParent.Name.StartsWith("Skill"))
                {
                    ID = int.Parse(subProperty.Name);
                    name = StringUtility.GetSkill(subProperty.Name);

                    icon = (WzCanvasProperty)subProperty.GetFromPath("icon");
                }
                else
                { // for breadcrumb like: 'category.img/{ID}/info/icon' (etc.wz)
                    string imgName = subProperty.Name;
                    properties = BuildProperties(subProperty);
                    ID = int.Parse(imgName);
                    if (ItemConstants.IsEtc(ID)) name = StringUtility.GetEtc(ID);
                    else if (ItemConstants.IsCash(ID)) name = StringUtility.GetCash(ID);
                    else if (ItemConstants.IsChair(ID)) name = StringUtility.GetChair(ID);
                    else if (ItemConstants.IsConsume(ID)) name = StringUtility.GetConsume(ID);

                    WzImageProperty imgIcon = subProperty.GetFromPath("info/icon");
                    if (imgIcon is WzUOLProperty ufo) imgIcon = (WzCanvasProperty)ufo.LinkValue;
                    else if (imgIcon is WzCanvasProperty canvas) imgIcon = canvas;
                    if (imgIcon != null) icon = (WzCanvasProperty)imgIcon;
                }
            } else
                return;
            Bitmap bitmap = null;
            try { bitmap = icon?.GetBitmap(); } catch (Exception) { }
            grid.Rows.Add(new object[] { ID, bitmap, name, properties });
        }
        #endregion

        /// <summary>
        /// Concatenate all properties excluding image and sound properties
        /// </summary>
        /// <param name="wzObject">a WzSubProperty or WzImage</param>
        /// <returns></returns>
        private string BuildProperties(object wzObject)
        {
            string properties = "";
            WzImageProperty infoRoot = null;
            if (wzObject is WzSubProperty subProperty) infoRoot = subProperty.GetFromPath("info");
            else if (wzObject is WzImage wzImage) infoRoot = wzImage.GetFromPath("info");
            if (infoRoot != null && infoRoot.WzProperties != null)
            {
                foreach (WzImageProperty imgProperties in infoRoot.WzProperties)
                {
                    switch (imgProperties.PropertyType)
                    {
                        default:
                            properties += string.Format("\r\n{0}={1}", imgProperties.Name, imgProperties.WzValue);
                            break;
                        case WzPropertyType.Canvas:
                        case WzPropertyType.PNG:
                        case WzPropertyType.Sound:
                            break;
                    }
                }

            }
            return properties;
        }

        private void LoadWzData(WzMapleVersion mapleVersion, string mapleDirectory)
        {
            int selected_root = TabControlMain.SelectedIndex; 
            switch (selected_root)
            {
                default:
                    Console.WriteLine("Unable to load WZ data unhandled selected index: " + TabControlMain.SelectedIndex);
                    break;
                case 0: // Equips
                    {
                        LoadWzFileIfAbsent(ref _CharacterWZ, mapleDirectory + "/Character", mapleVersion);
                        DataGridView dGrid = (DataGridView)TabEquips.SelectedTab.Controls[0];
                        dGrid.Rows.Clear();
                        List<WzImage> children = _CharacterWZ.WzDirectory.GetChildImages();
                        children.Sort((a, b) => a.Name.CompareTo(b.Name));
                        for (int i = 0; i < _CharacterWZ.WzDirectory.CountImages(); i++)
                        {
                            WzImage image = children[i];
                            string name = Path.GetFileNameWithoutExtension(image.Name);
                            if (int.TryParse(name, out int equip_id))
                            {
                                int selectedTab = TabEquips.SelectedIndex;
                                int bodyPart = equip_id / 10000;
                                switch (bodyPart)
                                {
                                    default:
                                        if (selectedTab == 2 && bodyPart >= 130 && bodyPart <= 170) AddGridRow(GridEWeapons, image);
                                        else if (selectedTab == 1 && bodyPart == 2) AddFaceRow(image);
                                        else if (selectedTab == 0 && bodyPart == 3) AddHairRow(image); 
                                        break;
                                    case 100: // Caps
                                        if (selectedTab == 4) AddGridRow(GridECaps, image);
                                        break;
                                    case 101:
                                    case 102:
                                    case 103:
                                    case 112:
                                    case 113:
                                    case 114: // Accessory
                                        if (selectedTab == 3) AddGridRow(GridEAccessory, image);
                                        break;
                                    case 110: // Cape
                                        if (selectedTab == 9) AddGridRow(GridECapes, image);
                                        break;
                                    case 104: // Coat
                                        if (selectedTab == 6) AddGridRow(GridETops, image);
                                        break;
                                    case 108: // Glove
                                        if (selectedTab == 10) AddGridRow(GridEGloves, image);
                                        break;
                                    case 105: // Longcoat
                                        if (selectedTab == 5) AddGridRow(GridELongcoats, image);
                                        break;
                                    case 106: // Pants
                                        if (selectedTab == 7) AddGridRow(GridEBottoms, image);
                                        break;
                                    case 180:
                                    case 181:
                                    case 182:
                                    case 183: // Pet Equips
                                        // image.ParseImage();
                                        break;
                                    case 111: // Rings
                                        if (selectedTab == 11) AddGridRow(GridERings, image);
                                        break;
                                    case 109: // Shield
                                        if (selectedTab == 12) AddGridRow(GridEShields, image);
                                        break;
                                    case 107: // Shoes
                                        if (selectedTab == 8) AddGridRow(GridEShoes, image);
                                        break;
                                    case 190:
                                    case 191:
                                    case 193: // Taming Mob
                                        if (selectedTab == 13) AddGridRow(GridETames, image);
                                        break;
                                }
                            }
                        }
                        _CharacterWZ.Dispose();
                        _CharacterWZ = null;
                        break;
                    }
                case 1: // Use
                case 2: // Setup
                case 3: // Etc
                case 4: // Cash
                    {
                        LoadWzFileIfAbsent(ref _ItemWZ, mapleDirectory + "/Item", mapleVersion);
                        if (selected_root == 1)
                            ((DataGridView)TabUse.SelectedTab.Controls[0]).Rows.Clear();
                        else if (selected_root == 2)
                            ((DataGridView)TabSetup.SelectedTab.Controls[0]).Rows.Clear();
                        else if (selected_root == 3)
                            ((DataGridView)TabEtcPage.Controls[0]).Rows.Clear();
                        else if (selected_root == 4)
                            ((DataGridView)TabCashPage.Controls[0]).Rows.Clear();

                        List<WzImage> children = _ItemWZ.WzDirectory.GetChildImages();
                        children.Sort((a, b) => a.Name.CompareTo(b.Name));
                        for (int i = 0; i < _ItemWZ.WzDirectory.CountImages(); i++)
                        {
                            WzImage image = children[i];
                            string name = Path.GetFileNameWithoutExtension(image.Name);
                            if (int.TryParse(name, out int item_id))
                            {
                                switch (item_id)
                                {
                                    default:
                                        image.ParseImage();
                                        if (selected_root == 3 && ItemConstants.IsEtc(item_id)) // etc
                                            image.WzProperties.ForEach(img => AddGridRow(GridEtc, img));
                                        if (selected_root == 4 && ItemConstants.IsCash(item_id)) // cash
                                            image.WzProperties.ForEach(img => AddGridRow(GridCash, img));
                                        if (selected_root == 1 && ItemConstants.IsConsume(item_id)) // consume
                                            image.WzProperties.ForEach(img => AddGridRow(GridUConsumes, img));
                                        break;
                                    case 204: // scrolls
                                        if (selected_root == 1)
                                            image.WzProperties.ForEach(img => AddGridRow(GridUScrolls, img));
                                        break;
                                    case 206:
                                    case 207:
                                    case 233: // projectiles
                                        if (selected_root == 1)
                                            image.WzProperties.ForEach(img => AddGridRow(GridUProjectiles, img));
                                        break;
                                    case 301: // chairs
                                    case 399: // x-mas characters
                                        if (selected_root == 2)
                                            image.WzProperties.ForEach(img => AddGridRow(item_id == 301 ? GridSChairs : GridSOthers, img));
                                        break;
                                }
                            }
                        }
                        break;
                    }
                case 5: // Map
                    {
                        LoadWzFileIfAbsent(ref _MapWZ, mapleDirectory + "/Map", mapleVersion);
                        GridMaps.Rows.Clear();

                        List<WzImage> children = _MapWZ.WzDirectory.GetChildImages();
                        children.Sort((a, b) => a.Name.CompareTo(b.Name));
                        for (int i = 0; i < _MapWZ.WzDirectory.CountImages(); i++)
                        {
                            WzImage image = children[i];
                            string str_map_id = Path.GetFileNameWithoutExtension(image.Name);
                            if (int.TryParse(str_map_id, out int map_id))
                            {
                                image.ParseImage();
                                string properties = BuildProperties(image);
                                WzCanvasProperty icon = (WzCanvasProperty)image.GetFromPath("miniMap/canvas");
                                string name = StringUtility.GetFieldFullName(map_id);

                                GridMaps.Rows.Add(new object[] { map_id, icon?.GetBitmap(), name, properties });
                            }
                        }
                        break;
                    }
                case 6: // Mob
                    {
                        LoadWzFileIfAbsent(ref _MobWZ, mapleDirectory + "/Mob", mapleVersion);
                        GridMobs.Rows.Clear();

                        List<WzImage> children = _MobWZ.WzDirectory.GetChildImages();
                        children.Sort((a, b) => a.Name.CompareTo(b.Name));
                        for (int i = 0; i < _MobWZ.WzDirectory.CountImages(); i++)
                        {
                            WzImage image = children[i];
                            AddGridRow(GridMobs, image);
                        }
                        _MobWZ.Dispose();
                        _MobWZ = null;
                        break;
                    }
                case 7: // Skills
                    {
                        LoadWzFileIfAbsent(ref _SkillWZ, mapleDirectory + "/Skill", mapleVersion);
                        GridSkills.Rows.Clear();

                        List<WzImage> children = _SkillWZ.WzDirectory.GetChildImages();
                        children.Sort((a, b) => a.Name.CompareTo(b.Name));
                        for (int i = 0; i < _SkillWZ.WzDirectory.CountImages(); i++)
                        {
                            WzImage image = children[i];
                            string name = Path.GetFileNameWithoutExtension(image.Name);
                            if (int.TryParse(name, out int job_id))
                            {
                                WzImageProperty tree = image.GetFromPath("skill");
                                if (tree is WzSubProperty sub)
                                {
                                    List<WzImageProperty> skills = tree.WzProperties;
                                    skills.ForEach(s => AddGridRow(GridSkills, s));
                                    skills.Clear();
                                }
                            }
                        }
                        break;
                    }
                case 8: // NPCs
                    {
                        LoadWzFileIfAbsent(ref _NpcWZ, mapleDirectory + "/Npc", mapleVersion);                        
                        GridNPCs.Rows.Clear();

                        List<WzImage> children = _NpcWZ.WzDirectory.GetChildImages();
                        children.Sort((a, b) => a.Name.CompareTo(b.Name));
                        for (int i = 0; i < _NpcWZ.WzDirectory.CountImages(); i++)
                        {
                            WzImage image = children[i];
                            AddGridRow(GridNPCs, image);
                        }
                        break;
                    }
            }
        }

        private void LoadWzFileIfAbsent(ref WzFile wzFile, string fileName, WzMapleVersion mapleVersion)
        {
            if (wzFile != null) return;
            if (File.Exists(fileName + ".wz"))
            {
                wzFile = new WzFile(fileName + ".wz", (short) MapleVersion.Value, mapleVersion);
                wzFile.ParseWzFile();
            } else
            { // KMS
                wzFile = new WzFile(fileName, mapleVersion);
                WzDirectory dir = new WzDirectory(fileName, wzFile);
                wzFile.WzDirectory = dir;
                RecursivelyLoadDirectory(dir, fileName, mapleVersion);
            }
        }

        private void RecursivelyLoadDirectory(WzDirectory dir, string directoryPath, WzMapleVersion mapleVersion)
        {
            if (!Directory.Exists(directoryPath)) return;
            string[] files = Directory.GetFiles(directoryPath);
            foreach (string file in files)
            {
                FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
                WzImage img = new WzImage(Path.GetFileName(file), stream, mapleVersion);
                dir.AddImage(img);
            }
            files = Directory.GetDirectories(directoryPath);
            foreach (string sub in files)
            {
                WzDirectory subDir = new WzDirectory(Path.GetFileNameWithoutExtension(sub));
                RecursivelyLoadDirectory(subDir, sub, mapleVersion);
                dir.AddDirectory(subDir);
            }
        }

        /// <summary>
        /// clears all collections, closes underlying file readers 
        /// then calls the garbage collector for each loaded WZ file
        /// </summary>
        private void DisposeWzFiles()
        {
            _StringWZ?.Dispose();
            _ItemWZ?.Dispose();
            _CharacterWZ?.Dispose();
            _MapWZ?.Dispose();
            _MobWZ?.Dispose();
            _SkillWZ?.Dispose();
            _NpcWZ?.Dispose();

            _StringWZ = null;
            _ItemWZ = null;
            _CharacterWZ = null;
            _MapWZ = null;
            _MobWZ = null;
            _SkillWZ = null;
            _NpcWZ = null;
        }

        /// <summary>
        /// Add a row data to the specified grid view using 
        /// parsed bin data
        /// </summary>
        /// <param name="grid">the grid view to add a row to</param>
        /// <param name="binData">bin data (wz files that were parsed then saved as a bin file type)</param>
        public void AddGridRow(DataGridView grid, BinData binData)
        {
            string allProperties = "";
            foreach (string prop in binData.properties)
                allProperties += prop + "\r\n";

            string filter = SearchTextBox.Text;
            if (filter?.Length > 0 && !binData.Search(filter))
                return;

            if (!IsDisposed && InvokeRequired)
            {
                Image image = binData?.image;
                Invoke(new Action(() => {
                    grid.Rows.Add(new object[] { binData.ID, image, binData.Name, allProperties });
                }));
            }
        }

        #region event handling

        /// <summary>
        /// Update the Window's clipboard when a cell is selected
        /// </summary>
        private void Grid_RowStateChanged(object sender, DataGridViewCellStateChangedEventArgs e)
        {
            object cellValue = e.Cell.Value;
            if (cellValue == null) return;
            if (cellValue is int)
                Clipboard.SetText(((int)cellValue).ToString());
            else if (cellValue is string str && str.Length > 0)
                Clipboard.SetText(str);
            else
                Clipboard.Clear();
        }

        /// <summary>
        /// Display the PropertiesViewer Form when a Properties column cell is double clicked
        /// </summary>
        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3 && e.RowIndex != -1)
            {
                viewer.SetProperties((string)((DataGridView)sender).SelectedCells[0].Value);
                viewer.Show();
            }
        }

        /// <summary>
        /// Open the FolderBrowser dialog window when the text box is clicked and set the selected
        /// directory as the root folder containing WZ files
        /// </summary>
        private void TextWzPath_Click(object sender, EventArgs e)
        {
            if (FB.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(FB.SelectedPath))
            {
                string[] files = Directory.GetFiles(FB.SelectedPath);
                TextWzPath.Text = FB.SelectedPath;
            }
        }

        /// <summary>
        /// Begin loading WZ data corresponding to the selected tab
        /// </summary>
        private void BtnWzLoad_Click(object sender, EventArgs e)
        {
            string folderPath = TextWzPath.Text;
            if (folderPath.Length > 0)
            {
                if (!folderPath.Equals(Settings.Default.PathCache))
                {
                    Settings.Default.MapleVersion = MapleVersion.Value;
                    Settings.Default.PathCache = folderPath;
                    Settings.Default.Save();
                }
                WzMapleVersion mapleVersion = (WzMapleVersion)ComboEncType.SelectedIndex;
                string stringWzPath = folderPath + @"/String";
                if (File.Exists(stringWzPath + ".wz"))
                {
                    _StringWZ = new WzFile(stringWzPath + ".wz", mapleVersion);
                    _StringWZ.ParseWzFile();
                }
                else if (Directory.Exists(stringWzPath))
                { // KMS
                    _StringWZ = new WzFile(stringWzPath, mapleVersion);
                    WzDirectory dir = new WzDirectory("String", _StringWZ);
                    _StringWZ.WzDirectory = dir;
                    RecursivelyLoadDirectory(dir, stringWzPath, mapleVersion);
                }
                else
                {
                    MessageBox.Show("Failed to load String data in the specified directory", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    DisposeWzFiles();
                    return;
                }
                StringUtility = new WzStringUtility(_StringWZ);
                LoadWzData(mapleVersion, folderPath);
            }
            DisposeWzFiles();
        }

        /// <summary>
        /// Enable or disable the WZ path controls if BIN loading is selected
        /// </summary>
        private void ComboLoadType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = (ComboLoadType.SelectedIndex == 1);
            TextWzPath.Enabled = enabled;
            BtnWzLoad.Enabled = enabled;
        }

        /// <summary>
        /// upon clicking the save button, store data of the current opened grid.
        /// Some tabs may have another TabControl in which that Control contains a Grid control.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs ev)
        {
            MouseEventArgs e = (MouseEventArgs)ev;
            switch (e.Button) {
                case MouseButtons.Left:
                    {
                        var control = TabControlMain.SelectedTab.Controls[0];
                        if (control is DataGridView grid) // no child tabs and contains 1 child Control (DataGridView)
                            GridIOUtility.ExportGrid(grid, TabControlMain.SelectedTab.Text);
                        else if (control is TabControl tab) // sub-categories (e.g. Equips.Hairs, Equips.Faces)
                        {
                            control = tab.SelectedTab; // The selected child Tab (e.g. Equips.Hairs)
                            GridIOUtility.ExportGrid((DataGridView)control.Controls[0], TabControlMain.SelectedTab.Text); // The DataGridView contained in the TabPage control
                        }
                        MessageBox.Show("Bin save complete!");
                        break;
                    }
                case MouseButtons.Right:
                    {
                        var control = TabControlMain.SelectedTab.Controls[0];
                        if (control is DataGridView grid) // no child tabs and contains 1 child Control (DataGridView)
                            GridIOUtility.ExportGridImages(grid, TabControlMain.SelectedTab.Text);
                        else if (control is TabControl tab) // sub-categories (e.g. Equips.Hairs, Equips.Faces)
                        {
                            control = tab.SelectedTab; // The selected child Tab (e.g. Equips.Hairs)
                            GridIOUtility.ExportGridImages((DataGridView)control.Controls[0], TabControlMain.SelectedTab.Text); // The DataGridView contained in the TabPage control
                        }
                        MessageBox.Show("Image save complete!");
                        break;
                    }
            }
        }

        #region tab change events

        private void TabEquips_Selected(object sender, TabControlEventArgs e)
        {
            Tab_Selected(sender, e.TabPage);
        }

        private void TabUse_Selected(object sender, TabControlEventArgs e)
        {
            Tab_Selected(sender, e.TabPage);
        }

        private void TabSetup_Selected(object sender, TabControlEventArgs e)
        {
            Tab_Selected(sender, e.TabPage);
        }

        private void TabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            Tab_Selected(sender, GetSelectedTab());
        }

        private void Tab_Selected(object sender, TabPage tab)
        {
            FieldInfo[] fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (field.Name.StartsWith("Grid") && field.GetValue(this) is DataGridView grid && grid.Rows.Count > 0)
                {
                    grid.Rows.Clear();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                if (field.Name.Equals(tab.Name))
                {
                    int mainSelectedIndex = TabControlMain.SelectedIndex;
                    GridIOUtility.ImportGrid(string.Format("{0}/{1}.bin", TabControlMain.SelectedTab.Text, tab.Text), (DataGridView)tab.Controls[0], AddGridRow);
                }
            }
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            GridIOUtility.ImportGrid("equips/Hairs.bin", GridEHairs, AddGridRow);
        }

        private void SearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            int keyCode = (int)e.KeyChar;
            if (keyCode == (int)Keys.Enter)
                Tab_Selected(sender, GetSelectedTab());
        }
        #endregion

        private TabPage GetSelectedTab()
        {
            TabPage root = TabControlMain.SelectedTab;
            object control = root.Controls[0];
            return (control is TabControl tab) ? tab.SelectedTab : root;
        }
    }
}
