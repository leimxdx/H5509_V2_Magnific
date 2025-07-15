using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Vml;
using H5509_alarm;
using Sunny.UI;
using Sunny.UI.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static GTN.mc;
using static H5509_V2.APPmaster;
using static H5509_V2.WriteData;
using static H5509_V2.WriteData.LogType;

namespace H5509_V2
{
    public partial class ForMain : UIForm
    {

        #region     窗体操作
        /// <summary>
        /// 防止控件过多产生闪烁现象
        /// </summary>
        private void SetControlStyle()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // 双缓冲
            this.UpdateStyles();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        private void SetTablayoutDoubleBuffer()
        {
            typeof(Panel)
               .GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
               .SetValue(MainPanel, true, null);
        }
        #endregion

        private _main main;
        private _setting setting;
        private _offset offset;
        private _speed speed;
        private _IO io;
        private Handler handler;
        private MotionEx motionEx;

        public Action 刷新开Site信息;

        public ForMain()
        {
            InitializeComponent();

            Log = new _log();
            显示操作log = Log.DelegteDisplayOperateLog;
            显示报警log = Log.DelegteDisplayAlarmLog;

            string result = Init_APPmaster();
            if (result != "Success")
            {
                //Log.AddLog("error", "run", "软件加载失败,请关闭软件重开!" + result);
                MessageBox.Show("软件加载失败,请关闭软件重开!", result);
            }
            //Log.AddLog("debug","run","读取数据库数据完成");
        }

