using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using H5509_alarm;
using H5509_V2.界面;
using HardwareGXN;
using HardwareGXN.GENAxisCard;
using HardwareGXN.GSNAxisCard;
using IHardwareGXN.GXNCard;
using SQLiteHelper;
using SQLServerHelper;
using Sunny.UI;
using Sunny.UI.Win32;
using TextFileLogger;
//using DTMHelper;
using Thermostat;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static H5509_V2.APPmaster;

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
        private IGXNInterface iGXNCard;

        public Action 刷新开Site信息;

        private Task Task系统按钮检测流程;
        private Task Task界面刷新;
        private Task Task数据上传;
        private Task Task温度读取;
        private Task Task初始化流程;
        private Task Task结批流程;
        private Task Task入料手臂吸放料流程;
        private Task Task出料手臂吸放料流程;
        private Task Task飞梭移动流程;
        private Task Task测试手臂取待测料流程;
        private Task Task测试手臂取已测料流程;
        private Task Task测试区气缸下压动作流程;
        private Task Task测试区气缸上抬动作流程;
        private Task Task抓盘手臂取Empty盘;
        private Task Task抓盘手臂放空盘到Empty;
        private Task Task抓盘手臂取Load空盘;
        private Task TaskEmpty顶盘流程;
        private Task TaskEmpty下降流程;
        private Task TaskEmpty下降回零流程;
        private Task TaskLoad顶盘流程;
        private Task TaskLoad下降回零流程;
        private Task TaskAuto1顶盘流程;
        private Task TaskAuto1下降回零流程;
        private Task TaskAuto2顶盘流程;
        private Task TaskAuto2下降回零流程;
        private Task Task抓盘手臂放空盘到Auto1;
        private Task Task抓盘手臂放空盘到Auto2;

        public ForMain()
        {
            InitializeComponent();

            ControlName = new Control[] { button_setting, button_offset, button_speed, button_io, button_log, button_exit };

            Log = new _log();
            显示操作log = Log.DelegteDisplayOperateLog;
            显示报警log = Log.DelegteDisplayAlarmLog;

            string result = Init_APPmaster();
            if (result != "Success")
            {
                //Log.AddLog("error", "run", "软件加载失败,请关闭软件重开!" + result);
                MessageBox.Show($"软件加载失败,请关闭软件重开!\n{result}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            string GSN = "HardwareGXN.GSNAxisController,HardwareGXN";
            string GEN = "HardwareGXN.GENAxisController,HardwareGXN";

            Type type = Type.GetType(GXN轴卡);
            var instance = Activator.CreateInstance(type);
            iGXNCard = (IGXNInterface)instance; //IGXNInterface gxncard = new HardwareImplementations.GENAxisCard.GENAxisController();
            iGXNCard.RunLog += Runlog2;

            //Form form = new FrmGSN();
            //form.Show();

            //gxn.InitCard();

            //MessageBox.Show(gxn.PrintGXN());

            //Log.AddLog("debug","run","读取数据库数据完成");

        }

        private void ForMain_Load(object sender, EventArgs e)
        {
            string result = "";
            motionEx = new MotionEx(iGXNCard);
            Com = new Communication();
            main = new _main()
            {
                刷新界面权限 = 刷新界面权限,
            };
            setting = new _setting()
            {
                刷新分BIN信息 = main.刷新分BIN信息,
                刷新温度信息 = main.刷新温度显示,
                刷新按钮状态 = 刷新按钮权限,
                刷新开Site状态 = main.刷新开Site状态,
            };
            handler = new Handler()
            {
                刷新批次信息 = main.刷新批次信息,
                刷新分BIN信息 = main.刷新分BIN信息,
                刷新权限 = main.切换为操作员权限,
                显示Dout颜色 = main.显示Dout颜色,
            };
            offset = new _offset();
            speed = new _speed();
            io = new _IO();

            Sqlite.RunLog += Runlog2;
            SqlServer.RunLog += Runlog2;

            Log.发送批次信息 = main.SendLotNo;

            Com.RefrshLog = Log.DelegteDisplayComInfo;

            达温延时 = new System.Timers.Timer() { Interval = 达温后延时时间 * 1000 }; //2分钟
            达温延时.Elapsed += main.TempFirst;

            dTMHelper = new Thermostat.DTMHelper(TCPIP, TCPPort);//"192.168.1.5", 502

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

            //Form formw = new Thermostat.FrmMain(dTMHelper);
            //formw.Show();


            //MotionEx motionex = MotionEx.GetInstance();
            //MotionEx motionex2 = MotionEx.GetInstance();

            //防止控件过多产生闪烁现象
            SetControlStyle();

            SetTablayoutDoubleBuffer();

            changeMainPanel(main);

            handler.参数初始化();

            刷新按钮权限();

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

            Form gxnform;
            if (iGXNCard.PrintGXN() == "GSN轴卡")
            {
                FrmGSN frmGsn = new HardwareGXN.GSNAxisCard.FrmGSN(iGXNCard);
                frmGsn.AxisInfoList = APPmaster.AxisInfoList;
                frmGsn.AxisHomeInfoList = APPmaster.AxisHomeInfoList;
                gxnform = frmGsn;
            }
            else gxnform = new HardwareGXN.GENAxisCard.FrmGEN(iGXNCard);

            gxnform.Show();

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
                                if (ButtonName.Contains(con.Name))
                                {
                                    con.BackColor = Color.White;
                                }
                            }
                            else
                            {
                                con.Enabled = false;
                                if (ButtonName.Contains(con.Name))
                                {
                                    con.BackColor = Color.FromArgb(226, 226, 226);
                                }
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
                            if (ButtonName.Contains(con.Name))
                            {
                                con.BackColor = Color.White;
                            }
                            con.Enabled = true;
                        }
                        else
                        {
                            con.Enabled = false;
                            if (ButtonName.Contains(con.Name))
                            {
                                con.BackColor = Color.FromArgb(226, 226, 226);
                            }
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
                UIProgressIndicatorEx progressIndicator = new UIProgressIndicatorEx
                {
                    IndicateText = "退出保存数据中...",
                    BackColor = System.Drawing.Color.Transparent,
                };

                // 创建并显示进度指示器
                this.Invoke((Action)(() =>
                {
                    this.Controls.Add(progressIndicator);
                    progressIndicator.BringToFront();
                    progressIndicator.CenterToParent();
                    progressIndicator.Start();
                }));

                Task.Run(() =>
                {
                    try
                    {
                        setting.PrintState();

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        while (true)
                        {
                            if (stopwatch.ElapsedMilliseconds > 10000) break;
                            if (LogQueue.Count == 0 && Log.logNeed2Add.Count == 0 && Log.IsSave) break;
                            Thread.Sleep(20);
                        }

                        stopwatch.Stop();
                        stopwatch.Reset();

                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        RunLog($"退出异常: {ex.Message}");
                        UIMessageTip.ShowError(this, $"退出异常: {ex.Message}", 2000);
                    }
                    finally
                    {
                        // 确保进度指示器在数据完全加载后才关闭
                        this.Invoke((Action)(() =>
                        {
                            progressIndicator.Stop();
                            this.Controls.Remove(progressIndicator);
                            progressIndicator.Dispose();
                        }));

                        //writeDataHelper.PrintLog("软件退出");
                        退出标记 = true; //让报警弹窗也退出，否则主程序退出不了
                                     //gg.CloseCard(); //关闭轴卡
                        DialogResult = DialogResult.Cancel;
                        Application.Exit(); //退出软件
                    }
                });
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

                        List<string> runlog = new List<string>();
                        List<DateTime> runlogtime = new List<DateTime>();

                        List<string> operatelog = new List<string>();
                        List<DateTime> operatelogtime = new List<DateTime>();

                        List<string> alarmlog = new List<string>();
                        List<DateTime> alarmlogtime = new List<DateTime>();

                        List<string> comlog = new List<string>();
                        List<DateTime> comlogtime = new List<DateTime>();

                        List<LogManager> logsTemp = new List<LogManager>();
                        for (int i = 0; i < countTemp; i++)
                        {
                            if (LogQueue.TryDequeue(out LogManager logs))
                            {
                                if (logs.TxtLogtype == LogType.RunLog)
                                {
                                    runlog.Add(logs.Msg);
                                    runlogtime.Add(logs.Time);
                                }
                                else if (logs.TxtLogtype == LogType.OperateLog || logs.TxtLogtype == LogType.ParamLog)
                                {
                                    operatelog.Add(logs.Msg);
                                    operatelogtime.Add(logs.Time);
                                }
                                else if (logs.TxtLogtype == LogType.AlarmLog)
                                {
                                    alarmlog.Add(logs.Msg);
                                    alarmlogtime.Add(logs.Time);
                                }
                                else if (logs.TxtLogtype == LogType.ComLog)
                                {
                                    comlog.Add(logs.Msg);
                                    comlogtime.Add(logs.Time);
                                }
                            }
                        }
                        if (runlog.Count > 0)
                        {
                            writeDataHelper.RunLog(runlog.ToArray(), runlogtime.ToArray());
                        }

                        if (comlog.Count > 0)
                        {
                            writeDataHelper.RunLog(comlog.ToArray(), comlogtime.ToArray(), LogType.ComLog);
                        }

                        if (alarmlog.Count > 0)
                        {
                            writeDataHelper.RunLog(alarmlog.ToArray(), alarmlogtime.ToArray(), LogType.AlarmLog);
                            for (int i = 0; i < alarmlog.Count; i++)
                            {
                                显示报警log(alarmlog[i], alarmlogtime[i]);
                            }
                        }

                        if (operatelog.Count > 0)
                        {
                            writeDataHelper.RunLog(operatelog.ToArray(), operatelogtime.ToArray(), LogType.OperateLog);
                            for (int i = 0; i < operatelog.Count; i++)
                            {
                                显示操作log(operatelog[i], operatelogtime[i]);
                            }
                        }
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

        private void 刷新按钮权限()
        {
            for (int i = 0; i < ListConPerm.Count; i++)
            {
                for (int j = 0; j < ControlName.Length; j++)
                {
                    if (ListConPerm[i].控件名 == ControlName[i].Name)
                    {
                        ControlName[i].Tag = ListConPerm[i].显示 + ListConPerm[i].权限描述;
                        break;
                    }
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
                            form_alarm.Activate();
                            form_alarm.Show();
                            for (int i = 0; i < Listcode.Count; i++)
                            {
                                form_alarm.QueueCode.Enqueue(Listcode[i]); //将报警代码放入队列
                                //form_alarm.ShowCode(Listcode[i], true);
                                if (!报警忽略次数.Contains(Listcode[i])) 报警次数++;
                                Listcode.Remove(Listcode[i]);
                            }
                        }
                    }));

                    if (IsPause)
                    {
                        Thread.Sleep(50);
                        continue; //如果暂停则不刷新
                    }

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

                                            int line = Sqlite.runSqlNonQuery(SQLiteRunningString, $"update SocketUse set Num=Num+1 where Id='{doutstr}'");
                                            if (line <= 0) RunLog($"update SocketUse set Num=Num+1 where Id='{doutstr}',返回:{line}");
                                        }

                                        int passnum = Dout128PassNumber[sitenum * 8 + (m * 4 + n)];
                                        int failnum = Dout128FailNumber[sitenum * 8 + (m * 4 + n)];
                                        //RunLog($"update dutCurrent set pass={passnum},total={failnum + passnum},usedTime=CAST(usedTime as bigint)+{Com.listtesttimespan[sitenum].Ticks} where Id='{doutstr}'");
                                        sql = $"update DutCurrent set pass={passnum},total={failnum + passnum},usedTime=CAST(usedTime as INTEGER)+{Com.listtesttimespan[sitenum].Ticks} where id='{doutstr}'";
                                        int line1 = Sqlite.runSqlNonQuery(SQLiteRunningString, sql);
                                        if (line1 <= 0) RunLog(sql + $"返回:{line1}");
                                        //int line = SqlServer.runSqlNonQuery(SQLServerConnString, $"create table f_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(批次开始日期 nvarchar(50) NULL,获取MAP时间 nvarchar(50) NULL,料号 nvarchar(50) NULL,批号 nvarchar(20) NULL,MAP信息 nvarchar(50) NULL )");
                                        //int line1 = SqlServer.runSqlNonQuery(SQLServerConnString, $"create table o_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间 nvarchar(50) NULL,名称 nvarchar(50) NULL,大小 nvarchar(50) NULL,数据 nvarchar(MAX) NULL)");
                                        //命令：insert into f_ZRS202507192157_20250724_17_19_57(批次开始日期, 获取MAP时间, 料号, 批号, MAP信息)values('2025年7月24日', '2025/7/24 17:31:12', 'RS70B08G3S03F', 'E9002B', '@TEST_DONE 08 06 06 44 06 05 05 06 05')，错误信息：对象名 'f_ZRS202507192157_20250724_17_19_57' 无效。
                                        //RunLog($"Site{sitenum + 1:D2}-{m * 4 + n + 1}:Pass({passnum}):Fail({failnum}):Yield({(passnum / (float)(passnum + failnum) * 100):0.0}%)");
                                    }
                                }

                                bool isyieldex = false;
                                bool isalarm = 设置信息.测试机信息.Dout连续不良报警;
                                bool isautoclose = 设置信息.测试机信息.Dout连续不良自动关闭;
                                int doutfailnum = 设置信息.测试机信息.Dout连续不良个数;

                                for (int site = 0; site < doutContinueFail.Count; site++) //连续不良率报警
                                {
                                    for (int dut = 0; dut < doutContinueFail[site].Length; dut++)
                                    {
                                        if (doutContinueFail[site][dut] >= doutfailnum)
                                        {
                                            string code = $"6{site + 1:D2}0{dut + 1}14";
                                            if (isalarm) //dout连续不良报警
                                            {
                                                RunLog($"[Code]{code},[Msg]Site{site + 1}-Dut{dut + 1}:连续不良数量:{doutContinueFail[site][dut]}>={doutfailnum}", LogType.AlarmLog);
                                                doutContinueFail[site][dut] = 0; //如果已报警良率异常则清空重新计算
                                                isyieldex = true;
                                                Listcode.Add(code);
                                            }
                                            if (isautoclose) //dout连续不良自动关闭该dout，
                                            {
                                                int dout = site * 8 + dut + 1;
                                                string labelname = $"lbl_dout{dout}";
                                                设置信息.开site信息.SetSiteValue(site, dut, "0");
                                                result = UpdateSiteStatus(设置信息.开site信息.Site名称, site + 1, 设置信息.开site信息.GetSiteValue(site));
                                                if (result != "Success")
                                                {
                                                    RunLog("Site保存失败!" + result);
                                                }
                                                //result = GetSiteStatus(设置信息.开site信息.Site名称, out SiteStatus siteStatus);
                                                //if (result != "Success") 
                                                //{
                                                //    RunLog("Site读取失败!" + result); 
                                                //}
                                                //设置信息.开site信息 = siteStatus;
                                                main.刷新开Site状态();

                                                if (!IsDutAllCLose()) //当初次开始时检测有没有开dut
                                                {
                                                    Listcode.Add("1400442");
                                                    RunLog("[Code]1400442,[Msg](按下开始)Dut全部被关闭，请至少开1个Dut进行测试", LogType.AlarmLog);
                                                    ProgramPause = true;
                                                }
                                            }
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
                                    result = Sqlite.runSql(SQLiteRunningString, "select count(*) from SocketUse where num>" + 设置信息.测试机信息.socket使用次数, out string value);
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
            int readtemp1 = 0; //Site1-Site8探测温度读取次数
            int readtemp2 = 0; //Site9-Site16探测温度读取次数
            int readtemp3 = 0; //加热盘探测温度读取次数

            int readtemp4 = 0; //Site1-Site8设定温度读取次数
            int readtemp5 = 0; //Site9-Site16设定温度读取次数
            int readtemp6 = 0; //加热盘设定温度读取次数

            while (true)
            {
                try
                {
                    if (TCPConnect)
                    {
                        #region 读取探测温度

                        string result = dTMHelper.ReadPv(0, out ushort[] value);
                        if (result != "Success")
                        {
                            读取Site1_Site8探测温度Error = result;

                            if (!温控器读取Site1_Site8探测温度异常)
                            {
                                readtemp1++;
                                if (readtemp1 > 3)
                                {
                                    readtemp1 = 0;
                                    温控器读取Site1_Site8探测温度异常 = true;
                                    Listcode.Add($"1400449");
                                    RunLog($"[Code]1400449,[Msg]读取Site1-Site8探测温度失败!详细信息:" + result, LogType.AlarmLog);

                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                        }
                        else
                        {
                            readtemp1 = 0;
                            温控器读取Site1_Site8探测温度异常 = false;
                        }

                        string result2 = dTMHelper.ReadPv(1, out ushort[] value2);
                        if (result2 != "Success")
                        {
                            读取Site9_Site16探测温度Error = result2;

                            if (!温控器读取Site9_Site16探测温度异常)
                            {
                                readtemp2++;
                                if (readtemp2 > 3)
                                {
                                    readtemp2 = 0;
                                    温控器读取Site9_Site16探测温度异常 = true;
                                    Listcode.Add($"1400450");
                                    RunLog($"[Code]1400450,[Msg]读取Site9-Site16探测温度失败!详细信息:" + result2, LogType.AlarmLog);

                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                        }
                        else
                        {
                            readtemp2 = 0;
                            温控器读取Site9_Site16探测温度异常 = false;
                        }

                        string result3 = dTMHelper.ReadPv(2, 0, out ushort[] value3);
                        if (result3 != "Success")
                        {
                            读取加热盘探测温度Error = result3;

                            if (!温控器读取加热盘探测温度异常)
                            {
                                readtemp3++;
                                if (readtemp3 > 3)
                                {
                                    readtemp3 = 0;
                                    温控器读取加热盘探测温度异常 = true;
                                    Listcode.Add($"1400451");
                                    RunLog($"[Code]1400451,[Msg]读取加热盘探测温度失败!详细信息:" + result3, LogType.AlarmLog);

                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                        }
                        else
                        {
                            readtemp3 = 0;
                            温控器读取加热盘探测温度异常 = false;
                        }

                        #endregion

                        if (result != "Success" || result2 != "Success" || result3 != "Success")
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        else
                        {
                            温控器读取Site1_Site8探测温度异常 = false;
                            温控器读取Site9_Site16探测温度异常 = false;
                            温控器读取加热盘探测温度异常 = false;
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

                        #region 读取设定温度

                        string result4 = dTMHelper.ReadSv(0, out short[] svvalue);
                        if (result4 != "Success")
                        {
                            读取Site1_Site8设定温度Error = result4;

                            if (!温控器读取Site1_Site8设定温度异常)
                            {
                                readtemp4++;
                                if (readtemp4 > 3)
                                {
                                    readtemp4 = 0;
                                    温控器读取Site1_Site8设定温度异常 = true;
                                    Listcode.Add($"1400455");
                                    RunLog($"[Code]1400455,[Msg]读取Site1-Site8设定温度失败!详细信息:" + result4, LogType.AlarmLog);

                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                        }
                        else
                        {
                            readtemp4 = 0;
                            温控器读取Site1_Site8设定温度异常 = false;
                        }

                        string result5 = dTMHelper.ReadSv(1, out short[] svvalue2);
                        if (result5 != "Success")
                        {
                            读取Site9_Site16设定温度Error = result5;

                            if (!温控器读取Site9_Site16设定温度异常)
                            {
                                readtemp5++;
                                if (readtemp5 > 3)
                                {
                                    readtemp5 = 0;
                                    温控器读取Site9_Site16设定温度异常 = true;
                                    Listcode.Add($"1400456");
                                    RunLog($"[Code]1400456,[Msg]读取Site9-Site16设定温度失败!详细信息:" + result5, LogType.AlarmLog);

                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                        }
                        else
                        {
                            readtemp5 = 0;
                            温控器读取Site9_Site16设定温度异常 = false;
                        }

                        string result6 = dTMHelper.ReadSv(2, 0, out short[] svvalue3);
                        if (result6 != "Success")
                        {
                            读取加热盘设定温度Error = result6;

                            if (!温控器读取加热盘设定温度异常)
                            {
                                readtemp6++;
                                if (readtemp6 > 3)
                                {
                                    readtemp6 = 0;
                                    温控器读取加热盘设定温度异常 = true;
                                    Listcode.Add($"1400457");
                                    RunLog($"[Code]1400457,[Msg]读取加热盘设定温度失败!详细信息:" + result6, LogType.AlarmLog);

                                    ProgramPause = true;
                                    IsPause = true;
                                }
                            }
                        }
                        else
                        {
                            readtemp6 = 0;
                            温控器读取加热盘设定温度异常 = false;
                        }

                        if (result4 != "Success" || result5 != "Success" || result6 != "Success")
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        else
                        {
                            温控器读取Site1_Site8设定温度异常 = false;
                            温控器读取Site9_Site16设定温度异常 = false;
                            温控器读取加热盘设定温度异常 = false;
                        }

                        #endregion

                        for (int i = 0; i < svvalue.Length; i++)
                        {
                            SiteSvTemp[i] = (double)svvalue[i] / 10;
                        }

                        for (int i = 0; i < svvalue2.Length; i++)
                        {
                            SiteSvTemp[i + 8] = (double)svvalue2[i] / 10;
                        }

                        SiteSvTemp[16] = (double)svvalue3[0] / 10;

                        bool issetpass = true;
                        for (int i = 0; i < SiteSvTemp.Length; i++)
                        {
                            if (SiteSvTemp[i] != 设定温度)
                            {
                                if (设定温控器温度() != "Success")
                                {
                                    issetpass = false;
                                    break;
                                }
                                else
                                {
                                    break; //只设置一次(Site1-16以及加热盘会一起设置)，下个循环再判断
                                }
                            }
                        }

                        if (!issetpass)
                        {
                            if ((温控器设定Site1_Site8温度异常 || 温控器设定Site9_Site16温度异常 || 温控器设定加热盘温度异常) && 温控器读取_设定温度报警)
                            {
                                ProgramPause = true;
                                IsPause = true;
                            }
                            Thread.Sleep(500);
                            continue;
                        }
                        else
                        {
                            温控器设定Site1_Site8温度异常 = false;
                            温控器设定Site9_Site16温度异常 = false;
                            温控器设定加热盘温度异常 = false;
                        }

                        if (IsPause)
                        {
                            Thread.Sleep(50);
                            continue;
                        }

                        bool[] tempopen = 检测温度探头状态();

                        if (tempopen.Contains(true))
                        {
                            if (温度模式 == "高温")
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
                            if (!温控器异常) //防止log输出多次
                            {
                                温控器异常 = true;
                                ProgramPause = true;
                                IsPause = true;
                                Listcode.Add("1400448");
                                RunLog($"[Code]1400448,[Msg]温控器连接失败!", LogType.AlarmLog);
                            }
                        }
                    }

                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    ProgramPause = true;
                    IsPause = true;
                    RunLog("温度读取异常!" + ex.Message);
                    Thread.Sleep(100);
                }
            }
        }

        private string Init_FrMain()
        {
            cts = new CancellationTokenSource();

            Task系统按钮检测流程 = new Task(handler.系统按钮检测流程, cts.Token); //1

            Task界面刷新 = new Task(界面刷新, cts.Token); //2

            Task数据上传 = new Task(数据上传, cts.Token); //3

            Task温度读取 = new Task(温度读取, cts.Token); //4

            Task初始化流程 = new Task(handler.初始化流程, cts.Token); //5

            Task结批流程 = new Task(handler.结批流程, cts.Token); //6

            Task入料手臂吸放料流程 = new Task(handler.入料手臂吸放料流程, cts.Token); //7

            Task出料手臂吸放料流程 = new Task(handler.出料手臂吸放料流程, cts.Token); //8

            Task飞梭移动流程 = new Task(handler.飞梭移动流程, cts.Token); //9

            Task测试手臂取待测料流程 = new Task(handler.测试手臂取待测料流程, cts.Token); //10

            Task测试手臂取已测料流程 = new Task(handler.测试手臂取已测料流程, cts.Token); //11

            Task测试区气缸下压动作流程 = new Task(handler.测试区气缸下压动作流程, cts.Token); //12

            Task测试区气缸上抬动作流程 = new Task(handler.测试区气缸上抬动作流程, cts.Token); //13

            Task抓盘手臂取Empty盘 = new Task(handler.抓盘手臂取Empty盘, cts.Token); //14

            Task抓盘手臂放空盘到Empty = new Task(handler.抓盘手臂放空盘到Empty, cts.Token); //15

            Task抓盘手臂取Load空盘 = new Task(handler.抓盘手臂取Load空盘, cts.Token); //16

            TaskEmpty顶盘流程 = new Task(handler.Empty顶盘流程, cts.Token); //17

            TaskEmpty下降流程 = new Task(handler.Empty下降流程, cts.Token); //18

            TaskEmpty下降回零流程 = new Task(handler.Empty下降回零流程, cts.Token); //19

            TaskLoad顶盘流程 = new Task(handler.Load顶盘流程, cts.Token); //20

            TaskLoad下降回零流程 = new Task(handler.Load下降回零流程, cts.Token); //21

            TaskAuto1顶盘流程 = new Task(handler.Auto1顶盘流程, cts.Token); //22

            TaskAuto1下降回零流程 = new Task(handler.Auto1下降回零流程, cts.Token); //23

            TaskAuto2顶盘流程 = new Task(handler.Auto2顶盘流程, cts.Token); //24

            TaskAuto2下降回零流程 = new Task(handler.Auto2下降回零流程, cts.Token); //25

            Task抓盘手臂放空盘到Auto1 = new Task(handler.抓盘手臂放空盘到Auto1, cts.Token); //26

            Task抓盘手臂放空盘到Auto2 = new Task(handler.抓盘手臂放空盘到Auto2, cts.Token); //27

            StartTask = Task.Run(() => //通过线程去开启线程，防止卡顿
            {
                Task系统按钮检测流程.Start(); //1

                Task界面刷新.Start(); //2

                Task数据上传.Start(); //3

                Task温度读取.Start(); //4

                Task初始化流程.Start(); //5

                Task结批流程.Start(); //6

                Task入料手臂吸放料流程.Start(); //7

                Task出料手臂吸放料流程.Start(); //8

                Task飞梭移动流程.Start(); //9

                Task测试手臂取待测料流程.Start(); //10

                Task测试手臂取已测料流程.Start(); //11

                Task测试区气缸下压动作流程.Start(); //12

                Task测试区气缸上抬动作流程.Start(); //13

                Task抓盘手臂取Empty盘.Start(); //14

                Task抓盘手臂放空盘到Empty.Start(); //15

                Task抓盘手臂取Load空盘.Start(); //16

                TaskEmpty顶盘流程.Start(); //17

                TaskEmpty下降流程.Start(); //18

                TaskEmpty下降回零流程.Start(); //19

                TaskLoad顶盘流程.Start(); //20

                TaskLoad下降回零流程.Start(); //21

                TaskAuto1顶盘流程.Start(); //22

                TaskAuto1下降回零流程.Start(); //23

                TaskAuto2顶盘流程.Start(); //24

                TaskAuto2下降回零流程.Start(); //25

                Task抓盘手臂放空盘到Auto1.Start(); //26

                Task抓盘手臂放空盘到Auto2.Start(); //27
            });

            return "Success";
        }

        private void ForMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.None)
            {
                e.Cancel = true;
            }
        }
    }

}
