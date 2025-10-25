using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H5509_V2
{
    public partial class YieldRateShow : UserControl
    {
        public int[] dut_8Pass = new int[8];
        public int[] dut_8Fail = new int[8];

        private Control[] txt_passs = new Control[8];
        private Control[] txt_fails = new Control[8];
        private Control[] txt_yields = new Control[8];


        public YieldRateShow()
        {
            InitializeComponent();

            txt_passs = new Control[] { txt_pass1, txt_pass2, txt_pass3, txt_pass4, txt_pass5, txt_pass6, txt_pass7, txt_pass8 };
            txt_fails = new Control[] { txt_fail1, txt_fail2, txt_fail3, txt_fail4, txt_fail5, txt_fail6, txt_fail7, txt_fail8 };
            txt_yields = new Control[] { txt_yield1, txt_yield2, txt_yield3, txt_yield4, txt_yield5, txt_yield6, txt_yield7, txt_yield8 };
        }

        public void ShowYieldRate()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    for (int i = 0; i < 8; i++)
                    {
                        txt_passs[i].Text = dut_8Pass[i].ToString();
                        txt_fails[i].Text = dut_8Fail[i].ToString();
                        if (dut_8Pass[i] == 0 && dut_8Fail[i] == 0) txt_yields[i].Text = "0";
                        else
                        {
                            float yield = (dut_8Pass[i] / (float)(dut_8Pass[i] + dut_8Fail[i]) * 100);
                            if (yield == 100) txt_yields[i].Text = "100";
                            else txt_yields[i].Text = yield.ToString("0.00");
                        }
                    }
                }));
            }
            catch { }
        }
    }
}
