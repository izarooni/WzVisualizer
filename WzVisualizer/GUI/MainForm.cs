using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using MapleLib.WzLib.WzProperties;
using WzVisualizer.GUI.Controls;
using WzVisualizer.IO;
using WzVisualizer.Properties;
using WzVisualizer.Util;

namespace WzVisualizer.GUI {
    internal delegate void AddGridRowCallBack(DataGridView grid, BinData binData);

    public partial class MainForm : Form {
        private readonly FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        private readonly PropertiesViewer viewer = new PropertiesViewer();
        private bool loadAll = false;

        public MainForm() {
            InitializeComponent();

            string[] names = Enum.GetNames(typeof(WzMapleVersion));
            ComboEncType.Items.Add(new ComboBoxItem("AUTO-DETECT", null));
            for (var i = 0; i < names.Length; i++) {
                var v = (WzMapleVersion)i;
                ComboEncType.Items.Add(new ComboBoxItem(names[i], v));
            }

            // Obtain the last used WZ root directory
            TextWzPath.Text = Settings.Default.PathCache;
            // Set default values for the ComboBoxes
            ComboLoadType.SelectedIndex = 0;
            ComboEncType.SelectedIndex = 0;

            TabControlMain.Selected += TabControl_Selected;
            foreach (var ctrl in TabControlMain.Controls) {
                if (ctrl is TabPage page) {
                    AddEventHandlers(page);
                }
            }
        }

        /// <summary>
        /// recursively add event handlers to all DataViewport components
        /// </summary>
        private void AddEventHandlers(TabPage page) {
            foreach (var c in page.Controls) {
                switch (c) {
                    case DataViewport view: {
                        view.Tag = new List<BinData>();
                        view.GridView.CellDoubleClick += Grid_CellDoubleClick;
                        view.GridView.CellStateChanged += Grid_RowStateChanged;
                        break;
                    }
                    case TabControl ctrl: {
                        ctrl.Selected += TabControl_Selected;
                        foreach (TabPage childPage in ctrl.TabPages) {
                            AddEventHandlers(childPage);
                        }

                        break;
                    }
                }
            }
        }

        private DataViewport GetCurrentDataViewport() {
            var main = TabControlMain.SelectedTab;
            var sub = (main.Controls[0] is TabControl tc ? tc.SelectedTab : main);
            return (DataViewport)sub.Controls[0];
        }

        /// <summary>
        /// Concatenate all properties excluding image and sound properties
        /// </summary>
        /// <param name="obj">a WzSubProperty or WzImage</param>
        /// <returns></returns>
        private string GetAllProperties(object obj) {
            WzImageProperty root;
            switch (obj) {
                default:
                    throw new Exception($"unhandled parameter type '{nameof(obj)}': {obj}");
                case WzSubProperty sub:
                    root = sub.GetFromPath("info")       // typical node
                           ?? sub.GetFromPath("level")   // skills
                           ?? sub.GetFromPath("common"); // skills with scaling properties 
                    break;
                case WzImage img:
                    root = img.GetFromPath("info");
                    break;
            }

            return AppendProperties(root, "");
        }

        private string AppendProperties(WzImageProperty parent, string prefix) {
            if (parent?.WzProperties == null) return "";
            string properties = "";
            foreach (WzImageProperty sub in parent.WzProperties) {
                switch (sub.PropertyType) {
                    default:
                        // append all properties
                        properties += $"\r\n{prefix}{sub.Name}={sub.WzValue}";
                        if (sub.PropertyType == WzPropertyType.SubProperty) {
                            // increase sub-properties indent by 1 tab character
                            properties += AppendProperties(sub, "\t" + prefix);
                        }

                        break;
                    case WzPropertyType.Canvas:
                    case WzPropertyType.PNG:
                    case WzPropertyType.Sound:
                        break;
                }
            }

            return properties;
        }

