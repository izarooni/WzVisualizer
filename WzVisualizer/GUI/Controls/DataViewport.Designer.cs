using System.ComponentModel;

namespace WzVisualizer.GUI.Controls {
    partial class DataViewport {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.propID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.propBitmap = new System.Windows.Forms.DataGridViewImageColumn();
            this.propName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.propProperties = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.GridView)).BeginInit();
            this.SuspendLayout();
            // 
            // GridView
            // 
            this.GridView.AllowUserToAddRows = false;
            this.GridView.AllowUserToDeleteRows = false;
            this.GridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.propID,
            this.propBitmap,
            this.propName,
            this.propProperties});
            this.GridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridView.Location = new System.Drawing.Point(0, 0);
            this.GridView.Name = "GridView";
            this.GridView.ReadOnly = true;
            this.GridView.RowTemplate.Height = 50;
            this.GridView.Size = new System.Drawing.Size(500, 250);
            this.GridView.TabIndex = 0;
            // 
            // propID
            // 
            this.propID.DataPropertyName = "IDProperty";
            this.propID.HeaderText = "ID";
            this.propID.Name = "propID";
            this.propID.ReadOnly = true;
            // 
            // propBitmap
            // 
            this.propBitmap.DataPropertyName = "ImageProperty";
            this.propBitmap.HeaderText = "Image";
            this.propBitmap.Name = "propBitmap";
            this.propBitmap.ReadOnly = true;
            this.propBitmap.Width = 150;
            // 
            // propName
            // 
            this.propName.DataPropertyName = "NameProperty";
            this.propName.FillWeight = 130F;
            this.propName.HeaderText = "Name";
            this.propName.Name = "propName";
            this.propName.ReadOnly = true;
            // 
            // propProperties
            // 
            this.propProperties.DataPropertyName = "PropertiesProperty";
            this.propProperties.HeaderText = "Properties";
            this.propProperties.Name = "propProperties";
            this.propProperties.ReadOnly = true;
            // 
            // DataViewport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GridView);
            this.Name = "DataViewport";
            this.Size = new System.Drawing.Size(500, 250);
            ((System.ComponentModel.ISupportInitialize)(this.GridView)).EndInit();
            this.ResumeLayout(false);

        }

        internal System.Windows.Forms.DataGridView GridView;

        #endregion

        private System.Windows.Forms.DataGridViewTextBoxColumn propID;
        private System.Windows.Forms.DataGridViewImageColumn propBitmap;
        private System.Windows.Forms.DataGridViewTextBoxColumn propName;
        private System.Windows.Forms.DataGridViewTextBoxColumn propProperties;
    }
}