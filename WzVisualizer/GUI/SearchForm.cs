using System;
using System.Windows.Forms;

namespace WzVisualizer.GUI {
    public partial class SearchForm : Form {
        public SearchForm() {
            InitializeComponent();
        }

        private void OnSearchButton_Click(object sender, EventArgs e) {
            if (Owner is not MainForm mainForm) return;
            mainForm.LoadCurrentTabPage();
            Hide();
        }

        private void SearchForm_KeyDown(object sender, KeyEventArgs e) {
            if (Owner is not MainForm mainForm) return;
        }

        private void SearchForm_KeyUp(object sender, KeyEventArgs e) {
            if (Owner is not MainForm mainForm) return;
        }

        private void SearchForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason != CloseReason.UserClosing) return;
            e.Cancel = true;
            Hide();
        }

        private void SearchForm_Shown(object sender, EventArgs e) {
            SearchBox.Focus();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue == (int)Keys.Enter) {
                OnSearchButton_Click(sender, e);
            }
        }
    }
}
