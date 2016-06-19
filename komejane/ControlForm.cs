using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Komejane
{
    public partial class ControlForm : Form
    {
        public ControlForm()
        {
            InitializeComponent();

            Config conf = Config.Instance;

            if (Komejane.isRun)
            {
                txtUrlSample.Text = "http://" + conf.ListenHost + ":" + conf.Port;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
