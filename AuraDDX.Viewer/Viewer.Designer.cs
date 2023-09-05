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
            ViewerMenu = new MenuStrip();
            FileMenu = new ToolStripMenuItem();
            FileMenuOpen = new ToolStripMenuItem();
            ExtensionMenu = new ToolStripMenuItem();
            ExtensionMenuRegister = new ToolStripMenuItem();
            ExtensionMenuUnregister = new ToolStripMenuItem();
            OpenNewFile = new OpenFileDialog();
            ViewerMenu.SuspendLayout();
            SuspendLayout();
            // 
            // ViewerMenu
            // 
            ViewerMenu.BackgroundImageLayout = ImageLayout.None;
            ViewerMenu.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            ViewerMenu.Items.AddRange(new ToolStripItem[] { FileMenu, ExtensionMenu });
            ViewerMenu.Location = new Point(0, 0);
            ViewerMenu.Name = "ViewerMenu";
            ViewerMenu.Size = new Size(1264, 29);
            ViewerMenu.TabIndex = 0;
            ViewerMenu.Text = "menuStrip1";
            // 
            // FileMenu
            // 
            FileMenu.BackgroundImageLayout = ImageLayout.None;
            FileMenu.DropDownItems.AddRange(new ToolStripItem[] { FileMenuOpen });
            FileMenu.Image = Properties.Resources.Document;
            FileMenu.Name = "FileMenu";
            FileMenu.Size = new Size(62, 25);
            FileMenu.Text = "File";
            // 
            // FileMenuOpen
            // 
            FileMenuOpen.BackgroundImageLayout = ImageLayout.None;
            FileMenuOpen.Image = Properties.Resources.OpenFile;
            FileMenuOpen.Name = "FileMenuOpen";
            FileMenuOpen.Size = new Size(118, 26);
            FileMenuOpen.Text = "Open";
            FileMenuOpen.Click += OpenFile;
            // 
            // ExtensionMenu
            // 
            ExtensionMenu.DropDownItems.AddRange(new ToolStripItem[] { ExtensionMenuRegister, ExtensionMenuUnregister });
            ExtensionMenu.Image = Properties.Resources.IconFile;
            ExtensionMenu.Name = "ExtensionMenu";
            ExtensionMenu.Size = new Size(104, 25);
            ExtensionMenu.Text = "Extension";
            // 
            // ExtensionMenuRegister
            // 
            ExtensionMenuRegister.Image = Properties.Resources.DocumentOK;
            ExtensionMenuRegister.Name = "ExtensionMenuRegister";
            ExtensionMenuRegister.Size = new Size(221, 26);
            ExtensionMenuRegister.Text = "Register AuraDDX";
            ExtensionMenuRegister.Click += RegisterAndExit;
            // 
            // ExtensionMenuUnregister
            // 
            ExtensionMenuUnregister.Image = Properties.Resources.DocumentError;
            ExtensionMenuUnregister.Name = "ExtensionMenuUnregister";
            ExtensionMenuUnregister.Size = new Size(221, 26);
            ExtensionMenuUnregister.Text = "Unregister AuraDDX";
            ExtensionMenuUnregister.Click += UnregisterAndExit;
            // 
            // OpenNewFile
            // 
            OpenNewFile.DefaultExt = "ddx";
            OpenNewFile.Filter = "DDX Files (*.ddx)|*.ddx|PNG Files (*.png)|*.png";
            OpenNewFile.RestoreDirectory = true;
            // 
            // Viewer
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(1264, 681);
            Controls.Add(ViewerMenu);
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = ViewerMenu;
            Margin = new Padding(4);
            Name = "Viewer";
            Text = "AuraDDX: Viewer";
            FormClosed += Viewer_FormClosed;
            ViewerMenu.ResumeLayout(false);
            ViewerMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip ViewerMenu;
        private ToolStripMenuItem FileMenu;
        private ToolStripMenuItem FileMenuOpen;
        private OpenFileDialog OpenNewFile;
        private ToolStripMenuItem ExtensionMenu;
        private ToolStripMenuItem ExtensionMenuRegister;
        private ToolStripMenuItem ExtensionMenuUnregister;
    }
}