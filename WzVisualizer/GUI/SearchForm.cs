using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WzVisualizer.GUI {
    public partial class SearchForm : Form {
        public SearchForm() {
            InitializeComponent();
        }

        private void OnSearchButton_Click(object sender, EventArgs e) {
            Hide();
        }

        private void SearchForm_KeyDown(object sender, KeyEventArgs e) {
            if (Owner is not MainForm mainForm) return;
            mainForm.MainForm_KeyDown(sender, e);
        }

        private void SearchForm_KeyUp(object sender, KeyEventArgs e) {
            if (Owner is not MainForm mainForm) return;
            mainForm.MainForm_KeyUp(sender, e);
        }

        private void SearchForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason != CloseReason.UserClosing) return;
            e.Cancel = true;
            Hide();
        }
    }
}
