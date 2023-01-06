
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using WzVisualizer.GUI.Controls;
using WzVisualizer.IO;
using WzVisualizer.Properties;
using WzVisualizer.Util;

namespace WzVisualizer.GUI {
    internal delegate void AddGridRowCallBack(DataGridView grid, BinData binData);

    public partial class MainForm : Form {
        private readonly FolderBrowserDialog folderBrowser = new();
        private readonly PropertiesViewer viewer = new();
        private bool loadAll = false;

        public MainForm() {
            InitializeComponent();
        }

        public string SearchQuery => SearchTextBox.Text;

        /// <summary>
        /// recursively add event handlers to all DataViewport components
        /// </summary>
        private void AddEventHandlers(TabControl tab) {
            foreach (Control ctrl in tab.Controls) {
                switch (ctrl.Controls[0]) {
                    case DataViewport dv:
                        dv.Data = new List<BinData>(); // cheeky little initializer

                        dv.GridView.CellDoubleClick += Grid_CellDoubleClick;
                        dv.GridView.CellStateChanged += Grid_RowStateChanged;
                        break;
                    case TabControl subTab:
                        subTab.Selected += TabControl_Selected;
                        AddEventHandlers(subTab);
                        break;
                }
            }
        }

        public DataViewport GetCurrentDataViewport() {
            var main = TabControlMain.SelectedTab;
            var sub = (main.Controls[0] is TabControl tc ? tc.SelectedTab : main);
            return (DataViewport)sub.Controls[0];
        }

        /// <summary>
        /// clears all collections, closes underlying file readers 
        /// then calls the garbage collector for each loaded WZ file
        /// </summary>
        private static void DisposeWzFiles() {
            foreach (var wz in Enum.GetValues(typeof(Wz)).Cast<Wz>()) {
                wz.Dispose();
            }
        }

        private void LoadWzData() {
            if (loadAll) {
                for (var i = 0; i < TabControlMain.TabCount; i++) {
                    TabControlMain.SelectedIndex = i;
                    VisualizerUtil.ProcessTab(i, this);
                }
                BtnSave_Click(null, new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                return;
            }

            VisualizerUtil.ProcessTab(TabControlMain.SelectedIndex, this);
        }

        /// <summary>
        /// Begin loading WZ data corresponding to the selected tab
        /// </summary>
        private void BtnWzLoad_Click(object sender, EventArgs e) {
            ClearAllPages(TabControlMain, true);
            DisposeWzFiles();

            loadAll = ModifierKeys == Keys.Shift;

            if (loadAll) {
                var result = MessageBox.Show(Resources.MassReadWarning, "Warning", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) return;
            }

            var path = TextWzPath.Text;

            if (!path.Equals(Settings.Default.PathCache)) {
                Settings.Default.PathCache = path;
                Settings.Default.Save();
            }

            if (string.IsNullOrEmpty(path)) {
                goto NO_FILES;
            }

            // 64-bit client update
            if (Directory.Exists(($@"{path}\Data"))) {
                foreach (var wz in Enum.GetValues(typeof(Wz)).Cast<Wz>()) {
                    try {
                        var files = Directory.GetFiles($@"{path}\Data\{wz}", "*.wz", SearchOption.AllDirectories);
                        foreach (var file in files) {
                            var name = Path.GetFileNameWithoutExtension(file);

                            // MapleLib fails to parse
                            if (name.StartsWith("script", StringComparison.CurrentCultureIgnoreCase)) continue;

                            wz.Load(file);
                        }
                    } catch (DirectoryNotFoundException _) {
                        // DirectoryNotFoundException : List.wz removed
                    }
                }
                LoadWzData();
                return;
            }

            var testFile = path + $@"\String{Resources.FileExtensionWZ}";
            // classic
            if (File.Exists(testFile)) {
                foreach (var wz in Enum.GetValues(typeof(Wz)).Cast<Wz>()) {

                    // not necessary, and cannot parse in this way
                    if (wz == Wz.List) continue;

                    wz.Load(@$"{path}\{wz}{Resources.FileExtensionWZ}", false);
                }
                LoadWzData();
                return;
            }

            // BMS
            if (Directory.Exists(testFile) && File.Exists($@"{testFile}\Cash.img")) {
                foreach (var wz in Enum.GetValues(typeof(Wz)).Cast<Wz>()) {
                    wz.Load(@$"{path}\{wz}{Resources.FileExtensionWZ}");
                }
                LoadWzData();
                return;
            }

            NO_FILES:
            MessageBox.Show(Resources.GameFilesNotFound, Resources.FileNotFound, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void ExportBinary(TabControl tab, bool saveAll = false) {
            var selectedTab = tab.SelectedTab;
            if (saveAll) {
                for (var i = 0; i < TabControlMain.TabCount; i++) {
                    BinaryDataUtil.ExportBinary(TabControlMain.TabPages[i], TabControlMain.TabPages[i].Text);
                }
            } else BinaryDataUtil.ExportBinary(selectedTab, selectedTab.Text);
        }

        void ExportPictures(TabControl tab, bool saveAll = false) {
            var selectedTab = tab.SelectedTab;
            if (saveAll) {
                for (var i = 0; i < TabControlMain.TabCount; i++) {
                    BinaryDataUtil.ExportPictures(TabControlMain.TabPages[i], TabControlMain.TabPages[i].Text);
                }
            } else BinaryDataUtil.ExportPictures(selectedTab, selectedTab.Text);
        }

        /// <summary>
        /// upon clicking the save button, store data of the current opened grid.
        /// Some tabs may have another TabControl in which that Control contains a Grid control.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs ev) {
            var button = ((MouseEventArgs)ev).Button;
            var saveAll = loadAll || ModifierKeys == Keys.Shift;

            if (!loadAll && saveAll) {
                var result = MessageBox.Show(Resources.MassWriteWarning, "Warning", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) return;
            }

            if (button == MouseButtons.Left) {
                ExportBinary(TabControlMain, saveAll);
                MessageBox.Show(Resources.CompleteSaveBIN, "Save Complete");
            } else if (button == MouseButtons.Right) {
                ExportPictures(TabControlMain, saveAll);
                MessageBox.Show(Resources.CompleteSaveImages, "Save Complete");
            }

            loadAll = false;
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
                    case Bitmap { Width: >= 0, Height: >= 0 } image:
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
            if (e.ColumnIndex != 3 || e.RowIndex == -1) return;

            viewer.SetProperties((string)((DataGridView)sender).SelectedCells[0].Value);
            if (!viewer.Visible) {
                viewer.Height = Height;
                viewer.StartPosition = FormStartPosition.Manual;

                viewer.Left = Right;
                viewer.Top = Top;
            }
            viewer.Show();
            viewer.BringToFront();
        }

        /// <summary>
        /// Open the FolderBrowser dialog window when the text box is clicked and set the selected
        /// directory as the root folder containing WZ files
        /// </summary>
        private void TextWzPath_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(folderBrowser.SelectedPath)) return;
            if (folderBrowser.ShowDialog() != DialogResult.OK) return;

            TextWzPath.Text = folderBrowser.SelectedPath;
        }

