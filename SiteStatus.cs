using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class SiteStatus
    {
        public string Site名称 { get; set; }

        public string[] Site1 = new string[8];
        public string[] Site2 = new string[8];
        public string[] Site3 = new string[8];
        public string[] Site4 = new string[8];
        public string[] Site5 = new string[8];
        public string[] Site6 = new string[8];
        public string[] Site7 = new string[8];
        public string[] Site8 = new string[8];
        public string[] Site9 = new string[8];
        public string[] Site10 = new string[8];
        public string[] Site11 = new string[8];
        public string[] Site12 = new string[8];
        public string[] Site13 = new string[8];
        public string[] Site14 = new string[8];
        public string[] Site15 = new string[8];
        public string[] Site16 = new string[8];

        public string GetSiteValue(int site)
        {
            switch (site)
            {
                case 0: return string.Join("", Site1);
                case 1: return string.Join("", Site2);
                case 2: return string.Join("", Site3);
                case 3: return string.Join("", Site4);
                case 4: return string.Join("", Site5);
                case 5: return string.Join("", Site6);
                case 6: return string.Join("", Site7);
                case 7: return string.Join("", Site8);
                case 8: return string.Join("", Site9);
                case 9: return string.Join("", Site10);
                case 10: return string.Join("", Site11);
                case 11: return string.Join("", Site12);
                case 12: return string.Join("", Site13);
                case 13: return string.Join("", Site14);
                case 14: return string.Join("", Site15);
                case 15: return string.Join("", Site16);
                default: return "Error";
            }
        }

        public string GetSiteNum(int site, int dut)
        {
            switch (site)
            {
                case 0: return Site1[dut];
                case 1: return Site2[dut];
                case 2: return Site3[dut];
                case 3: return Site4[dut];
                case 4: return Site5[dut];
                case 5: return Site6[dut];
                case 6: return Site7[dut];
                case 7: return Site8[dut];
                case 8: return Site9[dut];
                case 9: return Site10[dut];
                case 10: return Site11[dut];
                case 11: return Site12[dut];
                case 12: return Site13[dut];
                case 13: return Site14[dut];
                case 14: return Site15[dut];
                case 15: return Site16[dut];
                default: return "Error";
            }
        }
        public string SetSiteNum(int site, int dut, string value)
        {
            switch (site)
            {
                case 0: return Site1[dut] = value;
                case 1: return Site2[dut] = value;
                case 2: return Site3[dut] = value;
                case 3: return Site4[dut] = value;
                case 4: return Site5[dut] = value;
                case 5: return Site6[dut] = value;
                case 6: return Site7[dut] = value;
                case 7: return Site8[dut] = value;
                case 8: return Site9[dut] = value;
                case 9: return Site10[dut] = value;
                case 10: return Site11[dut] = value;
                case 11: return Site12[dut] = value;
                case 12: return Site13[dut] = value;
                case 13: return Site14[dut] = value;
                case 14: return Site15[dut] = value;
                case 15: return Site16[dut] = value;
                default: return "Error";
            }
        }

        public void SetSiteNum(int site, string value)
        {
            switch (site)
            {
                case 0: Site1 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 1: Site2 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 2: Site3 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 3: Site4 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 4: Site5 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 5: Site6 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 6: Site7 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 7: Site8 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 8: Site9 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 9: Site10 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 10: Site11 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 11: Site12 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 12: Site13 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 13: Site14 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 14: Site15 = new string[] { value, value, value, value, value, value, value, value }; break;
                case 15: Site16 = new string[] { value, value, value, value, value, value, value, value }; break;
                default: break;
            }
        }
    }
}