        private void ForMain_Load(object sender, EventArgs e)
        {
            string result = "";
            Com = new Communication();
            main = new _main();
            setting = new _setting();
            offset = new _offset();
            speed = new _speed();
            io = new _IO();
            handler = new Handler();
            motionEx = MotionEx.GetInstance();

            setting.刷新分BIN信息 = main.刷新分BIN信息;
            setting.刷新温度信息 = main.刷新温度显示;

            handler.刷新批次信息 = main.刷新批次信息;
            handler.刷新分BIN信息 = main.刷新分BIN信息;
            handler.刷新权限 = main.切换为操作员权限;
            handler.显示Dout颜色 = main.显示Dout颜色;

            main.刷新界面权限 = 刷新界面权限;
            main.handler = handler;

            setting.handler = handler;
            io.handler = handler;
            Com.RefrshLog = Log.DelegteDisplayComInfo;

            达温延时 = new System.Timers.Timer() { Interval = 达温后延时时间 * 1000 }; //2分钟
            达温延时.Elapsed += main.TempFirst;

            dTMHelper = new DTMHelper(TCPIP, TCPPort);//"192.168.1.5", 502

            if (!dTMHelper.Connect())
            {
                TCPConnect = false;
                MessageBox.Show("温控器连接失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                TCPConnect = true;
                main.刷新温度显示();
            }

            //MotionEx motionex = MotionEx.GetInstance();
            //MotionEx motionex2 = MotionEx.GetInstance();

            //防止控件过多产生闪烁现象
            SetControlStyle();

            SetTablayoutDoubleBuffer();

            changeMainPanel(main);

            handler.参数初始化();

            result = motionEx.GSNInitCard();
            if (result != "Success")
            {
                MessageBox.Show("系统加载失败！详细信息：\n" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            result = Init_FrMain();
            if (result != "Success")
            {
                RunLog("软件加载失败,请关闭软件重开!" + result);
                MessageBox.Show("软件加载失败,请关闭软件重开!", result);
            }

            #region 调试参数

            //温度模式 = 0;
            ////结批标记 = true;
            //放料目标值();
            //飞梭前入料IC = new bool[2][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //HeatDicTimer["5,11"].Status = 2;
            //HeatDicTimer["5,10"].Status = 2;
            //HeatDicTimer["5,9"].Status = 2;
            //HeatDicPoint["5,11"] = true;
            //HeatDicPoint["5,10"] = true;
            //HeatDicPoint["5,9"] = true;
            //handler.debug();

            #endregion

            main.canVisable(this);
            刷新界面权限();

            //listcode.Add("1400441"); Dut全部被关闭，请长按结批按钮5S结批
        }

        public void 刷新界面权限()
        {
            CanVisable(this);
            CanVisable(setting);
            CanVisable(offset);
            CanVisable(speed);
            CanVisable(io);
        }


        public void CanVisable(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                if (con is UITabControl uItabl)
                {
                    for (int i = 0; i < uItabl.TabPages.Count; i++)
                    {
                        CanVisable(uItabl.TabPages[i]);
                    }
                    continue;
                }

                if (con.Tag != null)
                {
                    //根据操作权限控制visable与enable
                    if (con.Tag.ToString().Contains("#"))
                    {
                        string t = con.Tag.ToString();
                        t = t.Substring(t.IndexOf("#") + 1);
                        t = t.Substring(0, t.IndexOf("#"));

                        if (t.Contains("0"))
                        {
                            if (t.Contains(操作权限))
                            {
                                con.Visible = true;
                            }
                            else
                            {
                                con.Visible = false;
                                continue;
                            }
                        }
                        else //if (t.Contains("1"))没有设置默认都是1
                        {
                            if (t.Contains(操作权限))
                            {
                                con.Enabled = true;
                            }
                            else
                            {
                                con.Enabled = false;
                                continue;
                            }
                        }
                    }

                    //根据运行状态控制enable
                    if (con.Tag.ToString().Contains("@"))
                    {
                        string t = con.Tag.ToString();
                        t = t.Substring(t.IndexOf("@") + 1);
                        t = t.Substring(0, t.IndexOf("@"));
                        if (t.Contains(设备状态.ToString()))
                        {
                            con.Enabled = true;
                        }
                        else
                        {
                            con.Enabled = false;
                        }
                    }
                }

                if (con.Controls.Count > 0)
                {
                    CanVisable(con);
                }
            }
        }


        /// <summary>
        /// MainPanel界面切换
        /// </summary>
        /// <param name="uc"></param>
        private void changeMainPanel(UserControl uc)
        {
            if (uc == io)
            {
                io.IOUpdateTimer.Start();
            }
            else
            {
                io.IOUpdateTimer.Stop();
            }

            MainPanel.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            MainPanel.Controls.Add(uc);
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否退出软件?", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                //writeDataHelper.PrintLog("软件退出");
                退出标记 = true; //让报警弹窗也退出，否则主程序退出不了
                //gg.CloseCard(); //关闭轴卡
                Application.Exit(); //退出软件
            }
        }

        private void button_main_Click(object sender, EventArgs e)
        {
            changeMainPanel(main);
        }

        private void button_setting_Click(object sender, EventArgs e)
        {
            changeMainPanel(setting);
            setting.刷新设置与维护界面();
        }

        private void button_offset_Click(object sender, EventArgs e)
        {
            changeMainPanel(offset);
            offset.刷新Offset();
        }

        private void button_speed_Click(object sender, EventArgs e)
        {
            changeMainPanel(speed);
            speed.刷新速度值();
        }

        private void button_io_Click(object sender, EventArgs e)
        {
            changeMainPanel(io);
        }

        private void button_log_Click(object sender, EventArgs e)
        {
            changeMainPanel(Log);
            Log.刷新日志界面();
        }

        public class 分Bin
        {
            public string bin;
            public string pan;
            public int num;

            public 分Bin(string b, string p, int n)
            {
                bin = b;
                pan = p;
                num = n;
            }
        }


        public void bin(string b)
        {
            for (int i = 0; i < 分bin.Count; i++)
            {
                if (分bin[i].bin == b) 分bin[i].num++;
            }
        }

        private void 数据上传()
        {
            while (true)
            {
                try
                {
                    Log.LogTimer_Tick(null, null);
                    if (LogQueue.Count > 0)
                    {
                        //Debug.WriteLine("获取日志数量:" + LogQueue.Count);

                        int countTemp = LogQueue.Count;
                        List<LogManager> logsTemp = new List<LogManager>();
                        for (int i = 0; i < countTemp; i++)
                        {
                            LogQueue.TryDequeue(out LogManager log);
                            logsTemp.Add(log);
                        }

                        string[] logs = new string[logsTemp.Count];
                        DateTime[] logtime = new DateTime[logsTemp.Count];

                        for (int i = 0; i < logsTemp.Count; i++)
                        {
                            logs[i] = logsTemp[i].Msg;
                            logtime[i] = logsTemp[i].Time;
                        }

                        writeDataHelper.RunLog(logs, logtime);

                        for (int i = 0; i < logsTemp.Count; i++)
                        {
                            if (logsTemp[i].TxtLogtype == LogType.OperateLog || logsTemp[i].TxtLogtype == LogType.ParamLog)
                            {
                                显示操作log(logsTemp[i].Msg, logsTemp[i].Time);
                            }
                            else if (logsTemp[i].TxtLogtype == LogType.AlarmLog)
                            {
                                显示报警log(logsTemp[i].Msg, logsTemp[i].Time);
                            }
                        }

                        //LogQueue.RemoveRange(0, countTemp);// 清空已添加的日志
                    }

                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    RunLog("数据上传异常!" + ex.Message);
                    Thread.Sleep(1000);
                }
            }
        }

        private void 界面刷新()
        {
            string result = "";
            int testnum = 0;
            while (true)
            {
                try
                {
                    if (退出标记) break;

                    this.Invoke(new Action(() =>
                    {
                        if (Listcode.Count != 0)
                        {
                            form_alarm.Show();
                            for (int i = 0; i < Listcode.Count; i++)
                            {
                                form_alarm.code(Listcode[i], true);
                                Listcode.Remove(Listcode[i]);
                                //alarm_count++;
                            }
                        }
                    }));


                    if (IsPause) continue; //如果暂停则不刷新

                    bool ispass = false;

                    if (!空跑模式)
                    {
                        if (InNozzleIC)
                        {
                            result = handler.入料吸嘴_真空检测(out string[] iovalue);

                            for (int i = 0; i < iovalue.Length; i++)
                            {
                                if (InNozzleICOpen[i] && InNozzleIC)
                                {
                                    if (iovalue[i] == "0")
                                    {
                                        ispass = true;
                                        Listcode.Add($"30{i + 1}0001"); //出料手臂-吸--1号吸嘴丢失IC
                                        RunLog($"[Code]30{i + 1}0001,[Msg]入料手臂-吸--{i + 1}号吸嘴丢失IC", LogType.AlarmLog);
                                    }
                                }
                            }
                        }

                        if (OutNozzleIC)
                        {
                            result = handler.出料吸嘴_真空检测(out string[] iovalue);

                            for (int i = 0; i < iovalue.Length; i++)
                            {
                                if (OutNozzleICOpen[i] && OutNozzleIC)
                                {
                                    if (iovalue[i] == "0")
                                    {
                                        ispass = true;
                                        Listcode.Add($"30{i + 5}0001"); //出料手臂-吸--1号吸嘴丢失IC
                                        RunLog($"[Code]30{i + 5}0001,[Msg]出料手臂-吸--{i + 1}号吸嘴丢失IC", LogType.AlarmLog);
                                    }
                                }
                            }
                        }

                        if (TestNozzleIC)
                        {
                            result = handler.测试吸嘴_真空检测(out string[] iovalue);

                            for (int i = 0; i < iovalue.Length; i++)
                            {
                                if (TestNozzleICOpen[i] && TestNozzleIC)
                                {
                                    if (iovalue[i] == "0")
                                    {
                                        ispass = true;
                                        Listcode.Add($"3{i + 9:D2}9901"); //出料手臂-吸--1号吸嘴丢失IC
                                        RunLog($"[Code]3{i + 9:D2}9901,[Msg]测试手臂-吸--{i + 1}号吸嘴丢失IC", LogType.AlarmLog);
                                    }
                                }
                            }
                        }
                    }

                    if (ispass)
                    {
                        ProgramPause = true;
                        IsPause = true;
                    }

                    if (!空跑模式 && 设备状态 != 0)
                    {
                        this.Invoke(new Action(() =>
                        {
                            if (Com.Iswaitout[testnum]) //如果接收超时
                            {
                                Com.Iswaitout[testnum] = false;
                                Listcode.Add($"8{testnum:D2}018");
                                RunLog($"[Code]8{testnum:D2}018,[Msg]Site{testnum + 1}:接收超时！触发停止", LogType.AlarmLog);
                                ProgramPause = true; //如果有接收超时则触发停止
                                IsPause = true;

                                //后续是否需要重新发送
                            }

                            //上传bin
                            //计算每个dut的pass和fail数量
                            //上传Num=Num+1
                            //Socket使用时间
                            //连续不良报警
                            //自动关site
                            //累计不良报警
                            //总良率报警
                            //Socket使用次数
                            if (Com.Listtestsingal.Count != 0)
                            {
                                int sitenum = Com.Listtestsingal[0];
                                RunLog(DateTime.Now.ToString("HH:mm:ss.fff") + ":接收Site：" + sitenum);
                                //Com.Listtestsingal.Remove(sitenum);
                                Com.Listtestsingal.RemoveAt(0);
                                bool[][] open = SiteTestIC[sitenum];

                                Com.OutBin.TryDequeue(out string[] bins);

                                //main.显示Dout颜色(sitenum, open, bins);

                                string bb = "@TEST_DONE " + sitenum.ToString("D2");
                                foreach (string s in bins)
                                {
                                    bb += " " + s;
                                }
                                string sql = $"insert into f_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期,获取MAP时间,料号,批号,MAP信息)" +
                                $"values('{开始时间.ToString("D")}','{DateTime.Now.ToString()}','{LotNo[3]}','{LotNo[4]}','{bb}')";
                                SqlServer.runSqlNonQueryAsync(SQLServerConnString, sql);

                                foreach (string b in bins)
                                {
                                    bin(b);
                                }

                                int doutnum = sitenum * 8;
                                for (int m = 0; m < open.Length; m++)
                                {
                                    for (int n = 0; n < open[m].Length; n++)
                                    {
                                        string doutstr = $"{sitenum:D2}{m * 4 + n + 1}";
                                        if (open[m][n])
                                        {
                                            if (Auto1.Contains(bins[m * 4 + n]))
                                            {
                                                Dout128PassNumber[doutnum + (m * 4 + n)]++;
                                                doutContinueFail[sitenum][n + m * 4] = 0;
                                            }
                                            else
                                            {
                                                Dout128FailNumber[doutnum + (m * 4 + n)]++;
                                                doutContinueFail[sitenum][n + m * 4]++;
                                                单dut总不良计数[sitenum][n + m * 4]++;
                                                单dut总不良累计计数[sitenum][n + m * 4]++;
                                                总良率报警间隔计数++;
                                            }

                                            int line = SQLiteHelper.runSqlNonQuery(SQLiteRunningString, $"update SocketUse set Num=Num+1 where Id='{doutstr}'");
                                            if (line <= 0) RunLog($"update SocketUse set Num=Num+1 where Id='{doutstr}',返回:{line}");
                                        }

                                        int passnum = Dout128PassNumber[sitenum * 8 + (m * 4 + n)];
                                        int failnum = Dout128FailNumber[sitenum * 8 + (m * 4 + n)];
                                        //RunLog($"update dutCurrent set pass={passnum},total={failnum + passnum},usedTime=CAST(usedTime as bigint)+{Com.listtesttimespan[sitenum].Ticks} where Id='{doutstr}'");
                                        sql = $"update DutCurrent set pass={passnum},total={failnum + passnum},usedTime=CAST(usedTime as INTEGER)+{Com.listtesttimespan[sitenum].Ticks} where id='{doutstr}'";
                                        int line1 = SQLiteHelper.runSqlNonQuery(SQLiteRunningString, sql);
                                        if (line1 <= 0) RunLog(sql + $"返回:{line1}");
                                        RunLog($"Site{sitenum + 1:D2}-{m * 4 + n + 1}:Pass({passnum}):Fail({failnum}):Yield({(passnum / (float)(passnum + failnum) * 100):0.0}%)");
                                    }
                                }

                                bool isyieldex = false;
                                bool isalarm = 设置信息.测试机信息.Dout连续不良报警;
                                bool isautoclose = 设置信息.测试机信息.Dout连续不良自动关闭;
                                int doutfailnum = 设置信息.测试机信息.Dout连续不良个数;

                                for (int m = 0; m < doutContinueFail.Count; m++) //连续不良率报警
                                {
                                    for (int n = 0; n < doutContinueFail[m].Length; n++)
                                    {
                                        if (doutContinueFail[m][n] >= doutfailnum)
                                        {
                                            string code = $"6{m + 1:D2}0{n + 1}14";
                                            if (isalarm) //dout连续不良报警
                                            {
                                                isyieldex = true;
                                                Listcode.Add(code);
                                            }
                                            if (isautoclose) //dout连续不良自动关闭该dout，
                                            {
                                                int dout = m * 8 + n + 1;
                                                string labelname = $"lbl_dout{dout}";
                                                设置信息.开site信息.SetSiteValue(sitenum, dout, "0");
                                                result = UpdateSiteStatus(设置信息.开site信息.Site名称, 6 + 1, 设置信息.开site信息.GetSiteValue(sitenum));
                                                if (result != "Success") { MessageBox.Show("保存失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                                                result = GetSiteStatus(设置信息.开site信息.Site名称, out SiteStatus siteStatus);
                                                if (result != "Success") { MessageBox.Show("读取失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                                                设置信息.开site信息 = siteStatus;
                                                main.刷新开Site状态();
                                            }
                                            RunLog($"[Code]{code},[Msg]Site{m + 1}-Dut{n + 1}:连续不良数量:{doutContinueFail[m][n]}>={doutfailnum}", LogType.AlarmLog);
                                            doutContinueFail[m][n] = 0; //如果已报警良率异常则清空重新计算
                                        }
                                    }
                                }

                                if (设置信息.测试机信息.Dut累计不良报警使能)
                                {
                                    for (int i = 0; i < 单dut总不良计数.Count; i++)
                                    {
                                        for (int j = 0; j < 单dut总不良计数[i].Length; j++)
                                        {
                                            if (单dut总不良计数[i][j] >= 设置信息.测试机信息.Dut累计不良报警个数)
                                            {
                                                isyieldex = true;
                                                string code = $"6{i + 1:D2}0{j + 1}16";
                                                Listcode.Add(code);
                                                RunLog($"[Code]{code},[Msg]Site{i + 1}-Dut{i + 1}:累计不良数量:{单dut总不良计数[i][j]}>{设置信息.测试机信息.Dut累计不良报警个数}", LogType.AlarmLog);
                                                单dut总不良计数[i][j] = 0;
                                            }
                                        }
                                    }
                                }

                                if (设置信息.测试机信息.总良率报警使能
                                    && 出料数 >= 设置信息.测试机信息.总良率报警基数
                                    && 总良率报警间隔计数 >= 设置信息.测试机信息.总良率报警间隔
                                    && Auto1良率 <= 设置信息.测试机信息.总良率报警百分比)
                                {
                                    isyieldex = true;
                                    string code = "1400320";
                                    Listcode.Add(code);
                                    RunLog($"[Code]{code},[Msg]总良率：{Auto1良率}<{设置信息.测试机信息.总良率报警百分比.ToString("P1")}", LogType.AlarmLog);
                                    总良率报警间隔计数 = 0;
                                }
                                try
                                {
                                    result = SQLiteHelper.runSql(SQLiteRunningString, "select count(*) from SocketUse where num>" + 设置信息.测试机信息.socket使用次数, out string value);
                                    //object su = await SQLiteHelper.runSql(SQLiteRunningString, "select count(*) from SocketUse where num>" + motionEx.testerIFParam.socket使用次数);
                                    bool isint = int.TryParse(value.ToString(), out int socketnum);
                                    if (isint)
                                    {
                                        if (socketnum > Socket使用次数超限数)
                                        {
                                            Listcode.Add("1400600");
                                            Socket使用次数超限数 = socketnum;
                                        }
                                        else
                                        {
                                            if (socketnum == 0)
                                            {
                                                Socket使用次数超限数 = 0;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex) { RunLog("读取socket使用次数超限数异常：" + ex.Message); }

                                if (isyieldex) //如果良率异常
                                {
                                    ProgramPause = true; //如果有良率异常则触发停止
                                    IsPause = true;
                                }
                            }

                        }));
                    }

                    testnum++;
                    if (testnum >= 16)
                    {
                        testnum = 0;
                    }

                }
                catch (Exception ex)
                {
                    RunLog("屏幕刷新异常!" + ex.Message);
                    Thread.Sleep(100);
                }

                Thread.Sleep(50);
            }
        }

        private void 温度读取()
        {
            while (true)
            {
                try
                {
                    //if (IsPause)
                    //{
                    //    Thread.Sleep(100);
                    //    continue;
                    //}

                    if (TCPConnect)
                    {
                        string result = dTMHelper.ReadPv(0, out ushort[] value);
                        string result2 = dTMHelper.ReadPv(1, out ushort[] value2);
                        string result3 = dTMHelper.ReadPv(2, 0, out ushort[] value3);

                        if (result != "Success" || result2 != "Success" || result3 != "Success")
                        {
                            TCPConnect = false;
                            ProgramPause = true;
                            IsPause = true;
                            Listcode.Add("1400448");
                            RunLog($"[Code]1400448,[Msg]温控器连接失败!", LogType.AlarmLog);
                            continue;
                        }

                        for (int i = 0; i < value.Length; i++)
                        {
                            if (value[i] != 32770)
                            {
                                SiteTemp[i] = (double)value[i] / 10 + 加热盘.Site温度补偿(i);
                            }
                            else SiteTemp[i] = value[i];
                        }

                        for (int i = 0; i < value2.Length; i++)
                        {
                            if (value2[i] != 32770)
                            {
                                SiteTemp[i + 8] = (double)value2[i] / 10 + 加热盘.Site温度补偿(i + 8);
                            }
                            else SiteTemp[i + 8] = value2[i];
                        }

                        if (value3[0] != 32770)
                        {
                            SiteTemp[16] = (double)value3[0] / 10 + 加热盘.温控器温度补偿;
                        }
                        else SiteTemp[16] = value3[0];

                        if (IsPause) continue;

                        bool[] tempopen = 检测温度探头状态();

                        if (tempopen.Contains(true))
                        {
                            for (int i = 0; i < tempopen.Length; i++)
                            {
                                if (tempopen[i])
                                {
                                    Listcode.Add($"8{i:D2}219");
                                    if (i == 15) RunLog($"[Code]8{i:D2}219,[Msg]加热盘探头异常!", LogType.AlarmLog);
                                    else RunLog($"[Code]8{i:D2}219,[Msg]Site{i}探头异常!", LogType.AlarmLog);
                                }
                            }
                            ProgramPause = true; //如果有温度异常则触发停止
                            IsPause = true;
                        }
                        else
                        {
                            if (设备状态 == 1 && 温度模式 == "高温") //&& !IsPause
                            {
                                double tempwavemax = 加热盘.波动温度上限;
                                double tempwavemin = 加热盘.波动温度下限;
                                if (!温控达温)
                                {
                                    tempopen = 检测温度是否达温(tempwavemax, tempwavemin);
                                    if (tempopen[16]) //如果加热盘已经达温则开始延时
                                    {
                                        if (!初次达温) //初次达温开启延时定时器
                                        {
                                            if (!达温延时.Enabled)
                                            {
                                                RunLog("初次达温延时开始");
                                                达温延时.Start();
                                            }
                                        }
                                    }
                                    if (!tempopen.Contains(false)) //有false(没有达温:False,达温/探头异常：true)说明还有Site没有达温
                                    {
                                        if (初次达温)
                                        {
                                            温控达温 = true;
                                            达温时间 = DateTime.Now;
                                            RunLog("加热盘已达温");
                                            RunLog("达温时间：" + 达温时间.ToString("yyyy-MM-dd HH:mm:ss"));
                                            RunLog("升温时间：" + 升温时间.ToString(@"hh\:mm\:ss"));
                                        }
                                    }
                                    else
                                    {
                                        TimeSpan timeSpan = DateTime.Now - 开始时间;
                                        if (timeSpan.TotalSeconds > 加热盘.升温超时报警)
                                        {
                                            Listcode.Add($"1400446");
                                            RunLog($"[Code]1400446,[Msg]升温超时!", LogType.AlarmLog);
                                            ProgramPause = true; //如果升温超时则触发停止
                                            IsPause = true;
                                        }
                                    }
                                }
                                else
                                {
                                    tempopen = 检测温度是否异常(tempwavemax, tempwavemin);
                                    if (tempopen.Contains(true)) //有true说明有Site超出波动范围
                                    {
                                        for (int site = 0; site < SiteTemp.Length; site++)
                                        {
                                            if (tempopen[site] && SiteTemp[site] != 32770)
                                            {
                                                double temp = SiteTemp[site];
                                                if (temp > 加热盘.波动温度上限)
                                                {
                                                    Listcode.Add($"8{site:D2}319");
                                                    if (site == 16) RunLog($"[Code]8{site:D2}319,[Msg]温控器:温度[{temp}]℃高于上限[{加热盘.波动温度上限}]℃", LogType.AlarmLog);
                                                    else RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃高于上限[{加热盘.波动温度上限}]℃", LogType.AlarmLog);
                                                }
                                                else if (temp <= 加热盘.波动温度下限)
                                                {
                                                    Listcode.Add($"8{site:D2}319");
                                                    if (site == 16) RunLog($"[Code]8{site:D2}319,[Msg]温控器:温度[{temp}]℃低于下限[{加热盘.波动温度下限}]℃", LogType.AlarmLog);
                                                    else RunLog($"[Code]8{site:D2}319,[Msg]Site{site + 1}:温度[{temp}]℃低于下限[{加热盘.波动温度下限}]℃", LogType.AlarmLog);
                                                }
                                            }
                                        }
                                        //RunLog("这里触发暂停");
                                        ProgramPause = true;
                                        IsPause = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (dTMHelper.Connect())
                        {
                            TCPConnect = true;
                            温控器异常 = false;
                        }
                        else
                        {
                            ProgramPause = true;
                            IsPause = true;
                            Listcode.Add("1400448");
                            if (!温控器异常) //防止log输出多次
                            {
                                温控器异常 = true;
                                RunLog($"[Code]1400448,[Msg]温控器连接失败!", LogType.AlarmLog);
                            }
                        }
                    }

                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    RunLog("温度读取异常!" + ex.Message);
                    Thread.Sleep(100);
                }
            }
        }

        private string Init_FrMain()
        {
            StartTask = Task.Run(() => //通过线程去开启线程，防止卡顿
            {
                cts = new CancellationTokenSource();

                Task.Run(() =>
                {
                    handler.系统按钮检测流程();
                });
                Task.Run(() =>
                {
                    界面刷新();
                });
                Task.Run((() =>
                {
                    数据上传();
                }));
                Task.Run((() =>
                {
                    温度读取();
                }));
                Task.Run(() =>
                {
                    handler.初始化流程();
                });
                Task.Run(() =>
                {
                    handler.结批流程();
                });
                //Task.Run(() =>
                //{
                //    handler.入料手臂吸放料流程();
                //});
                Task.Run(() =>
                {
                    handler.入料手臂吸放料流程();
                });
                Task.Run(() =>
                {
                    handler.出料手臂吸放料流程();
                });
                Task.Run(() =>
                {
                    handler.飞梭移动流程();
                });
                Task.Run(() =>
                {
                    handler.测试手臂取待测料流程();
                });
                Task.Run(() =>
                {
                    handler.测试手臂取已测料流程();
                });
                Task.Run(() =>
                {
                    handler.测试区气缸下压动作流程();
                });
                Task.Run(() =>
                {
                    handler.测试区气缸上抬动作流程();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂取Empty盘();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂放空盘到Empty();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂取Load空盘();
                });
                Task.Run(() =>
                {
                    handler.Empty顶盘流程();
                });
                Task.Run(() =>
                {
                    handler.Empty下降流程();
                });
                Task.Run(() =>
                {
                    handler.Empty下降回零流程();
                });
                Task.Run(() =>
                {
                    handler.Load顶盘流程();
                });
                Task.Run(() =>
                {
                    handler.Load下降回零流程();
                });
                Task.Run(() =>
                {
                    handler.Auto1顶盘流程2();
                });
                Task.Run(() =>
                {
                    handler.Auto1下降回零流程();
                });
                Task.Run(() =>
                {
                    handler.Auto2顶盘流程2();
                });
                Task.Run(() =>
                {
                    handler.Auto2下降回零流程();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂放空盘到Auto1();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂放空盘到Auto2();
                });
            });

            return "Success";
        }
    }
}
