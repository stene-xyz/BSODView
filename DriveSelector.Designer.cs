
namespace BSODView
{
    partial class DriveSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DriveSelector));
            this.driveSelectorBox = new System.Windows.Forms.ListBox();
            this.selectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // driveSelectorBox
            // 
            this.driveSelectorBox.FormattingEnabled = true;
            this.driveSelectorBox.Location = new System.Drawing.Point(13, 13);
            this.driveSelectorBox.Name = "driveSelectorBox";
            this.driveSelectorBox.Size = new System.Drawing.Size(222, 199);
            this.driveSelectorBox.TabIndex = 0;
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(12, 218);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(223, 23);
            this.selectButton.TabIndex = 1;
            this.selectButton.Text = "Select";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.selectButton_Click);
            // 
            // DriveSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(247, 247);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.driveSelectorBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "DriveSelector";
            this.ShowIcon = false;
            this.Text = "Select a Drive";
            this.Load += new System.EventHandler(this.DriveSelector_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox driveSelectorBox;
        private System.Windows.Forms.Button selectButton;
    }
}