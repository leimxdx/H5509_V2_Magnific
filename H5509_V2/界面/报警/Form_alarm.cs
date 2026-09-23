using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using H5509_V2;
using SQLiteHelper;
using Sunny.UI.Win32;
using TextFileLogger;
using static H5509_V2.APPmaster;

namespace H5509_alarm
{
    public partial class Form_alarm : Form
    {
        //创建GXN轴卡对象

        //private WriteData writeDataHelper = WriteData.GetWriteDataInstance();

        public ConcurrentQueue<string> QueueCode = new ConcurrentQueue<string>();

        public Task RefreshTask;

        public bool 报警已关闭 = true;
        public int AlarmRows { get { return dataGridView_alarm.Rows.Count; } }

        public Form_alarm()
        {
            InitializeComponent();

            string currentDirectory = Directory.GetCurrentDirectory();
            //writeDataHelper.RunLog("当前工作目录: " + currentDirectory);

            try
            {
                using (var workbook = new XLWorkbook("H5509_alarm.xlsx"))
                {
                    var worksheet = workbook.Worksheet(1); // 获取第一个工作表


                    int rowCount = worksheet.LastRowUsed().RowNumber();

                    for (int i = 1; i <= rowCount; i++)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells[0].Value = worksheet.Cell(i, 2).Value;
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells[1].Value = worksheet.Cell(i, 4).Value;
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells[2].Value = worksheet.Cell(i, 5).Value;
                        dataGridView_all.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                string error = "打开文件失败：H5509_alarm.xlsx，详细信息：" + ex.Message;
                RunLog(error);
                MessageBox.Show(error, "异常", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        bool r = false;
        bool y = false;
        bool g = false;
        bool v = false;
        //0灭，1亮，2闪 
        public void AlarmLight(int red, int yellow, int green, string voice)
        {
            r = false;
            switch (red)
            {
                case 0: motionEx.SetDo("Y012_红灯", false); break;
                case 1: motionEx.SetDo("Y012_红灯", true); break;
                case 2: r = true; break;
            }
            y = false;
            switch (yellow)
            {
                case 0: motionEx.SetDo("Y013_黄灯", false); break;
                case 1: motionEx.SetDo("Y013_黄灯", true); break;
                case 2: y = true; break;
            }

            g = false;
            switch (green)
            {
                case 0: motionEx.SetDo("Y014_绿灯", false); break;
                case 1: motionEx.SetDo("Y014_绿灯", true); break;
                case 2: g = true; break;
            }
            v = true;
            switch (voice)
            {
                case "音乐1": motionEx.SetDo("Y036_报警音乐1", true); break;
                case "音乐2": motionEx.SetDo("Y037_报警音乐2", true); break;
                case "音乐3": motionEx.SetDo("Y038_报警音乐3", true); break;
                case "音乐4": motionEx.SetDo("Y039_报警音乐4", true); break;
                default:
                    {
                        v = false;
                        motionEx.SetDo("Y036_报警音乐1", false);
                        motionEx.SetDo("Y037_报警音乐2", false);
                        motionEx.SetDo("Y038_报警音乐3", false);
                        motionEx.SetDo("Y039_报警音乐4", false);
                    }
                    break;
            }
        }


        private void dataGridView_alarm_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            AlarmLight(2, 0, 0, 报警状态灯设置.报警音乐); //添加报警
        }
        private void dataGridView_alarm_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (dataGridView_alarm.Rows.Count == 0)
            {
                AlarmLight(0, 0, 1, "无音乐"); //取消报警
                this.Hide();
            }
        }
        //Control.Enabled=false报警闪灯
        //Control.Enabled=true
        bool light = true;
        private void timer_light_Tick(object sender, EventArgs e)
        {
            if (r) motionEx.SetDo("Y012_红灯", light);
            if (y) motionEx.SetDo("Y013_黄灯", light);
            if (g) motionEx.SetDo("Y014_绿灯", light);

            //可直接访问的Control
            string cyan = "Comm,Sys,Temp,Lonizer,SafeDoor,Yield,Other,Cylinder,Shuttle1,Shuttle2,TranArmL,TranArmR,CatchArm,TestArm";
            cyan += ",panel_TestArmSuck,panel_TranArmLSuck,panel_TranArmRSuck";
            string lawnGreen = "Empty,Loader,Auto1,Auto2,Fix1,Fix2,Fix3";
            string salmon = "Hot";
            foreach (Control c in Controls)
            {
                if (cyan.Contains(c.Name) && !c.Enabled)
                {
                    if (light) c.BackColor = Color.Red;
                    else c.BackColor = Color.Cyan;
                }
                if (lawnGreen.Contains(c.Name) && !c.Enabled)
                {
                    if (light) c.BackColor = Color.Red;
                    else c.BackColor = Color.LawnGreen;
                }
                if (salmon.Contains(c.Name) && !c.Enabled)
                {
                    if (light) c.BackColor = Color.Red;
                    else c.BackColor = Color.Salmon;
                }
            }

            //套一层的Control
            string transparent = "TranArmLSuck1,TranArmLSuck2,TranArmLSuck3,TranArmLSuck4,TranArmRSuck1,TranArmRSuck2,TranArmRSuck3,TranArmRSuck4";
            transparent += ",TestArmSuck1,TestArmSuck2,TestArmSuck3,TestArmSuck4,TestArmSuck5,TestArmSuck6,TestArmSuck7,TestArmSuck8";
            string cyan2 = ",panel_S00,panel_S01,panel_S02,panel_S03,panel_S04,panel_S05,panel_S06,panel_S07,panel_S08,panel_S09,panel_S10,panel_S11,panel_S12,panel_S13,panel_S14,panel_S15";
            Control.ControlCollection[] p = { panel_TestArmSuck.Controls, panel_TranArmLSuck.Controls, panel_TranArmRSuck.Controls
                                                , panel_1.Controls, panel_2.Controls, panel_3.Controls, panel_4.Controls };
            for (int i = 0; i < p.Length; i++)
            {
                foreach (Control c in p[i])
                {
                    if (transparent.Contains(c.Name) && !c.Enabled)
                    {
                        if (light) c.BackColor = Color.Red;
                        else c.BackColor = Color.Transparent;
                    }
                    if (cyan2.Contains(c.Name) && !c.Enabled)
                    {
                        if (light) c.BackColor = Color.Red;
                        else c.BackColor = Color.Cyan;
                    }
                }
            }

            //套2层的Control
            Control.ControlCollection[] dut = { panel_1.Controls, panel_2.Controls, panel_3.Controls, panel_4.Controls };
            for (int i = 0; i < dut.Length; i++)
            {
                foreach (Control c in dut[i])
                {
                    foreach (Control c2 in c.Controls)
                    {
                        if (!c2.Enabled)
                        {
                            if (light) c2.BackColor = Color.Red;
                            else c2.BackColor = Color.Transparent;
                        }
                    }
                }
            }

            light = !light;
        }


        public void ShowCode(string[] alarmCode, bool alarm)
        {
            foreach (string c in alarmCode)
            {
                ShowCode(c, alarm);
            }
        }



        string S_Comm = "";
        string S_Sys = "";
        string S_Temp = "";
        string S_Lonizer = "";
        string S_SafeDoor = "";
        string S_Yield = "";
        string S_Other = "";
        string S_Cylinder = "";

        string S_TranArmLSuck = "";
        string S_TranArmLSuck1 = "";
        string S_TranArmLSuck2 = "";
        string S_TranArmLSuck3 = "";
        string S_TranArmLSuck4 = "";
        string S_TranArmRSuck = "";
        string S_TranArmRSuck1 = "";
        string S_TranArmRSuck2 = "";
        string S_TranArmRSuck3 = "";
        string S_TranArmRSuck4 = "";
        string S_TestArmSuck = "";
        string S_TestArmSuck1 = "";
        string S_TestArmSuck2 = "";
        string S_TestArmSuck3 = "";
        string S_TestArmSuck4 = "";
        string S_TestArmSuck5 = "";
        string S_TestArmSuck6 = "";
        string S_TestArmSuck7 = "";
        string S_TestArmSuck8 = "";

        string S_Shuttle1 = "";
        string S_Shuttle2 = "";
        string S_TranArmL = "";
        string S_TestArm = "";
        string S_TranArmR = "";
        string S_CatchArm = "";

        string S_Empty = "";
        string S_Loader = "";
        string S_Hot = "";
        string S_Auto1 = "";
        string S_Auto2 = "";
        string S_Fix1 = "";
        string S_Fix2 = "";
        string S_Fix3 = "";

        string[,] S_dut = new string[16, 8];


        public void ShowCode(string alarmCode, bool alarm)
        {
            //if (alarm) this.Show();
            Control.ControlCollection[] dut = { panel_1.Controls, panel_2.Controls, panel_3.Controls, panel_4.Controls };
            this.Invoke(new Action(() =>
            {
                switch (alarmCode)
                {
                    //气缸异常
                    case "1000000":
                    case "1000001":
                    case "1001000":
                    case "1001001":
                    case "1002000":
                    case "1002001":
                    case "1003000":
                    case "1003001":
                    case "1004000":
                    case "1004001":
                    case "1005000":
                    case "1005001":
                    case "1006000":
                    case "1006001":
                    case "1007000":
                    case "1007001":
                    case "1008000":
                    case "1008001":
                    case "1009000":
                    case "1009001":
                    case "1010000":
                    case "1010001":
                    case "1010002":
                    case "1010003":
                    case "1011000":
                    case "1011001":
                    case "1014000":
                    case "1014001":
                    case "1015000":
                    case "1015001":
                    case "1016000":
                    case "1016001":
                    case "1017000":
                    case "1017001":
                    case "1018000":
                    case "1018001":
                    case "1019000":
                    case "1019001":
                    case "1020000":
                    case "1020001":
                    case "1021000":
                    case "1021001":
                    case "1022000":
                    case "1022001":
                    case "1023000":
                    case "1023001":
                    case "1024000":
                    case "1024001":
                    case "1025000":
                    case "1025001":
                    case "1026000":
                    case "1026001":
                    case "1027000":
                    case "1027001":
                    case "1028000":
                    case "1028001":
                    case "1029000":
                    case "1029001":
                    case "1030000":
                    case "1030001":
                    case "1031000":
                    case "1031001":
                    case "1032000":
                    case "1032001":
                    case "1033000":
                    case "1033001":
                    case "1034000":
                    case "1034001":
                    case "1035000":
                    case "1035001":
                    case "1036000":
                    case "1036001":
                    case "1037000":
                    case "1037001":
                    case "1038000":
                    case "1038001":
                    case "1039000":
                    case "1039001":
                    case "1040000":
                    case "1040001":
                    case "1041000":
                    case "1041001":
                    case "1042000":
                    case "1042001":
                    case "1043000":
                    case "1043001":
                    case "1044000":
                    case "1044001":
                    case "1045000":
                    case "1045001":
                    case "1047000":
                    case "1047001":
                    case "800019":
                    case "801019":
                    case "802019":
                    case "803019":
                    case "804019":
                    case "805019":
                    case "806019":
                    case "807019":
                    case "808019":
                    case "809019":
                    case "810019":
                    case "811019":
                    case "812019":
                    case "813019":
                    case "814019":
                    case "815019":
                        if (alarm)
                        {
                            S_Cylinder += alarmCode + ",";
                        }
                        else
                        {
                            S_Cylinder = S_Cylinder.Replace(alarmCode + ",", "");
                        }
                        if (S_Cylinder.Length > 0)
                        {
                            Cylinder.Enabled = false;
                        }
                        else
                        {
                            Cylinder.Enabled = true;
                            Cylinder.BackColor = Color.Cyan;
                        }
                        break;


                    //入料手臂
                    case "2000000":
                    case "2000001":
                    case "2000002":
                    case "2000003":
                    case "2000004":
                    case "2000005":
                    case "2000006":
                    case "2001000":
                    case "2001001":
                    case "2001002":
                    case "2001003":
                    case "2001004":
                    case "2001005":
                    case "2001006":
                    case "2008000":
                    case "2008001":
                    case "2008002":
                    case "2008003":
                    case "2008004":
                    case "2008005":
                    case "2008006":
                    case "2009000":
                    case "2009001":
                    case "2009002":
                    case "2009003":
                    case "2009004":
                    case "2009005":
                    case "2009006":
                    case "2010000":
                    case "2010001":
                    case "2010002":
                    case "2010003":
                    case "2010004":
                    case "2010005":
                    case "2010006":
                    case "2011000":
                    case "2011001":
                    case "2011002":
                    case "2011003":
                    case "2011004":
                    case "2011005":
                    case "2011006":
                    case "2012000":
                    case "2012001":
                    case "2012002":
                    case "2012003":
                    case "2012004":
                    case "2012005":
                    case "2012006":
                        if (alarm)
                        {
                            S_TranArmL += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmL = S_TranArmL.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmL.Length > 0)
                        {
                            TranArmL.Enabled = false;
                        }
                        else
                        {
                            TranArmL.Enabled = true;
                            TranArmL.BackColor = Color.Cyan;
                        }
                        break;

                    //测试手臂
                    case "2002000":
                    case "2002001":
                    case "2002002":
                    case "2002003":
                    case "2002004":
                    case "2002005":
                    case "2002006":
                    case "2007000":
                    case "2007001":
                    case "2007002":
                    case "2007003":
                    case "2007004":
                    case "2007005":
                    case "2007006":
                    case "1400468":
                        if (alarm)
                        {
                            S_TestArm += alarmCode + ",";
                        }
                        else
                        {
                            S_TestArm = S_TestArm.Replace(alarmCode + ",", "");
                        }
                        if (S_TestArm.Length > 0)
                        {
                            TestArm.Enabled = false;
                        }
                        else
                        {
                            TestArm.Enabled = true;
                            TestArm.BackColor = Color.Cyan;

                        }
                        break;


                    //出料手臂
                    case "2003000":
                    case "2003001":
                    case "2003002":
                    case "2003003":
                    case "2003004":
                    case "2003005":
                    case "2003006":
                    case "2004000":
                    case "2004001":
                    case "2004002":
                    case "2004003":
                    case "2004004":
                    case "2004005":
                    case "2004006":
                    case "2013000":
                    case "2013001":
                    case "2013002":
                    case "2013003":
                    case "2013004":
                    case "2013005":
                    case "2013006":
                    case "2014000":
                    case "2014001":
                    case "2014002":
                    case "2014003":
                    case "2014004":
                    case "2014005":
                    case "2014006":
                    case "2015000":
                    case "2015001":
                    case "2015002":
                    case "2015003":
                    case "2015004":
                    case "2015005":
                    case "2015006":
                    case "2016000":
                    case "2016001":
                    case "2016002":
                    case "2016003":
                    case "2016004":
                    case "2016005":
                    case "2016006":
                    case "2017000":
                    case "2017001":
                    case "2017002":
                    case "2017003":
                    case "2017004":
                    case "2017005":
                    case "2017006":
                        if (alarm)
                        {
                            S_TranArmR += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmR = S_TranArmR.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmR.Length > 0)
                        {
                            TranArmR.Enabled = false;
                        }
                        else
                        {
                            TranArmR.Enabled = true;
                            TranArmR.BackColor = Color.Cyan;
                        }
                        break;

                    //空盘手臂
                    case "2005000":
                    case "2005001":
                    case "2005002":
                    case "2005003":
                    case "2005004":
                    case "2005005":
                    case "2005006":
                    case "3000000":
                    case "3000001":
                    case "3000002":
                        if (alarm)
                        {
                            S_CatchArm += alarmCode + ",";
                        }
                        else
                        {
                            S_CatchArm = S_CatchArm.Replace(alarmCode + ",", "");
                        }
                        if (S_CatchArm.Length > 0)
                        {
                            CatchArm.Enabled = false;
                        }
                        else
                        {
                            CatchArm.Enabled = true;
                            CatchArm.BackColor = Color.Cyan;
                        }
                        break;

                    //梭车前后
                    case "2006000":
                    case "2006001":
                    case "2006002":
                    case "2006003":
                    case "2006004":
                    case "2006005":
                    case "2006006":
                    case "5000000":
                        if (alarm)
                        {
                            S_Shuttle1 += alarmCode + ",";
                            S_Shuttle2 += alarmCode + ",";
                        }
                        else
                        {
                            S_Shuttle1 = S_Shuttle1.Replace(alarmCode + ",", "");
                            S_Shuttle2 = S_Shuttle2.Replace(alarmCode + ",", "");
                        }
                        if (S_Shuttle1.Length > 0)
                        {
                            Shuttle1.Enabled = false;
                        }
                        else
                        {
                            Shuttle1.Enabled = true;
                            Shuttle1.BackColor = Color.Cyan;
                        }
                        if (S_Shuttle2.Length > 0)
                        {
                            Shuttle2.Enabled = false;
                        }
                        else
                        {
                            Shuttle2.Enabled = true;
                            Shuttle2.BackColor = Color.Cyan;
                        }
                        break;

                    //梭车前
                    case "5430000":
                    case "5430001":
                    case "5430002":
                    case "5430003":
                    case "5440000":
                    case "5440001":
                    case "5440002":
                    case "5440003":
                    case "5450000":
                    case "5450001":
                    case "5450002":
                    case "5450003":
                    case "5460000":
                    case "5460001":
                    case "5460002":
                    case "5460003":
                    case "5430101":
                    case "5430102":
                    case "5430103":
                    case "5440100":
                    case "5440101":
                    case "5440102":
                    case "5440103":
                    case "5450100":
                    case "5450101":
                    case "5450102":
                    case "5450103":
                    case "5460100":
                    case "5460101":
                    case "5460102":
                    case "5460103":
                        if (alarm)
                        {
                            S_Shuttle1 += alarmCode + ",";
                        }
                        else
                        {
                            S_Shuttle1 = S_Shuttle1.Replace(alarmCode + ",", "");
                        }
                        if (S_Shuttle1.Length > 0)
                        {
                            Shuttle1.Enabled = false;
                        }
                        else
                        {
                            Shuttle1.Enabled = true;
                            Shuttle1.BackColor = Color.Cyan;
                        }
                        break;

                    //梭车后
                    case "5470002":
                    case "5470003":
                    case "5470004":
                    case "5470005":
                    case "5480002":
                    case "5480003":
                    case "5480004":
                    case "5480005":
                    case "5490002":
                    case "5490003":
                    case "5490004":
                    case "5490005":
                    case "5500002":
                    case "5500003":
                    case "5500004":
                    case "5500005":
                    case "5470100":
                    case "5470101":
                    case "5470102":
                    case "5470103":
                    case "5480100":
                    case "5480101":
                    case "5480102":
                    case "5480103":
                    case "5490100":
                    case "5490101":
                    case "5490102":
                    case "5490103":
                    case "5500100":
                    case "5500101":
                    case "5500102":
                    case "5500103":
                        if (alarm)
                        {
                            S_Shuttle2 += alarmCode + ",";
                        }
                        else
                        {
                            S_Shuttle2 = S_Shuttle2.Replace(alarmCode + ",", "");
                        }
                        if (S_Shuttle2.Length > 0)
                        {
                            Shuttle2.Enabled = false;
                        }
                        else
                        {
                            Shuttle2.Enabled = true;
                            Shuttle2.BackColor = Color.Cyan;
                        }
                        break;

                    //空盘区
                    case "2018000":
                    case "2018001":
                    case "2018002":
                    case "2018003":
                    case "2018004":
                    case "2018005":
                    case "2018006":
                    case "4018004":
                    case "4018006":
                    case "4018008":
                        if (alarm)
                        {
                            S_Empty += alarmCode + ",";
                        }
                        else
                        {
                            S_Empty = S_Empty.Replace(alarmCode + ",", "");
                        }
                        if (S_Empty.Length > 0)
                        {
                            Empty.Enabled = false;
                        }
                        else
                        {
                            Empty.Enabled = true;
                            Empty.BackColor = Color.LawnGreen;
                        }
                        break;

                    //待测盘
                    case "2019000":
                    case "2019001":
                    case "2019002":
                    case "2019003":
                    case "2019004":
                    case "2019005":
                    case "2019006":
                    case "4019018":
                    case "4019016":
                    case "4019003":
                    case "4019002":
                    case "4019008":
                    case "1400472":
                        if (alarm)
                        {
                            S_Loader += alarmCode + ",";
                        }
                        else
                        {
                            S_Loader = S_Loader.Replace(alarmCode + ",", "");
                        }
                        if (S_Loader.Length > 0)
                        {
                            Loader.Enabled = false;
                        }
                        else
                        {


                            Loader.Enabled = true;
                            Loader.BackColor = Color.LawnGreen;



                        }
                        break;

                    //自动盘1
                    case "2020000":
                    case "2020001":
                    case "2020002":
                    case "2020003":
                    case "2020004":
                    case "2020005":
                    case "2020006":
                    case "4020009":
                    case "4020019":
                    case "4020010":
                    case "4020017":
                    case "4020001":
                    case "4020008":
                        if (alarm)
                        {
                            S_Auto1 += alarmCode + ",";
                        }
                        else
                        {
                            S_Auto1 = S_Auto1.Replace(alarmCode + ",", "");
                        }
                        if (S_Auto1.Length > 0)
                        {
                            Auto1.Enabled = false;
                        }
                        else
                        {
                            Auto1.Enabled = true;
                            Auto1.BackColor = Color.LawnGreen;
                        }
                        break;

                    //自动盘2
                    case "2021000":
                    case "2021001":
                    case "2021002":
                    case "2021003":
                    case "2021004":
                    case "2021005":
                    case "2021006":
                    case "4021009":
                    case "4021010":
                    case "4021017":
                    case "4021001":
                    case "4021008":
                        if (alarm)
                        {
                            S_Auto2 += alarmCode + ",";
                        }
                        else
                        {
                            S_Auto2 = S_Auto2.Replace(alarmCode + ",", "");
                        }
                        if (S_Auto2.Length > 0)
                        {
                            Auto2.Enabled = false;
                        }
                        else
                        {
                            Auto2.Enabled = true;
                            Auto2.BackColor = Color.LawnGreen;
                        }
                        break;

                    //手动盘1
                    case "4030009":
                    case "4030010":
                    case "4030011":
                    case "4030012":
                    case "4030013":
                        if (alarm)
                        {
                            S_Fix1 += alarmCode + ",";
                        }
                        else
                        {
                            S_Fix1 = S_Fix1.Replace(alarmCode + ",", "");
                        }
                        if (S_Fix1.Length > 0)
                        {
                            Fix1.Enabled = false;
                        }
                        else
                        {
                            Fix1.Enabled = true;
                            Fix1.BackColor = Color.LawnGreen;
                        }
                        break;

                    //手动盘2
                    case "4031009":
                    case "4031010":
                    case "4031011":
                    case "4031012":
                    case "4031013":
                        if (alarm)
                        {
                            S_Fix2 += alarmCode + ",";
                        }
                        else
                        {
                            S_Fix2 = S_Fix2.Replace(alarmCode + ",", "");
                        }
                        if (S_Fix2.Length > 0)
                        {
                            Fix2.Enabled = false;
                        }
                        else
                        {
                            Fix2.Enabled = true;
                            Fix2.BackColor = Color.LawnGreen;
                        }
                        break;

                    //手动盘3
                    case "4032009":
                    case "4032010":
                    case "4032011":
                    case "4032012":
                    case "4032013":
                        if (alarm)
                        {
                            S_Fix3 += alarmCode + ",";
                        }
                        else
                        {
                            S_Fix3 = S_Fix3.Replace(alarmCode + ",", "");
                        }
                        if (S_Fix3.Length > 0)
                        {
                            Fix3.Enabled = false;
                        }
                        else
                        {
                            Fix3.Enabled = true;
                            Fix3.BackColor = Color.LawnGreen;
                        }
                        break;

                    //入料吸嘴1
                    case "3010000":
                    case "3010001":
                    case "3010002":
                    case "1400460":
                        if (alarm)
                        {
                            S_TranArmLSuck1 += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmLSuck1 = S_TranArmLSuck1.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmLSuck1.Length > 0)
                        {
                            TranArmLSuck1.Enabled = false;
                        }
                        else
                        {
                            TranArmLSuck1.Enabled = true;
                            TranArmLSuck1.BackColor = Color.Transparent;
                        }
                        break;

                    //入料吸嘴2
                    case "3020000":
                    case "3020001":
                    case "3020002":
                    case "1400461":
                        if (alarm)
                        {
                            S_TranArmLSuck2 += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmLSuck2 = S_TranArmLSuck2.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmLSuck2.Length > 0)
                        {
                            TranArmLSuck2.Enabled = false;
                        }
                        else
                        {
                            TranArmLSuck2.Enabled = true;
                            TranArmLSuck2.BackColor = Color.Transparent;
                        }
                        break;


                    //入料吸嘴3
                    case "3030000":
                    case "3030001":
                    case "3030002":
                    case "1400462":
                        if (alarm)
                        {
                            S_TranArmLSuck3 += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmLSuck3 = S_TranArmLSuck3.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmLSuck3.Length > 0)
                        {
                            TranArmLSuck3.Enabled = false;
                        }
                        else
                        {
                            TranArmLSuck3.Enabled = true;
                            TranArmLSuck3.BackColor = Color.Transparent;
                        }
                        break;

                    //入料吸嘴4
                    case "3040000":
                    case "3040001":
                    case "3040002":
                    case "1400463":
                        if (alarm)
                        {
                            S_TranArmLSuck4 += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmLSuck4 = S_TranArmLSuck4.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmLSuck4.Length > 0)
                        {
                            TranArmLSuck4.Enabled = false;
                        }
                        else
                        {
                            TranArmLSuck4.Enabled = true;
                            TranArmLSuck4.BackColor = Color.Transparent;
                        }
                        break;

                    //出料吸嘴1
                    case "3050000":
                    case "3050001":
                    case "3050002":
                    case "1400464":
                        if (alarm)
                        {
                            S_TranArmRSuck1 += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmRSuck1 = S_TranArmRSuck1.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmRSuck1.Length > 0)
                        {
                            TranArmRSuck1.Enabled = false;
                        }
                        else
                        {
                            TranArmRSuck1.Enabled = true;
                            TranArmRSuck1.BackColor = Color.Transparent;
                        }
                        break;

                    //出料吸嘴2
                    case "3060000":
                    case "3060001":
                    case "3060002":
                    case "1400465":
                        if (alarm)
                        {
                            S_TranArmRSuck2 += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmRSuck2 = S_TranArmRSuck2.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmRSuck2.Length > 0)
                        {
                            TranArmRSuck2.Enabled = false;
                        }
                        else
                        {
                            TranArmRSuck2.Enabled = true;
                            TranArmRSuck2.BackColor = Color.Transparent;
                        }
                        break;

                    //出料吸嘴3
                    case "3070000":
                    case "3070001":
                    case "3070002":
                    case "1400466":
                        if (alarm)
                        {
                            S_TranArmRSuck3 += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmRSuck3 = S_TranArmRSuck3.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmRSuck3.Length > 0)
                        {
                            TranArmRSuck3.Enabled = false;
                        }
                        else
                        {
                            TranArmRSuck3.Enabled = true;
                            TranArmRSuck3.BackColor = Color.Transparent;
                        }
                        break;

                    //出料吸嘴4
                    case "3080000":
                    case "3080001":
                    case "3080002":
                    case "1400467":
                        if (alarm)
                        {
                            S_TranArmRSuck4 += alarmCode + ",";
                        }
                        else
                        {
                            S_TranArmRSuck4 = S_TranArmRSuck4.Replace(alarmCode + ",", "");
                        }
                        if (S_TranArmRSuck4.Length > 0)
                        {
                            TranArmRSuck4.Enabled = false;
                        }
                        else
                        {
                            TranArmRSuck4.Enabled = true;
                            TranArmRSuck4.BackColor = Color.Transparent;
                        }
                        break;

                    //测试吸嘴1
                    case "3090000":
                    case "3090100":
                    case "3090200":
                    case "3090300":
                    case "3090400":
                    case "3090500":
                    case "3090600":
                    case "3090700":
                    case "3090800":
                    case "3090900":
                    case "3091000":
                    case "3091100":
                    case "3091200":
                    case "3091300":
                    case "3091400":
                    case "3091500":
                    case "3099900":
                    case "3099901":
                    case "3099902":
                        if (alarm)
                        {
                            S_TestArmSuck1 += alarmCode + ",";
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 0] += alarmCode + ",";
                            }
                        }
                        else
                        {
                            S_TestArmSuck1 = S_TestArmSuck1.Replace(alarmCode + ",", "");
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 0] = S_dut[int.Parse(alarmCode.Substring(3, 2)), 0].Replace(alarmCode + ",", "");
                            }
                        }
                        if (S_TestArmSuck1.Length > 0)
                        {
                            TestArmSuck1.Enabled = false;
                        }
                        else
                        {
                            TestArmSuck1.Enabled = true;
                            TestArmSuck1.BackColor = Color.Transparent;
                        }
                        if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                        {
                            if (S_dut[int.Parse(alarmCode.Substring(3, 2)), 0].Length > 0)
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "1"))
                                            {
                                                c2.Enabled = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "1"))
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //测试吸嘴2
                    case "3100000":
                    case "3100100":
                    case "3100200":
                    case "3100300":
                    case "3100400":
                    case "3100500":
                    case "3100600":
                    case "3100700":
                    case "3100800":
                    case "3100900":
                    case "3101000":
                    case "3101100":
                    case "3101200":
                    case "3101300":
                    case "3101400":
                    case "3101500":
                    case "3109900":
                    case "3109901":
                    case "3109902":
                        if (alarm)
                        {
                            S_TestArmSuck2 += alarmCode + ",";
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 1] += alarmCode + ",";
                            }
                        }
                        else
                        {
                            S_TestArmSuck2 = S_TestArmSuck2.Replace(alarmCode + ",", "");
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 1] = S_dut[int.Parse(alarmCode.Substring(3, 2)), 1].Replace(alarmCode + ",", "");
                            }
                        }
                        if (S_TestArmSuck2.Length > 0)
                        {
                            TestArmSuck2.Enabled = false;
                        }
                        else
                        {
                            TestArmSuck2.Enabled = true;
                            TestArmSuck2.BackColor = Color.Transparent;
                        }
                        if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                        {
                            if (S_dut[int.Parse(alarmCode.Substring(3, 2)), 1].Length > 0)
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "2"))
                                            {
                                                c2.Enabled = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "2"))
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //测试吸嘴3
                    case "3110000":
                    case "3110100":
                    case "3110200":
                    case "3110300":
                    case "3110400":
                    case "3110500":
                    case "3110600":
                    case "3110700":
                    case "3110800":
                    case "3110900":
                    case "3111000":
                    case "3111100":
                    case "3111200":
                    case "3111300":
                    case "3111400":
                    case "3111500":
                    case "3119900":
                    case "3119901":
                    case "3119902":
                        if (alarm)
                        {
                            S_TestArmSuck3 += alarmCode + ",";
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 2] += alarmCode + ",";
                            }
                        }
                        else
                        {
                            S_TestArmSuck3 = S_TestArmSuck3.Replace(alarmCode + ",", "");
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 2] = S_dut[int.Parse(alarmCode.Substring(3, 2)), 2].Replace(alarmCode + ",", "");
                            }
                        }
                        if (S_TestArmSuck3.Length > 0)
                        {
                            TestArmSuck3.Enabled = false;
                        }
                        else
                        {
                            TestArmSuck3.Enabled = true;
                            TestArmSuck3.BackColor = Color.Transparent;
                        }
                        if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                        {
                            if (S_dut[int.Parse(alarmCode.Substring(3, 2)), 2].Length > 0)
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "3"))
                                            {
                                                c2.Enabled = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "3"))
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //测试吸嘴4
                    case "3120000":
                    case "3120100":
                    case "3120200":
                    case "3120300":
                    case "3120400":
                    case "3120500":
                    case "3120600":
                    case "3120700":
                    case "3120800":
                    case "3120900":
                    case "3121000":
                    case "3121100":
                    case "3121200":
                    case "3121300":
                    case "3121400":
                    case "3121500":
                    case "3129900":
                    case "3129901":
                    case "3129902":
                        if (alarm)
                        {
                            S_TestArmSuck4 += alarmCode + ",";
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 3] += alarmCode + ",";
                            }
                        }
                        else
                        {
                            S_TestArmSuck4 = S_TestArmSuck4.Replace(alarmCode + ",", "");
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 3] = S_dut[int.Parse(alarmCode.Substring(3, 2)), 3].Replace(alarmCode + ",", "");
                            }
                        }
                        if (S_TestArmSuck4.Length > 0)
                        {
                            TestArmSuck4.Enabled = false;
                        }
                        else
                        {
                            TestArmSuck4.Enabled = true;
                            TestArmSuck4.BackColor = Color.Transparent;
                        }
                        if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                        {
                            if (S_dut[int.Parse(alarmCode.Substring(3, 2)), 3].Length > 0)
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "4"))
                                            {
                                                c2.Enabled = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "4"))
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //测试吸嘴5
                    case "3130000":
                    case "3130100":
                    case "3130200":
                    case "3130300":
                    case "3130400":
                    case "3130500":
                    case "3130600":
                    case "3130700":
                    case "3130800":
                    case "3130900":
                    case "3131000":
                    case "3131100":
                    case "3131200":
                    case "3131300":
                    case "3131400":
                    case "3131500":
                    case "3139900":
                    case "3139901":
                    case "3139902":
                        if (alarm)
                        {
                            S_TestArmSuck5 += alarmCode + ",";
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 4] += alarmCode + ",";
                            }
                        }
                        else
                        {
                            S_TestArmSuck5 = S_TestArmSuck5.Replace(alarmCode + ",", "");
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 4] = S_dut[int.Parse(alarmCode.Substring(3, 2)), 4].Replace(alarmCode + ",", "");
                            }
                        }
                        if (S_TestArmSuck5.Length > 0)
                        {
                            TestArmSuck5.Enabled = false;
                        }
                        else
                        {
                            TestArmSuck5.Enabled = true;
                            TestArmSuck5.BackColor = Color.Transparent;
                        }
                        if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                        {
                            if (S_dut[int.Parse(alarmCode.Substring(3, 2)), 4].Length > 0)
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "5"))
                                            {
                                                c2.Enabled = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "5"))
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //测试吸嘴6
                    case "3140000":
                    case "3140100":
                    case "3140200":
                    case "3140300":
                    case "3140400":
                    case "3140500":
                    case "3140600":
                    case "3140700":
                    case "3140800":
                    case "3140900":
                    case "3141000":
                    case "3141100":
                    case "3141200":
                    case "3141300":
                    case "3141400":
                    case "3141500":
                    case "3149900":
                    case "3149901":
                    case "3149902":
                        if (alarm)
                        {
                            S_TestArmSuck6 += alarmCode + ",";
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 5] += alarmCode + ",";
                            }
                        }
                        else
                        {
                            S_TestArmSuck6 = S_TestArmSuck6.Replace(alarmCode + ",", "");
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 5] = S_dut[int.Parse(alarmCode.Substring(3, 2)), 5].Replace(alarmCode + ",", "");
                            }
                        }
                        if (S_TestArmSuck6.Length > 0)
                        {
                            TestArmSuck6.Enabled = false;
                        }
                        else
                        {
                            TestArmSuck6.Enabled = true;
                            TestArmSuck6.BackColor = Color.Transparent;
                        }
                        if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                        {
                            if (S_dut[int.Parse(alarmCode.Substring(3, 2)), 5].Length > 0)
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "6"))
                                            {
                                                c2.Enabled = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "6"))
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //测试吸嘴7
                    case "3150000":
                    case "3150100":
                    case "3150200":
                    case "3150300":
                    case "3150400":
                    case "3150500":
                    case "3150600":
                    case "3150700":
                    case "3150800":
                    case "3150900":
                    case "3151000":
                    case "3151100":
                    case "3151200":
                    case "3151300":
                    case "3151400":
                    case "3151500":
                    case "3159900":
                    case "3159901":
                    case "3159902":
                        if (alarm)
                        {
                            S_TestArmSuck7 += alarmCode + ",";
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 6] += alarmCode + ",";
                            }
                        }
                        else
                        {
                            S_TestArmSuck7 = S_TestArmSuck7.Replace(alarmCode + ",", "");
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 6] = S_dut[int.Parse(alarmCode.Substring(3, 2)), 6].Replace(alarmCode + ",", "");
                            }
                        }
                        if (S_TestArmSuck7.Length > 0)
                        {
                            TestArmSuck7.Enabled = false;
                        }
                        else
                        {
                            TestArmSuck7.Enabled = true;
                            TestArmSuck7.BackColor = Color.Transparent;
                        }
                        if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                        {
                            if (S_dut[int.Parse(alarmCode.Substring(3, 2)), 6].Length > 0)
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "7"))
                                            {
                                                c2.Enabled = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "7"))
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //测试吸嘴8
                    case "3160000":
                    case "3160100":
                    case "3160200":
                    case "3160300":
                    case "3160400":
                    case "3160500":
                    case "3160600":
                    case "3160700":
                    case "3160800":
                    case "3160900":
                    case "3161000":
                    case "3161100":
                    case "3161200":
                    case "3161300":
                    case "3161400":
                    case "3161500":
                    case "3169900":
                    case "3169901":
                    case "3169902":
                        if (alarm)
                        {
                            S_TestArmSuck8 += alarmCode + ",";
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 7] += alarmCode + ",";
                            }
                        }
                        else
                        {
                            S_TestArmSuck8 = S_TestArmSuck8.Replace(alarmCode + ",", "");
                            if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                            {
                                S_dut[int.Parse(alarmCode.Substring(3, 2)), 7] = S_dut[int.Parse(alarmCode.Substring(3, 2)), 7].Replace(alarmCode + ",", "");
                            }
                        }
                        if (S_TestArmSuck8.Length > 0)
                        {
                            TestArmSuck8.Enabled = false;
                        }
                        else
                        {
                            TestArmSuck8.Enabled = true;
                            TestArmSuck8.BackColor = Color.Transparent;
                        }
                        if (int.Parse(alarmCode.Substring(3, 2)) < 16)
                        {
                            if (S_dut[int.Parse(alarmCode.Substring(3, 2)), 7].Length > 0)
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "8"))
                                            {
                                                c2.Enabled = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < dut.Length; i++)
                                {
                                    foreach (Control c in dut[i])
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(3, 2) + "8"))
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //良率异常
                    case "6010114":
                    case "6010214":
                    case "6010314":
                    case "6010414":
                    case "6010514":
                    case "6010614":
                    case "6010714":
                    case "6010814":
                    case "6020114":
                    case "6020214":
                    case "6020314":
                    case "6020414":
                    case "6020514":
                    case "6020614":
                    case "6020714":
                    case "6020814":
                    case "6030114":
                    case "6030214":
                    case "6030314":
                    case "6030414":
                    case "6030514":
                    case "6030614":
                    case "6030714":
                    case "6030814":
                    case "6040114":
                    case "6040214":
                    case "6040314":
                    case "6040414":
                    case "6040514":
                    case "6040614":
                    case "6040714":
                    case "6040814":
                    case "6050114":
                    case "6050214":
                    case "6050314":
                    case "6050414":
                    case "6050514":
                    case "6050614":
                    case "6050714":
                    case "6050814":
                    case "6060114":
                    case "6060214":
                    case "6060314":
                    case "6060414":
                    case "6060514":
                    case "6060614":
                    case "6060714":
                    case "6060814":
                    case "6070114":
                    case "6070214":
                    case "6070314":
                    case "6070414":
                    case "6070514":
                    case "6070614":
                    case "6070714":
                    case "6070814":
                    case "6080114":
                    case "6080214":
                    case "6080314":
                    case "6080414":
                    case "6080514":
                    case "6080614":
                    case "6080714":
                    case "6080814":
                    case "6090114":
                    case "6090214":
                    case "6090314":
                    case "6090414":
                    case "6090514":
                    case "6090614":
                    case "6090714":
                    case "6090814":
                    case "6100114":
                    case "6100214":
                    case "6100314":
                    case "6100414":
                    case "6100514":
                    case "6100614":
                    case "6100714":
                    case "6100814":
                    case "6110114":
                    case "6110214":
                    case "6110314":
                    case "6110414":
                    case "6110514":
                    case "6110614":
                    case "6110714":
                    case "6110814":
                    case "6120114":
                    case "6120214":
                    case "6120314":
                    case "6120414":
                    case "6120514":
                    case "6120614":
                    case "6120714":
                    case "6120814":
                    case "6130114":
                    case "6130214":
                    case "6130314":
                    case "6130414":
                    case "6130514":
                    case "6130614":
                    case "6130714":
                    case "6130814":
                    case "6140114":
                    case "6140214":
                    case "6140314":
                    case "6140414":
                    case "6140514":
                    case "6140614":
                    case "6140714":
                    case "6140814":
                    case "6150114":
                    case "6150214":
                    case "6150314":
                    case "6150414":
                    case "6150514":
                    case "6150614":
                    case "6150714":
                    case "6150814":
                    case "6160114":
                    case "6160214":
                    case "6160314":
                    case "6160414":
                    case "6160514":
                    case "6160614":
                    case "6160714":
                    case "6160814":
                    case "6011114":
                    case "6011214":
                    case "6011314":
                    case "6011414":
                    case "6011514":
                    case "6011614":
                    case "6011714":
                    case "6011814":
                    case "6021114":
                    case "6021214":
                    case "6021314":
                    case "6021414":
                    case "6021514":
                    case "6021614":
                    case "6021714":
                    case "6021814":
                    case "6031114":
                    case "6031214":
                    case "6031314":
                    case "6031414":
                    case "6031514":
                    case "6031614":
                    case "6031714":
                    case "6031814":
                    case "6041114":
                    case "6041214":
                    case "6041314":
                    case "6041414":
                    case "6041514":
                    case "6041614":
                    case "6041714":
                    case "6041814":
                    case "6051114":
                    case "6051214":
                    case "6051314":
                    case "6051414":
                    case "6051514":
                    case "6051614":
                    case "6051714":
                    case "6051814":
                    case "6061114":
                    case "6061214":
                    case "6061314":
                    case "6061414":
                    case "6061514":
                    case "6061614":
                    case "6061714":
                    case "6061814":
                    case "6071114":
                    case "6071214":
                    case "6071314":
                    case "6071414":
                    case "6071514":
                    case "6071614":
                    case "6071714":
                    case "6071814":
                    case "6081114":
                    case "6081214":
                    case "6081314":
                    case "6081414":
                    case "6081514":
                    case "6081614":
                    case "6081714":
                    case "6081814":
                    case "6091114":
                    case "6091214":
                    case "6091314":
                    case "6091414":
                    case "6091514":
                    case "6091614":
                    case "6091714":
                    case "6091814":
                    case "6101114":
                    case "6101214":
                    case "6101314":
                    case "6101414":
                    case "6101514":
                    case "6101614":
                    case "6101714":
                    case "6101814":
                    case "6111114":
                    case "6111214":
                    case "6111314":
                    case "6111414":
                    case "6111514":
                    case "6111614":
                    case "6111714":
                    case "6111814":
                    case "6121114":
                    case "6121214":
                    case "6121314":
                    case "6121414":
                    case "6121514":
                    case "6121614":
                    case "6121714":
                    case "6121814":
                    case "6131114":
                    case "6131214":
                    case "6131314":
                    case "6131414":
                    case "6131514":
                    case "6131614":
                    case "6131714":
                    case "6131814":
                    case "6141114":
                    case "6141214":
                    case "6141314":
                    case "6141414":
                    case "6141514":
                    case "6141614":
                    case "6141714":
                    case "6141814":
                    case "6151114":
                    case "6151214":
                    case "6151314":
                    case "6151414":
                    case "6151514":
                    case "6151614":
                    case "6151714":
                    case "6151814":
                    case "6161114":
                    case "6161214":
                    case "6161314":
                    case "6161414":
                    case "6161514":
                    case "6161614":
                    case "6161714":
                    case "6161814":
                    case "6010116":
                    case "6010216":
                    case "6010316":
                    case "6010416":
                    case "6010516":
                    case "6010616":
                    case "6010716":
                    case "6010816":
                    case "6020116":
                    case "6020216":
                    case "6020316":
                    case "6020416":
                    case "6020516":
                    case "6020616":
                    case "6020716":
                    case "6020816":
                    case "6030116":
                    case "6030216":
                    case "6030316":
                    case "6030416":
                    case "6030516":
                    case "6030616":
                    case "6030716":
                    case "6030816":
                    case "6040116":
                    case "6040216":
                    case "6040316":
                    case "6040416":
                    case "6040516":
                    case "6040616":
                    case "6040716":
                    case "6040816":
                    case "6050116":
                    case "6050216":
                    case "6050316":
                    case "6050416":
                    case "6050516":
                    case "6050616":
                    case "6050716":
                    case "6050816":
                    case "6060116":
                    case "6060216":
                    case "6060316":
                    case "6060416":
                    case "6060516":
                    case "6060616":
                    case "6060716":
                    case "6060816":
                    case "6070116":
                    case "6070216":
                    case "6070316":
                    case "6070416":
                    case "6070516":
                    case "6070616":
                    case "6070716":
                    case "6070816":
                    case "6080116":
                    case "6080216":
                    case "6080316":
                    case "6080416":
                    case "6080516":
                    case "6080616":
                    case "6080716":
                    case "6080816":
                    case "6090116":
                    case "6090216":
                    case "6090316":
                    case "6090416":
                    case "6090516":
                    case "6090616":
                    case "6090716":
                    case "6090816":
                    case "6100116":
                    case "6100216":
                    case "6100316":
                    case "6100416":
                    case "6100516":
                    case "6100616":
                    case "6100716":
                    case "6100816":
                    case "6110116":
                    case "6110216":
                    case "6110316":
                    case "6110416":
                    case "6110516":
                    case "6110616":
                    case "6110716":
                    case "6110816":
                    case "6120116":
                    case "6120216":
                    case "6120316":
                    case "6120416":
                    case "6120516":
                    case "6120616":
                    case "6120716":
                    case "6120816":
                    case "6130116":
                    case "6130216":
                    case "6130316":
                    case "6130416":
                    case "6130516":
                    case "6130616":
                    case "6130716":
                    case "6130816":
                    case "6140116":
                    case "6140216":
                    case "6140316":
                    case "6140416":
                    case "6140516":
                    case "6140616":
                    case "6140716":
                    case "6140816":
                    case "6150116":
                    case "6150216":
                    case "6150316":
                    case "6150416":
                    case "6150516":
                    case "6150616":
                    case "6150716":
                    case "6150816":
                    case "6160116":
                    case "6160216":
                    case "6160316":
                    case "6160416":
                    case "6160516":
                    case "6160616":
                    case "6160716":
                    case "6160816":
                    case "6010117":
                    case "6010217":
                    case "6010317":
                    case "6010417":
                    case "6010517":
                    case "6010617":
                    case "6010717":
                    case "6010817":
                    case "6020117":
                    case "6020217":
                    case "6020317":
                    case "6020417":
                    case "6020517":
                    case "6020617":
                    case "6020717":
                    case "6020817":
                    case "6030117":
                    case "6030217":
                    case "6030317":
                    case "6030417":
                    case "6030517":
                    case "6030617":
                    case "6030717":
                    case "6030817":
                    case "6040117":
                    case "6040217":
                    case "6040317":
                    case "6040417":
                    case "6040517":
                    case "6040617":
                    case "6040717":
                    case "6040817":
                    case "6050117":
                    case "6050217":
                    case "6050317":
                    case "6050417":
                    case "6050517":
                    case "6050617":
                    case "6050717":
                    case "6050817":
                    case "6060117":
                    case "6060217":
                    case "6060317":
                    case "6060417":
                    case "6060517":
                    case "6060617":
                    case "6060717":
                    case "6060817":
                    case "6070117":
                    case "6070217":
                    case "6070317":
                    case "6070417":
                    case "6070517":
                    case "6070617":
                    case "6070717":
                    case "6070817":
                    case "6080117":
                    case "6080217":
                    case "6080317":
                    case "6080417":
                    case "6080517":
                    case "6080617":
                    case "6080717":
                    case "6080817":
                    case "6090117":
                    case "6090217":
                    case "6090317":
                    case "6090417":
                    case "6090517":
                    case "6090617":
                    case "6090717":
                    case "6090817":
                    case "6100117":
                    case "6100217":
                    case "6100317":
                    case "6100417":
                    case "6100517":
                    case "6100617":
                    case "6100717":
                    case "6100817":
                    case "6110117":
                    case "6110217":
                    case "6110317":
                    case "6110417":
                    case "6110517":
                    case "6110617":
                    case "6110717":
                    case "6110817":
                    case "6120117":
                    case "6120217":
                    case "6120317":
                    case "6120417":
                    case "6120517":
                    case "6120617":
                    case "6120717":
                    case "6120817":
                    case "6130117":
                    case "6130217":
                    case "6130317":
                    case "6130417":
                    case "6130517":
                    case "6130617":
                    case "6130717":
                    case "6130817":
                    case "6140117":
                    case "6140217":
                    case "6140317":
                    case "6140417":
                    case "6140517":
                    case "6140617":
                    case "6140717":
                    case "6140817":
                    case "6150117":
                    case "6150217":
                    case "6150317":
                    case "6150417":
                    case "6150517":
                    case "6150617":
                    case "6150717":
                    case "6150817":
                    case "6160117":
                    case "6160217":
                    case "6160317":
                    case "6160417":
                    case "6160517":
                    case "6160617":
                    case "6160717":
                    case "6160817":
                    case "1400080":
                    case "1400320":
                        if (alarm)
                        {
                            S_Yield += alarmCode + ",";
                        }
                        else
                        {
                            S_Yield = S_Yield.Replace(alarmCode + ",", "");
                        }
                        if (S_Yield.Length > 0)
                        {
                            Yield.Enabled = false;
                        }
                        else
                        {
                            Yield.Enabled = true;
                            Yield.BackColor = Color.Cyan;
                        }
                        if (alarmCode[0] == '6')
                        {
                            for (int i = 0; i < dut.Length; i++)
                            {
                                foreach (Control c in dut[i])
                                {
                                    if (c.Name.Equals("panel_S" + (int.Parse(alarmCode.Substring(1, 2)) - 1).ToString("D2")))
                                    {
                                        foreach (Control c2 in c.Controls)
                                        {
                                            if (c2.Name.Equals("dut_" + (int.Parse(alarmCode.Substring(1, 2)) - 1).ToString("D2") + alarmCode.Substring(4, 1)))
                                            {
                                                if (alarm)
                                                {
                                                    S_dut[int.Parse(alarmCode.Substring(1, 2)) - 1, int.Parse(alarmCode.Substring(4, 1)) - 1] += alarmCode + ",";
                                                }
                                                else
                                                {
                                                    S_dut[int.Parse(alarmCode.Substring(1, 2)) - 1, int.Parse(alarmCode.Substring(4, 1)) - 1] =
                                                    S_dut[int.Parse(alarmCode.Substring(1, 2)) - 1, int.Parse(alarmCode.Substring(4, 1)) - 1].Replace(alarmCode + ",", "");
                                                }
                                                if (S_dut[int.Parse(alarmCode.Substring(1, 2)) - 1, int.Parse(alarmCode.Substring(4, 1)) - 1].Length > 0)
                                                {
                                                    c2.Enabled = false;
                                                }
                                                else
                                                {
                                                    c2.Enabled = true;
                                                    c2.BackColor = Color.Transparent;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //dut
                    case "6010115":
                    case "6010215":
                    case "6010315":
                    case "6010415":
                    case "6010515":
                    case "6010615":
                    case "6010715":
                    case "6010815":
                    case "6020115":
                    case "6020215":
                    case "6020315":
                    case "6020415":
                    case "6020515":
                    case "6020615":
                    case "6020715":
                    case "6020815":
                    case "6030115":
                    case "6030215":
                    case "6030315":
                    case "6030415":
                    case "6030515":
                    case "6030615":
                    case "6030715":
                    case "6030815":
                    case "6040115":
                    case "6040215":
                    case "6040315":
                    case "6040415":
                    case "6040515":
                    case "6040615":
                    case "6040715":
                    case "6040815":
                    case "6050115":
                    case "6050215":
                    case "6050315":
                    case "6050415":
                    case "6050515":
                    case "6050615":
                    case "6050715":
                    case "6050815":
                    case "6060115":
                    case "6060215":
                    case "6060315":
                    case "6060415":
                    case "6060515":
                    case "6060615":
                    case "6060715":
                    case "6060815":
                    case "6070115":
                    case "6070215":
                    case "6070315":
                    case "6070415":
                    case "6070515":
                    case "6070615":
                    case "6070715":
                    case "6070815":
                    case "6080115":
                    case "6080215":
                    case "6080315":
                    case "6080415":
                    case "6080515":
                    case "6080615":
                    case "6080715":
                    case "6080815":
                    case "6090115":
                    case "6090215":
                    case "6090315":
                    case "6090415":
                    case "6090515":
                    case "6090615":
                    case "6090715":
                    case "6090815":
                    case "6100115":
                    case "6100215":
                    case "6100315":
                    case "6100415":
                    case "6100515":
                    case "6100615":
                    case "6100715":
                    case "6100815":
                    case "6110115":
                    case "6110215":
                    case "6110315":
                    case "6110415":
                    case "6110515":
                    case "6110615":
                    case "6110715":
                    case "6110815":
                    case "6120115":
                    case "6120215":
                    case "6120315":
                    case "6120415":
                    case "6120515":
                    case "6120615":
                    case "6120715":
                    case "6120815":
                    case "6130115":
                    case "6130215":
                    case "6130315":
                    case "6130415":
                    case "6130515":
                    case "6130615":
                    case "6130715":
                    case "6130815":
                    case "6140115":
                    case "6140215":
                    case "6140315":
                    case "6140415":
                    case "6140515":
                    case "6140615":
                    case "6140715":
                    case "6140815":
                    case "6150115":
                    case "6150215":
                    case "6150315":
                    case "6150415":
                    case "6150515":
                    case "6150615":
                    case "6150715":
                    case "6150815":
                    case "6160115":
                    case "6160215":
                    case "6160315":
                    case "6160415":
                    case "6160515":
                    case "6160615":
                    case "6160715":
                    case "6160815":
                        for (int i = 0; i < dut.Length; i++)
                        {
                            foreach (Control c in dut[i])
                            {
                                if (c.Name.Equals("panel_S" + (int.Parse(alarmCode.Substring(1, 2)) - 1).ToString("D2")))
                                {
                                    foreach (Control c2 in c.Controls)
                                    {
                                        if (c2.Name.Equals("dut_" + (int.Parse(alarmCode.Substring(1, 2)) - 1).ToString("D2") + alarmCode.Substring(4, 1)))
                                        {
                                            if (alarm)
                                            {
                                                S_dut[int.Parse(alarmCode.Substring(1, 2)) - 1, int.Parse(alarmCode.Substring(4, 1)) - 1] += alarmCode + ",";
                                            }
                                            else
                                            {
                                                S_dut[int.Parse(alarmCode.Substring(1, 2)) - 1, int.Parse(alarmCode.Substring(4, 1)) - 1] =
                                                S_dut[int.Parse(alarmCode.Substring(1, 2)) - 1, int.Parse(alarmCode.Substring(4, 1)) - 1].Replace(alarmCode + ",", "");
                                            }
                                            if (S_dut[int.Parse(alarmCode.Substring(1, 2)) - 1, int.Parse(alarmCode.Substring(4, 1)) - 1].Length > 0)
                                            {
                                                c2.Enabled = false;
                                            }
                                            else
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //X轴dut
                    case "7005100":
                    case "7005200":
                    case "7005300":
                    case "7005400":
                    case "7005101":
                    case "7005201":
                    case "7005301":
                    case "7005401":
                    case "7005102":
                    case "7005202":
                    case "7005302":
                    case "7005402":
                    case "7005103":
                    case "7005203":
                    case "7005303":
                    case "7005403":
                    case "7005104":
                    case "7005204":
                    case "7005304":
                    case "7005404":
                    case "7005105":
                    case "7005205":
                    case "7005305":
                    case "7005405":
                    case "7005106":
                    case "7005206":
                    case "7005306":
                    case "7005406":
                    case "7005107":
                    case "7005207":
                    case "7005307":
                    case "7005407":
                    case "7005108":
                    case "7005208":
                    case "7005308":
                    case "7005408":
                    case "7005109":
                    case "7005209":
                    case "7005309":
                    case "7005409":
                    case "7005110":
                    case "7005210":
                    case "7005310":
                    case "7005410":
                    case "7005111":
                    case "7005211":
                    case "7005311":
                    case "7005411":
                    case "7005112":
                    case "7005212":
                    case "7005312":
                    case "7005412":
                    case "7005113":
                    case "7005213":
                    case "7005313":
                    case "7005413":
                    case "7005114":
                    case "7005214":
                    case "7005314":
                    case "7005414":
                    case "7005115":
                    case "7005215":
                    case "7005315":
                    case "7005415":
                        for (int i = 0; i < dut.Length; i++)
                        {
                            foreach (Control c in dut[i])
                            {
                                if (c.Name.Equals("panel_S" + alarmCode.Substring(5, 2)))
                                {
                                    foreach (Control c2 in c.Controls)
                                    {
                                        if (c2.Name.Equals("dut_" + alarmCode.Substring(5, 2) + alarmCode.Substring(4, 1))
                                            || c2.Name.Equals("dut_" + alarmCode.Substring(5, 2) + (int.Parse(alarmCode.Substring(4, 1)) + 4).ToString()))
                                        {
                                            if (alarm)
                                            {
                                                S_dut[int.Parse(c2.Name.Substring(4, 2)), int.Parse(c2.Name.Substring(6, 1)) - 1] += alarmCode + ",";
                                            }
                                            else
                                            {
                                                S_dut[int.Parse(c2.Name.Substring(4, 2)), int.Parse(c2.Name.Substring(6, 1)) - 1] =
                                                S_dut[int.Parse(c2.Name.Substring(4, 2)), int.Parse(c2.Name.Substring(6, 1)) - 1].Replace(alarmCode + ",", "");
                                            }
                                            if (S_dut[int.Parse(c2.Name.Substring(4, 2)), int.Parse(c2.Name.Substring(6, 1)) - 1].Length > 0)
                                            {
                                                c2.Enabled = false;
                                            }
                                            else
                                            {
                                                c2.Enabled = true;
                                                c2.BackColor = Color.Transparent;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //Y轴dut
                    case "7007800":
                    case "7007801":
                    case "7007802":
                    case "7007803":
                    case "7007804":
                    case "7007805":
                    case "7007806":
                    case "7007807":
                    case "7007808":
                    case "7007809":
                    case "7007810":
                    case "7007811":
                    case "7007812":
                    case "7007813":
                    case "7007814":
                    case "7007815":
                    case "7007900":
                    case "7007901":
                    case "7007902":
                    case "7007903":
                    case "7007904":
                    case "7007905":
                    case "7007906":
                    case "7007907":
                    case "7007908":
                    case "7007909":
                    case "7007910":
                    case "7007911":
                    case "7007912":
                    case "7007913":
                    case "7007914":
                    case "7007915":
                    case "1400442":
                    case "1400480":
                    case "1400481":
                    case "1400482":
                    case "1400483":
                    case "1400484":
                    case "1400485":
                    case "1400486":
                    case "1400487":

                        for (int i = 0; i < dut.Length; i++)
                        {
                            foreach (Control c in dut[i])
                            {
                                if (c.Name.Equals("panel_S" + alarmCode.Substring(5, 2)))
                                {
                                    foreach (Control c2 in c.Controls)
                                    {
                                        int y0, x;
                                        if (alarmCode.Substring(4, 1).Equals("8"))
                                        {
                                            y0 = 1;
                                            x = 5;
                                        }
                                        else
                                        {
                                            y0 = 5;
                                            x = 9;
                                        }
                                        for (int y = y0; y < x; y++)
                                        {
                                            if (c2.Name.Equals("dut_" + alarmCode.Substring(5, 2) + y.ToString()))
                                            {
                                                if (alarm)
                                                {
                                                    S_dut[int.Parse(c2.Name.Substring(4, 2)), int.Parse(c2.Name.Substring(6, 1)) - 1] += alarmCode + ",";
                                                }
                                                else
                                                {
                                                    S_dut[int.Parse(c2.Name.Substring(4, 2)), int.Parse(c2.Name.Substring(6, 1)) - 1] =
                                                    S_dut[int.Parse(c2.Name.Substring(4, 2)), int.Parse(c2.Name.Substring(6, 1)) - 1].Replace(alarmCode + ",", "");
                                                }
                                                if (S_dut[int.Parse(c2.Name.Substring(4, 2)), int.Parse(c2.Name.Substring(6, 1)) - 1].Length > 0)
                                                {
                                                    c2.Enabled = false;
                                                }
                                                else
                                                {
                                                    c2.Enabled = true;
                                                    c2.BackColor = Color.Transparent;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    //通信异常
                    case "800007":
                    case "801007":
                    case "802007":
                    case "803007":
                    case "804007":
                    case "805007":
                    case "806007":
                    case "807007":
                    case "800011":
                    case "801011":
                    case "802011":
                    case "803011":
                    case "804011":
                    case "805011":
                    case "806011":
                    case "807011":
                    case "800027":
                    case "801027":
                    case "802027":
                    case "803027":
                    case "804027":
                    case "805027":
                    case "806027":
                    case "807027":
                    case "800018":
                    case "801018":
                    case "802018":
                    case "803018":
                    case "804018":
                    case "805018":
                    case "806018":
                    case "807018":
                    case "808018":
                    case "809018":
                    case "810018":
                    case "811018":
                    case "812018":
                    case "813018":
                    case "814018":
                    case "815018":
                    case "1400430":
                    case "1400440":

                        if (alarm)
                        {
                            S_Comm += alarmCode + ",";
                        }
                        else
                        {
                            S_Comm = S_Comm.Replace(alarmCode + ",", "");
                        }
                        if (S_Comm.Length > 0)
                        {
                            Comm.Enabled = false;
                        }
                        else
                        {
                            Comm.Enabled = true;
                            Comm.BackColor = Color.Cyan;
                        }
                        break;

                    //温度异常
                    case "801219":
                    case "802219":
                    case "803219":
                    case "804219":
                    case "805219":
                    case "806219":
                    case "807219":
                    case "808219":
                    case "809219":
                    case "810219":
                    case "811219":
                    case "812219":
                    case "813219":
                    case "814219":
                    case "815219":
                    case "816219":
                    case "800319":
                    case "801319":
                    case "802319":
                    case "803319":
                    case "804319":
                    case "805319":
                    case "806319":
                    case "807319":
                    case "808319":
                    case "809319":
                    case "810319":
                    case "811319":
                    case "812319":
                    case "813319":
                    case "814319":
                    case "815319":
                    case "816319":
                    case "1400446":
                    case "1400447":
                    case "1400448":
                    case "1400449":
                    case "1400450":
                    case "1400451":
                    case "1400452":
                    case "1400453":
                    case "1400454":
                    case "1400455":
                    case "1400456":
                    case "1400457":
                    case "5430100":

                        if (alarm)
                        {
                            S_Temp += alarmCode + ",";
                        }
                        else
                        {
                            S_Temp = S_Temp.Replace(alarmCode + ",", "");
                        }
                        if (S_Temp.Length > 0)
                        {
                            Temp.Enabled = false;
                        }
                        else
                        {
                            Temp.Enabled = true;
                            Temp.BackColor = Color.Cyan;
                        }
                        break;

                    //安全门
                    case "9000000":
                    case "9000010":
                    case "9000020":
                    case "9000030":
                    case "9000040":
                    case "9000050":
                        if (alarm)
                        {
                            S_SafeDoor += alarmCode + ",";
                        }
                        else
                        {
                            S_SafeDoor = S_SafeDoor.Replace(alarmCode + ",", "");
                        }
                        if (S_SafeDoor.Length > 0)
                        {
                            SafeDoor.Enabled = false;
                        }
                        else
                        {
                            SafeDoor.Enabled = true;
                            SafeDoor.BackColor = Color.Cyan;
                        }
                        break;

                    //其他异常
                    //case "1000000":
                    case "1000010":
                    case "1400100":
                    case "1400120":
                    case "1400130":
                    case "1400220":
                    case "1400230":
                    case "1400240":
                    case "1400250":
                    case "1400260":
                    case "1400300":
                    case "1400310":
                    case "1400330":
                    case "1400350":
                    case "1400360":
                    case "1400370":
                    case "1400380":
                    case "1400390":
                    case "1400400":
                    case "1400410":
                    case "1400420":
                    case "1400441":
                    case "1400443":
                    case "1400444":
                    case "1400500":
                    case "1400600":
                    case "1400469":
                    case "1400470":
                    case "1400471":
                    case "1400489":
                        if (alarm)
                        {
                            S_Other += alarmCode + ",";
                        }
                        else
                        {
                            S_Other = S_Other.Replace(alarmCode + ",", "");
                        }
                        if (S_Other.Length > 0)
                        {
                            Other.Enabled = false;
                        }
                        else
                        {
                            Other.Enabled = true;
                            Other.BackColor = Color.Cyan;
                        }
                        break;

                    //离子风扇
                    case "1100000":
                    case "1100010":
                    case "1100020":
                    case "1100030":
                        if (alarm)
                        {
                            S_Lonizer += alarmCode + ",";
                        }
                        else
                        {
                            S_Lonizer = S_Lonizer.Replace(alarmCode + ",", "");
                        }
                        if (S_Lonizer.Length > 0)
                        {
                            Lonizer.Enabled = false;
                        }
                        else
                        {
                            Lonizer.Enabled = true;
                            Lonizer.BackColor = Color.Cyan;
                        }
                        break;

                    //系统异常
                    case "1200000":
                    case "1200010":
                    case "1200020":
                    case "1200030":
                    case "1200040":
                    case "1200050":
                    case "1200060":
                    case "1200070":
                    case "1200080":
                    case "1200090":
                    case "1200100":
                    case "1200110":
                    case "1400000":
                    case "1400010":
                    case "1400020":
                    case "1400030":
                    case "1400040":
                    case "1400050":
                    case "1400060":
                    case "1400070":
                    case "1400090":
                    case "1400110":
                    case "1400140":
                    case "1400150":
                    case "1400160":
                    case "1400170":
                    case "1400180":
                    case "1400190":
                    case "1400200":
                    case "1400210":
                    case "1400270":
                    case "1400280":
                    case "1400290":
                    case "1400340":
                    case "1400445":
                    case "1400488":
                    case "1400499":   // 软件未授权
                    case "1400501":   // 软件授权即将到期
                        if (alarm)
                        {
                            S_Sys += alarmCode + ",";
                        }
                        else
                        {
                            S_Sys = S_Sys.Replace(alarmCode + ",", "");
                        }
                        if (S_Sys.Length > 0)
                        {
                            Sys.Enabled = false;
                        }
                        else
                        {
                            Sys.Enabled = true;
                            Sys.BackColor = Color.Cyan;
                        }
                        break;


                    default:

                        break;


                }

                DataGridViewRow row = new DataGridViewRow();
                row.Cells.Add(new DataGridViewTextBoxCell());
                row.Cells[0].Value = alarmCode;
                //row.Cells.Add(new DataGridViewTextBoxCell());
                //row.Cells[1].Value = dataGridView_all.CurrentRow.Cells[1].Value;
                //row.Cells.Add(new DataGridViewTextBoxCell());
                //row.Cells[2].Value = dataGridView_all.CurrentRow.Cells[2].Value;


                for (int i = 0; i < dataGridView_alarm.Rows.Count; i++)
                {
                    // 查重
                    if (dataGridView_alarm.Rows[i].Cells[0].Value.ToString().Equals(alarmCode))
                    {
                        return;
                    }
                }

                foreach (DataGridViewRow r in dataGridView_all.Rows)
                {
                    if (r.Cells[0].Value.ToString().Equals(alarmCode))
                    {
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells[1].Value = r.Cells[1].Value;
                        row.Cells.Add(new DataGridViewTextBoxCell());
                        row.Cells[2].Value = r.Cells[2].Value;

                        //添加动态报警信息
                        switch (alarmCode)
                        {
                            case "1400500": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("othAll", othAll).Replace("oth", oth); break;
                            case "1400600":
                                {
                                    Sqlite.runSql(SQLiteRunningString, "select id from SocketUse where num>" + 设置信息.测试机信息.socket使用次数, out List<object> res);
                                    string sid = "";
                                    for (int i = 0; i < res.Count; i++)
                                    {
                                        sid += res[i].ToString() + ",";
                                    }
                                    sid = sid.TrimEnd(',');
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("sid", sid);
                                }
                                break;

                            case "1400420": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 全部回零异常Error); break;

                            case "1400449": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 读取Site1_Site8探测温度Error); break;

                            case "1400450": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 读取Site9_Site16探测温度Error); break;

                            case "1400451": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 读取加热盘探测温度Error); break;

                            case "1400452": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 设定Site1_Site8温度Error); break;

                            case "1400453": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 设定Site9_Site16温度Error); break;

                            case "1400454": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 设定加热盘温度Error); break;

                            case "1400455": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 读取Site1_Site8设定温度Error); break;

                            case "1400456": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 读取Site9_Site16设定温度Error); break;

                            case "1400457": row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("Msg", 读取加热盘设定温度Error); break;


                            case "2008007":  //MXX_MoveFail M1_入料手臂Z1轴
                                { row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M01_入料手臂Z1轴").Replace("Msg", M1_入料手臂Z1轴ErrorMsg); }
                                break;
                            case "2009007":  //MXX_MoveFail M2_入料手臂Z2轴
                                { row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M02_入料手臂Z2轴").Replace("Msg", M2_入料手臂Z2轴ErrorMsg); }
                                break;
                            case "2010007": //MXX_MoveFail M3_入料手臂Z3轴
                                { row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M03_入料手臂Z3轴").Replace("Msg", M3_入料手臂Z3轴ErrorMsg); }
                                break;
                            case "2011007":  //MXX_MoveFail M4_入料手臂Z4轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M04_入料手臂Z4轴").Replace("Msg", M4_入料手臂Z4轴ErrorMsg);
                                }
                                break;
                            case "2012007":  //MXX_MoveFail M5_入料手臂P轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M05_入料手臂P轴").Replace("Msg", M5_入料手臂P轴ErrorMsg);
                                }
                                break;
                            case "2013007":  //MXX_MoveFail M6_出料手臂Z1轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M06_出料手臂Z1轴").Replace("Msg", M6_出料手臂Z1轴ErrorMsg);
                                }
                                break;
                            case "2014007":  //MXX_MoveFail M7_出料手臂Z2轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M07_出料手臂Z2轴").Replace("Msg", M7_出料手臂Z2轴ErrorMsg);
                                }
                                break;
                            case "2015007":  //MXX_MoveFail M8_出料手臂Z3轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M08_出料手臂Z3轴").Replace("Msg", M8_出料手臂Z3轴ErrorMsg);
                                }
                                break;
                            case "2016007":  //MXX_MoveFail M9_出料手臂Z4轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M09_出料手臂Z4轴").Replace("Msg", M9_出料手臂Z4轴ErrorMsg);
                                }
                                break;
                            case "2017007":  //MXX_MoveFail M10_出料手臂P轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M10_出料手臂P轴").Replace("Msg", M10_出料手臂P轴ErrorMsg);
                                }
                                break;
                            case "2018007":  //MXX_MoveFail M11_Fix3放IC轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M11_Fix3放IC轴").Replace("Msg", M11_Fix3放IC轴ErrorMsg);
                                }
                                break;
                            case "2019007":  //MXX_MoveFail M12_Empty顶盘轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M12_Empty顶盘轴").Replace("Msg", M12_Empty顶盘轴ErrorMsg);
                                }
                                break;
                            case "2020007":  //MXX_MoveFail M13_Loader顶盘轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M13_Loader顶盘轴").Replace("Msg", M13_Loader顶盘轴ErrorMsg);
                                }
                                break;
                            case "2021007":  //MXX_MoveFail M14_Auto1顶盘轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M14_Auto1顶盘轴").Replace("Msg", M14_Auto1顶盘轴ErrorMsg);
                                }
                                break;
                            case "2022007":  //MXX_MoveFail M15_Auto2顶盘轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M15_Auto2顶盘轴").Replace("Msg", M15_Auto2顶盘轴ErrorMsg);
                                }
                                break;
                            case "2023007":  //MXX_MoveFail M16_入料手臂Y轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M16_入料手臂Y轴").Replace("Msg", M16_入料手臂Y轴ErrorMsg);
                                }
                                break;
                            case "2024007":  //MXX_MoveFail M17_出料手臂Y轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M17_出料手臂Y轴").Replace("Msg", M17_出料手臂Y轴ErrorMsg);
                                }
                                break;
                            case "2025007":  //MXX_MoveFail M18_放Tary盘轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M18_放Tary盘轴").Replace("Msg", M18_放Tary盘轴ErrorMsg);
                                }
                                break;
                            case "2026007":  //MXX_MoveFail M19_测试手臂Z轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M19_测试手臂Z轴").Replace("Msg", M19_测试手臂Z轴ErrorMsg);
                                }
                                break;
                            case "2027007":  //MXX_MoveFail M20_飞梭轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M20_飞梭轴").Replace("Msg", M20_飞梭轴ErrorMsg);
                                }
                                break;
                            case "2028007":  //MXX_MoveFail M21_测试手臂X轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M21_测试手臂X轴").Replace("Msg", M21_测试手臂X轴ErrorMsg);
                                }
                                break;
                            case "2029007":  //MXX_MoveFail M22_入料手臂X轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M22_入料手臂X轴").Replace("Msg", M22_入料手臂X轴ErrorMsg);
                                }
                                break;
                            case "2030007":  //MXX_MoveFail M23_出料手臂X轴
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("MXX_Name", "M23_出料手臂X轴").Replace("Msg", M23_出料手臂X轴ErrorMsg);
                                }
                                break;
                            case "1400070":  //系统未定义错误ErrorMsg
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("系统未定义错误", 系统未定义错误ErrorMsg);
                                }
                                break;
                            case "1400472":  //系统未定义错误ErrorMsg
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("second", Load门开门时间.ToString());
                                }
                                break;
                            case "1400480":  //Site(xx)的Dut1测试超时
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("(xx)", 测试超时的Dut对应site.ToString());
                                }
                                break;
                            case "1400481":  //Site(xx)的Dut1测试超时
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("(xx)", 测试超时的Dut对应site.ToString());
                                }
                                break;
                            case "1400482":  //Site(xx)的Dut2测试超时
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("(xx)", 测试超时的Dut对应site.ToString());
                                }
                                break;
                            case "1400483":  //Site(xx)的Dut3测试超时
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("(xx)", 测试超时的Dut对应site.ToString());
                                }
                                break;
                            case "1400484":  //Site(xx)的Dut4测试超时
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("(xx)", 测试超时的Dut对应site.ToString());
                                }
                                break;
                            case "1400485":  //Site(xx)的Dut5测试超时
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("(xx)", 测试超时的Dut对应site.ToString());
                                }
                                break;
                            case "1400486":  //Site(xx)的Dut6测试超时
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("(xx)", 测试超时的Dut对应site.ToString());
                                }
                                break;
                            case "1400487":  //Site(xx)的Dut7测试超时
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("(xx)", 测试超时的Dut对应site.ToString());
                                }
                                break;
                            case "1400488":  //超时BIN(20A)未分配
                                {
                                    row.Cells[1].Value = r.Cells[1].Value.ToString().Replace("20A", 未分配超时BIN);
                                }
                                break;
                            case "1400150":  //测试机tester接收的通讯数据(Data)解析失败！详细信息：Msg
                                {
                                    row.Cells[1].Value = 串口数据异常Msg;
                                }
                                break;
                            case "3010002":
                                {
                                    row.Cells[1].Value = 入料吸嘴回原失败Msg;
                                }
                                break;
                            case "3050002":
                                {
                                    row.Cells[1].Value = 出料吸嘴回原失败Msg;
                                }
                                break;
                        }
                    }

                }
                dataGridView_alarm.Rows.Add(row);
            }));
        }
        private void button_start_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.Cells.Add(new DataGridViewTextBoxCell());
            row.Cells[0].Value = dataGridView_all.CurrentRow.Cells[0].Value;
            row.Cells.Add(new DataGridViewTextBoxCell());
            row.Cells[1].Value = dataGridView_all.CurrentRow.Cells[1].Value;
            row.Cells.Add(new DataGridViewTextBoxCell());
            row.Cells[2].Value = dataGridView_all.CurrentRow.Cells[2].Value;


            for (int i = 0; i < dataGridView_alarm.Rows.Count; i++)
            {
                // 查重
                if (dataGridView_alarm.Rows[i].Cells[0].Value.ToString().Equals(dataGridView_all.CurrentRow.Cells[0].Value.ToString()))
                {
                    return;
                }
            }

            dataGridView_alarm.Rows.Add(row);
            ShowCode(dataGridView_all.CurrentRow.Cells[0].Value.ToString(), true);
        }

        private void button_cencel_Click(object sender, EventArgs e)
        {

            if (dataGridView_alarm.Rows.Count < 1) return;
            ShowCode(dataGridView_alarm.CurrentRow.Cells[0].Value.ToString(), false);

            dataGridView_alarm.Rows.RemoveAt(dataGridView_alarm.CurrentRow.Index);
        }

        private void dataGridView_all_DoubleClick(object sender, EventArgs e)
        {
            textBox_Search.Text = dataGridView_all.CurrentRow.Cells[0].Value.ToString();

            button_start.PerformClick();
        }
        private void dataGridView_alarm_DoubleClick(object sender, EventArgs e)
        {
            button_cencel.PerformClick();
        }

        private void CancelAlarm()
        {
            foreach (DataGridViewRow r in dataGridView_alarm.Rows)
            {
                ShowCode(r.Cells[0].Value.ToString(), false);
            }
            dataGridView_alarm.Rows.Clear();
        }

        public void CancelAllAlarm()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    CancelAlarm();
                }));
            }
            else
            {
                CancelAlarm();
            }
        }

        private void textBox_Search_TextChanged(object sender, EventArgs e)
        {
            // 遍历所有行
            foreach (DataGridViewRow r in dataGridView_all.Rows)
            {
                foreach (DataGridViewCell c in r.Cells)
                {
                    if (c.Value.ToString().Contains(textBox_Search.Text))
                    {
                        dataGridView_all.ClearSelection();
                        r.Selected = true;
                        dataGridView_all.CurrentCell = r.Cells[0];
                        return;
                    }
                }
            }
            dataGridView_all.ClearSelection();

        }


        public bool 判断是否有梭车放歪报警()
        {
            string[] ErrorCode = { "543000", "544000", "545000", "546000", "547000", "548000", "549000", "550000" };
            for (int i = 0; i < dataGridView_alarm.Rows.Count; i++)
            {
                if (ErrorCode.Contains(dataGridView_alarm.Rows[i].Cells[0].Value.ToString().Substring(0, 6)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>“软件未授权”报警代码（归在“系统异常”类）</summary>
        public const string LicenseAlarmCode = "1400499";

        /// <summary>“授权即将到期”提醒代码（归在“系统异常”类）</summary>
        public const string ExpireSoonAlarmCode = "1400501";

        /// <summary>触发“软件未授权”报警（重复调用只保留一条）</summary>
        public void RaiseLicenseAlarm()
        {
            try
            {
                if (!IsHandleCreated || IsDisposed) return;

                Invoke(new Action(() =>
                {
                    foreach (DataGridViewRow r in dataGridView_alarm.Rows)
                    {
                        if (Convert.ToString(r.Cells[0].Value) == LicenseAlarmCode)
                        {
                            ShowCode(LicenseAlarmCode, true);
                            return;
                        }
                    }

                    var row = new DataGridViewRow();
                    row.Cells.Add(new DataGridViewTextBoxCell { Value = LicenseAlarmCode });
                    row.Cells.Add(new DataGridViewTextBoxCell { Value = "软件未授权" });
                    row.Cells.Add(new DataGridViewTextBoxCell { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
                    dataGridView_alarm.Rows.Add(row);

                    // 写入报警日志（报警日志页 / 日志文件 / Alarm.txt）
                    try { RunLog("[Code]" + LicenseAlarmCode + ",[Msg]软件未授权", LogType.AlarmLog); } catch { }

                    ShowCode(LicenseAlarmCode, true);

                    if (!Visible) Show();
                    Activate();
                }));
            }
            catch { }
        }

        /// <summary>清除“软件未授权”报警（授权恢复后调用）</summary>
        public void ClearLicenseAlarm()
        {
            try
            {
                if (!IsHandleCreated || IsDisposed) return;

                Invoke(new Action(() =>
                {
                    ShowCode(LicenseAlarmCode, false);

                    bool removed = false;
                    for (int i = dataGridView_alarm.Rows.Count - 1; i >= 0; i--)
                    {
                        if (Convert.ToString(dataGridView_alarm.Rows[i].Cells[0].Value) == LicenseAlarmCode)
                        {
                            dataGridView_alarm.Rows.RemoveAt(i);
                            removed = true;
                        }
                    }

                    // 解除也记一条操作日志，方便对账
                    if (removed)
                    {
                        try { RunLog("[Code]" + LicenseAlarmCode + ",[Msg]软件未授权报警已解除（授权已恢复）", LogType.OperateLog); } catch { }
                    }
                }));
            }
            catch { }
        }

        /// <summary>
        /// “授权即将到期”提醒：加到报警列表并写报警日志，N 秒后自动消失（不常驻，避免一直亮红灯）。
        /// </summary>
        public void RaiseExpireSoonAlarm(string message, int displaySeconds)
        {
            try
            {
                if (!IsHandleCreated || IsDisposed) return;

                Invoke(new Action(() =>
                {
                    // 先清掉上一条提醒，避免每 2 小时堆一条
                    RemoveExpireSoonAlarm();

                    var row = new DataGridViewRow();
                    row.Cells.Add(new DataGridViewTextBoxCell { Value = ExpireSoonAlarmCode });
                    row.Cells.Add(new DataGridViewTextBoxCell { Value = message });
                    row.Cells.Add(new DataGridViewTextBoxCell { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
                    dataGridView_alarm.Rows.Add(row);

                    ShowCode(ExpireSoonAlarmCode, true);

                    try { RunLog("[Code]" + ExpireSoonAlarmCode + ",[Msg]" + message, LogType.AlarmLog); } catch { }

                    if (!Visible) Show();

                    if (displaySeconds > 0)
                    {
                        var timer = new System.Windows.Forms.Timer { Interval = displaySeconds * 1000 };
                        timer.Tick += (s, e) =>
                        {
                            timer.Stop();
                            timer.Dispose();
                            try { RemoveExpireSoonAlarm(); } catch { }
                        };
                        timer.Start();
                    }
                }));
            }
            catch { }
        }

        private void RemoveExpireSoonAlarm()
        {
            bool removed = false;

            for (int i = dataGridView_alarm.Rows.Count - 1; i >= 0; i--)
            {
                if (Convert.ToString(dataGridView_alarm.Rows[i].Cells[0].Value) == ExpireSoonAlarmCode)
                {
                    dataGridView_alarm.Rows.RemoveAt(i);
                    removed = true;
                }
            }

            if (removed) ShowCode(ExpireSoonAlarmCode, false);
        }

        public bool 判断是否有dut放歪报警()
        {
            string[] ErrorCode = { "7005", "7007"};
            for (int i = 0; i < dataGridView_alarm.Rows.Count; i++)
            {
                if (ErrorCode.Contains(dataGridView_alarm.Rows[i].Cells[0].Value.ToString().Substring(0, 4)))
                {
                    return true;
                }
            }
            return false;
        }

        private void Form_alarm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!退出标记)
            {
                //alarmLight(0, 1, 0, 0);

                //CancelAlarm(); //2025-11-26 做权限管理
                报警已关闭 = true;
                e.Cancel = true;
                this.Hide();
                Thread.Sleep(1);
                AlarmLight(0, 1, 0, "无音乐"); //关闭报警界面
            }
        }

        private void Form_alarm_Load(object sender, EventArgs e)
        {
            RefreshTask = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (QueueCode.Count != 0)
                        {
                            报警已关闭 = false;
                            //this.Activate();
                            if (QueueCode.TryDequeue(out string code))
                            {
                                ShowCode(code, true);
                            }
                            Thread.Sleep(1);
                        }
                        else Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        RunLog("刷新Code异常!" + ex.Message);
                        Thread.Sleep(1000);
                    }
                }
            });
        }
    }
}
