using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BSODView
{
    public partial class DriveSelector : Form
    {
        public DriveSelector()
        {
            InitializeComponent();
        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            if(Directory.Exists(driveSelectorBox.Items[driveSelectorBox.SelectedIndex] + "Windows\\System32\\winevt\\Logs"))
            {
                foreach (Form form in Application.OpenForms)
                {
                    if (form.Name.Equals("Form1"))
                    {
                        if (form is Form1)
                        {
                            Form1 form1 = (Form1)form;
                            form1.LoadEVTXFile(driveSelectorBox.Items[driveSelectorBox.SelectedIndex] + "Windows\\System32\\winevt\\Logs\\System.evtx");
                        }
                    }
                }
                Close();
            }
            else
            {
                ErrorInfoForm infoForm = new ErrorInfoForm();
                infoForm.UpdateText("Selected drive has no Windows folder", new FileNotFoundException());
                infoForm.Show();
            }
        }

        private void DriveSelector_Load(object sender, EventArgs e)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach(DriveInfo drive in drives)
            {
                driveSelectorBox.Items.Add(drive.Name);
            }
        }
    }
}
