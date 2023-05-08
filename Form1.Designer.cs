
namespace BSODView
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.CrashSelector = new System.Windows.Forms.ListBox();
            this.CrashInfo = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.loadButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.helpButton = new System.Windows.Forms.Button();
            this.loadFromSystemButton = new System.Windows.Forms.Button();
            this.loadFromDriveButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CrashSelector
            // 
            this.CrashSelector.Font = new System.Drawing.Font("Lucida Console", 9.25F);
            this.CrashSelector.FormattingEnabled = true;
            this.CrashSelector.ItemHeight = 12;
            this.CrashSelector.Location = new System.Drawing.Point(12, 36);
            this.CrashSelector.Name = "CrashSelector";
            this.CrashSelector.Size = new System.Drawing.Size(162, 352);
            this.CrashSelector.TabIndex = 0;
            this.CrashSelector.SelectedIndexChanged += new System.EventHandler(this.CrashSelector_SelectedIndexChanged);
            // 
            // CrashInfo
            // 
            this.CrashInfo.Font = new System.Drawing.Font("Lucida Console", 9.25F);
            this.CrashInfo.FormattingEnabled = true;
            this.CrashInfo.HorizontalScrollbar = true;
            this.CrashInfo.ItemHeight = 12;
            this.CrashInfo.Location = new System.Drawing.Point(180, 12);
            this.CrashInfo.Name = "CrashInfo";
            this.CrashInfo.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.CrashInfo.Size = new System.Drawing.Size(498, 376);
            this.CrashInfo.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(7, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "BSODView RC3";
            // 
            // loadButton
            // 
            this.loadButton.Location = new System.Drawing.Point(348, 394);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(162, 22);
            this.loadButton.TabIndex = 3;
            this.loadButton.Text = "Load from file";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "xml";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Windows Event Logs|*.evtx";
            this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
            // 
            // helpButton
            // 
            this.helpButton.Location = new System.Drawing.Point(516, 394);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(162, 22);
            this.helpButton.TabIndex = 4;
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // loadFromSystemButton
            // 
            this.loadFromSystemButton.Location = new System.Drawing.Point(180, 394);
            this.loadFromSystemButton.Name = "loadFromSystemButton";
            this.loadFromSystemButton.Size = new System.Drawing.Size(162, 22);
            this.loadFromSystemButton.TabIndex = 5;
            this.loadFromSystemButton.Text = "Load from System";
            this.loadFromSystemButton.UseVisualStyleBackColor = true;
            this.loadFromSystemButton.Click += new System.EventHandler(this.loadFromSystemButton_Click);
            // 
            // loadFromDriveButton
            // 
            this.loadFromDriveButton.Location = new System.Drawing.Point(12, 394);
            this.loadFromDriveButton.Name = "loadFromDriveButton";
            this.loadFromDriveButton.Size = new System.Drawing.Size(162, 22);
            this.loadFromDriveButton.TabIndex = 6;
            this.loadFromDriveButton.Text = "Load from Drive";
            this.loadFromDriveButton.UseVisualStyleBackColor = true;
            this.loadFromDriveButton.Click += new System.EventHandler(this.loadFromDriveButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 424);
            this.Controls.Add(this.loadFromDriveButton);
            this.Controls.Add(this.loadFromSystemButton);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.loadButton);
            this.Controls.Add(this.CrashInfo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CrashSelector);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "BSODView";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox CrashSelector;
        private System.Windows.Forms.ListBox CrashInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button loadFromSystemButton;
        private System.Windows.Forms.Button loadFromDriveButton;
    }
}

