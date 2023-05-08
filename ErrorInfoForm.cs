using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BSODView
{
    public partial class ErrorInfoForm : Form
    {
        public ErrorInfoForm()
        {
            InitializeComponent();
        }

        private void ErrorInfo_Load(object sender, EventArgs e)
        {

        }

        public void UpdateText(string message, Exception e)
        {
            errorDataBox.Text = message + "\r\n" + e.Message + "\r\nThe stack trace is below:\r\n" + e.StackTrace;
        }
    }
}
