using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WzVisualizer
{
    public partial class PropertiesViewer : Form
    {
        public PropertiesViewer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Change the text box value and reset the caret position
        /// </summary>
        /// <param name="text">The new value of the multi-line text box</param>
        internal void SetProperties(string text)
        {
            if (text.Length > 4)
                text = text.Substring(4); // Removes the first linebreak
            PropertiesBox.Text = text;
            PropertiesBox.SelectionStart = 0;
        }

        /// <summary>
        /// Prevents disposing the Form and instead hides it for reuse
        /// </summary>
        private void PropertiesViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}
