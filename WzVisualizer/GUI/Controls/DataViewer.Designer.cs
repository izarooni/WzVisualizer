namespace WzVisualizer.GUI.Controls {
    partial class DataViewer {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.GridView = new System.Windows.Forms.DataGridView();
            this.ID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Image = new System.Windows.Forms.DataGridViewImageColumn();
            this.Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Properties = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
            this.SuspendLayout();
            // 
            // GridView
            // 
            this.GridView.AllowUserToAddRows = false;
            this.GridView.AllowUserToDeleteRows = false;
            this.GridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.Image,
            this.Name,
            this.Properties});
            this.GridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridView.Location = new System.Drawing.Point(0, 0);
            this.GridView.Name = "GridView";
            this.GridView.ReadOnly = true;
            this.GridView.RowTemplate.Height = 50;
            this.GridView.Size = new System.Drawing.Size(150, 150);
            this.GridView.TabIndex = 0;
            // 
            // ID
            // 
            this.ID.DataPropertyName = "IDProperty";
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            // 
            // Image
            // 
            this.Image.DataPropertyName = "ImageProperty";
            this.Image.HeaderText = "Image";
            this.Image.Name = "Image";
            this.Image.ReadOnly = true;
            this.Image.Width = 150;
            // 
            // Name
            // 
            this.Name.DataPropertyName = "NameProperty";
            this.Name.HeaderText = "Name";
            this.Name.Name = "Name";
            this.Name.ReadOnly = true;
            // 
            // Properties
            // 
            this.Properties.DataPropertyName = "PropertiesProperty";
            this.Properties.HeaderText = "Properties";
            this.Properties.Name = "Properties";
            this.Properties.ReadOnly = true;
            // 
            // DataViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GridView);
            ((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridViewTextBoxColumn ID;
        private System.Windows.Forms.DataGridViewImageColumn Image;
        private System.Windows.Forms.DataGridViewTextBoxColumn Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn Properties;
        internal System.Windows.Forms.DataGridView GridView;
    }
}
