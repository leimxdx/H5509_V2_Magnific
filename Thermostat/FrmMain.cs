using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thermostat
{
    public partial class FrmMain : Form
    {
        private DTMHelper _DTMHelper;

        public FrmMain()
        {
            InitializeComponent();
        }

        public FrmMain(DTMHelper dTMHelper) : this()
        {
            _DTMHelper = dTMHelper;
        }
    }
}
