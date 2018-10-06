namespace WzVisualizer
{
    partial class PropertiesViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertiesViewer));
            this.PropertiesBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // PropertiesBox
            // 
            this.PropertiesBox.BackColor = System.Drawing.SystemColors.Control;
            this.PropertiesBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PropertiesBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertiesBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PropertiesBox.Location = new System.Drawing.Point(0, 0);
            this.PropertiesBox.Multiline = true;
            this.PropertiesBox.Name = "PropertiesBox";
            this.PropertiesBox.ReadOnly = true;
            this.PropertiesBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.PropertiesBox.Size = new System.Drawing.Size(446, 244);
            this.PropertiesBox.TabIndex = 0;
            // 
            // PropertiesViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(446, 244);
            this.Controls.Add(this.PropertiesBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PropertiesViewer";
            this.Text = "Properties Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PropertiesViewer_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox PropertiesBox;
    }
}