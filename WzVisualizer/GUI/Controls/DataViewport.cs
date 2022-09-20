using System.Collections.Generic;
using System.Windows.Forms;

namespace WzVisualizer.GUI.Controls {
    public partial class DataViewport : UserControl {

        public List<BinData> Data { get; } = new List<BinData>();

        public DataViewport() {
            InitializeComponent();
        }

        public override string ToString() {
            return $"DataViewport({Name})";
        }
    }
}