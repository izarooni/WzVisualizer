﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WzVisualizer.GUI.Controls {
    public partial class DataViewer : UserControl {
        public DataViewer() {
            InitializeComponent();

            Tag = new List<BinData>();
        }
    }
}
