namespace AuraDDX.Viewer
{
    partial class Viewer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Viewer));
            OpenNewFile = new OpenFileDialog();
            Menu = new ToolStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            openToolStripMenuItem = new ToolStripMenuItem();
            BtnUpdate = new ToolStripButton();
            CurrentVersion = new ToolStripLabel();
            ImageDisplay = new PictureBox();
            Menu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ImageDisplay).BeginInit();
            SuspendLayout();
            // 
            // OpenNewFile
            // 
            OpenNewFile.DefaultExt = "ddx";
            OpenNewFile.Filter = "Image Files (*.bmp, *.jpg, *.jpeg, *.png,  *.tiff, *.ddx, *.dds)|*.bmp;*.jpg;*.jpeg;*.png;* .tiff;*.ddx; *.dds|All Files (*.*)|*.*\"";
            OpenNewFile.RestoreDirectory = true;
            // 
            // Menu
            // 
            Menu.BackgroundImageLayout = ImageLayout.None;
            Menu.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Menu.GripStyle = ToolStripGripStyle.Hidden;
            Menu.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1, BtnUpdate, CurrentVersion });
            Menu.Location = new Point(0, 0);
            Menu.Name = "Menu";
            Menu.Size = new Size(1264, 25);
            Menu.TabIndex = 1;
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.BackgroundImageLayout = ImageLayout.None;
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem });
            toolStripDropDownButton1.Image = Properties.Resources.Document;
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(29, 22);
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Image = Properties.Resources.OpenFile;
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(118, 26);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += OpenFileAsync;
            // 
            // BtnUpdate
            // 
            BtnUpdate.Alignment = ToolStripItemAlignment.Right;
            BtnUpdate.BackgroundImageLayout = ImageLayout.None;
            BtnUpdate.DisplayStyle = ToolStripItemDisplayStyle.Image;
            BtnUpdate.Image = Properties.Resources.UpdateScript;
            BtnUpdate.ImageTransparentColor = Color.Magenta;
            BtnUpdate.Name = "BtnUpdate";
            BtnUpdate.Size = new Size(23, 22);
            BtnUpdate.Visible = false;
            BtnUpdate.Click += UpdateProgramAsync;
            // 
            // CurrentVersion
            // 
            CurrentVersion.Alignment = ToolStripItemAlignment.Right;
            CurrentVersion.BackgroundImageLayout = ImageLayout.None;
            CurrentVersion.DisplayStyle = ToolStripItemDisplayStyle.Text;
            CurrentVersion.Enabled = false;
            CurrentVersion.Name = "CurrentVersion";
            CurrentVersion.Size = new Size(62, 22);
            CurrentVersion.Text = "Version";
            // 
            // ImageDisplay
            // 
            ImageDisplay.BackgroundImageLayout = ImageLayout.None;
            ImageDisplay.Dock = DockStyle.Fill;
            ImageDisplay.Location = new Point(0, 25);
            ImageDisplay.Name = "ImageDisplay";
            ImageDisplay.Size = new Size(1264, 656);
            ImageDisplay.SizeMode = PictureBoxSizeMode.Zoom;
            ImageDisplay.TabIndex = 2;
            ImageDisplay.TabStop = false;
            // 
            // Viewer
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(1264, 681);
            Controls.Add(ImageDisplay);
            Controls.Add(Menu);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            Name = "Viewer";
            Text = "AuraDDX: Viewer";
            FormClosed += Viewer_FormClosed;
            Menu.ResumeLayout(false);
            Menu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ImageDisplay).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private OpenFileDialog OpenNewFile;
        private ToolStrip Menu;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripLabel CurrentVersion;
        private ToolStripButton BtnUpdate;
        private PictureBox ImageDisplay;
    }
}