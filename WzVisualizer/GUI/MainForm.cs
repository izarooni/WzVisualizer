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
    delegate void AddGridRowCallBack(DataGridView grid, BinData binData);

    public partial class MainForm : Form {

        private readonly PropertiesViewer _viewer = new();
        internal readonly SearchForm SearchForm = new();

        public MainForm() {
            InitializeComponent();

            AddOwnedForm(SearchForm);
            SearchForm.Location = new Point(Right - SearchForm.Width - 5, Top + SearchForm.Height / 2);
            LocationChanged += (o, args) => SearchForm.Location = new Point(Right - SearchForm.Width - 5, Top + SearchForm.Height / 2);
            SearchForm.searchButton.Click += (o, args) => LoadCurrentTabPage();
        }
        public bool LoadAll { get; set; }
        public string SearchQuery => SearchForm.SearchBox.Text;

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

        public TabPage GetCurrentTabPage() {
            var main = TabControlMain.SelectedTab;
            var sub = (main.Controls[0] is TabControl tc ? tc.SelectedTab : main);
            return sub;
        }

        public DataViewport GetCurrentDataViewport() {
            var tab = GetCurrentTabPage();
            return (DataViewport)tab.Controls[0];
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
            if (LoadAll) {
                for (var i = 0; i < TabControlMain.TabCount; i++) {
                    TabControlMain.SelectedIndex = i;
                    VisualizerUtil.ProcessTab(i, this);
                }
                SaveBinary(true);
            } else {
                VisualizerUtil.ProcessTab(TabControlMain.SelectedIndex, this);
            }
        }

        internal void LoadCurrentTabPage() {
            ClearAllPages(TabControlMain);

            var main = TabControlMain.SelectedTab;
            var tab = GetCurrentTabPage();
            var dv = GetCurrentDataViewport();
            BinaryDataUtil.ImportGrid($"{main.Text}/{tab.Text}.bin", dv, (grid, data) => VisualizerUtil.AddNewRow(this, grid, data));
        }

        private void ExportPictures() {
            for (var i = 0; i < TabControlMain.TabCount; i++) {
                // changing the selected tab will trigger the TabControl_Selected event
                // which will prepare the data for us to export
                TabControlMain.SelectedIndex = i;
                BinaryDataUtil.ExportPictures(TabControlMain.TabPages[i], TabControlMain.TabPages[i].Text);
            }
            MessageBox.Show(Resources.CompleteSaveImages, Resources.SaveComplete);
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
                        break;
                    }
                    case TabControl tc:
                        if (tc == TabControlMain && tc.SelectedTab == TabControlMain.SelectedTab)
                            break;
                        ClearAllPages(tc);
                        break;
                }
            }
            GC.Collect();
        }

        /// <summary>
        /// upon clicking the save button, store data of the current opened grid.
        /// Some tabs may have another TabControl in which that Control contains a Grid control.
        /// </summary>
        private void SaveBinary(bool everything) {
            if (everything) {
                for (var i = 0; i < TabControlMain.TabCount; i++) {
                    TabControlMain.SelectedIndex = i;
                    BinaryDataUtil.ExportBinary(TabControlMain.TabPages[i], TabControlMain.SelectedTab.Text);
                }
            } else {
                var selectedTab = GetCurrentTabPage();
                BinaryDataUtil.ExportBinary(selectedTab, TabControlMain.SelectedTab.Text);
            }

            MessageBox.Show(Resources.CompleteSaveBIN, Resources.SaveComplete);
            LoadAll = false;
        }

        /// <summary>
        /// Begin loading WZ data corresponding to the selected tab
        /// </summary>
        private void VerifyWzFolder(string path) {
            ClearAllPages(TabControlMain, true);
            DisposeWzFiles();

            if (LoadAll) {
                var result = MessageBox.Show(Resources.MassReadWarning, @"Warning", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) return;
            }

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
                    } catch (DirectoryNotFoundException) {
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

        /// <summary>
        /// Update the Window's clipboard when a cell is selected
        /// </summary>
        private static void Grid_RowStateChanged(object sender, DataGridViewCellStateChangedEventArgs e) {
            try {
                switch (e.Cell.Value) {
                    case int i:
                        Clipboard.SetText(i.ToString());
                        break;
                    case string { Length: > 0 } str:
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
            if (e.RowIndex < 0) return;

            var grid = (DataGridView)sender;
            var cell = grid.SelectedCells[0];

            switch (cell.ColumnIndex) {
                case 0:
                    Clipboard.SetText($"!item {cell.Value}");
                    break;
                case 1:
                    var bmp = cell.Value as Bitmap;
                    if (bmp == null) return;
                    cell.OwningColumn.Width = bmp.Width + 15;
                    cell.OwningRow.Height = bmp.Height + 15;
                    break;
                case 3:
                    _viewer.SetProperties((string)((DataGridView)sender).SelectedCells[0].Value);
                    if (!_viewer.Visible) {
                        _viewer.Height = Height;
                        _viewer.StartPosition = FormStartPosition.Manual;

                        _viewer.Left = Right;
                        _viewer.Top = Top;
                    }
                    _viewer.Show();
                    _viewer.BringToFront();
                    break;
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            TabControlMain.Selected += TabControl_Selected;
            AddEventHandlers(TabControlMain);

            LoadCurrentTabPage();
        }

        private void TabControl_Selected(object sender, TabControlEventArgs e) {
            LoadCurrentTabPage();
        }

        private void OnLoadCurrentTabPage(object sender, EventArgs e) {
            LoadAll = false;

            var folderDlg = new FolderBrowserDialog {
                Description = "Select a folder",
                ShowNewFolderButton = false,
                SelectedPath = Settings.Default.PathCache
            };
            if (folderDlg.ShowDialog() == DialogResult.OK) {
                VerifyWzFolder(folderDlg.SelectedPath);
            }
        }
        private void OnLoadEverything(object sender, EventArgs e) {
            LoadAll = true;

            var folderDlg = new FolderBrowserDialog {
                Description = "Select a folder",
                ShowNewFolderButton = false,
                SelectedPath = Settings.Default.PathCache
            };
            if (folderDlg.ShowDialog() == DialogResult.OK) {
                VerifyWzFolder(folderDlg.SelectedPath);
            }
        }
        private void OnSaveCurrentTabPage(object sender, EventArgs e) {
            SaveBinary(false);
        }
        private void OnSaveEverything(object sender, EventArgs e) {
            var result = MessageBox.Show(Resources.MassWriteWarning, "Warning", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes) return;

            SaveBinary(true);
        }
        private void OnExportPictures(object sender, EventArgs e) {
            ExportPictures();
        }
        private void OnShowSearchForm(object sender, EventArgs e) {
            SearchForm.Show();
        }

        private void OnOpenningMainToolstrip(object sender, EventArgs e) {
            OpenCurrentMenuItem.Text = $"Load {GetCurrentTabPage().Text}";
        }

        private void exportTocsvToolStripMenuItem_Click(object sender, EventArgs e) {
            BinaryDataUtil.ExportCSV(GetCurrentTabPage(), GetCurrentTabPage().Text);
        }
    }
}