        private void LoadWzData(int selectedRoot, WzMapleVersion encryption, string directory) {
            if (selectedRoot == -1) {
                var result = MessageBox.Show("You have opted to load data for every tab. Are you sure you want to do this?", "Warning", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) return;
                loadAll = true;
                for (int i = 0; i <= 10; i++) {
                    try {
                        TabControlMain.SelectedIndex = i;
                        LoadWzData(i, encryption, directory);
                    } catch (Exception e) {
                        MessageBox.Show($"Could not load tab {i}: {e.Message}");
                    }
                }

                BtnSave_Click(null, new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                return;
            }

            switch (selectedRoot) {
                case 0: // Equips
                {
                    // var selectedTab = EquipTab.SelectedIndex;
                    var file = Wz.Character.LoadFile($@"{directory}\Character", encryption, false);
                    List<WzImage> children = file.WzDirectory.GetChildImages();
                    children.Sort((a, b) => a.Name.CompareTo(b.Name));
                    foreach (var img in children) {
                        string sId = Path.GetFileNameWithoutExtension(img.Name);
                        if (int.TryParse(sId, out int itemId)) {
                            var name = StringWz.GetEqp(itemId);
                            var properties = GetAllProperties(img);
                            var icon = img.GetFromPath("info/icon");
                            Bitmap image = null;

                            DataViewport dv;
                            int bodyPart = itemId / 10000;
                            switch (bodyPart) {
                                default: continue;
                                case int n when (n == 3 || n == 4): {
                                    image = (img.GetFromPath("default/hairOverHead") ?? img.GetFromPath("default/hair"))?.GetBitmap();
                                    var hairBelowBody = (img.GetFromPath("default/hairBelowBody") as WzCanvasProperty)?.GetBitmap();
                                    if (image != null && hairBelowBody != null) {
                                        var merge = new Bitmap(Math.Max(image.Width, hairBelowBody.Width), Math.Max(image.Height, hairBelowBody.Height));
                                        using (var g = Graphics.FromImage(merge)) {
                                            g.DrawImage(hairBelowBody, Point.Empty);
                                            g.DrawImage(image, Point.Empty);
                                        }

                                        image = merge;
                                    }

                                    dv = EquipHairsView;
                                    break;
                                }
                                case int n when (n == 2 || n == 5):
                                    image = img.GetFromPath("blink/0/face")?.GetBitmap();
                                    dv = EquipFacesView;
                                    break;
                                case int n when (n >= 130 && n <= 170):
                                    dv = EquipWeaponsView;
                                    break;
                                case int n when ((n >= 101 && n <= 103) || (n >= 112 && n <= 114)):
                                    dv = EquipAccessoryView;
                                    break;
                                case 100:
                                    dv = EquipCapsView;
                                    break;
                                case 105:
                                    dv = EquipsOverallsView;
                                    break;
                                case 104:
                                    dv = EquipTopsView;
                                    break;
                                case 106:
                                    dv = EquipPantsView;
                                    break;
                                case 107:
                                    dv = EquipShoesView;
                                    break;
                                case 110:
                                    dv = EquipCapesView;
                                    break;
                                case 108:
                                    dv = EquipGlovesView;
                                    break;
                                case 111:
                                    dv = EquipRingsView;
                                    break;
                                case 109:
                                    dv = EquipShieldsView;
                                    break;
                                case int n when (n is 190 or 191 || n == 193):
                                    dv = EquipMountsView;
                                    break;
                            }

                            if (dv == null) continue;
                            image ??= icon?.GetBitmap();
                            ((List<BinData>) dv.Tag)?.Add(new BinData(itemId, image, name, properties));
                            dv.GridView.Rows.Add(itemId, image, name, properties);
                        }
                    }

                    Wz.Character.Dispose();

                    break;
                }
                case 1: // Use
                case 2: // Setup
                case 3: // Etc
                case 4: // Cash
                case 9: // Pets
                {
                    var file = Wz.Item.LoadFile($@"{directory}\Item", encryption, false);
                    var children = file.WzDirectory.GetChildImages();
                    children.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));

                    void AddRow(WzImage wz, DataViewport dv) =>
                        wz.WzProperties.ForEach(imgs => AddGridRow(dv.GridView, imgs));

                    foreach (var img in children) {
                        string name = Path.GetFileNameWithoutExtension(img.Name);
                        if (int.TryParse(name, out int itemId)) {
                            switch (selectedRoot) {
                                case 1:
                                    switch (itemId) {
                                        default:
                                            if (ItemConstants.IsConsume(itemId)) AddRow(img, UseConsumeView);
                                            break;
                                        case 204:
                                        case 234:
                                            AddRow(img, UseScrollsView);
                                            break;
                                        case 206:
                                        case 207:
                                        case 233:
                                            AddRow(img, UseProjectileView);
                                            break;
                                    }

                                    break;
                                case 2 when itemId == 301 || itemId == 399:
                                    AddRow(img, (itemId == 301 ? SetupChairsView : SetupOthersView));
                                    break;
                                case 3 when ItemConstants.IsEtc(itemId):
                                    AddRow(img, EtcView);
                                    break;
                                case 4 when ItemConstants.IsCash(itemId):
                                    AddRow(img, CashView);
                                    break;
                                case 9 when ItemConstants.IsPet(itemId):
                                    AddGridRow(PetsView.GridView, img);
                                    break;
                            }
                        }
                    }

                    Wz.Item.Dispose();

                    break;
                }
                case 5: {
                    // Map
                    var file = Wz.Map.LoadFile($@"{directory}\Map", encryption, false);

                    var children = file.WzDirectory.GetChildImages();
                    children.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    foreach (var img in children) {
                        string sMapId = Path.GetFileNameWithoutExtension(img.Name);
                        if (!int.TryParse(sMapId, out int mapId)) continue;

                        img.ParseImage();
                        string properties = GetAllProperties(img);
                        WzCanvasProperty icon = (WzCanvasProperty)img.GetFromPath("miniMap/canvas");
                        string name = StringWz.GetFieldFullName(mapId);

                        MapsView.GridView.Rows.Add(mapId, icon?.GetBitmap(), name, properties);
                    }

                    Wz.Map.Dispose();

                    break;
                }
                case 6: {
                    // Mob
                    var file = Wz.Mob.LoadFile($@"{directory}\Mob", encryption, false);
                    var children = file.WzDirectory.GetChildImages();
                    children.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    foreach (var img in children) {
                        AddGridRow(MobsView.GridView, img);
                    }

                    Wz.Mob.Dispose();

                    break;
                }
                case 7: {
                    // Skills
                    var file = Wz.Skill.LoadFile($@"{directory}\Skill", encryption, false);
                    var children = file.WzDirectory.GetChildImages();
                    foreach (var img in children) {
                        string name = Path.GetFileNameWithoutExtension(img.Name);
                        if (!int.TryParse(name, out _)) continue;
                        WzImageProperty tree = img.GetFromPath("skill");

                        if (!(tree is WzSubProperty)) continue;
                        List<WzImageProperty> skills = tree.WzProperties;
                        skills.ForEach(s => AddSkillRow(s));
                    }

                    Wz.Skill.Dispose();

                    break;
                }
                case 8: {
                    // NPCs
                    var file = Wz.Npc.LoadFile($@"{directory}\Npc", encryption, false);
                    var children = file.WzDirectory.GetChildImages();
                    children.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    foreach (var img in children) {
                        AddGridRow(NPCView.GridView, img);
                    }

                    Wz.Npc.Dispose();

                    break;
                }
                case 10: {
                    // Reactors
                    var file = Wz.Reactor.LoadFile($@"{directory}\Reactor", encryption, false);
                    var children = file.WzDirectory.GetChildImages();
                    children.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));
                    foreach (var img in children) {
                        AddGridRow(ReactorView.GridView, img);
                    }

