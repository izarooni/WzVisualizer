using System.Collections.Generic;
using System.Windows.Forms;

namespace WzVisualizer.GUI.Controls {
    public partial class DataViewport : UserControl {
        public DataViewport() {
            InitializeComponent();

            Tag = new List<BinData>();
        }
    }
}