using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IHardwareGXN.GXNCard;

namespace HardwareGXN.GENAxisCard
{
    public partial class FrmGEN : Form
    {
        IGXNInterface iGxnInterface = null;

        public FrmGEN()
        {
            InitializeComponent();
        }

        public FrmGEN(IGXNInterface gxnInterface) : this()
        {
            iGxnInterface = gxnInterface;
            //iGxnInterface = new GSNAxisController();
        }
    }
}