        /// <summary>
        /// Enable or disable the WZ path controls if BIN loading is selected
        /// </summary>
        private void ComboLoadType_SelectedIndexChanged(object sender, EventArgs e) {
            var enabled = (ComboLoadType.SelectedIndex == 1);
            TextWzPath.Enabled = enabled;
            BtnWzLoad.Enabled = enabled;
        }

        private void SearchTextBox_KeyPress(object sender, KeyPressEventArgs e) {
            if ((int)e.KeyChar == 13) {
                OnTabControlChanged();
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            // Obtain the last used WZ root directory
            TextWzPath.Text = Settings.Default.PathCache;
            // Set default values for the ComboBoxes
            ComboLoadType.SelectedIndex = 0;

            TabControlMain.Selected += TabControl_Selected;
            AddEventHandlers(TabControlMain);

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
            var dv = GetCurrentDataViewport();
            BinaryDataUtil.ImportGrid($"{main.Text}/{dv.Parent.Text}.bin", dv, (grid, data) => VisualizerUtil.AddNewRow(this, grid, data));
        }

        private void ForEachPage(TabControl control, Action<TabPage, string> testfor) {
            for (var i = 0; i < control.Controls.Count; i++) {
                var child = control.Controls[i];
                control.SelectedIndex = i;

                if (child.Controls[0] is DataViewport) {
                    testfor.Invoke(child as TabPage, TabControlMain.SelectedTab.Text);
                } else if (child.Controls[0] is TabControl tc) {
                    ForEachPage(child.Controls[0] as TabControl, testfor);
                }
            }
        }

        /// <summary>
        /// Clear all DataViewport grids to allow re-populating data, especially when search queries are present
        /// </summary>
        private void ClearAllPages(TabControl tabControl, bool clearData = false) {
            foreach (TabPage page in tabControl.TabPages) {
                switch (page.Controls[0]) {
                    case DataViewport dv: {
                            if (clearData) dv.Data.Clear();
                            dv.GridView.Rows.Clear();
                            GC.Collect();
                            break;
                        }
                    case TabControl tc:
                        if (tc == TabControlMain && tc.SelectedTab == TabControlMain.SelectedTab)
                            break;
                        ClearAllPages(tc);
                        break;
                }
            }
        }
    }
}