                    Wz.Reactor.Dispose();

                    break;
                }
            }
        }

        /// <summary>
        /// clears all collections, closes underlying file readers 
        /// then calls the garbage collector for each loaded WZ file
        /// </summary>
        private static void DisposeWzFiles() {
            var wzs = Enum.GetValues(typeof(Wz)).Cast<Wz>();
            foreach (var wz in wzs) {
                wz.Dispose();
            }
        }

        /// <summary>
        /// Add a row data to the specified grid view using parsed bin data
        /// </summary>
        /// <param name="grid">the grid view to add a row to</param>
        /// <param name="bin">bin data (wz files that were parsed then saved as a bin file type)</param>
        public void AddGridRow(DataGridView grid, BinData bin) {
            string queries = SearchTextBox.Text;
            if (!string.IsNullOrEmpty(queries)) {
                string[] lines = queries.Split(new[] { " " }, StringSplitOptions.None);
                foreach (var query in lines) {
                    // all queries must match
                    if (!bin.Search(query)) return;
                }
            }

            var properties = "";
            foreach (string p in bin.Properties) {
                properties += p + "\r\n";
            }

            grid.Rows.Add(bin.ID, bin.Image, bin.Name, properties);
        }

        #region row append

        private void AddSkillRow(WzImageProperty s) {
            var id = int.Parse(s.Name);
            var name = StringWz.GetSkill(s.Name);
            var properties = GetAllProperties(s);
            WzObject icon = s.GetFromPath("icon");
            SkillsView.GridView.Rows.Add(id, icon?.GetBitmap(), name, properties);
        }

        private void AddGridRow(DataGridView grid, object wzObject) {
            int id;
            string name = null;
            WzObject png;
            string properties = GetAllProperties(wzObject);

            if (wzObject is WzImage img) {
                id = int.Parse(Path.GetFileNameWithoutExtension(img.Name));
                WzImageProperty link = img.GetFromPath("info/link");
                if (link != null) {
                    string linkName = ((WzStringProperty)link).Value;
                    img = ((WzDirectory)img.Parent).GetChildImages().Find(p => p.Name.Equals(linkName + ".img"));
                    if (img == null) return;
                }

                png = img.GetFromPath("stand/0");
                if (img.WzFileParent.Name.StartsWith("Npc")) {
                    // icon path like: '{ID}/stand/0'
                    name = StringWz.GetNpc(id);
                } else if (img.WzFileParent.Name.StartsWith("Mob")) {
                    // icon path like: '{ID}/(move|stand|fly)/0'
                    name = StringWz.GetMob(id);
                    if (png == null) {
                        png = img.GetFromPath("fly/0") ??
                              img.GetFromPath("move/0"); // attempt to get image of the monster
                    }
                } else if (img.WzFileParent.Name.StartsWith("Reactor")) {
                    name = img.GetFromPath("action")?.GetString();
                    png = img.GetFromPath("0/0");
                } else {
                    // for breadcrumb like: '{ID}.img/info/icon'
                    if (ItemConstants.IsEquip(id)) name = StringWz.GetEqp(id);
                    else if (ItemConstants.IsPet(id)) name = StringWz.GetPet(id);
                    png = img.GetFromPath("info/icon");
                }
            } else if (wzObject is WzSubProperty subProperty) {
                // for path like: 'category.img/{ID}/info/icon' (Etc.wz)
                id = int.Parse(subProperty.Name);
                if (ItemConstants.IsEtc(id)) name = StringWz.GetEtc(id);
                else if (ItemConstants.IsCash(id)) name = StringWz.GetCash(id);
                else if (ItemConstants.IsChair(id)) name = StringWz.GetChair(id);
                else if (ItemConstants.IsConsume(id)) name = StringWz.GetConsume(id);

                png = subProperty.GetFromPath("info/icon");
            } else
                return;

            if (grid.Parent is DataViewport dv) {
                ((List<BinData>)dv.Tag)?.Add(new BinData(id, png?.GetBitmap(), name, properties));
            }
            grid.Rows.Add(id, png?.GetBitmap(), name, properties);
        }

        #endregion

        /// <summary>
        /// Begin loading WZ data corresponding to the selected tab
        /// </summary>
        private void BtnWzLoad_Click(object sender, EventArgs e) {
            ClearAllPages(TabControlMain);
            DisposeWzFiles();

            var wzFolderPath = TextWzPath.Text;
            var wzStringPath = wzFolderPath + @"\String";
            if (string.IsNullOrEmpty(wzFolderPath)) {
                MessageBox.Show(Resources.Error, Resources.FileNotFound, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!wzFolderPath.Equals(Settings.Default.PathCache)) {
                Settings.Default.PathCache = wzFolderPath;
                Settings.Default.Save();
            }

            var item = (ComboBoxItem)ComboEncType.Items[ComboEncType.SelectedIndex];
            WzMapleVersion encryption;
            if (item.Value == null) {
                // auto detect
                if (File.Exists(wzStringPath + Resources.FileExtensionWZ)) {
                    encryption = WzTool.DetectMapleVersion(wzStringPath += Resources.FileExtensionWZ, out _);

                    short? gameVersion = null;
                    if (WzTool.GetDecryptionSuccessRate(wzStringPath, encryption, ref gameVersion) < 0.8) {
                        MessageBox.Show(Resources.BadEncryption, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                } else if (Directory.Exists(wzStringPath)) {
                    encryption = (WzMapleVersion)WzTool.DetectMapleVersion(wzStringPath, "Cash.img");
                } else {
                    MessageBox.Show(Resources.MissingStringFile, Resources.FileNotFound, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (Wz.String.LoadFile(wzStringPath, encryption) == null) {
                    MessageBox.Show(Resources.MissingStringFile, Resources.FileNotFound, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            } else {
                // manual selection
                encryption = (WzMapleVersion)item.Value;
            }

            loadAll = false;
            var selectedIndex = (ModifierKeys == Keys.Shift) ? -1 : TabControlMain.SelectedIndex;
            LoadWzData(selectedIndex, encryption, wzFolderPath);
        }

        /// <summary>
        /// upon clicking the save button, store data of the current opened grid.
        /// Some tabs may have another TabControl in which that Control contains a Grid control.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs ev) {
            var main = TabControlMain.SelectedTab;

            switch (((MouseEventArgs)ev).Button) {
                case MouseButtons.Left: {
                    if (loadAll || ModifierKeys == Keys.Shift) {
                        ForEachPage(TabControlMain,
                            (page, text) => BinaryDataUtil.ExportBinary(page, text));
                    } else {
                        BinaryDataUtil.ExportBinary(main, main.Text);
                    }

                    if (!loadAll) MessageBox.Show(Resources.CompleteSaveBIN);
                    break;
                }
                case MouseButtons.Right: {
                    if (ModifierKeys == Keys.Shift) {
                        ForEachPage(TabControlMain, (page, text) => BinaryDataUtil.ExportPictures(page, text));
                    } else {
                        if (main.Controls[0] is TabControl tc) {
                            foreach (TabPage page in tc.TabPages) {
                                BinaryDataUtil.ExportPictures(page, main.Text);
                            }
                        } else {
                            BinaryDataUtil.ExportPictures(main, main.Text);
                        }
                    }

                    if (!loadAll) MessageBox.Show(Resources.CompleteSaveImages);
                    break;
                }
            }
        }

        /// <summary>
        /// Update the Window's clipboard when a cell is selected
        /// </summary>
        private static void Grid_RowStateChanged(object sender, DataGridViewCellStateChangedEventArgs e) {
            try {
                switch (e.Cell.Value) {
                    case int i:
                        Clipboard.SetText(i.ToString());
                        break;
                    case string str when str.Length > 0:
                        Clipboard.SetText(str);
                        break;
                    case Bitmap image:
                        Clipboard.SetImage(image);
                        break;
                    default:
                        Clipboard.Clear();
                        break;
                }
            } catch (Exception) {
                // ExternalException        "typically occurs when the Clipboard is being used by another process."
                // ArgumentNullException    "text is null or Empty."
            }
        }

        /// <summary>
        /// Display the PropertiesViewer Form when a Properties column cell is double clicked
        /// </summary>
        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex == 3 && e.RowIndex != -1) {
                viewer.SetProperties((string)((DataGridView)sender).SelectedCells[0].Value);
                viewer.Show();
                viewer.BringToFront();
            }
        }

        /// <summary>
        /// Open the FolderBrowser dialog window when the text box is clicked and set the selected
        /// directory as the root folder containing WZ files
        /// </summary>
        private void TextWzPath_Click(object sender, EventArgs e) {
            if (folderBrowser.ShowDialog() == DialogResult.OK &&
                !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath)) {
                TextWzPath.Text = folderBrowser.SelectedPath;
            }
        }

        /// <summary>
        /// Enable or disable the WZ path controls if BIN loading is selected
        /// </summary>
        private void ComboLoadType_SelectedIndexChanged(object sender, EventArgs e) {
            var enabled = (ComboLoadType.SelectedIndex == 1);
            TextWzPath.Enabled = enabled;
            BtnWzLoad.Enabled = enabled;
        }

        /// <summary>
        /// expand the search text box as necessary, to fit multiple search queries
        /// </summary>
        private void SearchTextBox_TextChanged(object sender, EventArgs e) {
            string[] lines = SearchTextBox.Text.Split(new[] { "\r\n" }, StringSplitOptions.None);
            SearchTextBox.Height = Math.Max(20, 15 * Math.Max(1, lines.Length));
        }

        private void SearchTextBox_KeyPress(object sender, KeyPressEventArgs e) {
            if ((int) e.KeyChar == 13) {
                OnTabControlChanged();
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            OnTabControlChanged();
        }

        private void TabControl_Selected(object sender, TabControlEventArgs e) {
            OnTabControlChanged();
        }

        private void BtnSearch_Click(object sender, EventArgs e) {
            // re-load the tab, but this time we should have a search query
            OnTabControlChanged();
        }

        private void OnTabControlChanged() {
            ClearAllPages(TabControlMain);

            var main = TabControlMain.SelectedTab;
            var sub = (main.Controls[0] is TabControl tc ? tc.SelectedTab : main);
            BinaryDataUtil.ImportGrid($"{main.Text}/{sub.Text}.bin", (DataViewport)sub.Controls[0], AddGridRow);
        }

        private void ForEachPage(TabControl control, Action<TabPage, string> testfor) {
            for (var i = 0; i < control.Controls.Count; i++) {
                var child = control.Controls[i];
                if (control == TabControlMain) {
                    TabControlMain.SelectedIndex = i;
                }
                
                if (child.Controls[0] is DataViewport) {
                    testfor.Invoke(child as TabPage, TabControlMain.SelectedTab.Text);
                } else if (child.Controls[0] is TabControl) {
                    ForEachPage(child.Controls[0] as TabControl, testfor);
                }
            }
        }

        /// <summary>
        /// Clear all DataViewport grids to allow re-populating data, especially when search queries are present
        /// </summary>
        private static void ClearAllPages(TabControl tabControl) {
            foreach (TabPage page in tabControl.TabPages) {
                switch (page.Controls[0]) {
                    case DataViewport dv:
                        if (dv.GridView.RowCount > 0) {
                            dv.GridView.Rows.Clear();
                        }

                        break;
                    case TabControl tc:
                        ClearAllPages(tc);
                        break;
                }
            }
        }
    }
}