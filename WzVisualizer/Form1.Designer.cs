namespace WzVisualizer
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.TabControlMain = new System.Windows.Forms.TabControl();
            this.TabEquipsPage = new System.Windows.Forms.TabPage();
            this.TabEWeapons = new System.Windows.Forms.TabControl();
            this.TabsEWeaponsPage = new System.Windows.Forms.TabPage();
            this.GridEWeapons = new System.Windows.Forms.DataGridView();
            this.TabECaps = new System.Windows.Forms.TabPage();
            this.GridECaps = new System.Windows.Forms.DataGridView();
            this.GridECapsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridECapsImages = new System.Windows.Forms.DataGridViewImageColumn();
            this.TabUse = new System.Windows.Forms.TabPage();
            this.TabSetup = new System.Windows.Forms.TabPage();
            this.TabEtc = new System.Windows.Forms.TabPage();
            this.TabCash = new System.Windows.Forms.TabPage();
            this.ComboLoadType = new System.Windows.Forms.ComboBox();
            this.ComboEncType = new System.Windows.Forms.ComboBox();
            this.TextWzPath = new System.Windows.Forms.TextBox();
            this.BtnWzLoad = new System.Windows.Forms.Button();
            this.BtnSave = new System.Windows.Forms.Button();
            this.TabEOveralls = new System.Windows.Forms.TabPage();
            this.TabETops = new System.Windows.Forms.TabPage();
            this.TabEBottoms = new System.Windows.Forms.TabPage();
            this.GridEWeaponsID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GridEWeaponsImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.TabControlMain.SuspendLayout();
            this.TabEquipsPage.SuspendLayout();
            this.TabEWeapons.SuspendLayout();
            this.TabsEWeaponsPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridEWeapons)).BeginInit();
            this.TabECaps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GridECaps)).BeginInit();
            this.SuspendLayout();
            // 
            // TabControlMain
            // 
            this.TabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControlMain.Controls.Add(this.TabEquipsPage);
            this.TabControlMain.Controls.Add(this.TabUse);
            this.TabControlMain.Controls.Add(this.TabSetup);
            this.TabControlMain.Controls.Add(this.TabEtc);
            this.TabControlMain.Controls.Add(this.TabCash);
            this.TabControlMain.Font = new System.Drawing.Font("Meiryo", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TabControlMain.Location = new System.Drawing.Point(0, 29);
            this.TabControlMain.Margin = new System.Windows.Forms.Padding(0);
            this.TabControlMain.Name = "TabControlMain";
            this.TabControlMain.Padding = new System.Drawing.Point(15, 5);
            this.TabControlMain.SelectedIndex = 0;
            this.TabControlMain.Size = new System.Drawing.Size(800, 421);
            this.TabControlMain.TabIndex = 0;
            // 
            // TabEquipsPage
            // 
            this.TabEquipsPage.Controls.Add(this.TabEWeapons);
            this.TabEquipsPage.Location = new System.Drawing.Point(4, 33);
            this.TabEquipsPage.Margin = new System.Windows.Forms.Padding(0);
            this.TabEquipsPage.Name = "TabEquipsPage";
            this.TabEquipsPage.Size = new System.Drawing.Size(792, 384);
            this.TabEquipsPage.TabIndex = 0;
            this.TabEquipsPage.Text = "Equips";
            this.TabEquipsPage.UseVisualStyleBackColor = true;
            // 
            // TabEWeapons
            // 
            this.TabEWeapons.Controls.Add(this.TabsEWeaponsPage);
            this.TabEWeapons.Controls.Add(this.TabECaps);
            this.TabEWeapons.Controls.Add(this.TabEOveralls);
            this.TabEWeapons.Controls.Add(this.TabETops);
            this.TabEWeapons.Controls.Add(this.TabEBottoms);
            this.TabEWeapons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabEWeapons.Location = new System.Drawing.Point(0, 0);
            this.TabEWeapons.Name = "TabEWeapons";
            this.TabEWeapons.SelectedIndex = 0;
            this.TabEWeapons.Size = new System.Drawing.Size(792, 384);
            this.TabEWeapons.TabIndex = 2;
            // 
            // TabsEWeaponsPage
            // 
            this.TabsEWeaponsPage.Controls.Add(this.GridEWeapons);
            this.TabsEWeaponsPage.Location = new System.Drawing.Point(4, 29);
            this.TabsEWeaponsPage.Name = "TabsEWeaponsPage";
            this.TabsEWeaponsPage.Padding = new System.Windows.Forms.Padding(3);
            this.TabsEWeaponsPage.Size = new System.Drawing.Size(784, 351);
            this.TabsEWeaponsPage.TabIndex = 0;
            this.TabsEWeaponsPage.Text = "Weapons";
            this.TabsEWeaponsPage.UseVisualStyleBackColor = true;
            // 
            // GridEWeapons
            // 
            this.GridEWeapons.AllowUserToDeleteRows = false;
            this.GridEWeapons.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridEWeapons.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GridEWeaponsID,
            this.GridEWeaponsImage});
            this.GridEWeapons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridEWeapons.Location = new System.Drawing.Point(3, 3);
            this.GridEWeapons.Name = "GridEWeapons";
            this.GridEWeapons.ReadOnly = true;
            this.GridEWeapons.Size = new System.Drawing.Size(778, 345);
            this.GridEWeapons.TabIndex = 3;
            // 
            // TabECaps
            // 
            this.TabECaps.Controls.Add(this.GridECaps);
            this.TabECaps.Location = new System.Drawing.Point(4, 29);
            this.TabECaps.Name = "TabECaps";
            this.TabECaps.Padding = new System.Windows.Forms.Padding(3);
            this.TabECaps.Size = new System.Drawing.Size(784, 351);
            this.TabECaps.TabIndex = 1;
            this.TabECaps.Text = "Caps";
            this.TabECaps.UseVisualStyleBackColor = true;
            // 
            // GridECaps
            // 
            this.GridECaps.AllowUserToDeleteRows = false;
            this.GridECaps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.GridECaps.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GridECapsID,
            this.GridECapsImages});
            this.GridECaps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GridECaps.Location = new System.Drawing.Point(3, 3);
            this.GridECaps.Name = "GridECaps";
            this.GridECaps.ReadOnly = true;
            this.GridECaps.Size = new System.Drawing.Size(778, 345);
            this.GridECaps.TabIndex = 0;
            // 
            // GridECapsID
            // 
            this.GridECapsID.HeaderText = "ID";
            this.GridECapsID.Name = "GridECapsID";
            this.GridECapsID.ReadOnly = true;
            // 
            // GridECapsImages
            // 
            this.GridECapsImages.HeaderText = "Images";
            this.GridECapsImages.Name = "GridECapsImages";
            this.GridECapsImages.ReadOnly = true;
            this.GridECapsImages.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.GridECapsImages.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // TabUse
            // 
            this.TabUse.Location = new System.Drawing.Point(4, 33);
            this.TabUse.Name = "TabUse";
            this.TabUse.Padding = new System.Windows.Forms.Padding(3);
            this.TabUse.Size = new System.Drawing.Size(792, 384);
            this.TabUse.TabIndex = 1;
            this.TabUse.Text = "Use";
            this.TabUse.UseVisualStyleBackColor = true;
            // 
            // TabSetup
            // 
            this.TabSetup.Location = new System.Drawing.Point(4, 33);
            this.TabSetup.Name = "TabSetup";
            this.TabSetup.Padding = new System.Windows.Forms.Padding(3);
            this.TabSetup.Size = new System.Drawing.Size(792, 384);
            this.TabSetup.TabIndex = 2;
            this.TabSetup.Text = "Setup";
            this.TabSetup.UseVisualStyleBackColor = true;
            // 
            // TabEtc
            // 
            this.TabEtc.Location = new System.Drawing.Point(4, 33);
            this.TabEtc.Name = "TabEtc";
            this.TabEtc.Padding = new System.Windows.Forms.Padding(3);
            this.TabEtc.Size = new System.Drawing.Size(792, 384);
            this.TabEtc.TabIndex = 4;
            this.TabEtc.Text = "Etc";
            this.TabEtc.UseVisualStyleBackColor = true;
            // 
            // TabCash
            // 
            this.TabCash.Location = new System.Drawing.Point(4, 33);
            this.TabCash.Name = "TabCash";
            this.TabCash.Padding = new System.Windows.Forms.Padding(3);
            this.TabCash.Size = new System.Drawing.Size(792, 384);
            this.TabCash.TabIndex = 5;
            this.TabCash.Text = "Cash";
            this.TabCash.UseVisualStyleBackColor = true;
            // 
            // ComboLoadType
            // 
            this.ComboLoadType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboLoadType.FormattingEnabled = true;
            this.ComboLoadType.Items.AddRange(new object[] {
            "BIN",
            "WZ"});
            this.ComboLoadType.Location = new System.Drawing.Point(5, 5);
            this.ComboLoadType.Name = "ComboLoadType";
            this.ComboLoadType.Size = new System.Drawing.Size(55, 21);
            this.ComboLoadType.TabIndex = 1;
            this.ComboLoadType.SelectedIndexChanged += new System.EventHandler(this.ComboLoadType_SelectedIndexChanged);
            // 
            // ComboEncType
            // 
            this.ComboEncType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboEncType.FormattingEnabled = true;
            this.ComboEncType.Items.AddRange(new object[] {
            "LucianMS",
            "GMS"});
            this.ComboEncType.Location = new System.Drawing.Point(67, 5);
            this.ComboEncType.Name = "ComboEncType";
            this.ComboEncType.Size = new System.Drawing.Size(88, 21);
            this.ComboEncType.TabIndex = 2;
            // 
            // TextWzPath
            // 
            this.TextWzPath.Enabled = false;
            this.TextWzPath.Location = new System.Drawing.Point(162, 5);
            this.TextWzPath.Name = "TextWzPath";
            this.TextWzPath.Size = new System.Drawing.Size(227, 20);
            this.TextWzPath.TabIndex = 3;
            this.TextWzPath.Click += new System.EventHandler(this.TextWzPath_Click);
            // 
            // BtnWzLoad
            // 
            this.BtnWzLoad.Enabled = false;
            this.BtnWzLoad.Location = new System.Drawing.Point(396, 5);
            this.BtnWzLoad.Name = "BtnWzLoad";
            this.BtnWzLoad.Size = new System.Drawing.Size(75, 23);
            this.BtnWzLoad.TabIndex = 4;
            this.BtnWzLoad.Text = "Load WZ";
            this.BtnWzLoad.UseVisualStyleBackColor = true;
            this.BtnWzLoad.Click += new System.EventHandler(this.BtnWzLoad_Click);
            // 
            // BtnSave
            // 
            this.BtnSave.BackColor = System.Drawing.Color.Transparent;
            this.BtnSave.FlatAppearance.BorderSize = 0;
            this.BtnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BtnSave.Image = global::WzVisualizer.Properties.Resources.floppy_icon;
            this.BtnSave.Location = new System.Drawing.Point(478, 5);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Size = new System.Drawing.Size(23, 23);
            this.BtnSave.TabIndex = 5;
            this.BtnSave.UseVisualStyleBackColor = false;
            // 
            // TabEOveralls
            // 
            this.TabEOveralls.Location = new System.Drawing.Point(4, 29);
            this.TabEOveralls.Name = "TabEOveralls";
            this.TabEOveralls.Padding = new System.Windows.Forms.Padding(3);
            this.TabEOveralls.Size = new System.Drawing.Size(784, 351);
            this.TabEOveralls.TabIndex = 2;
            this.TabEOveralls.Text = "Overalls";
            this.TabEOveralls.UseVisualStyleBackColor = true;
            // 
            // TabETops
            // 
            this.TabETops.Location = new System.Drawing.Point(4, 29);
            this.TabETops.Name = "TabETops";
            this.TabETops.Padding = new System.Windows.Forms.Padding(3);
            this.TabETops.Size = new System.Drawing.Size(784, 351);
            this.TabETops.TabIndex = 3;
            this.TabETops.Text = "Tops";
            this.TabETops.UseVisualStyleBackColor = true;
            // 
            // TabEBottoms
            // 
            this.TabEBottoms.Location = new System.Drawing.Point(4, 29);
            this.TabEBottoms.Name = "TabEBottoms";
            this.TabEBottoms.Padding = new System.Windows.Forms.Padding(3);
            this.TabEBottoms.Size = new System.Drawing.Size(784, 351);
            this.TabEBottoms.TabIndex = 4;
            this.TabEBottoms.Text = "Bottoms";
            this.TabEBottoms.UseVisualStyleBackColor = true;
            // 
            // GridEWeaponsID
            // 
            this.GridEWeaponsID.HeaderText = "ID";
            this.GridEWeaponsID.Name = "GridEWeaponsID";
            this.GridEWeaponsID.ReadOnly = true;
            // 
            // GridEWeaponsImage
            // 
            this.GridEWeaponsImage.HeaderText = "Image";
            this.GridEWeaponsImage.Name = "GridEWeaponsImage";
            this.GridEWeaponsImage.ReadOnly = true;
            this.GridEWeaponsImage.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.GridEWeaponsImage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.BtnWzLoad);
            this.Controls.Add(this.TextWzPath);
            this.Controls.Add(this.ComboEncType);
            this.Controls.Add(this.ComboLoadType);
            this.Controls.Add(this.TabControlMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "WzVisualizer";
            this.TabControlMain.ResumeLayout(false);
            this.TabEquipsPage.ResumeLayout(false);
            this.TabEWeapons.ResumeLayout(false);
            this.TabsEWeaponsPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridEWeapons)).EndInit();
            this.TabECaps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GridECaps)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl TabControlMain;
        private System.Windows.Forms.TabPage TabEquipsPage;
        private System.Windows.Forms.TabPage TabUse;
        private System.Windows.Forms.TabPage TabSetup;
        private System.Windows.Forms.TabPage TabEtc;
        private System.Windows.Forms.ComboBox ComboLoadType;
        private System.Windows.Forms.TabPage TabCash;
        private System.Windows.Forms.ComboBox ComboEncType;
        private System.Windows.Forms.TextBox TextWzPath;
        private System.Windows.Forms.Button BtnWzLoad;
        private System.Windows.Forms.Button BtnSave;
        private System.Windows.Forms.TabControl TabEWeapons;
        private System.Windows.Forms.TabPage TabsEWeaponsPage;
        private System.Windows.Forms.DataGridView GridEWeapons;
        private System.Windows.Forms.TabPage TabECaps;
        private System.Windows.Forms.DataGridView GridECaps;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridECapsID;
        private System.Windows.Forms.DataGridViewImageColumn GridECapsImages;
        private System.Windows.Forms.TabPage TabEOveralls;
        private System.Windows.Forms.TabPage TabETops;
        private System.Windows.Forms.TabPage TabEBottoms;
        private System.Windows.Forms.DataGridViewTextBoxColumn GridEWeaponsID;
        private System.Windows.Forms.DataGridViewImageColumn GridEWeaponsImage;
    }
}

