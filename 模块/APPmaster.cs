using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using H5509_alarm;
using Sunny.UI;
using Sunny.UI.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static GTN.mc;
using static H5509_V2.ForMain;
using Ttimer = System.Timers.Timer;
namespace H5509_V2
{
    public class APPmaster
    {
        public static string SQLiteConnString = $"Data Source={Directory.GetCurrentDirectory() + "\\配置\\SystemData"};Polling=true;FailIfMissing=false";

        public static string SQLiteRunningString = $"Data Source={Directory.GetCurrentDirectory() + "\\配置\\RunningData"};Polling=true;FailIfMissing=false";

        public static string SQLServerConnString = "";

        public static bool InitedOK = false;

        public static bool TCPConnect = false;

        public static string TCPIP;

        public static int TCPPort;

        public static int 开Site数量 = 0;

        public static int 关Site数量 = 0;

        public static int StartSite数量 = 0;

        public static string trayTemp = "";

        public static int Socket使用次数超限数 = 0;

        public static List<分Bin> 分bin = new List<分Bin>();

        public static _log Log;

        public static ConcurrentQueue<LogManager> LogQueue = new ConcurrentQueue<LogManager>(); //log队列

        public static WriteData writeDataHelper = WriteData.GetWriteDataInstance();

        public static List<IOInfo> IOList;

        public static List<AxisInfo> AxisInfoList;

        public static List<AxisRunInfo> AxisRunInfoList;

        public static List<AxisHomeInfo> AxisHomeInfoList;

        public static InitPoint 初始点位 = new InitPoint();

        public static SetupInfo 设置信息 = new SetupInfo();

        public static Action<string, DateTime> 显示操作log;

        public static Action<string, DateTime> 显示报警log;

        public static Form_alarm form_alarm = new Form_alarm();

        public static List<string> Listcode = new List<string>(); //保存报警代码

        public static DTMHelper dTMHelper; //温控器

        public static string 当前档案;

        public static string 当前权限;

        public static int 权限时间
        {
            get
            {
                if (操作权限 == "D") return 600;
                else return 60;
            }
        }

        public static string M1_入料手臂Z1轴ErrorMsg = "";

        public static string M2_入料手臂Z2轴ErrorMsg = "";

        public static string M3_入料手臂Z3轴ErrorMsg = "";

        public static string M4_入料手臂Z4轴ErrorMsg = "";

        public static string M5_入料手臂P轴ErrorMsg = "";

        public static string M6_出料手臂Z1轴ErrorMsg = "";

        public static string M7_出料手臂Z2轴ErrorMsg = "";

        public static string M8_出料手臂Z3轴ErrorMsg = "";

        public static string M9_出料手臂Z4轴ErrorMsg = "";

        public static string M10_出料手臂P轴ErrorMsg = "";

        public static string M11_Fix3放IC轴ErrorMsg = "";

        public static string M12_Empty顶盘轴ErrorMsg = "";

        public static string M13_Loader顶盘轴ErrorMsg = "";

        public static string M14_Auto1顶盘轴ErrorMsg = "";

        public static string M15_Auto2顶盘轴ErrorMsg = "";

        public static string M16_入料手臂Y轴ErrorMsg = "";

        public static string M17_出料手臂Y轴ErrorMsg = "";

        public static string M18_放Tary盘轴ErrorMsg = "";

        public static string M19_测试手臂Z轴ErrorMsg = "";

        public static string M20_飞梭轴ErrorMsg = "";

        public static string M21_测试手臂X轴ErrorMsg = "";

        public static string M22_入料手臂X轴ErrorMsg = "";

        public static string M23_出料手臂X轴ErrorMsg = "";

        public static string 操作权限 { get; set; } = "A"; //A：operator B：engineer  C：administrator D：supplier

        public static bool IsStart = false; //开始

        public static bool ProgramStart = false; //程序开始

        public static bool ProgramBtnStart = false; //程序按钮开始

        public static bool IsPause = false; //停止

        public static bool IsLoadPause = false; //停止

        public static bool IsEmptyPause = false; //停止

        public static bool ProgramPause = false; //程序暂停

        public static bool ProgramBtnPause = false; //程序按钮暂停

        public static bool IsForecePause = false; //强制停止

        public static int 第二段距离 = 8000;

        public static double 第二段速度 = 0.3;

        public static int 出入料Z第二段距离 = 300;

        public static double 出入料Z第二段速度 = 0.5;

        public static bool 退出标记 = false;

        public static int 设备状态 = 0;

        public static bool 空跑模式 = false;

        public static bool Tray盘模式 = true;

        public static bool 无测试机模式 = false;

        public static bool 安全门禁用 = false;

        public static bool 温控加热 = false;

        public static System.Timers.Timer 达温延时;

        public static bool 温控达温 = false;

        public static bool 初次达温 = false; //初次达温标记

        public static double 设定温度
        {
            get { return 设置信息.Heat盘信息.设定温度; }
            set { 设置信息.Heat盘信息.设定温度 = value; }
        }

        public static string 温度模式
        {
            get { return 设置信息.温度模式; }
        }

        public static int IC加热时间
        {
            get { return 设置信息.Heat盘信息.IC加热时间; }
        }
        public static int 达温后延时时间
        {
            get { return 设置信息.Heat盘信息.达温后延时; }
        }

        public static bool H5509归零 = false;

        public static bool H5509正在归零 = false;

        public static bool H5509初始化 = false;

        public static bool H5509正在初始化 = false;

        public static bool 结批标记 = false;

        public static bool 正在结批 = false;

        public static bool 结批完成 = false;

        public static bool 强制结批标记 = false;

        public static bool 定点停止标记 = false;

        public static bool 定点停止允许 = false;

        public static bool 开舱门上顶标记 = false;

        public static bool Load舱门上顶标记 = false;

        public static bool Empty舱门上顶标记 = false;

        public static bool Auto1舱门上顶标记 = false;

        public static bool Auto2舱门上顶标记 = false;

        public static int Load舱门开启标记 = 0;

        public static int Empty舱门开启标记 = 0;

        public static int Auto1舱门开启标记 = 0;

        public static int Auto2舱门开启标记 = 0;

        public static bool Empty下降回零开关 = false;

        public static bool Load下降回零开关 = false;

        public static bool Auto1下降回零开关 = false;

        public static bool Auto2下降回零开关 = false;

        public static bool Empty正在下降回零 = false;

        public static bool Load正在下降回零 = false;

        public static bool Auto1正在下降回零 = false;

        public static bool Auto2正在下降回零 = false;

        public static bool Empty舱门正在上顶 = false;

        public static bool Load舱门正在上顶 = false;

        public static bool Auto1舱门正在上顶 = false;

        public static bool Auto2舱门正在上顶 = false;

        public static bool Load换盘标记 = false;

        public static bool Auto1换盘标记 = false;

        public static bool Auto2换盘标记 = false;

        public static bool Auto等待换盘标记 = false;

        public static int 气缸动作超时时间 = 3000; //ms

        public static int 跟随误差 = 250;

        public static Communication Com;

        public static double TestShuttleLPos = 800;

        public static double TestShuttleRPos = 900.6;

        public static double Testx1pos = 12;

        public static double Testx2pos = 401.4;

        public static double Testx3pos = 1089.5;

        public static double Testx4pos = 1478.8;

        public static double[] TestXPoint; //Site点位坐标

        public static double[] LoaderXPoint; //Loader待测盘

        public static double[] LoaderYPoint; //Loader待测盘

        public static double[] HeatXPoint;  //加热盘X坐标

        public static double[] HeatYPoint; //加热盘Y坐标

        public static double[] Auto1XPoint; //自动盘1X坐标

        public static double[] Auto1YPoint; //自动盘1Y坐标

        public static double[] Auto2XPoint; //自动盘2X坐标

        public static double[] Auto2YPoint; //自动盘2Y坐标

        public static double[] Fix1XPoint; //手动盘1X坐标

        public static double[] Fix1YPoint; //手动盘1Y坐标

        public static double[] Fix2XPoint; //手动盘2X坐标

        public static double[] Fix2YPoint; //手动盘2Y坐标

        public static double[] Fix3XPoint; //手动盘3X坐标

        public static double[] Fix3YPoint; //手动盘3Y坐标

        public static Dictionary<string, bool> LoaderDicPoint = new Dictionary<string, bool>(); //待测盘Map图

        public static Dictionary<string, bool> HeatDicPoint = new Dictionary<string, bool>(); //加热盘Map图

        public static Dictionary<string, HeatMapTimer> HeatDicTimer = new Dictionary<string, HeatMapTimer>(); //加热盘状态图

        public static Dictionary<string, bool> Auto1DicPoint = new Dictionary<string, bool>(); //自动盘1Map图

        public static Dictionary<string, bool> Auto2DicPoint = new Dictionary<string, bool>(); //自动盘2Map图

        public static Dictionary<string, bool> Fix1DicPoint = new Dictionary<string, bool>(); //手动盘1Map图

        public static Dictionary<string, bool> Fix2DicPoint = new Dictionary<string, bool>(); //手动盘2Map图

        public static Dictionary<string, bool> Fix3DicPoint = new Dictionary<string, bool>(); //手动盘3Map图

        public static Dictionary<string, string> Auto1DicBin { get; set; } = new Dictionary<string, string>();

        public static Dictionary<string, string> Auto2DicBin { get; set; } = new Dictionary<string, string>();

        public static Dictionary<string, string> Fix1DicBin { get; set; } = new Dictionary<string, string>();

        public static Dictionary<string, string> Fix2DicBin { get; set; } = new Dictionary<string, string>();

        public static Dictionary<string, string> Fix3DicBin { get; set; } = new Dictionary<string, string>();

        public static int Fix1Num = 1;

        public static int Fix2Num = 1;

        public static int Fix3Num = 1;

        public static string Auto1 = "01"; //"01"

        public static string Auto2 = "02,03"; //"02,03,04,05,06"

        public static string Fix1 = "04";

        public static string Fix1_1 = "07";

        public static string Fix1_2 = "07";

        public static string Fix1_3 = "07";

        public static string Fix2 = "05";//07

        public static string Fix2_1 = "08";

        public static string Fix2_2 = "08";

        public static string Fix2_3 = "08";

        public static string Fix3 = "06";

        public static string Fix3_1 = "09";

        public static string Fix3_2 = "09";

        public static string Fix3_3 = "09";

        public static string oth = "";

        public static string othAll = "";

        public static int[] Dout128PassNumber = new int[128];

        public static int[] Dout128FailNumber = new int[128];

        public static List<int[]> doutContinueFail;

        public static List<int[]> 单dut总不良计数; //单dut总不良计数（取消报警后清零）

        public static List<int[]> 单dut总不良累计计数;//单dut总不良累计计数（取消报警后不清理）

        public static int 总良率报警间隔计数 { get; set; } = 0;

        public static bool 入料IC忽略;

        public static bool 入料IC吸不起结批;

        public static bool 无待测盘结批;

        public static bool 出料IC忽略;

        public static bool 测试IC忽略;

        public static int 飞梭2位置 = 0; //0:后面 1:前面

        public static bool 调机标记 = true;

        public static int Tray平移左右标记 = 0;

        public static bool Tray平移运动标记; //是否正在平移

        public static short Interval = 2; //吸料间隔

        public static short HeatInterval = 1; //Heat吸放料间隔

        public static double 梭车列间距 = 40; //mm

        public static double 梭车行间距 = 36; //mm

        public static double Auto1初始点 = 193;

        public static double Auto2初始点 = 193;

        public static double Auto1上移固定距离 = 29.5;

        public static double Auto2上移固定距离 = 29.5;

        public static DateTime IndexTime = DateTime.Now;

        public static TimeSpan DIndexTime = new TimeSpan(0);

        public static TimeSpan TestTime = new TimeSpan(0);

        public static DateTime 软件开启时间 = DateTime.Now;

        public static DateTime 开始时间 = DateTime.Now;

        public static DateTime 达温时间 = DateTime.Now;

        public static DateTime 按下结批时间 = DateTime.Now;

        public static DateTime 结束时间 = DateTime.Now;

        public static TimeSpan 实时运行时间 { get { return DateTime.Now - 开始时间; } }

        public static TimeSpan 批次运行时间 { get { return 结束时间 - 开始时间; } }

        public static int RealUPHNum { get { return (int)(出料数 / 实时运行时间.TotalSeconds * 3600); } }

        public static int UPHNum
        {
            get
            {
                if (出料数 == 0) return 0;
                else return (int)(出料数 / 批次运行时间.TotalSeconds * 3600);
            }
        }

        public static TimeSpan 结批时间 { get { return 按下结批时间 - 结束时间; } }

        public static TimeSpan 升温时间 { get { return 达温时间 - 开始时间; } }

        public static double[] SiteTemp = new double[17];

        public static double HeatTrayTemp { get { return SiteTemp[16]; } }

        public static int UphNumber = 0;

        public static int 载入数 = 0;

        public static int 出料数 { get { return Auto1数量 + Auto2数量 + Fix1数量 + Fix2数量 + Fix3数量; } }

        public static int Auto1数量 = 0;

        public static int Auto2数量 = 0;

        public static int Fix1数量 = 0;

        public static int Fix2数量 = 0;

        public static int Fix3数量 = 0;

        public static double Auto1良率
        {
            get
            {
                if (出料数 == 0) return 0;
                else return (double)Auto1数量 / 出料数 * 100;
            }
        }

        public static double Auto2良率
        {
            get
            {
                if (出料数 == 0) return 0;
                else return (double)Auto2数量 / 出料数 * 100;
            }
        }

        public static double Fix1良率
        {
            get
            {
                if (出料数 == 0) return 0;
                else return (double)Fix1数量 / 出料数 * 100;
            }
        }

        public static double Fix2良率
        {
            get
            {
                if (出料数 == 0) return 0;
                else return (double)Fix2数量 / 出料数 * 100;
            }
        }

        public static double Fix3良率
        {
            get
            {
                if (出料数 == 0) return 0;
                else return (double)Fix3数量 / 出料数 * 100;
            }
        }

        public static double Tray_厚度
        {
            get { return 设置信息.Tray盘信息.Height; }
        }

        public static HeatInfo 加热盘
        {
            get { return 设置信息.Heat盘信息; }
        }

        public static OffsetPoint 点位微调
        {
            get { return 设置信息.OffsetPoint; }
        }

        public static SiteStatus 开Site信息
        {
            get { return 设置信息.开site信息; }
        }

        public static int Heat_X_Num
        {
            get { return 设置信息.Heat盘信息.X_Num; }
        }

        public static int Heat_Y_Num
        {
            get { return 设置信息.Heat盘信息.Y_Num; }
        }
        public static int Tray_X_Num
        {
            get { return 设置信息.Tray盘信息.X_Num; }
        }

        public static int Tray_Y_Num
        {
            get { return 设置信息.Tray盘信息.Y_Num; }
        }

        public static double Tray_Y_Pitch
        {
            get { return 设置信息.Tray盘信息.Y_Pitch; }
        }

        public static double Heat_Y_Pitch
        {
            get { return 设置信息.Heat盘信息.Y_Pitch; }
        }

        public static double 入料吸料间距 //tray吸
        {
            get { return 初始点位.入料_变距最大值 - Tray_Y_Pitch * Interval; }
        }

        public static double 入料放料间距 //梭车放
        {
            get { return 初始点位.入料_变距最大值 - 梭车行间距; }
        }

        public static double 出料吸料间距 //梭车吸
        {
            get { return 初始点位.出料_变距最大值 - 梭车行间距; }
        }

        public static double 出料放料间距 //tray放
        {
            get { return 初始点位.出料_变距最大值 - Tray_Y_Pitch * Interval; }
        }

        public static double 入料吸放料Heat间距 //Heat吸放
        {
            get { return 初始点位.入料_变距最大值 - Heat_Y_Pitch * HeatInterval; }
        }

        public static int[] SiteTestState = new int[16];

        public static string[] LotNo = new string[17];

        //public static List<bool[][]> SiteStatus = new List<bool[][]>(); //保存Site测试开关状态

        public static List<bool[][]> SiteTestIC = new List<bool[][]>(); //保存Site放入的IC状态

        public static ConcurrentQueue<string[]> BinSaveQueue = new ConcurrentQueue<string[]>(); //用来保存接收的Bin数据的队列

        public static ConcurrentQueue<int> InSiteQueue = new ConcurrentQueue<int>(); //入料队列

        public static ConcurrentQueue<int> OutSiteQueue = new ConcurrentQueue<int>(); //出料队列

        //public static ConcurrentQueue<int> OutSiteQueue = new ConcurrentQueue<int>(); //出料队列

        public static bool 空盘上顶到最大值 = false;

        public static bool 空盘下降到最小值 = false;

        public static int 空盘上顶最大值 = 429895;

        public static int 空盘下降最小值 = 10000;

        public static int 空盘满盘判断值 = 370000; //30000//370000//367864//12700

        public static bool 待测盘上顶到最大值 = false;

        public static bool 待测盘下降到最小值 = false;

        public static int 待测盘上顶最大值 = 434675;

        public static int 待测盘下降最小值 = 15000;

        public static int Auto1上顶最大值 = 468000;

        public static int Auto1下降最小值 = 10000;

        public static int Auto1满盘判断值 = 25000;//360000

        public static int Auto2上顶最大值 = 468000;

        public static int Auto2下降最小值 = 10000;

        public static int Auto2满盘判断值 = 25000;

        public static bool Auto1料仓满盘标记;

        public static bool Auto2料仓满盘标记;

        public static bool Auto1放盘等待标记;

        public static bool Auto2放盘等待标记;

        public static bool Empty料仓打开标记;

        public static bool Load料仓打开标记;

        public static bool Auto1料仓打开标记;

        public static bool Auto2料仓打开标记;

        public static bool Fix1Tray盘放置标记;

        public static bool Fix1Tray盘拿走标记;

        public static bool Fix2Tray盘放置标记;

        public static bool Fix2Tray盘拿走标记;

        public static bool Fix3Tray盘放置标记;

        public static bool Fix3Tray盘拿走标记;

        public static bool 入料手臂工作完成标记;

        public static bool 加热盘停止入料标记;

        public static bool 入料手臂停止工作标记;

        public static bool 测试手臂入料停止工作标记;

        public static bool 测试手臂出料停止工作标记;

        public static bool 出料手臂出料停止工作标记;

        public static bool 飞梭停止工作;

        public static bool 出料手臂工作完成标记;

        public static bool 测试手臂工作完成标记;

        public static bool 入料手臂抓料等待标记;

        public static short 飞梭移动状态;

        public static short 飞梭前入料状态;

        public static short 飞梭前出料状态;

        public static short 飞梭后入料状态;

        public static short 飞梭后出料状态;

        public static bool InNozzleIC = false;

        public static bool[] InNozzleICOpen = new bool[4];

        public static bool OutNozzleIC = false;

        public static bool[] OutNozzleICOpen = new bool[4];

        public static bool TestNozzleIC = false;

        public static bool[] TestNozzleICOpen = new bool[8];

        public static bool[] 保存开Site更改;

        public static bool[][] 保存关Site更改;

        public static bool[] 正在吸取后入料;

        public static bool[][] 正在放置前入料;

        public static bool[][] 飞梭前入料IC;

        public static bool[][] 飞梭前出料IC;

        public static bool[][] 飞梭后入料IC;

        public static bool[][] 飞梭后出料IC;

        public static short Empty上顶标记;

        public static short Load上顶标记;

        public static short Empty下降标记;

        public static short Load下降标记;

        public static short Auto1上顶标记;

        public static short Auto2上顶标记;

        public static short Empty下降回零标记;

        public static short Load下降回零标记;

        public static short Auto1下降回零标记;

        public static short Auto2下降回零标记;

        public static int 抓盘手臂取Empty空盘标记;

        public static int 抓盘手臂取Load空盘标记;

        public static int 抓盘手臂给Auto1空盘标记;

        public static int 抓盘手臂给Auto2空盘标记;

        public static int 抓盘手臂放空盘到Empty标记;

        public static int 抓盘手臂放空盘到Auto1标记;

        public static int 抓盘手臂放空盘到Auto2标记;

        public static bool[][] 放料IC目标值;

        public static string[][] 飞梭前出料BIN项;

        public static string[][] 飞梭后出料BIN项;

        public static Task StartTask;

        public static CancellationTokenSource cts;

        public static ST_ProcessManage PS初始化流程;

        public static ST_ProcessManage PS结批流程;

        public static ST_ProcessManage PS入料手臂吸放料流程;

        public static ST_ProcessManage PS出料手臂吸放料流程;

        public static ST_ProcessManage PS测试手臂取待测料流程;

        public static ST_ProcessManage PS测试手臂取已测料流程;

        public static ST_ProcessManage PS测试区气缸下压动作流程;

        public static ST_ProcessManage PS测试区气缸上抬动作流程;

        public static ST_ProcessManage PS空盘上顶流程;

        public static ST_ProcessManage PS空盘下降流程;

        public static ST_ProcessManage PS空盘下降回零流程;

        public static ST_ProcessManage PS待测盘上顶流程;

        public static ST_ProcessManage PS待测盘下降回零流程;

        public static ST_ProcessManage PS待测盘下降流程;

        public static ST_ProcessManage PS手动盘1上顶流程;

        public static ST_ProcessManage PS手动盘1下降回零流程;

        public static ST_ProcessManage PS手动盘2上顶流程;

        public static ST_ProcessManage PS手动盘2下降回零流程;

        public static ST_ProcessManage PS入料手臂放料流程;

        public static ST_ProcessManage PS抓盘手臂取Empty空盘流程;

        public static ST_ProcessManage PS抓盘手臂取Load空盘流程;

        public static ST_ProcessManage PS抓盘手臂放空盘到Empty流程;

        public static ST_ProcessManage PS抓盘手臂放空盘到Auto1流程;

        public static ST_ProcessManage PS抓盘手臂放空盘到Auto2流程;

        public static ST_ProcessManage PS抓盘手臂给Auto1空盘流程;

        public static ST_ProcessManage PS抓盘手臂给Auto2空盘流程;

        public static ST_ProcessManage PS抓盘手臂换Auto1空盘流程;

        public static ST_ProcessManage PS抓盘手臂换Auto2空盘流程;

        public static ST_ProcessManage PS入飞梭移动流程;

        public static ST_ProcessManage PS整机回零流程;

        /// <summary>
        /// 程序是否触发开始
        /// </summary>
        public bool IsProgramStart { get; set; } = false;

        /// <summary>
        /// 程序是否触发开始
        /// </summary>
        public bool IsProgramPause { get; set; } = false;

        /// <summary>
        /// 创建开始上升沿的缓存值
        /// </summary>
        public static bool StartRiseCache = false;

        /// <summary>
        /// 创建停止上升沿的缓存值
        /// </summary>
        public static bool StopRiseCache = false;

        /// <summary>
        /// 创建安全扭上升沿的缓存值
        /// </summary>
        public static bool SafeswRiseCache = false;

        /// <summary>
        /// 创建重试上升沿的缓存值
        /// </summary>
        public static bool AgainRiseCache = false;

        /// <summary>
        /// 创建忽略上升沿的缓存值
        /// </summary>
        public static bool IgnoreRiseCache = false;

        /// <summary>
        /// 创建定点停止上升沿的缓存值
        /// </summary>
        public static bool FixedStopRiseCache = false;

        /// <summary>
        /// 创建定点开始上升沿的缓存值
        /// </summary>
        public static bool FixedStartRiseCache = false;

        /// <summary>
        /// 创建结批上升沿的缓存值
        /// </summary>
        public static bool StopBatchRiseCache = false;

        /// <summary>
        /// 创建结批下降沿的缓存值
        /// </summary>
        public static bool StopBatchFallCache = false;

        /// <summary>
        /// 创建Loader开门上升沿的缓存值
        /// </summary>
        public static bool LoaderDoorRiseCache = false;

        /// <summary>
        /// 创建Loader开门下降沿的缓存值
        /// </summary>
        public static bool LoaderDoorFallCache = false;

        /// <summary>
        /// 创建Empty开门上升沿的缓存值
        /// </summary>
        public static bool EmptyDoorRiseCache = false;

        /// <summary>
        /// 创建Empty开门下降沿的缓存值
        /// </summary>
        public static bool EmptyDoorFallCache = false;
        /// <summary>
        /// 创建Auto1开门上升沿的缓存值
        /// </summary>
        public static bool Auto1DoorRiseCache = false;

        /// <summary>
        /// 创建Auto1开门下降沿的缓存值
        /// </summary>
        public static bool Auto1DoorFallCache = false;

        /// <summary>
        /// 创建Auto2开门上升沿的缓存值
        /// </summary>
        public static bool Auto2DoorRiseCache = false;

        /// <summary>
        /// 创建Auto2开门下降沿的缓存值
        /// </summary>
        public static bool Auto2DoorFallCache = false;

        /// <summary>
        /// 创建Fix1上升沿的缓存值
        /// </summary>
        public static bool Fix1TrayRiseCache = false;

        /// <summary>
        /// 创建Fix1下降沿的缓存值
        /// </summary>
        public static bool Fix1TrayFallCache = false;

        /// <summary>
        /// 创建Fix2上升沿的缓存值
        /// </summary>
        public static bool Fix2TrayRiseCache = false;

        /// <summary>
        /// 创建Fix2下降沿的缓存值
        /// </summary>
        public static bool Fix2TrayFallCache = false;

        /// <summary>
        /// 创建Fix3上升沿的缓存值
        /// </summary>
        public static bool Fix3TrayRiseCache = false;

        /// <summary>
        /// 创建Fix3下降沿的缓存值
        /// </summary>
        public static bool Fix3TrayFallCache = false;

        /// <summary>
        /// 创建一个循环上升沿的缓存值
        /// </summary>
        public static bool OnecycleRiseCache = false;

        /// <summary>
        /// 创建归零上升沿的缓存值
        /// </summary>
        public static bool HomeRiseCache = false;

        /// <summary>
        /// 创建换Tray上升沿的缓存值
        /// </summary>
        public static bool ExchangetrayRiseCache = false;

        /// <summary>
        /// 创建暂停上升沿的缓存值
        /// </summary>
        public static bool EndRiseCache = false;

        /// <summary>
        /// 创建前门禁下降沿的缓存值
        /// </summary>
        public static bool FrontDoorFallCache = false;

        /// <summary>
        /// 创建后门禁下降沿的缓存值
        /// </summary>
        public static bool RearDoorFallCache = false;

        /// <summary>
        /// 创建左门禁下降沿的缓存值
        /// </summary>
        public static bool LeftDoorFallCache = false;

        /// <summary>
        /// 创建右门禁下降沿的缓存值
        /// </summary>
        public static bool RightDoorFallCache = false;

        /// <summary>
        /// 创建Fix盘平移按键上升沿的缓存值
        /// </summary>
        public static bool TrayMoveRiseCache = false;

        public static bool 是否可以初始化()
        {
            if (IsPause == true || IsStart == false)
            {
                return false;
            }

            if (H5509归零 == false)
            {
                return false;
            }

            return true;
        }

        public static bool 是否可以开始运行()
        {
            if (IsPause == true || IsStart == false)
            {
                return false;
            }

            if (H5509初始化 == false)
            {
                return false;
            }

            //if (定点停止允许 == true)
            //{
            //    if (定点停止标记 == true)
            //    {
            //        return false;
            //    }
            //    //按下开始 定点停止标记为true
            //}

            return true;
        }

        public static void CheckStatus()
        {

        }

        public static void GetCoreAxis(string axisname, out short core, out short axis)
        {
            AxisInfo axisInfo = new AxisInfo();
            for (int i = 0; i < AxisInfoList.Count; i++)
            {
                if (AxisInfoList[i].轴名称 == axisname)
                {
                    axisInfo = AxisInfoList[i];
                }
            }
            core = axisInfo.核号;
            axis = axisInfo.轴号;
        }

        public static void AxisRunGetCoreAxis()
        {
            for (int i = 0; i < AxisRunInfoList.Count; i++)
            {
                GetCoreAxis(AxisRunInfoList[i].轴名称, out short core, out short axis);
                AxisRunInfoList[i].核号 = core;
                AxisRunInfoList[i].轴号 = axis;
            }
        }

        public static void AxisHomeGetCoreAxis()
        {
            for (int i = 0; i < AxisHomeInfoList.Count; i++)
            {
                GetCoreAxis(AxisHomeInfoList[i].轴名称, out short core, out short axis);
                AxisHomeInfoList[i].核号 = core;
                AxisHomeInfoList[i].轴号 = axis;
            }
        }

        public static string GetIOInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from IOInfo", out DataTable dataTable);
            if (result == "Success")
            {
                APPmaster.IOList = new List<IOInfo>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    APPmaster.IOList.Add(new IOInfo()
                    {
                        模块类型 = dr["模块型号"].ToString(),
                        输入 = Convert.ToInt16(dr["输入"].ToString()),
                        输出 = Convert.ToInt16(dr["输出"].ToString()),
                        核号 = Convert.ToInt16(dr["核号"].ToString()),
                        模块号 = Convert.ToInt16(dr["模块号"].ToString()),
                        硬件索引 = Convert.ToInt16(dr["硬件索引"].ToString()),
                        软件索引 = Convert.ToInt16(dr["软件索引"].ToString()),
                        IO编号 = dr["IO编号"].ToString(),
                        IO名称 = dr["IO名称"].ToString(),
                        IO区域 = dr["区域"].ToString()
                    });
                }
            }
            return result;
        }

        public static string GetInitPoint()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from InitPoint", out DataTable dataTable);
            if (result == "Success")
            {
                InitPoint initPoint = new InitPoint();
                foreach (DataRow item in dataTable.Rows)
                {
                    double value = Convert.ToDouble(item["value"].ToString());
                    switch (item["ptname"].ToString())
                    {
                        case "入料_Load_X": initPoint.入料_Load_X = value; break;
                        case "入料_Load_Y": initPoint.入料_Load_Y = value; break;
                        case "入料_Load_Z": initPoint.入料_Load_Z = value; break;
                        case "入料_Heat_X": initPoint.入料_Heat_X = value; break;
                        case "入料_Heat_Y": initPoint.入料_Heat_Y = value; break;
                        case "入料_Heat吸_Z": initPoint.入料_Heat吸_Z = value; break;
                        case "入料_Heat放_Z": initPoint.入料_Heat放_Z = value; break;
                        case "入料_梭车1_X": initPoint.入料_梭车1_X = value; break;
                        case "入料_梭车1_Y": initPoint.入料_梭车1_Y = value; break;
                        case "入料_梭车1_Z": initPoint.入料_梭车1_Z = value; break;
                        case "入料_梭车2_X": initPoint.入料_梭车2_X = value; break;
                        case "入料_梭车2_Y": initPoint.入料_梭车2_Y = value; break;
                        case "入料_梭车2_Z": initPoint.入料_梭车2_Z = value; break;
                        case "入料_梭车列间隔": initPoint.入料_梭车列间隔 = value; break;
                        case "入料_变距最大值": initPoint.入料_变距最大值 = value; break;
                        case "入料_变距最小值": initPoint.入料_变距最小值 = value; break;
                        case "入料_Z1_硬件补偿": initPoint.入料_Z1_硬件补偿 = value; break;
                        case "入料_Z2_硬件补偿": initPoint.入料_Z2_硬件补偿 = value; break;
                        case "入料_Z3_硬件补偿": initPoint.入料_Z3_硬件补偿 = value; break;
                        case "入料_Z4_硬件补偿": initPoint.入料_Z4_硬件补偿 = value; break;
                        case "出料_梭车1_X": initPoint.出料_梭车1_X = value; break;
                        case "出料_梭车1_Y": initPoint.出料_梭车1_Y = value; break;
                        case "出料_梭车1_Z": initPoint.出料_梭车1_Z = value; break;
                        case "出料_梭车2_X": initPoint.出料_梭车2_X = value; break;
                        case "出料_梭车2_Y": initPoint.出料_梭车2_Y = value; break;
                        case "出料_梭车2_Z": initPoint.出料_梭车2_Z = value; break;
                        case "出料_Auto1_X": initPoint.出料_Auto1_X = value; break;
                        case "出料_Auto1_Y": initPoint.出料_Auto1_Y = value; break;
                        case "出料_Auto1_Z": initPoint.出料_Auto1_Z = value; break;
                        case "出料_Auto2_X": initPoint.出料_Auto2_X = value; break;
                        case "出料_Auto2_Y": initPoint.出料_Auto2_Y = value; break;
                        case "出料_Auto2_Z": initPoint.出料_Auto2_Z = value; break;
                        case "出料_Fix1_X": initPoint.出料_Fix1_X = value; break;
                        case "出料_Fix1_Y": initPoint.出料_Fix1_Y = value; break;
                        case "出料_Fix1_Z": initPoint.出料_Fix1_Z = value; break;
                        case "出料_Fix2_X": initPoint.出料_Fix2_X = value; break;
                        case "出料_Fix2_Y": initPoint.出料_Fix2_Y = value; break;
                        case "出料_Fix2_Z": initPoint.出料_Fix2_Z = value; break;
                        case "出料_Fix3_X": initPoint.出料_Fix3_X = value; break;
                        case "出料_Fix3_Y": initPoint.出料_Fix3_Y = value; break;
                        case "出料_Fix3_Z": initPoint.出料_Fix3_Z = value; break;
                        case "出料_梭车列间隔": initPoint.出料_梭车列间隔 = value; break;
                        case "出料_变距最大值": initPoint.出料_变距最大值 = value; break;
                        case "出料_变距最小值": initPoint.出料_变距最小值 = value; break;
                        case "出料_Fix3_右移动": initPoint.出料_Fix3_右移动 = value; break;
                        case "出料_Z1_硬件补偿": initPoint.出料_Z1_硬件补偿 = value; break;
                        case "出料_Z2_硬件补偿": initPoint.出料_Z2_硬件补偿 = value; break;
                        case "出料_Z3_硬件补偿": initPoint.出料_Z3_硬件补偿 = value; break;
                        case "出料_Z4_硬件补偿": initPoint.出料_Z4_硬件补偿 = value; break;
                        case "测试_梭车1吸_X": initPoint.测试_梭车1吸_X = value; break;
                        case "测试_梭车1吸_Z": initPoint.测试_梭车1吸_Z = value; break;
                        case "测试_梭车2吸_X": initPoint.测试_梭车2吸_X = value; break;
                        case "测试_梭车2吸_Z": initPoint.测试_梭车2吸_Z = value; break;
                        case "测试_梭车1放_X": initPoint.测试_梭车1放_X = value; break;
                        case "测试_梭车1放_Z": initPoint.测试_梭车1放_Z = value; break;
                        case "测试_梭车2放_X": initPoint.测试_梭车2放_X = value; break;
                        case "测试_梭车2放_Z": initPoint.测试_梭车2放_Z = value; break;
                        case "测试_梭车列间隔": initPoint.测试_梭车列间隔 = value; break;
                        case "测试_Test1区_X": initPoint.测试_Test1区_X = value; break;
                        case "测试_Test2区_X": initPoint.测试_Test2区_X = value; break;
                        case "测试_Test3区_X": initPoint.测试_Test3区_X = value; break;
                        case "测试_Test4区_X": initPoint.测试_Test4区_X = value; break;
                        case "测试_Test区Site间隔": initPoint.测试_Test区Site间隔 = value; break;
                        case "测试_Test区吸_Z": initPoint.测试_Test区吸_Z = value; break;
                        case "测试_Test区放_Z": initPoint.测试_Test区放_Z = value; break;
                        case "梭车2_前_Y": initPoint.梭车2_前_Y = value; break;
                        case "梭车2_后_Y": initPoint.梭车2_后_Y = value; break;
                        case "抓盘_Empty_X": initPoint.抓盘_Empty_X = value; break;
                        case "抓盘_Load_X": initPoint.抓盘_Load_X = value; break;
                        case "抓盘_Auto1_X": initPoint.抓盘_Auto1_X = value; break;
                        case "抓盘_Auto2_X": initPoint.抓盘_Auto2_X = value; break;
                    }
                }
                初始点位 = initPoint;
            }
            return result;
        }

        public static string GetHeatInfo(string name, out HeatInfo heatInfo)
        {
            heatInfo = new HeatInfo();
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from HeatInfo where name = '{name}'", out DataTable dataTable);
            if (result == "Success")
            {
                try
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dr = dataTable.Rows[0];

                        heatInfo = new HeatInfo()
                        {
                            HeatName = dr["name"].ToString(),
                            X_Num = Convert.ToInt32(dr["xnum"].ToString()),
                            Y_Num = Convert.ToInt32(dr["ynum"].ToString()),
                            X_Start = Convert.ToDouble(dr["xstart"].ToString()),
                            Y_Start = Convert.ToDouble(dr["ystart"].ToString()),
                            X_Pitch = Convert.ToDouble(dr["xpitch"].ToString()),
                            Y_Pitch = Convert.ToDouble(dr["ypitch"].ToString()),
                            Width = Convert.ToDouble(dr["width"].ToString()),
                            Length = Convert.ToDouble(dr["length"].ToString()),
                            Height = Convert.ToDouble(dr["height"].ToString()),
                            设定温度 = Convert.ToDouble(dr["设定温度"].ToString()),
                            波动温度 = Convert.ToDouble(dr["波动温度"].ToString()),
                            IC加热时间 = Convert.ToInt32(dr["IC加热时间"].ToString()),
                            达温后延时 = Convert.ToInt32(dr["达温后延时"].ToString()),
                            报警温度 = Convert.ToDouble(dr["报警温度"].ToString()),
                            超温温度 = Convert.ToDouble(dr["超温温度"].ToString()),
                            升温超时报警 = Convert.ToInt32(dr["升温超时报警"].ToString()),
                            达温标准 = dr["达温标准"].ToString(),
                            探头类型 = dr["探头类型"].ToString(),
                            温控器温度补偿 = Convert.ToDouble(dr["温控器温度补偿"].ToString()),
                            Site1温度补偿 = Convert.ToDouble(dr["Site1温度补偿"].ToString()),
                            Site2温度补偿 = Convert.ToDouble(dr["Site2温度补偿"].ToString()),
                            Site3温度补偿 = Convert.ToDouble(dr["Site3温度补偿"].ToString()),
                            Site4温度补偿 = Convert.ToDouble(dr["Site4温度补偿"].ToString()),
                            Site5温度补偿 = Convert.ToDouble(dr["Site5温度补偿"].ToString()),
                            Site6温度补偿 = Convert.ToDouble(dr["Site6温度补偿"].ToString()),
                            Site7温度补偿 = Convert.ToDouble(dr["Site7温度补偿"].ToString()),
                            Site8温度补偿 = Convert.ToDouble(dr["Site8温度补偿"].ToString()),
                            Site9温度补偿 = Convert.ToDouble(dr["Site9温度补偿"].ToString()),
                            Site10温度补偿 = Convert.ToDouble(dr["Site10温度补偿"].ToString()),
                            Site11温度补偿 = Convert.ToDouble(dr["Site11温度补偿"].ToString()),
                            Site12温度补偿 = Convert.ToDouble(dr["Site12温度补偿"].ToString()),
                            Site13温度补偿 = Convert.ToDouble(dr["Site13温度补偿"].ToString()),
                            Site14温度补偿 = Convert.ToDouble(dr["Site14温度补偿"].ToString()),
                            Site15温度补偿 = Convert.ToDouble(dr["Site15温度补偿"].ToString()),
                            Site16温度补偿 = Convert.ToDouble(dr["Site16温度补偿"].ToString()),
                        };
                    }
                    else result = $"无加热盘{name}信息";
                }
                catch (Exception ex)
                {
                    return "获取加热盘数据异常!" + ex.Message;
                }
            }
            return result;
        }

        public static string GetOffsetPoint(string name, out OffsetPoint offsetPoint)
        {
            offsetPoint = new OffsetPoint();
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from OffsetPoint where 微调点位名称 = '{name}'", out DataTable dataTable);
            if (result == "Success")
            {
                try
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow reader = dataTable.Rows[0];
                        offsetPoint = new OffsetPoint()
                        {
                            微调点位名称 = reader["微调点位名称"].ToString(),
                            入料_Load吸_X = Convert.ToDouble(reader["入料_Load吸_X"].ToString()),
                            入料_Load吸_Y = Convert.ToDouble(reader["入料_Load吸_Y"].ToString()),
                            入料_Load吸_Z = Convert.ToDouble(reader["入料_Load吸_Z"].ToString()),
                            入料_Load_变距 = Convert.ToDouble(reader["入料_Load_变距"].ToString()),
                            入料_Heat吸_X = Convert.ToDouble(reader["入料_Heat吸_X"].ToString()),
                            入料_Heat吸_Y = Convert.ToDouble(reader["入料_Heat吸_Y"].ToString()),
                            入料_Heat吸_Z = Convert.ToDouble(reader["入料_Heat吸_Z"].ToString()),
                            入料_Heat放_X = Convert.ToDouble(reader["入料_Heat放_X"].ToString()),
                            入料_Heat放_Y = Convert.ToDouble(reader["入料_Heat放_Y"].ToString()),
                            入料_Heat放_Z = Convert.ToDouble(reader["入料_Heat放_Z"].ToString()),
                            入料_Heat_变距 = Convert.ToDouble(reader["入料_Heat_变距"].ToString()),
                            入料_梭车1第1列放_X = Convert.ToDouble(reader["入料_梭车1第1列放_X"].ToString()),
                            入料_梭车1第1列放_Y = Convert.ToDouble(reader["入料_梭车1第1列放_Y"].ToString()),
                            入料_梭车1第1列放_Z = Convert.ToDouble(reader["入料_梭车1第1列放_Z"].ToString()),
                            入料_梭车1第2列放_X = Convert.ToDouble(reader["入料_梭车1第2列放_X"].ToString()),
                            入料_梭车1第2列放_Y = Convert.ToDouble(reader["入料_梭车1第2列放_Y"].ToString()),
                            入料_梭车1第2列放_Z = Convert.ToDouble(reader["入料_梭车1第2列放_Z"].ToString()),
                            入料_梭车2第1列放_X = Convert.ToDouble(reader["入料_梭车2第1列放_X"].ToString()),
                            入料_梭车2第1列放_Y = Convert.ToDouble(reader["入料_梭车2第1列放_Y"].ToString()),
                            入料_梭车2第1列放_Z = Convert.ToDouble(reader["入料_梭车2第1列放_Z"].ToString()),
                            入料_梭车2第2列放_X = Convert.ToDouble(reader["入料_梭车2第2列放_X"].ToString()),
                            入料_梭车2第2列放_Y = Convert.ToDouble(reader["入料_梭车2第2列放_Y"].ToString()),
                            入料_梭车2第2列放_Z = Convert.ToDouble(reader["入料_梭车2第2列放_Z"].ToString()),
                            入料_梭车_变距 = Convert.ToDouble(reader["入料_梭车_变距"].ToString()),
                            出料_梭车1第1列吸_X = Convert.ToDouble(reader["出料_梭车1第1列吸_X"].ToString()),
                            出料_梭车1第1列吸_Y = Convert.ToDouble(reader["出料_梭车1第1列吸_Y"].ToString()),
                            出料_梭车1第1列吸_Z = Convert.ToDouble(reader["出料_梭车1第1列吸_Z"].ToString()),
                            出料_梭车1第2列吸_X = Convert.ToDouble(reader["出料_梭车1第2列吸_X"].ToString()),
                            出料_梭车1第2列吸_Y = Convert.ToDouble(reader["出料_梭车1第2列吸_Y"].ToString()),
                            出料_梭车1第2列吸_Z = Convert.ToDouble(reader["出料_梭车1第2列吸_Z"].ToString()),
                            出料_梭车2第1列吸_X = Convert.ToDouble(reader["出料_梭车2第1列吸_X"].ToString()),
                            出料_梭车2第1列吸_Y = Convert.ToDouble(reader["出料_梭车2第1列吸_Y"].ToString()),
                            出料_梭车2第1列吸_Z = Convert.ToDouble(reader["出料_梭车2第1列吸_Z"].ToString()),
                            出料_梭车2第2列吸_X = Convert.ToDouble(reader["出料_梭车2第2列吸_X"].ToString()),
                            出料_梭车2第2列吸_Y = Convert.ToDouble(reader["出料_梭车2第2列吸_Y"].ToString()),
                            出料_梭车2第2列吸_Z = Convert.ToDouble(reader["出料_梭车2第2列吸_Z"].ToString()),
                            出料_梭车_变距 = Convert.ToDouble(reader["出料_梭车_变距"].ToString()),
                            出料_Auto1放_X = Convert.ToDouble(reader["出料_Auto1放_X"].ToString()),
                            出料_Auto1放_Y = Convert.ToDouble(reader["出料_Auto1放_Y"].ToString()),
                            出料_Auto1放_Z = Convert.ToDouble(reader["出料_Auto1放_Z"].ToString()),
                            出料_Auto2放_X = Convert.ToDouble(reader["出料_Auto2放_X"].ToString()),
                            出料_Auto2放_Y = Convert.ToDouble(reader["出料_Auto2放_Y"].ToString()),
                            出料_Auto2放_Z = Convert.ToDouble(reader["出料_Auto2放_Z"].ToString()),
                            出料_Fix1放_X = Convert.ToDouble(reader["出料_Fix1放_X"].ToString()),
                            出料_Fix1放_Y = Convert.ToDouble(reader["出料_Fix1放_Y"].ToString()),
                            出料_Fix1放_Z = Convert.ToDouble(reader["出料_Fix1放_Z"].ToString()),
                            出料_Fix2放_X = Convert.ToDouble(reader["出料_Fix2放_X"].ToString()),
                            出料_Fix2放_Y = Convert.ToDouble(reader["出料_Fix2放_Y"].ToString()),
                            出料_Fix2放_Z = Convert.ToDouble(reader["出料_Fix2放_Z"].ToString()),
                            出料_Fix3放_X = Convert.ToDouble(reader["出料_Fix3放_X"].ToString()),
                            出料_Fix3放_Y = Convert.ToDouble(reader["出料_Fix3放_Y"].ToString()),
                            出料_Fix3放_Z = Convert.ToDouble(reader["出料_Fix3放_Z"].ToString()),
                            出料_AF_变距 = Convert.ToDouble(reader["出料_AF_变距"].ToString()),
                            测试_梭车1左吸_X = Convert.ToDouble(reader["测试_梭车1左吸_X"].ToString()),
                            测试_梭车1左吸_Z = Convert.ToDouble(reader["测试_梭车1左吸_Z"].ToString()),
                            测试_梭车2左吸_X = Convert.ToDouble(reader["测试_梭车2左吸_X"].ToString()),
                            测试_梭车2左吸_Z = Convert.ToDouble(reader["测试_梭车2左吸_Z"].ToString()),
                            测试_梭车1右放_X = Convert.ToDouble(reader["测试_梭车1右放_X"].ToString()),
                            测试_梭车1右放_Z = Convert.ToDouble(reader["测试_梭车1右放_Z"].ToString()),
                            测试_梭车2右放_X = Convert.ToDouble(reader["测试_梭车2右放_X"].ToString()),
                            测试_梭车2右放_Z = Convert.ToDouble(reader["测试_梭车2右放_Z"].ToString()),
                            测试_Site1吸_X = Convert.ToDouble(reader["测试_Site1吸_X"].ToString()),
                            测试_Site2吸_X = Convert.ToDouble(reader["测试_Site2吸_X"].ToString()),
                            测试_Site3吸_X = Convert.ToDouble(reader["测试_Site3吸_X"].ToString()),
                            测试_Site4吸_X = Convert.ToDouble(reader["测试_Site4吸_X"].ToString()),
                            测试_Site5吸_X = Convert.ToDouble(reader["测试_Site5吸_X"].ToString()),
                            测试_Site6吸_X = Convert.ToDouble(reader["测试_Site6吸_X"].ToString()),
                            测试_Site7吸_X = Convert.ToDouble(reader["测试_Site7吸_X"].ToString()),
                            测试_Site8吸_X = Convert.ToDouble(reader["测试_Site8吸_X"].ToString()),
                            测试_Site9吸_X = Convert.ToDouble(reader["测试_Site9吸_X"].ToString()),
                            测试_Site10吸_X = Convert.ToDouble(reader["测试_Site10吸_X"].ToString()),
                            测试_Site11吸_X = Convert.ToDouble(reader["测试_Site11吸_X"].ToString()),
                            测试_Site12吸_X = Convert.ToDouble(reader["测试_Site12吸_X"].ToString()),
                            测试_Site13吸_X = Convert.ToDouble(reader["测试_Site13吸_X"].ToString()),
                            测试_Site14吸_X = Convert.ToDouble(reader["测试_Site14吸_X"].ToString()),
                            测试_Site15吸_X = Convert.ToDouble(reader["测试_Site15吸_X"].ToString()),
                            测试_Site16吸_X = Convert.ToDouble(reader["测试_Site16吸_X"].ToString()),
                            测试_Site1吸_Z = Convert.ToDouble(reader["测试_Site1吸_Z"].ToString()),
                            测试_Site2吸_Z = Convert.ToDouble(reader["测试_Site2吸_Z"].ToString()),
                            测试_Site3吸_Z = Convert.ToDouble(reader["测试_Site3吸_Z"].ToString()),
                            测试_Site4吸_Z = Convert.ToDouble(reader["测试_Site4吸_Z"].ToString()),
                            测试_Site5吸_Z = Convert.ToDouble(reader["测试_Site5吸_Z"].ToString()),
                            测试_Site6吸_Z = Convert.ToDouble(reader["测试_Site6吸_Z"].ToString()),
                            测试_Site7吸_Z = Convert.ToDouble(reader["测试_Site7吸_Z"].ToString()),
                            测试_Site8吸_Z = Convert.ToDouble(reader["测试_Site8吸_Z"].ToString()),
                            测试_Site9吸_Z = Convert.ToDouble(reader["测试_Site9吸_Z"].ToString()),
                            测试_Site10吸_Z = Convert.ToDouble(reader["测试_Site10吸_Z"].ToString()),
                            测试_Site11吸_Z = Convert.ToDouble(reader["测试_Site11吸_Z"].ToString()),
                            测试_Site12吸_Z = Convert.ToDouble(reader["测试_Site12吸_Z"].ToString()),
                            测试_Site13吸_Z = Convert.ToDouble(reader["测试_Site13吸_Z"].ToString()),
                            测试_Site14吸_Z = Convert.ToDouble(reader["测试_Site14吸_Z"].ToString()),
                            测试_Site15吸_Z = Convert.ToDouble(reader["测试_Site15吸_Z"].ToString()),
                            测试_Site16吸_Z = Convert.ToDouble(reader["测试_Site16吸_Z"].ToString()),
                            测试_Site1放_X = Convert.ToDouble(reader["测试_Site1放_X"].ToString()),
                            测试_Site2放_X = Convert.ToDouble(reader["测试_Site2放_X"].ToString()),
                            测试_Site3放_X = Convert.ToDouble(reader["测试_Site3放_X"].ToString()),
                            测试_Site4放_X = Convert.ToDouble(reader["测试_Site4放_X"].ToString()),
                            测试_Site5放_X = Convert.ToDouble(reader["测试_Site5放_X"].ToString()),
                            测试_Site6放_X = Convert.ToDouble(reader["测试_Site6放_X"].ToString()),
                            测试_Site7放_X = Convert.ToDouble(reader["测试_Site7放_X"].ToString()),
                            测试_Site8放_X = Convert.ToDouble(reader["测试_Site8放_X"].ToString()),
                            测试_Site9放_X = Convert.ToDouble(reader["测试_Site9放_X"].ToString()),
                            测试_Site10放_X = Convert.ToDouble(reader["测试_Site10放_X"].ToString()),
                            测试_Site11放_X = Convert.ToDouble(reader["测试_Site11放_X"].ToString()),
                            测试_Site12放_X = Convert.ToDouble(reader["测试_Site12放_X"].ToString()),
                            测试_Site13放_X = Convert.ToDouble(reader["测试_Site13放_X"].ToString()),
                            测试_Site14放_X = Convert.ToDouble(reader["测试_Site14放_X"].ToString()),
                            测试_Site15放_X = Convert.ToDouble(reader["测试_Site15放_X"].ToString()),
                            测试_Site16放_X = Convert.ToDouble(reader["测试_Site16放_X"].ToString()),
                            测试_Site1放_Z = Convert.ToDouble(reader["测试_Site1放_Z"].ToString()),
                            测试_Site2放_Z = Convert.ToDouble(reader["测试_Site2放_Z"].ToString()),
                            测试_Site3放_Z = Convert.ToDouble(reader["测试_Site3放_Z"].ToString()),
                            测试_Site4放_Z = Convert.ToDouble(reader["测试_Site4放_Z"].ToString()),
                            测试_Site5放_Z = Convert.ToDouble(reader["测试_Site5放_Z"].ToString()),
                            测试_Site6放_Z = Convert.ToDouble(reader["测试_Site6放_Z"].ToString()),
                            测试_Site7放_Z = Convert.ToDouble(reader["测试_Site7放_Z"].ToString()),
                            测试_Site8放_Z = Convert.ToDouble(reader["测试_Site8放_Z"].ToString()),
                            测试_Site9放_Z = Convert.ToDouble(reader["测试_Site9放_Z"].ToString()),
                            测试_Site10放_Z = Convert.ToDouble(reader["测试_Site10放_Z"].ToString()),
                            测试_Site11放_Z = Convert.ToDouble(reader["测试_Site11放_Z"].ToString()),
                            测试_Site12放_Z = Convert.ToDouble(reader["测试_Site12放_Z"].ToString()),
                            测试_Site13放_Z = Convert.ToDouble(reader["测试_Site13放_Z"].ToString()),
                            测试_Site14放_Z = Convert.ToDouble(reader["测试_Site14放_Z"].ToString()),
                            测试_Site15放_Z = Convert.ToDouble(reader["测试_Site15放_Z"].ToString()),
                            测试_Site16放_Z = Convert.ToDouble(reader["测试_Site16放_Z"].ToString()),
                            梭车2_前_Y = Convert.ToDouble(reader["梭车2_前_Y"].ToString()),
                            梭车2_后_Y = Convert.ToDouble(reader["梭车2_后_Y"].ToString()),
                            抓盘_Empty_X = Convert.ToDouble(reader["抓盘_Empty_X"].ToString()),
                            抓盘_Load_X = Convert.ToDouble(reader["抓盘_Load_X"].ToString()),
                            抓盘_Auto1_X = Convert.ToDouble(reader["抓盘_Auto1_X"].ToString()),
                            抓盘_Auto2_X = Convert.ToDouble(reader["抓盘_Auto2_X"].ToString()),
                        };
                    }
                    else result = $"无点位微调{name}信息";
                }
                catch (Exception ex)
                {
                    return "获取点位微调数据异常!" + ex.Message;
                }
            }
            return result;
        }

        public static string GetAxisInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from AxisInfo", out DataTable dataTable);
            if (result == "Success")
            {
                APPmaster.AxisInfoList = new List<AxisInfo>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    APPmaster.AxisInfoList.Add(new AxisInfo()
                    {
                        轴名称 = dr["轴名称"].ToString(),
                        核号 = Convert.ToInt16(dr["核号"].ToString()),
                        轴号 = Convert.ToInt16(dr["轴号"].ToString()),
                    });
                }
            }
            return result;
        }

        public static string GetAxisRunInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from AxisRunInfo", out DataTable dataTable);
            if (result == "Success")
            {
                AxisRunInfoList = new List<AxisRunInfo>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    AxisRunInfoList.Add(new AxisRunInfo()
                    {
                        轴名称 = dr["轴名称"].ToString(),
                        最大速度 = Convert.ToDouble(dr["最大速度"].ToString()),
                        加速度 = Convert.ToDouble(dr["加速度"].ToString()),
                        减速度 = Convert.ToDouble(dr["减速度"].ToString()),
                        步长mm = Convert.ToDouble(dr["步长mm"].ToString()),
                        速度百分比 = Convert.ToDouble(dr["速度百分比"].ToString()),
                        平滑时间 = Convert.ToInt16(dr["平滑时间"].ToString()),
                    });
                }
                AxisRunGetCoreAxis();
            }
            return result;
        }

        public static string GetAxisHomeInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from AxisHomeInfo", out DataTable dataTable);
            if (result == "Success")
            {
                APPmaster.AxisHomeInfoList = new List<AxisHomeInfo>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    APPmaster.AxisHomeInfoList.Add(new AxisHomeInfo()
                    {
                        轴名称 = dr["轴名称"].ToString(),
                        模式 = Convert.ToInt16(dr["模式"].ToString()),
                        回零启动方向 = Convert.ToInt16(dr["回零启动方向"].ToString()),
                        Index搜索方向 = Convert.ToInt16(dr["Index搜索方向"].ToString()),
                        捕获触发沿 = Convert.ToInt16(dr["捕获触发沿"].ToString()),
                        捕获触发器 = Convert.ToInt16(dr["捕获触发器"].ToString()),
                        搜索速度 = Convert.ToDouble(dr["搜索速度"].ToString()),
                        搜索Index速度 = Convert.ToDouble(dr["搜索Index速度"].ToString()),
                        定位速度 = Convert.ToDouble(dr["定位速度"].ToString()),
                        加速度 = Convert.ToDouble(dr["加速度"].ToString()),
                        减速度 = Convert.ToDouble(dr["减速度"].ToString()),
                        平滑时间 = Convert.ToInt16(dr["平滑时间"].ToString()),
                        零点偏置 = Convert.ToInt32(dr["零点偏置"].ToString()),
                        Home最大搜索距离 = Convert.ToInt32(dr["Home最大搜索距离"].ToString()),
                        Index最大搜索距离 = Convert.ToInt32(dr["Index最大搜索距离"].ToString()),
                        反向脱离步长 = Convert.ToInt32(dr["反向脱离步长"].ToString()),
                    });
                }
                AxisHomeGetCoreAxis();
            }
            return result;
        }

        public static string GetconfigValue(string name, out string value)
        {
            return SQLiteHelper.runSql(SQLiteConnString, $"select value from config where name = '{name}'", out value);
        }

        public static string GetconfigData()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from config", out DataTable dataTable);
            if (result == "Success")
            {
                foreach (DataRow item in dataTable.Rows)
                {
                    string value = item["value"].ToString();
                    switch (item["name"].ToString())
                    {
                        case "当前档案": 当前档案 = value; break;
                        case "TCPIP": TCPIP = value; break;
                        case "TCPPort": TCPPort = Convert.ToInt32(value); break;
                        case "connectionString": SQLServerConnString = value; break;
                    }
                }
            }
            return result;
        }

        public static string GetSetupValue(string name, string field, out string value)
        {
            return SQLiteHelper.runSql(SQLiteConnString, $"select {field} from SetupInfo where 档案名称 = '{name}'", out value);
        }

        public static string GetPassWord(string name, out string value)
        {
            return SQLiteHelper.runSql(SQLiteConnString, $"select password from Jurisdiction where realName = '{name}'", out value);
        }

        public static string GetSetupInfo(string name, out SetupInfo setupInfo)//
        {
            setupInfo = new SetupInfo();
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from SetupInfo where 档案名称 = '{name}'", out DataTable dataTable);
            if (result == "Success")
            {
                if (dataTable.Rows.Count > 0)
                {
                    DataRow reader = dataTable.Rows[0];

                    setupInfo.档案名称 = reader["档案名称"].ToString();
                    setupInfo.Tray盘信息.TrayName = reader["tray盘名称"].ToString();
                    setupInfo.Heat盘信息.HeatName = reader["加热盘名称"].ToString();
                    setupInfo.OffsetPoint.微调点位名称 = reader["偏移点位名称"].ToString();
                    setupInfo.开site信息.Site名称 = reader["site名称"].ToString();
                    setupInfo.分BIN信息.分BIN名称 = reader["分BIN名称"].ToString();
                    setupInfo.测试机信息.测试机名称 = reader["测试机名称"].ToString();
                    setupInfo.梭车名称 = reader["梭车名称"].ToString();
                    setupInfo.温度模式 = reader["温度模式"].ToString();
                }
                else return $"无档案{name}信息";
            }
            return "Success";
        }

        public static string GetSetupInfoName(out DataTable dt)
        {
            return SQLiteHelper.runSql(SQLiteConnString, "select 档案名称 from SetupInfo", out dt);
        }

        public static string GetJurisdictionName(out DataTable dt)
        {
            return SQLiteHelper.runSql(SQLiteConnString, "select realName from Jurisdiction", out dt);
        }

        public static string GetTrayInfo(string name, out TrayInfo trayInfo)
        {
            trayInfo = new TrayInfo();
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from TrayInfo where name = '{name}'", out DataTable dataTable);
            if (result == "Success")
            {
                try
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dr = dataTable.Rows[0];

                        trayInfo = new TrayInfo()
                        {
                            TrayName = dr["name"].ToString(),
                            X_Num = Convert.ToInt32(dr["xnum"].ToString()),
                            Y_Num = Convert.ToInt32(dr["ynum"].ToString()),
                            X_Start = Convert.ToDouble(dr["xstart"].ToString()),
                            Y_Start = Convert.ToDouble(dr["ystart"].ToString()),
                            X_Pitch = Convert.ToDouble(dr["xpitch"].ToString()),
                            Y_Pitch = Convert.ToDouble(dr["ypitch"].ToString()),
                            Width = Convert.ToDouble(dr["width"].ToString()),
                            Length = Convert.ToDouble(dr["length"].ToString()),
                            Height = Convert.ToDouble(dr["height"].ToString()),
                        };
                    }
                    else result = $"无Tray盘{设置信息.Tray盘信息.TrayName}信息";
                }
                catch (Exception ex)
                {
                    return "获取tray盘数据信息异常" + ex.Message;
                }
            }
            return result;
        }

        public static string GetSiteStatus(string name, out SiteStatus siteStatus)
        {
            siteStatus = new SiteStatus();
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from SiteStatus where Site名称 = '{name}'", out DataTable dataTable);
            if (result == "Success")
            {
                try
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dr = dataTable.Rows[0];

                        siteStatus = new SiteStatus()
                        {
                            Site名称 = dr["Site名称"].ToString(),
                            Site1 = dr["Site1"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site2 = dr["Site2"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site3 = dr["Site3"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site4 = dr["Site4"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site5 = dr["Site5"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site6 = dr["Site6"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site7 = dr["Site7"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site8 = dr["Site8"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site9 = dr["Site9"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site10 = dr["Site10"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site11 = dr["Site11"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site12 = dr["Site12"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site13 = dr["Site13"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site14 = dr["Site14"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site15 = dr["Site15"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                            Site16 = dr["Site16"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                        };
                    }
                    else result = $"无Site名称{name}信息";
                }
                catch (Exception ex)
                {
                    return "获取开Site数据异常!" + ex.Message;
                }
            }
            return result;
        }

        public static string GetBINParam(string name, out BinParam binParam)
        {
            binParam = new BinParam();
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from BinParam where 分BIN名称 = '{name}'", out DataTable dataTable);
            if (result == "Success")
            {
                try
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dr = dataTable.Rows[0];
                        binParam = new BinParam()
                        {
                            分BIN名称 = dr["分BIN名称"].ToString(),
                            auto1 = dr["auto1"].ToString(),
                            auto2 = dr["auto2"].ToString(),
                            fix1 = dr["fix1"].ToString(),
                            fix2 = dr["fix2"].ToString(),
                            fix3 = dr["fix3"].ToString(),
                            fix1num = Convert.ToInt32(dr["fix1num"].ToString()),
                            fix2num = Convert.ToInt32(dr["fix2num"].ToString()),
                            fix3num = Convert.ToInt32(dr["fix3num"].ToString()),
                            color1 = Convert.ToInt32(dr["color1"].ToString()),
                            color2 = Convert.ToInt32(dr["color2"].ToString()),
                            color3 = Convert.ToInt32(dr["color3"].ToString()),
                            color4 = Convert.ToInt32(dr["color4"].ToString()),
                            color5 = Convert.ToInt32(dr["color5"].ToString()),
                            name1 = dr["name1"].ToString(),
                            name2 = dr["name2"].ToString(),
                            name3 = dr["name3"].ToString(),
                            name4 = dr["name4"].ToString(),
                            name5 = dr["name5"].ToString(),
                        };
                    }
                    else result = $"无分BIN名称{name}信息";
                }
                catch (Exception ex)
                {
                    return "获取分BIN数据异常!" + ex.Message;
                }
            }
            return result;
        }

        public static string GetTesterIFParam(string name, out TesterIFParam tester)
        {
            tester = new TesterIFParam();
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from TesterIFParam where 测试机名称 = '{name}'", out DataTable dataTable);
            if (result == "Success")
            {
                try
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        DataRow dr = dataTable.Rows[0];

                        tester = new TesterIFParam()
                        {
                            测试机名称 = dr["测试机名称"].ToString(),
                            Dout连续不良个数 = Convert.ToInt32(dr["Dout连续不良个数"].ToString()),
                            Dout连续不良自动关闭 = Convert.ToBoolean(dr["Dout连续不良自动关闭"].ToString()),
                            Dout连续不良报警 = Convert.ToBoolean(dr["Dout连续不良报警"].ToString()),
                            Dut累计不良报警个数 = Convert.ToInt32(dr["Dut累计不良报警个数"].ToString()),
                            Dut累计不良报警使能 = Convert.ToBoolean(dr["Dut累计不良报警使能"].ToString()),
                            总良率报警百分比 = Convert.ToDouble(dr["总良率报警百分比"].ToString()),
                            总良率报警基数 = Convert.ToInt32(dr["总良率报警基数"].ToString()),
                            总良率报警间隔 = Convert.ToInt32(dr["总良率报警间隔"].ToString()),
                            总良率报警使能 = Convert.ToBoolean(dr["总良率报警使能"].ToString()),
                            socket使用次数 = Convert.ToInt32(dr["socket使用次数"].ToString()),
                            测试超时时间 = Convert.ToInt32(dr["测试超时时间"].ToString()),
                            timeout超时时间 = Convert.ToInt32(dr["timeout超时时间"].ToString()),
                            超时发送timeout = Convert.ToBoolean(dr["超时发送timeout"].ToString()),
                            串口号 = dr["串口号"].ToString(),
                            波特率 = Convert.ToInt32(dr["波特率"].ToString()),
                            校验位 = (Parity)Enum.Parse(typeof(Parity), dr["校验位"].ToString(), true),
                            数据位 = Convert.ToInt32(dr["数据位"].ToString()),
                            停止位 = (StopBits)Enum.Parse(typeof(StopBits), dr["停止位"].ToString(), true),
                        };
                    }
                    else result = $"无测试机名称{name}信息";
                }
                catch (Exception ex)
                {
                    return "获取测试机名称数据异常!" + ex.Message;
                }
            }
            return result;
        }

        //public static string GetLightSet(

        public static string GetSetupInfoData(string name)
        {
            ///设备档案
            string result = GetSetupInfo(name, out SetupInfo setupInfo);
            if (result == "Success")
            {
                ///tray盘信息
                result = GetTrayInfo(setupInfo.Tray盘信息.TrayName, out TrayInfo trayInfo);
                if (result != "Success") return result;

                ///加热盘信息
                result = GetHeatInfo(setupInfo.Heat盘信息.HeatName, out HeatInfo heatInfo);
                if (result != "Success") return result;

                ///点位微调
                result = GetOffsetPoint(setupInfo.OffsetPoint.微调点位名称, out OffsetPoint offsetPoint);
                if (result != "Success") return result;

                ///开Site信息
                result = GetSiteStatus(setupInfo.开site信息.Site名称, out SiteStatus siteStatus);
                if (result != "Success") return result;

                //分BIN参数
                result = GetBINParam(setupInfo.分BIN信息.分BIN名称, out BinParam binparam);
                if (result != "Success") return result;

                //测试机参数
                result = GetTesterIFParam(setupInfo.测试机信息.测试机名称, out TesterIFParam tester);
                if (result != "Success") return result;

                设置信息.档案名称 = setupInfo.档案名称;
                设置信息.Tray盘信息 = trayInfo;
                设置信息.Heat盘信息 = heatInfo;
                设置信息.OffsetPoint = offsetPoint;
                设置信息.开site信息 = siteStatus;
                设置信息.分BIN信息 = binparam;
                设置信息.测试机信息 = tester;
                设置信息.梭车名称 = setupInfo.梭车名称;
                设置信息.温度模式 = setupInfo.温度模式;
            }
            return result;
        }

        public static string Init_TrayPoint()
        {
            try
            {
                LoaderTrayICPointInit();
                HeatTrayICPointInit();
                Auto1TrayICPointInit();
                Auto2TrayICPointInit();
                Fix1TrayICPointInit();
                Fix2TrayICPointInit();
                Fix3TrayICPointInit();

                LoaderTrayICMapInit();
                HeatTrayICMapInit();
                Auto1TrayICMapInit();
                Auto2TrayICMapInit();
                Fix1TrayICMapInit();
                Fix2TrayICMapInit();
                Fix3TrayICMapInit();
            }
            catch (Exception ex)
            {
                return $"点位初始化失败:{ex.Message}";
            }

            return "Success";
        }

        public static string Init_APPmaster()
        {
            ///IO信息
            string result = GetIOInfo();
            if (result != "Success") return result;

            ///轴信息
            result = GetAxisInfo();
            if (result != "Success") return result;

            ///轴运行信息
            result = GetAxisRunInfo();
            if (result != "Success") return result;

            ///轴回零信息
            result = GetAxisHomeInfo();
            if (result != "Success") return result;

            ///设备config
            result = GetconfigData();
            if (result != "Success") return result;

            ///档案信息
            result = GetSetupInfoData(当前档案);
            if (result != "Success") return result;

            ///初始点位
            result = GetInitPoint();
            if (result != "Success") return result;

            ///测试区点位
            result = TestPointInit();
            if (result != "Success") return result;

            ///Tray盘点位
            result = Init_TrayPoint();
            if (result != "Success") return result;

            for (int i = 0; i < 16; i++)
            {
                APPmaster.SiteTestIC.Add(new bool[2][]
                {
                    new bool[] { false, false, false, false },
                    new bool[] { false, false, false, false },
                });
            }
            return "Success";
        }

        public static string GetAxisRunInfo(string axisInfo, out AxisRunInfo axisRun)
        {
            axisRun = new AxisRunInfo();
            for (int i = 0; i < AxisRunInfoList.Count; i++)
            {
                if (axisInfo == AxisRunInfoList[i].轴名称)
                {
                    axisRun = AxisRunInfoList[i];
                    return "Success";
                }
            }
            return $"轴({axisInfo})信息不存在!";
        }

        public static string GetAxisHomeInfo(string axisInfo, out AxisHomeInfo axisRun)
        {
            axisRun = new AxisHomeInfo();
            for (int i = 0; i < AxisHomeInfoList.Count; i++)
            {
                if (axisInfo == AxisHomeInfoList[i].轴名称)
                {
                    axisRun = AxisHomeInfoList[i];
                    return "Success";
                }
            }
            return $"轴({axisInfo})信息不存在!";
        }

        public static string GetAxisHomeInfo(string[] axisName, out List<AxisHomeInfo> axisHomes)
        {
            axisHomes = new List<AxisHomeInfo>();

            foreach (var item in AxisHomeInfoList)
            {
                for (int i = 0; i < axisName.Length; i++)
                {
                    if (axisName[i] == item.轴名称)
                    {
                        axisHomes.Add(item);
                        break;
                    }
                }
                if (axisHomes.Count == axisName.Length)
                {
                    break;
                }
            }
            if (axisHomes.Count == axisName.Length) return "Success";
            else return "有IO编号不存在";
        }

        public static string UpdateSiteStatus(string name, int site, string value)
        {
            string sql = $"update SiteStatus set Site{site} = '{value}' where Site名称 = '{name}'";
            int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, sql);
            if (line <= 0) return "保存失败";
            else return "Success";
        }

        public static string UpdateRecord(string value)
        {
            string sql = $"update config set value = '{value}' where name = '当前档案'";
            int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, sql);
            if (line <= 0) return "保存失败";
            else return "Success";
        }

        public static string UpdateBinParam(string name, BinParam binParam)
        {
            string sql = $"update BinParam set " +
                $"auto1 = '{binParam.auto1}'," +
                $"auto2 = '{binParam.auto2}'," +
                $"fix1 = '{binParam.fix1}'," +
                $"fix2 = '{binParam.fix2}'," +
                $"fix3 = '{binParam.fix3}'," +
                $"fix1num = '{binParam.fix1num}'," +
                $"fix2num = '{binParam.fix2num}'," +
                $"fix3num = '{binParam.fix3num}'," +
                $"color1 = '{binParam.color1}'," +
                $"color2 = '{binParam.color2}'," +
                $"color3 = '{binParam.color3}'," +
                $"color4 = '{binParam.color4}'," +
                $"color5 = '{binParam.color5}'," +
                $"name1 = '{binParam.name1}'," +
                $"name2 = '{binParam.name2}'," +
                $"name3 = '{binParam.name3}'," +
                $"name4 = '{binParam.name4}'," +
                $"name5 = '{binParam.name5}'" +
                $"where 分BIN名称 = '{name}'";

            int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, sql);
            if (line <= 0) return "保存失败";

            else return "Success";
        }

        public static string UpdateTesterIFParam(string name, TesterIFParam tester)
        {
            string sql = $"update TesterIFParam set " +
                $"Dout连续不良个数 = '{tester.Dout连续不良个数}'," +
                $"Dout连续不良自动关闭 = '{tester.Dout连续不良自动关闭}'," +
                $"Dout连续不良报警 = '{tester.Dout连续不良报警}'," +
                $"Dut累计不良报警个数 = '{tester.Dut累计不良报警个数}'," +
                $"Dut累计不良报警使能 = '{tester.Dut累计不良报警使能}'," +
                $"总良率报警百分比 = '{tester.总良率报警百分比}'," +
                $"总良率报警基数 = '{tester.总良率报警基数}'," +
                $"总良率报警间隔 = '{tester.总良率报警间隔}'," +
                $"总良率报警使能 = '{tester.总良率报警使能}'," +
                $"socket使用次数 = '{tester.socket使用次数}'," +
                $"测试超时时间 = '{tester.测试超时时间}'," +
                $"timeout超时时间 = '{tester.timeout超时时间}'," +
                $"超时发送timeout = '{tester.超时发送timeout}'," +
                $"串口号 = '{tester.串口号}'," +
                $"波特率 = '{tester.波特率}'," +
                $"校验位 = '{tester.校验位}'," +
                $"数据位 = '{tester.数据位}'," +
                $"停止位 = '{tester.停止位}'" +
                $" where 测试机名称 = '{name}'";

            int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, sql);
            if (line <= 0) return "保存失败";

            else return "Success";
        }

        public static void SaveBatchData()
        {
            string sql = "";
            try
            {
                string auto1_num = "";
                string auto2_num = "";
                string fix1_num = "";
                string fix2_num = "";
                string fix3_num = "";

                foreach (分Bin 分 in 分bin)
                {
                    switch (分.pan)
                    {
                        case "auto1": auto1_num += 分.bin + ":" + 分.num + "，"; break;
                        case "auto2": auto2_num += 分.bin + ":" + 分.num + "，"; break;
                        case "fix1": fix1_num += 分.bin + ":" + 分.num + "，"; break;
                        case "fix2": fix2_num += 分.bin + ":" + 分.num + "，"; break;
                        case "fix3": fix3_num += 分.bin + ":" + 分.num + "，"; break;
                    }
                }

                auto1_num = auto1_num.TrimEnd('，');
                auto2_num = auto2_num.TrimEnd('，');
                fix1_num = fix1_num.TrimEnd('，');
                fix2_num = fix2_num.TrimEnd('，');
                fix3_num = fix3_num.TrimEnd('，');

                //string[] axisname = { "M22_入料手臂X轴" , "M23_出料手臂X轴", "M21_测试手臂X轴" };
                APPmaster.GetAxisRunInfo("M22_入料手臂X轴", out AxisRunInfo axisRun);
                APPmaster.GetAxisRunInfo("M23_出料手臂X轴", out AxisRunInfo axisRun2);
                APPmaster.GetAxisRunInfo("M21_测试手臂X轴", out AxisRunInfo axisRun3);
                string 生产速度 = "入料手臂：" + axisRun.速度百分比 + "，出料手臂：" + axisRun2.速度百分比 + "，测试手臂：" + axisRun3.速度百分比;

                sql = $"insert into FlowcaedNoInfo(开始时间,工单号,生产模式,产品类型,产品批号,用户名,总数量,良品数量,良率," +
                       $"投入数量,结束时间,工作时间,档案名称,UPH,Auto1名称,Auto2名称," +
                       $"Fix1名称,Fix2名称,Fix3名称,Auto1总数,Auto2总数,Fix1总数,Fix2总数,Fix3总数," +
                       $"Auto1数量,Auto2数量,Fix1数量,Fix2数量,Fix3数量,Socket开启数量,生产中关闭Socket数量,报警次数,加热类型,加热设定值,加热误差范围,生产速度," +
                       $"IC吸取模式,测试模式)" +
                        $"values('{开始时间.ToString("F")}','{LotNo[0]}','正常生产','{LotNo[3]}','{LotNo[4]}','{LotNo[13]}','{出料数}','{Auto1数量}','{Auto1良率}%'," +
                       $"'{载入数}','{结束时间.ToString()}','{批次运行时间.ToString(@"dd\.hh\:mm\:ss")}','{设置信息.档案名称}','{UPHNum}','{Auto1}','{Auto2}'," +  //工作时间.Ticks
                       $"'{Fix1}','{Fix2}','{Fix3}','{Auto1数量}','{Auto2数量}','{Fix1数量}','{Fix2数量}','{Fix3数量}'," +
                       $"'{auto1_num}','{auto2_num}','{fix1_num}','{fix2_num}','{fix3_num}','{StartSite数量}','{StartSite数量 - 开Site数量}','{1}','{温度模式}','{设置信息.Heat盘信息.设定温度}','{设置信息.Heat盘信息.波动温度}','{生产速度}'," +
                       $"'正常','正常')";

                int line = SQLiteHelper.runSqlNonQuery(SQLiteRunningString, sql);

                if (line <= 0) Debug.WriteLine("sql保存失败！:" + sql);
            }
            catch (Exception ex)
            {
                RunLog($"执行SaveBatchData异常!sql:{sql}," + ex.Message);
            }
        }

        /// <summary>
        /// Loader TrayIC点位初始化
        /// </summary>
        public static void LoaderTrayICPointInit()
        {
            TrayInfo trayInfo = 设置信息.Tray盘信息;
            //OffsetPoint 点位微调 = 设置信息.OffsetPoint;
            double xStart = trayInfo.X_Start; //15.45;
            double xPitch = trayInfo.X_Pitch; //15;
            double yStart = trayInfo.Y_Start; //10.8;
            double yPitch = trayInfo.Y_Pitch; //16.3;
            int xNum = trayInfo.X_Num;
            int yNum = trayInfo.Y_Num;
            double loaderx = 初始点位.入料_Load_X; //motionEx.InitPointParam.InputArm_Loader_X + motionEx.OffsetParam.InputArm.InputArm_Loader_X;
            double loadery = 初始点位.入料_Load_Y; //motionEx.InitPointParam.InputArm_Loader_Y + motionEx.OffsetParam.InputArm.InputArm_Loader_Y;
            double total = 初始点位.入料_变距最大值 - yPitch * Interval; //要缩小的距离 最大变距值 - IC间隔 * 间隔吸的个数，

            double[] loaderXPoint = new double[xNum];
            double[] loaderYPoint = new double[yNum];

            loaderXPoint[0] = loaderx + xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                loaderXPoint[i] = loaderXPoint[i - 1] + xPitch; //累加X轴坐标
            }

            loaderYPoint[0] = loadery - yStart + 2 * total; //以第一根吸嘴为点位, 固定吸嘴在Z3,Y轴左上角初始点用的Z1，所以对于Y轴来说走了两倍的距离 //获取左上角第一颗芯片y轴坐标 33.900000000000006 50
            for (int i = 1; i < yNum; i++)
            {
                loaderYPoint[i] = loaderYPoint[i - 1] - yPitch; //累加Y轴坐标
            }

            LoaderXPoint = loaderXPoint;
            LoaderYPoint = loaderYPoint;
        }

        /// <summary>
        /// Heat TrayIC点位初始化
        /// </summary>
        public static void HeatTrayICPointInit()
        {
            HeatInfo heatInfo = 设置信息.Heat盘信息;
            double xStart = heatInfo.X_Start; //15.45;
            double xPitch = heatInfo.X_Pitch; //15;
            double yStart = heatInfo.Y_Start; //10.8;
            double yPitch = heatInfo.Y_Pitch; //16.3;

            int xNum = heatInfo.X_Num;
            int yNum = heatInfo.Y_Num;
            double heatx = 初始点位.入料_Heat_X; //motionEx.InitPointParam.InputArm_Loader_X + motionEx.OffsetParam.InputArm.InputArm_Loader_X;
            double heaty = 初始点位.入料_Heat_Y; //motionEx.InitPointParam.InputArm_Loader_Y + motionEx.OffsetParam.InputArm.InputArm_Loader_Y;
            double total = 初始点位.入料_变距最大值 - yPitch * HeatInterval; //要缩小的距离 最大变距值 - IC间隔 * 间隔吸的个数，

            double[] heatXPoint = new double[xNum];
            double[] heatYPoint = new double[yNum];

            heatXPoint[0] = heatx + xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                heatXPoint[i] = heatXPoint[i - 1] + xPitch; //累加X轴坐标
            }

            heatYPoint[0] = heaty - yStart + 2 * total; //以第一根吸嘴为点位, 固定吸嘴在Z3,Y轴左上角初始点用的Z1，所以对于Y轴来说走了两倍的距离 //获取左上角第一颗芯片y轴坐标 33.900000000000006 50
            for (int i = 1; i < yNum; i++)
            {
                heatYPoint[i] = heatYPoint[i - 1] - yPitch; //累加Y轴坐标
            }

            HeatXPoint = heatXPoint;
            HeatYPoint = heatYPoint;
        }

        /// <summary>
        /// Auto1 TrayIC点位初始化
        /// </summary>
        public static void Auto1TrayICPointInit()
        {
            TrayInfo trayInfo = 设置信息.Tray盘信息;
            //OffsetPoint 点位微调 = 设置信息.OffsetPoint;
            double xStart = trayInfo.X_Start; //15.45;
            double xPitch = trayInfo.X_Pitch; //15;
            double yStart = trayInfo.Y_Start; //10.8;
            double yPitch = trayInfo.Y_Pitch; //16.3;
            int xNum = trayInfo.X_Num;
            int yNum = trayInfo.Y_Num;
            double loaderx = 初始点位.出料_Auto1_X; //motionEx.InitPointParam.InputArm_Loader_X + motionEx.OffsetParam.InputArm.InputArm_Loader_X;
            double loadery = 初始点位.出料_Auto1_Y; //motionEx.InitPointParam.InputArm_Loader_Y + motionEx.OffsetParam.InputArm.InputArm_Loader_Y;
            double total = 初始点位.出料_变距最大值 - yPitch * Interval; //要缩小的距离 最大变距值 - IC间隔 * 间隔吸的个数，

            double[] xPoint = new double[xNum];
            double[] yPoint = new double[yNum];

            xPoint[0] = loaderx - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                xPoint[i] = xPoint[i - 1] - xPitch; //累加X轴坐标
            }

            yPoint[0] = loadery - yStart + 2 * total; //以第一根吸嘴为点位, 固定吸嘴在Z3,Y轴左上角初始点用的Z1，所以对于Y轴来说走了两倍的距离 //获取左上角第一颗芯片y轴坐标 33.900000000000006 50
            for (int i = 1; i < yNum; i++)
            {
                yPoint[i] = yPoint[i - 1] - yPitch; //累加Y轴坐标
            }

            Auto1XPoint = xPoint;
            Auto1YPoint = yPoint;
        }

        /// <summary>
        /// Auto2 TrayIC点位初始化
        /// </summary>
        public static void Auto2TrayICPointInit()
        {
            TrayInfo trayInfo = 设置信息.Tray盘信息;
            //OffsetPoint 点位微调 = 设置信息.OffsetPoint;
            double xStart = trayInfo.X_Start; //15.45;
            double xPitch = trayInfo.X_Pitch; //15;
            double yStart = trayInfo.Y_Start; //10.8;
            double yPitch = trayInfo.Y_Pitch; //16.3;
            int xNum = trayInfo.X_Num;
            int yNum = trayInfo.Y_Num;
            double loaderx = 初始点位.出料_Auto2_X; //motionEx.InitPointParam.InputArm_Loader_X + motionEx.OffsetParam.InputArm.InputArm_Loader_X;
            double loadery = 初始点位.出料_Auto2_Y; //motionEx.InitPointParam.InputArm_Loader_Y + motionEx.OffsetParam.InputArm.InputArm_Loader_Y;
            double total = 初始点位.出料_变距最大值 - yPitch * Interval; //要缩小的距离 最大变距值 - IC间隔 * 间隔吸的个数，

            double[] xPoint = new double[xNum];
            double[] yPoint = new double[yNum];

            xPoint[0] = loaderx - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                xPoint[i] = xPoint[i - 1] - xPitch; //累加X轴坐标
            }

            yPoint[0] = loadery - yStart + 2 * total; //以第一根吸嘴为点位, 固定吸嘴在Z3,Y轴左上角初始点用的Z1，所以对于Y轴来说走了两倍的距离 //获取左上角第一颗芯片y轴坐标 33.900000000000006 50
            for (int i = 1; i < yNum; i++)
            {
                yPoint[i] = yPoint[i - 1] - yPitch; //累加Y轴坐标
            }

            Auto2XPoint = xPoint;
            Auto2YPoint = yPoint;
        }

        /// <summary>
        /// Fix1 TrayIC点位初始化
        /// </summary>
        public static void Fix1TrayICPointInit()
        {
            TrayInfo trayInfo = 设置信息.Tray盘信息;
            //OffsetPoint 点位微调 = 设置信息.OffsetPoint;
            double xStart = trayInfo.X_Start; //15.45;
            double xPitch = trayInfo.X_Pitch; //15;
            double yStart = trayInfo.Y_Start; //10.8;
            double yPitch = trayInfo.Y_Pitch; //16.3;
            int xNum = trayInfo.X_Num;
            int yNum = trayInfo.Y_Num;
            double loaderx = 初始点位.出料_Fix1_X; //motionEx.InitPointParam.InputArm_Loader_X + motionEx.OffsetParam.InputArm.InputArm_Loader_X;
            double loadery = 初始点位.出料_Fix1_Y; //motionEx.InitPointParam.InputArm_Loader_Y + motionEx.OffsetParam.InputArm.InputArm_Loader_Y;
            double total = 初始点位.出料_变距最大值 - yPitch * Interval; //要缩小的距离 最大变距值 - IC间隔 * 间隔吸的个数，

            double[] xPoint = new double[xNum];
            double[] yPoint = new double[yNum];

            xPoint[0] = loaderx - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                xPoint[i] = xPoint[i - 1] - xPitch; //累加X轴坐标
            }

            yPoint[0] = loadery - yStart + 2 * total; //以第一根吸嘴为点位, 固定吸嘴在Z3,Y轴左上角初始点用的Z1，所以对于Y轴来说走了两倍的距离 //获取左上角第一颗芯片y轴坐标 33.900000000000006 50
            for (int i = 1; i < yNum; i++)
            {
                yPoint[i] = yPoint[i - 1] - yPitch; //累加Y轴坐标
            }

            Fix1XPoint = xPoint;
            Fix1YPoint = yPoint;
        }

        /// <summary>
        /// Fix2 TrayIC点位初始化
        /// </summary>
        public static void Fix2TrayICPointInit()
        {
            TrayInfo trayInfo = 设置信息.Tray盘信息;
            //OffsetPoint 点位微调 = 设置信息.OffsetPoint;
            double xStart = trayInfo.X_Start; //15.45;
            double xPitch = trayInfo.X_Pitch; //15;
            double yStart = trayInfo.Y_Start; //10.8;
            double yPitch = trayInfo.Y_Pitch; //16.3;
            int xNum = trayInfo.X_Num;
            int yNum = trayInfo.Y_Num;
            double loaderx = 初始点位.出料_Fix2_X; //motionEx.InitPointParam.InputArm_Loader_X + motionEx.OffsetParam.InputArm.InputArm_Loader_X;
            double loadery = 初始点位.出料_Fix2_Y; //motionEx.InitPointParam.InputArm_Loader_Y + motionEx.OffsetParam.InputArm.InputArm_Loader_Y;
            double total = 初始点位.出料_变距最大值 - yPitch * Interval; //要缩小的距离 最大变距值 - IC间隔 * 间隔吸的个数，

            double[] xPoint = new double[xNum];
            double[] yPoint = new double[yNum];

            xPoint[0] = loaderx - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                xPoint[i] = xPoint[i - 1] - xPitch; //累加X轴坐标
            }

            yPoint[0] = loadery - yStart + 2 * total; //以第一根吸嘴为点位, 固定吸嘴在Z3,Y轴左上角初始点用的Z1，所以对于Y轴来说走了两倍的距离 //获取左上角第一颗芯片y轴坐标 33.900000000000006 50
            for (int i = 1; i < yNum; i++)
            {
                yPoint[i] = yPoint[i - 1] - yPitch; //累加Y轴坐标
            }

            Fix2XPoint = xPoint;
            Fix2YPoint = yPoint;
        }

        /// <summary>
        /// Fix3 TrayIC点位初始化
        /// </summary>
        public static void Fix3TrayICPointInit()
        {
            TrayInfo trayInfo = 设置信息.Tray盘信息;
            //OffsetPoint 点位微调 = 设置信息.OffsetPoint;
            double xStart = trayInfo.X_Start; //15.45;
            double xPitch = trayInfo.X_Pitch; //15;
            double yStart = trayInfo.Y_Start; //10.8;
            double yPitch = trayInfo.Y_Pitch; //16.3;
            int xNum = trayInfo.X_Num;
            int yNum = trayInfo.Y_Num;
            double loaderx = 初始点位.出料_Fix3_X; //motionEx.InitPointParam.InputArm_Loader_X + motionEx.OffsetParam.InputArm.InputArm_Loader_X;
            double loadery = 初始点位.出料_Fix3_Y; //motionEx.InitPointParam.InputArm_Loader_Y + motionEx.OffsetParam.InputArm.InputArm_Loader_Y;
            double total = 初始点位.出料_变距最大值 - yPitch * Interval; //要缩小的距离 最大变距值 - IC间隔 * 间隔吸的个数，

            double[] xPoint = new double[xNum];
            double[] yPoint = new double[yNum];

            xPoint[0] = loaderx - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                xPoint[i] = xPoint[i - 1] - xPitch; //累加X轴坐标
            }

            yPoint[0] = loadery - yStart + 2 * total; //以第一根吸嘴为点位, 固定吸嘴在Z3,Y轴左上角初始点用的Z1，所以对于Y轴来说走了两倍的距离 //获取左上角第一颗芯片y轴坐标 33.900000000000006 50
            for (int i = 1; i < yNum; i++)
            {
                yPoint[i] = yPoint[i - 1] - yPitch; //累加Y轴坐标
            }

            Fix3XPoint = xPoint;
            Fix3YPoint = yPoint;
        }

        /// <summary>
        /// 测试手臂定位初始化
        /// </summary>
        public static string TestPointInit()
        {
            double[] testXPoint = new double[16];
            for (short site = 0; site < testXPoint.Length; site++)
            {
                string result = GetTestXPoint(site, out double xPos);
                if (result == "Success")
                {
                    testXPoint[site] = xPos;
                }
                else return result;
            }
            TestXPoint = testXPoint;
            return "Success";
        }

        /// <summary>
        /// 获取测试手臂测试点位
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static string GetTestXPoint(short site, out double xPos)
        {
            xPos = 0;
            double testx1pos = 初始点位.测试_Test1区_X;
            double testx2pos = 初始点位.测试_Test2区_X;
            double testx3pos = 初始点位.测试_Test3区_X;
            double testx4pos = 初始点位.测试_Test4区_X;
            switch (site)
            {
                case 0: xPos = testx1pos; break;
                case 1: xPos = testx1pos + 80; break;
                case 2: xPos = testx1pos + 80 * 2; break;
                case 3: xPos = testx1pos + 80 * 3; break;
                case 4: xPos = testx2pos; break;
                case 5: xPos = testx2pos + 80; break;
                case 6: xPos = testx2pos + 80 * 2; break;
                case 7: xPos = testx2pos + 80 * 3; break;
                case 8: xPos = testx3pos; break;
                case 9: xPos = testx3pos + 80; break;
                case 10: xPos = testx3pos + 80 * 2; break;
                case 11: xPos = testx3pos + 80 * 3; break;
                case 12: xPos = testx4pos; break;
                case 13: xPos = testx4pos + 80; break;
                case 14: xPos = testx4pos + 80 * 2; break;
                case 15: xPos = testx4pos + 80 * 3; break;
                default: return $"site:{site}不存在";
            }
            return "Success";
        }

        ///// <summary>
        ///// Heat TrayIC点位初始化
        ///// </summary>
        //public static void HeatTrayICPointInit()
        //{
        //    int xStart = (int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_InputArm_X_Precision);
        //    int xPitch = (int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_InputArm_X_Precision);
        //    int yStart = (int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_InputArm_Y_Precision);
        //    int yPitch = (int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_InputArm_Y_Precision);
        //    int xNum = (int)motionEx.trayParm.X_Quantity;
        //    int yNum = (int)motionEx.trayParm.Y_Quantity;
        //    int heatx = motionEx.InitPointParam.InputArm_Loader_X + motionEx.OffsetParam.InputArm.InputArm_Loader_X;
        //    int heaty = motionEx.InitPointParam.InputArm_Loader_Y + motionEx.OffsetParam.InputArm.InputArm_Loader_Y;
        //    double total = 41 - motionEx.trayParm.Y_Pitch * 2; //move 900
        //    //double total = 38.4 - motionEx.trayParm.Y_Pitch * 2; //move 580
        //    int ymove = (int)(total / motionEx.advancedParam.Axis_InputArm_Y_Precision);
        //    int pmove = (int)(total / motionEx.advancedParam.Axis_InputArm_P_Precision);
        //    HeatXPoint = new int[xNum];
        //    HeatYPoint = new int[yNum];

        //    HeatXPoint[0] = heatx + xStart; //获取左上角第一颗芯片x轴坐标
        //    for (int i = 1; i < xNum; i++)
        //    {
        //        HeatXPoint[i] = HeatXPoint[i - 1] + xPitch; //累加X轴坐标
        //    }
        //    HeatYPoint[0] = heaty - yStart + 2 * ymove; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
        //    for (int i = 1; i < yNum; i++)
        //    {
        //        HeatYPoint[i] = HeatYPoint[i - 1] - yPitch; //累加Y轴坐标
        //    }
        //}

        ///// <summary>
        ///// Auto1 TrayIC点位初始化
        ///// </summary>
        //public static void Auto1TrayICPointInit()
        //{
        //    int xStart = (int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int xPitch = (int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int yStart = (int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int yPitch = (int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int xNum = (int)motionEx.trayParm.X_Quantity;
        //    int yNum = (int)motionEx.trayParm.Y_Quantity;
        //    int auto1x = motionEx.InitPointParam.OutputArm_Auto1_X + motionEx.OffsetParam.OutputArm.OutputArm_Auto1_X;
        //    int auto1y = motionEx.InitPointParam.OutputArm_Auto1_Y + motionEx.OffsetParam.OutputArm.OutputArm_Auto1_Y; ;
        //    double total = 41 - motionEx.trayParm.Y_Pitch * 2; //move 580
        //    int move = (int)(total / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    Auto1XPoint = new int[xNum];
        //    Auto1YPoint = new int[yNum];

        //    Auto1XPoint[0] = auto1x - xStart; //获取左上角第一颗芯片x轴坐标
        //    for (int i = 1; i < xNum; i++)
        //    {
        //        Auto1XPoint[i] = Auto1XPoint[i - 1] - xPitch; //累加X轴坐标
        //    }
        //    Auto1YPoint[0] = auto1y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
        //    for (int i = 1; i < yNum; i++)
        //    {
        //        Auto1YPoint[i] = Auto1YPoint[i - 1] - yPitch; //累加Y轴坐标
        //    }
        //}

        ///// <summary>
        ///// Auto2 TrayIC点位初始化
        ///// </summary>
        //public static void Auto2TrayICPointInit()
        //{
        //    int xStart = (int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int xPitch = (int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int yStart = (int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int yPitch = (int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int xNum = (int)motionEx.trayParm.X_Quantity;
        //    int yNum = (int)motionEx.trayParm.Y_Quantity;
        //    int auto2x = motionEx.InitPointParam.OutputArm_Auto2_X + motionEx.OffsetParam.OutputArm.OutputArm_Auto2_X;
        //    int auto2y = motionEx.InitPointParam.OutputArm_Auto2_Y + motionEx.OffsetParam.OutputArm.OutputArm_Auto2_Y;
        //    double total = 41 - motionEx.trayParm.Y_Pitch * 2; //move 580
        //    int move = (int)(total / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    Auto2XPoint = new int[xNum];
        //    Auto2YPoint = new int[yNum];

        //    Auto2XPoint[0] = auto2x - xStart; //获取左上角第一颗芯片x轴坐标
        //    for (int i = 1; i < xNum; i++)
        //    {
        //        Auto2XPoint[i] = Auto2XPoint[i - 1] - xPitch; //累加X轴坐标
        //    }
        //    Auto2YPoint[0] = auto2y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
        //    for (int i = 1; i < yNum; i++)
        //    {
        //        Auto2YPoint[i] = Auto2YPoint[i - 1] - yPitch; //累加Y轴坐标
        //    }
        //}

        ///// <summary>
        ///// Fix1 TrayIC点位初始化
        ///// </summary>
        //public static void Fix1TrayICPointInit()
        //{
        //    int xStart = (int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int xPitch = (int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int yStart = (int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int yPitch = (int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int xNum = (int)motionEx.trayParm.X_Quantity;
        //    int yNum = (int)motionEx.trayParm.Y_Quantity;
        //    int fix1x = motionEx.InitPointParam.OutputArm_Fix1_X + motionEx.OffsetParam.OutputArm.OutputArm_Fix1_X;
        //    int fix1y = motionEx.InitPointParam.OutputArm_Fix1_Y + motionEx.OffsetParam.OutputArm.OutputArm_Fix1_Y;
        //    double total = 41 - motionEx.trayParm.Y_Pitch * 2; //move 580
        //    int move = (int)(total / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    Fix1XPoint = new int[xNum];
        //    Fix1YPoint = new int[yNum];

        //    Fix1XPoint[0] = fix1x - xStart; //获取左上角第一颗芯片x轴坐标
        //    for (int i = 1; i < xNum; i++)
        //    {
        //        Fix1XPoint[i] = Fix1XPoint[i - 1] - xPitch; //累加X轴坐标
        //    }
        //    Fix1YPoint[0] = fix1y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
        //    for (int i = 1; i < yNum; i++)
        //    {
        //        Fix1YPoint[i] = Fix1YPoint[i - 1] - yPitch; //累加Y轴坐标
        //    }
        //}

        ///// <summary>
        ///// Fix2 TrayIC点位初始化
        ///// </summary>
        //public static void Fix2TrayICPointInit()
        //{
        //    int xStart = (int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int xPitch = (int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int yStart = (int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int yPitch = (int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int xNum = (int)motionEx.trayParm.X_Quantity;
        //    int yNum = (int)motionEx.trayParm.Y_Quantity;
        //    int fix2x = motionEx.InitPointParam.OutputArm_Fix2_X + motionEx.OffsetParam.OutputArm.OutputArm_Fix2_X;
        //    int fix2y = motionEx.InitPointParam.OutputArm_Fix2_Y + motionEx.OffsetParam.OutputArm.OutputArm_Fix2_Y;
        //    double total = 41 - motionEx.trayParm.Y_Pitch * 2; //move 580
        //    int move = (int)(total / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    Fix2XPoint = new int[xNum];
        //    Fix2YPoint = new int[yNum];

        //    Fix2XPoint[0] = fix2x - xStart; //获取左上角第一颗芯片x轴坐标
        //    for (int i = 1; i < xNum; i++)
        //    {
        //        Fix2XPoint[i] = Fix2XPoint[i - 1] - xPitch; //累加X轴坐标
        //    }
        //    Fix2YPoint[0] = fix2y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
        //    for (int i = 1; i < yNum; i++)
        //    {
        //        Fix2YPoint[i] = Fix2YPoint[i - 1] - yPitch; //累加Y轴坐标
        //    }
        //}

        ///// <summary>
        ///// Fix3 TrayIC点位初始化
        ///// </summary>
        //public static void Fix3TrayICPointInit()
        //{
        //    int xStart = (int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int xPitch = (int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
        //    int yStart = (int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int yPitch = (int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    int xNum = (int)motionEx.trayParm.X_Quantity;
        //    int yNum = (int)motionEx.trayParm.Y_Quantity;
        //    int fix3x = motionEx.InitPointParam.OutputArm_Fix3_X + motionEx.OffsetParam.OutputArm.OutputArm_Fix3_X;
        //    int fix3y = motionEx.InitPointParam.OutputArm_Fix3_Y + motionEx.OffsetParam.OutputArm.OutputArm_Fix3_Y;
        //    double total = 41 - motionEx.trayParm.Y_Pitch * 2; //move 580
        //    int move = (int)(total / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
        //    Fix3XPoint = new int[xNum];
        //    Fix3YPoint = new int[yNum];

        //    Fix3XPoint[0] = fix3x - xStart; //获取左上角第一颗芯片x轴坐标
        //    for (int i = 1; i < xNum; i++)
        //    {
        //        Fix3XPoint[i] = Fix3XPoint[i - 1] - xPitch; //累加X轴坐标
        //    }
        //    Fix3YPoint[0] = fix3y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
        //    for (int i = 1; i < yNum; i++)
        //    {
        //        Fix3YPoint[i] = Fix3YPoint[i - 1] - yPitch; //累加Y轴坐标
        //    }
        //}

        public static bool 梭车是否需要放料(bool[] 实际值, bool[] 目标值)
        {
            for (int i = 0; i < 实际值.Length; i++)
            {
                if (目标值[i] == false) break; //如果不需要放
                if (实际值[i] != 目标值[i]) return true;
            }
            return false;
        }

        public static bool[] 梭车放料ICopen(bool[] 实际值, bool[] 目标值)
        {
            bool[] open = new bool[实际值.Length];
            for (int i = 0; i < 实际值.Length; i++)
            {
                if (目标值[i])
                {
                    if (!实际值[i])
                    {
                        open[i] = true;
                    }
                }
            }
            return open;
        }

        public static bool 飞梭前入料放料参数(out short colums, out bool[] open)
        {
            colums = 0;
            open = new bool[4];
            if (梭车是否需要放料(飞梭前入料IC[0], 放料IC目标值[0]))
            {
                open = 梭车放料ICopen(飞梭前入料IC[0], 放料IC目标值[0]);
                if (open.Contains(true))
                {
                    colums = 0;
                    return true;
                }
            }
            else if (梭车是否需要放料(飞梭前入料IC[1], 放料IC目标值[1]))
            {
                open = 梭车放料ICopen(飞梭前入料IC[1], 放料IC目标值[1]);
                if (open.Contains(true))
                {
                    colums = 1;
                    return true;
                }
            }
            return false;
        }

        public static bool 飞梭后入料放料参数(out short colums, out bool[] open)
        {
            colums = 0;
            open = new bool[4];
            if (梭车是否需要放料(飞梭后入料IC[0], 放料IC目标值[0]))
            {
                open = 梭车放料ICopen(飞梭后入料IC[0], 放料IC目标值[0]);
                if (open.Contains(true))
                {
                    colums = 0;
                    return true;
                }
            }
            else if (梭车是否需要放料(飞梭后入料IC[1], 放料IC目标值[1]))
            {
                open = 梭车放料ICopen(飞梭后入料IC[1], 放料IC目标值[1]);
                if (open.Contains(true))
                {
                    colums = 1;
                    return true;
                }
            }
            return false;
        }

        public static bool 获取Load盘坐标(bool ishave, out short xPoint, out short yPoint)
        {
            xPoint = yPoint = 0;
            for (short x = 0; x < LoaderXPoint.Length; x++)
            {
                for (short y = 0; y < LoaderYPoint.Length; y++)
                {
                    if (LoaderDicPoint[$"{x},{y}"] == ishave)
                    {
                        xPoint = x;
                        yPoint = y;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool 获取Heat盘坐标(int status, bool ishave, out short xPoint, out short yPoint)
        {
            xPoint = yPoint = 0;
            for (short x = 0; x < HeatXPoint.Length; x++)
            {
                for (short y = 0; y < HeatYPoint.Length; y++)
                {
                    if (HeatDicTimer[$"{x},{y}"].Status == status && HeatDicPoint[$"{x},{y}"] == ishave)
                    {
                        xPoint = x;
                        yPoint = y;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool 获取Heat盘未放IC的指定IC个数(int status, out int num)
        {
            num = 0;
            for (short x = 0; x < HeatXPoint.Length; x++)
            {
                for (short y = 0; y < HeatYPoint.Length; y++)
                {
                    if (HeatDicTimer[$"{x},{y}"].Status == status && HeatDicPoint[$"{x},{y}"] == false)
                    {
                        if (++num >= 4) return true;
                    }
                }
            }
            if (num == 0) return false;
            else return true;
        }

        public static bool 获取Heat盘指定状态IC个数(int status, out int num)
        {
            num = 0;
            for (short x = 0; x < HeatXPoint.Length; x++)
            {
                for (short y = 0; y < HeatYPoint.Length; y++)
                {
                    if (HeatDicTimer[$"{x},{y}"].Status == status && HeatDicPoint[$"{x},{y}"] == true)
                    {
                        num++;
                    }
                }
            }
            if (num > 0) return true;
            else return false;
        }

        public static bool 获取Heat盘加热好的指定IC个数(int status, bool ishave, int num, out int num2)
        {
            num2 = 0;
            for (short x = 0; x < HeatXPoint.Length; x++)
            {
                for (short y = 0; y < HeatYPoint.Length; y++)
                {
                    if (HeatDicTimer[$"{x},{y}"].Status == status && HeatDicPoint[$"{x},{y}"] == ishave)
                    {
                        if (++num2 == num) return true;
                    }
                }
            }
            return false;
        }

        public static bool 获取Heat盘状态(int status, int num, out short xPoint, out short yPoint)
        {
            int num2 = 0;
            xPoint = yPoint = 0;
            for (short x = 0; x < HeatXPoint.Length; x++)
            {
                for (short y = 0; y < HeatYPoint.Length; y++)
                {
                    if (HeatDicTimer[$"{x},{y}"].Status == status && HeatDicPoint[$"{x},{y}"] == true)
                    {
                        if (num2++ == 0) { xPoint = x; yPoint = y; }
                        if (num2 == num) return true;
                    }
                }
            }
            return false;
        }

        public static bool 获取Auto1盘坐标(bool ishave, out int xPoint, out int yPoint)
        {
            xPoint = yPoint = 0;
            for (short x = 0; x < Auto1XPoint.Length; x++)
            {
                for (short y = 0; y < Auto1YPoint.Length; y++)
                {
                    if (Auto1DicPoint[$"{x},{y}"] == ishave)
                    {
                        xPoint = x;
                        yPoint = y;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool 获取Auto2盘坐标(bool ishave, out int xPoint, out int yPoint)
        {
            xPoint = yPoint = 0;
            for (short x = 0; x < Auto2XPoint.Length; x++)
            {
                for (short y = 0; y < Auto2YPoint.Length; y++)
                {
                    if (Auto2DicPoint[$"{x},{y}"] == ishave)
                    {
                        xPoint = x;
                        yPoint = y;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool 获取Fix1盘坐标(bool ishave, int min, int max, out int xPoint, out int yPoint)
        {
            xPoint = 0;
            yPoint = min;
            for (int x = 0; x < Fix1XPoint.Length; x++)
            {
                for (int y = min; y < max; y++)
                {
                    if (Fix1DicPoint[$"{x},{y}"] == ishave)
                    {
                        xPoint = x;
                        yPoint = y;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool 获取Fix2盘坐标(bool ishave, int min, int max, out int xPoint, out int yPoint)
        {
            xPoint = 0;
            yPoint = min;
            for (int x = 0; x < Fix2XPoint.Length; x++)
            {
                for (int y = min; y < max; y++)
                {
                    if (Fix2DicPoint[$"{x},{y}"] == ishave)
                    {
                        xPoint = x;
                        yPoint = y;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool 获取Fix3盘坐标(bool ishave, int min, int max, out int xPoint, out int yPoint)
        {
            xPoint = 0;
            yPoint = min;
            for (int x = 0; x < Fix3XPoint.Length; x++)
            {
                for (int y = min; y < max; y++)
                {
                    if (Fix3DicPoint[$"{x},{y}"] == ishave)
                    {
                        xPoint = x;
                        yPoint = y;
                        return true;
                    }
                }
            }
            return false;
        }

        public static void LoaderTrayICMapInit()
        {
            LoaderDicPoint = new Dictionary<string, bool>();
            for (int i = 0; i < LoaderXPoint.Length; i++)
            {
                for (int j = 0; j < LoaderYPoint.Length; j++)
                {
                    LoaderDicPoint.Add($"{i},{j}", true);
                }
            }

            //LoaderDicPoint[$"{0},{0}"] = true;
            //LoaderDicPoint[$"{0},{1}"] = true;
            //LoaderDicPoint[$"{0},{2}"] = true;
            //LoaderDicPoint[$"{0},{3}"] = true;
            //LoaderDicPoint[$"{0},{4}"] = true;
            //LoaderDicPoint[$"{0},{5}"] = true;
            //LoaderDicPoint[$"{0},{6}"] = true;
            //LoaderDicPoint[$"{0},{7}"] = true;
            //LoaderDicPoint[$"{0},{8}"] = true;
            //LoaderDicPoint[$"{0},{9}"] = true;
            //LoaderDicPoint[$"{0},{10}"] = true;
            //LoaderDicPoint[$"{0},{11}"] = true;
            //LoaderDicPoint[$"{0},{12}"] = true;
            //LoaderDicPoint[$"{0},{13}"] = true;
            //LoaderDicPoint[$"{0},{14}"] = true;
            //LoaderDicPoint[$"{0},{15}"] = true;
            //LoaderDicPoint[$"{0},{16}"] = true;
            //LoaderDicPoint[$"{0},{17}"] = true;
            //LoaderDicPoint[$"{0},{18}"] = true;
        }

        public static void HeatTrayICMapInit()
        {
            HeatDicPoint = new Dictionary<string, bool>();
            HeatDicTimer = new Dictionary<string, HeatMapTimer>();
            for (int i = 0; i < HeatXPoint.Length; i++)
            {
                for (int j = 0; j < HeatYPoint.Length; j++)
                {
                    HeatDicPoint.Add($"{i},{j}", false);
                    HeatDicTimer.Add($"{i},{j}", new HeatMapTimer());
                }
            }
        }

        public static void Auto1TrayICMapInit()
        {
            Auto1DicPoint = new Dictionary<string, bool>();
            Auto1DicBin = new Dictionary<string, string>();
            for (int i = 0; i < Auto1XPoint.Length; i++)
            {
                for (int j = 0; j < Auto1YPoint.Length; j++)
                {
                    Auto1DicPoint.Add($"{i},{j}", false);
                    Auto1DicBin.Add($"{i},{j}", ""); //"0"
                }
            }
        }

        public static void Auto2TrayICMapInit()
        {
            Auto2DicPoint = new Dictionary<string, bool>();
            Auto2DicBin = new Dictionary<string, string>();
            for (int i = 0; i < Auto2XPoint.Length; i++)
            {
                for (int j = 0; j < Auto2YPoint.Length; j++)
                {
                    Auto2DicPoint.Add($"{i},{j}", false);
                    Auto2DicBin.Add($"{i},{j}", "");
                }
            }
        }

        public static void Fix1TrayICMapInit()
        {
            Fix1DicPoint = new Dictionary<string, bool>();
            Fix1DicBin = new Dictionary<string, string>();
            for (int i = 0; i < Fix1XPoint.Length; i++)
            {
                for (int j = 0; j < Fix1YPoint.Length; j++)
                {
                    Fix1DicPoint.Add($"{i},{j}", false);
                    Fix1DicBin.Add($"{i},{j}", "");
                }
            }
        }

        public static void Fix2TrayICMapInit()
        {
            Fix2DicPoint = new Dictionary<string, bool>();
            Fix2DicBin = new Dictionary<string, string>();
            for (int i = 0; i < Fix2XPoint.Length; i++)
            {
                for (int j = 0; j < Fix2YPoint.Length; j++)
                {
                    Fix2DicPoint.Add($"{i},{j}", false);
                    Fix2DicBin.Add($"{i},{j}", "");
                }
            }
        }

        public static void Fix3TrayICMapInit()
        {
            Fix3DicPoint = new Dictionary<string, bool>();
            Fix3DicBin = new Dictionary<string, string>();
            for (int i = 0; i < Fix3XPoint.Length; i++)
            {
                for (int j = 0; j < Fix3YPoint.Length; j++)
                {
                    Fix3DicPoint.Add($"{i},{j}", false);
                    Fix3DicBin.Add($"{i},{j}", "");
                }
            }
        }

        public static void LoaderTrayClear()
        {
            for (int i = 0; i < LoaderXPoint.Length; i++)
            {
                for (int j = 0; j < LoaderYPoint.Length; j++)
                {
                    LoaderDicPoint[$"{i},{j}"] = true;
                }
            }
        }

        public static void HeatTrayClear()
        {
            for (int i = 0; i < HeatXPoint.Length; i++)
            {
                for (int j = 0; j < HeatYPoint.Length; j++)
                {
                    HeatDicPoint[$"{i},{j}"] = false;
                    HeatDicTimer[$"{i},{j}"] = new HeatMapTimer();
                }
            }
        }

        public static void Auto1TrayClear()
        {
            for (int i = 0; i < Auto1XPoint.Length; i++)
            {
                for (int j = 0; j < Auto1YPoint.Length; j++)
                {
                    Auto1DicPoint[$"{i},{j}"] = false;
                    Auto1DicBin[$"{i},{j}"] = "";
                }
            }
        }

        public static void Auto2TrayClear()
        {
            trayTemp = "";
            for (int i = 0; i < Auto2XPoint.Length; i++)
            {
                for (int j = 0; j < Auto2YPoint.Length; j++)
                {
                    Auto2DicPoint[$"{i},{j}"] = false;
                    trayTemp += Auto2DicBin[$"{i},{j}"] + ",";
                    Auto2DicBin[$"{i},{j}"] = "";
                }
            }
            trayTemp = trayTemp.TrimEnd(',');
            if (trayTemp == "") return;
            string sql = $"insert into o_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间,名称,大小,数据) values('{DateTime.Now}','auto2','{Auto2XPoint.Length.ToString() + "x" + Auto2YPoint.Length.ToString()}','{trayTemp}')";
            //RunLog("Auto2 Sql:" + sql);
            SqlServer.runSqlNonQueryAsync(SQLServerConnString, sql);

        }

        public static void Fix1TrayClear()
        {
            trayTemp = "";
            for (int i = 0; i < Fix1XPoint.Length; i++)
            {
                for (int j = 0; j < Fix1YPoint.Length; j++)
                {
                    Fix1DicPoint[$"{i},{j}"] = false;
                    trayTemp += Fix1DicBin[$"{i},{j}"] + ",";
                    Fix1DicBin[$"{i},{j}"] = "";
                }
            }
            trayTemp = trayTemp.TrimEnd(',');
            if (trayTemp == "") return;
            string sql = $"insert into o_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间,名称,大小,数据) values('{DateTime.Now}','fix1','{Fix1XPoint.Length.ToString() + "x" + Fix1YPoint.Length.ToString()}','{trayTemp}')";
            SqlServer.runSqlNonQueryAsync(SQLServerConnString, sql);

        }

        public static void Fix2TrayClear()
        {
            trayTemp = "";
            for (int i = 0; i < Fix2XPoint.Length; i++)
            {
                for (int j = 0; j < Fix2YPoint.Length; j++)
                {
                    Fix2DicPoint[$"{i},{j}"] = false;
                    trayTemp += Fix2DicBin[$"{i},{j}"] + ",";
                    Fix2DicBin[$"{i},{j}"] = "";
                }
            }
            trayTemp = trayTemp.TrimEnd(',');
            if (trayTemp == "") return;
            string sql = $"insert into o_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间,名称,大小,数据) values('{DateTime.Now}','fix2','{Fix2XPoint.Length.ToString() + "x" + Fix2YPoint.Length.ToString()}','{trayTemp}')";
            SqlServer.runSqlNonQueryAsync(SQLServerConnString, sql);
        }

        public static void Fix3TrayClear()
        {
            trayTemp = "";
            for (int i = 0; i < Fix3XPoint.Length; i++)
            {
                for (int j = 0; j < Fix3YPoint.Length; j++)
                {
                    Fix3DicPoint[$"{i},{j}"] = false;
                    trayTemp += Fix3DicBin[$"{i},{j}"] + ",";
                    Fix3DicBin[$"{i},{j}"] = "";
                }
            }
            trayTemp = trayTemp.TrimEnd(',');
            if (trayTemp == "") return;
            string sql = $"insert into o_{LotNo[0] + "_" + 开始时间.ToString("yyyyMMdd_HH_mm_ss")}(时间,名称,大小,数据) values('{DateTime.Now}','fix3','{Fix3XPoint.Length.ToString() + "x" + Fix3YPoint.Length.ToString()}','{trayTemp}')";
            SqlServer.runSqlNonQueryAsync(SQLServerConnString, sql);

        }

        public static void GetAxisCore(string axisname, out short core, out short axis)
        {
            core = axis = 0;
            for (int i = 0; i < AxisInfoList.Count; i++)
            {
                if (axisname == AxisInfoList[i].轴名称)
                {
                    core = AxisInfoList[i].核号;
                    axis = AxisInfoList[i].轴号;
                    break;
                }
            }
        }

        public static bool[] GetSiteOpen(int site)
        {
            SiteStatus siteStatus = 设置信息.开site信息;
            switch (site)
            {
                case 0:
                    return new bool[] { siteStatus.Site1[0] == "1", siteStatus.Site1[1] == "1",siteStatus.Site1[2] == "1",siteStatus.Site1[3] == "1",
                siteStatus.Site1[4] == "1",siteStatus.Site1[5] == "1",siteStatus.Site1[6] == "1",siteStatus.Site1[7] == "1"};
                case 1:
                    return new bool[] { siteStatus.Site2[0] == "1", siteStatus.Site2[1] == "1",siteStatus.Site2[2] == "1",siteStatus.Site2[3] == "1",
                siteStatus.Site2[4] == "1",siteStatus.Site2[5] == "1",siteStatus.Site2[6] == "1",siteStatus.Site2[7] == "1"};
                case 2:
                    return new bool[] { siteStatus.Site3[0] == "1", siteStatus.Site3[1] == "1",siteStatus.Site3[2] == "1",siteStatus.Site3[3] == "1",
                siteStatus.Site3[4] == "1",siteStatus.Site3[5] == "1",siteStatus.Site3[6] == "1",siteStatus.Site3[7] == "1"};
                case 3:
                    return new bool[] { siteStatus.Site4[0] == "1", siteStatus.Site4[1] == "1",siteStatus.Site4[2] == "1",siteStatus.Site4[3] == "1",
                siteStatus.Site4[4] == "1",siteStatus.Site4[5] == "1",siteStatus.Site4[6] == "1",siteStatus.Site4[7] == "1"};
                case 4:
                    return new bool[] { siteStatus.Site5[0] == "1", siteStatus.Site5[1] == "1",siteStatus.Site5[2] == "1",siteStatus.Site5[3] == "1",
                siteStatus.Site5[4] == "1",siteStatus.Site5[5] == "1",siteStatus.Site5[6] == "1",siteStatus.Site5[7] == "1"};
                case 5:
                    return new bool[] { siteStatus.Site6[0] == "1", siteStatus.Site6[1] == "1",siteStatus.Site6[2] == "1",siteStatus.Site6[3] == "1",
                siteStatus.Site6[4] == "1",siteStatus.Site6[5] == "1",siteStatus.Site6[6] == "1",siteStatus.Site6[7] == "1"};
                case 6:
                    return new bool[] { siteStatus.Site7[0] == "1", siteStatus.Site7[1] == "1",siteStatus.Site7[2] == "1",siteStatus.Site7[3] == "1",
                siteStatus.Site7[4] == "1",siteStatus.Site7[5] == "1",siteStatus.Site7[6] == "1",siteStatus.Site7[7] == "1"};
                case 7:
                    return new bool[] { siteStatus.Site8[0] == "1", siteStatus.Site8[1] == "1",siteStatus.Site8[2] == "1",siteStatus.Site8[3] == "1",
                siteStatus.Site8[4] == "1",siteStatus.Site8[5] == "1",siteStatus.Site8[6] == "1",siteStatus.Site8[7] == "1"};
                case 8:
                    return new bool[] { siteStatus.Site9[0] == "1", siteStatus.Site9[1] == "1",siteStatus.Site9[2] == "1",siteStatus.Site9[3] == "1",
                siteStatus.Site9[4] == "1",siteStatus.Site9[5] == "1",siteStatus.Site9[6] == "1",siteStatus.Site9[7] == "1"};
                case 9:
                    return new bool[] { siteStatus.Site10[0] == "1", siteStatus.Site10[1] == "1",siteStatus.Site10[2] == "1",siteStatus.Site10[3] == "1",
                siteStatus.Site10[4] == "1",siteStatus.Site10[5] == "1",siteStatus.Site10[6] == "1",siteStatus.Site10[7] == "1"};
                case 10:
                    return new bool[] { siteStatus.Site11[0] == "1", siteStatus.Site11[1] == "1",siteStatus.Site11[2] == "1",siteStatus.Site11[3] == "1",
                siteStatus.Site11[4] == "1",siteStatus.Site11[5] == "1",siteStatus.Site11[6] == "1",siteStatus.Site11[7] == "1"};
                case 11:
                    return new bool[] { siteStatus.Site12[0] == "1", siteStatus.Site12[1] == "1",siteStatus.Site12[2] == "1",siteStatus.Site12[3] == "1",
                siteStatus.Site12[4] == "1",siteStatus.Site12[5] == "1",siteStatus.Site12[6] == "1",siteStatus.Site12[7] == "1"};
                case 12:
                    return new bool[] { siteStatus.Site13[0] == "1", siteStatus.Site13[1] == "1",siteStatus.Site13[2] == "1",siteStatus.Site13[3] == "1",
                siteStatus.Site13[4] == "1",siteStatus.Site13[5] == "1",siteStatus.Site13[6] == "1",siteStatus.Site13[7] == "1"};
                case 13:
                    return new bool[] { siteStatus.Site14[0] == "1", siteStatus.Site14[1] == "1",siteStatus.Site14[2] == "1",siteStatus.Site14[3] == "1",
                siteStatus.Site14[4] == "1",siteStatus.Site14[5] == "1",siteStatus.Site14[6] == "1",siteStatus.Site14[7] == "1"};
                case 14:
                    return new bool[] { siteStatus.Site15[0] == "1", siteStatus.Site15[1] == "1",siteStatus.Site15[2] == "1",siteStatus.Site15[3] == "1",
                siteStatus.Site15[4] == "1",siteStatus.Site15[5] == "1",siteStatus.Site15[6] == "1",siteStatus.Site15[7] == "1"};
                case 15:
                    return new bool[] { siteStatus.Site16[0] == "1", siteStatus.Site16[1] == "1",siteStatus.Site16[2] == "1",siteStatus.Site16[3] == "1",
                siteStatus.Site16[4] == "1",siteStatus.Site16[5] == "1",siteStatus.Site16[6] == "1",siteStatus.Site16[7] == "1"};
                case 16:
                    return new bool[] { true, true, true, true, true, true, true, true };
                default: return new bool[] { false, false, false, false, false, false, false, false };
            }
        }

        public static bool JudgeNozzleSts(bool[] lsopen, int[] opens, int value)
        {
            for (int i = 0; i < opens.Length; i++)
            {
                if (lsopen[i])
                {
                    if (opens[i] == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsDutAllCLose()
        {
            SiteStatus sitestatus = 设置信息.开site信息;
            for (int i = 0; i < 16; i++)
            {
                if (sitestatus.GetSiteValue(i).Contains("1"))
                {
                    return true;
                }
            }
            return false;
        }

        public static void StartRunIn()
        {
            APPmaster.InSiteQueue = new ConcurrentQueue<int>();
            SiteStatus siteStatus = 设置信息.开site信息;
            bool[] siteopen = new bool[16];

            for (int site = 0; site < 16; site++)
            {
                if (siteStatus.GetSiteValue(site).Contains("1"))
                {
                    siteopen[site] = true;
                }
            }

            for (int site = 0; site < siteopen.Length; site++)
            {
                if (siteopen[site])
                {
                    RunLog($"开始前入料添加Site:{site + 1}");
                    APPmaster.InSiteQueue.Enqueue(site);
                }
            }
        }

        public static void RunLog(string info, WriteData.LogType txtType = WriteData.LogType.RunLog, bool isReceive = false, string excelLevel = "debug", string excelType = "run")//int level = "debug", string logType = "run"
        {
            LogManager log = new LogManager()
            {
                Msg = info,
                Time = DateTime.Now,
                TxtLogtype = txtType,
                ExcelLevel = excelLevel,
                ExcelType = excelType,
                IsReceive = isReceive,
            };
            LogQueue.Enqueue(log);
            //Debug.WriteLine(info);
            ////Log.AddLog("debug", "run", info);
            //writeDataHelper.RunLog(info, logtype);
        }

        public static void 整组开site()
        {

        }

        public static bool[] 检测温度是否达温(double tempmax, double tempmin)
        {
            bool[] tempopen = new bool[17];

            for (int i = 0; i < SiteTemp.Length; i++)
            {
                if (GetSiteOpen(i).Contains(true))
                {
                    if (SiteTemp[i] != 32770)
                    {
                        if (SiteTemp[i] < tempmax && SiteTemp[i] >= tempmin)
                        {
                            tempopen[i] = true;
                        }
                        else tempopen[i] = false;
                    }
                }
                else tempopen[i] = true; //没有开Site True:默认达温, False默认无异常
            }

            return tempopen;
        }
        public static bool[] 检测温度是否异常(double tempmax, double tempmin)
        {
            bool[] tempopen = new bool[17];

            for (int i = 0; i < SiteTemp.Length; i++)
            {
                if (GetSiteOpen(i).Contains(true))
                {
                    if (SiteTemp[i] != 32770)
                    {
                        if (SiteTemp[i] > tempmax || SiteTemp[i] <= tempmin)
                        {
                            tempopen[i] = true;
                        }
                        else tempopen[i] = false;
                    }
                }
                else tempopen[i] = false; //没有开Site True:默认达温, False默认无异常
            }

            return tempopen;
        }

        public static bool[] 检测温度探头状态()
        {
            bool[] tempopen = new bool[17];

            for (int i = 0; i < SiteTemp.Length; i++)
            {
                if (GetSiteOpen(i).Contains(true))
                {
                    if (SiteTemp[i] == 32770)
                    {
                        tempopen[i] = true; //温度探头异常
                    }
                    else tempopen[i] = false; //温度探头正常
                }
                else tempopen[i] = false; //没有开Site不检测探头
            }

            return tempopen;
        }

        public static void 结批时判断是否关site(bool[] open, bool[] open2, bool[] open3, out bool[] isexception, out bool[] isexception2)
        {
            SiteStatus sitestatus = 设置信息.开site信息;

            isexception = new bool[8];
            isexception2 = new bool[8];

            for (int dut = 0; dut < open.Length; dut++)
            {
                if (!open3[dut]) //if (sitestatus.GetSiteValue(site, dut) == "1")
                {
                    for (int site = 0; site < 16; site++)
                    {
                        if (open[dut]) isexception[dut] = true;

                        if (open2[dut]) isexception2[dut] = true;

                        if (isexception[dut] || isexception2[dut]) break;
                    }
                }
            }
        }

        public static void 更新入料目标值()
        {
            SiteStatus siteStatus = 设置信息.开site信息;
            bool[] siteopen = new bool[8] { false, false, false, false, false, false, false, false };

            for (int dut = 0; dut < 8; dut++)
            {
                for (int site = 0; site < 16; site++)
                {
                    if (siteStatus.GetSiteValue(site, dut) == "1")
                    {
                        siteopen[dut] = true;
                        break;
                    }
                    if (siteopen[dut]) break;
                }
            }

            放料IC目标值 = new bool[][]
            {
                new bool[] { siteopen[0], siteopen[1], siteopen[2], siteopen[3] },
                new bool[] { siteopen[4], siteopen[5], siteopen[6], siteopen[7] }
            };
        }

        public static void 更新关site更改(bool[][] 修改前入料目标值)
        {
            bool[][] 新关site更改 = new bool[2][] { new bool[4], new bool[4] };
            for (int colum = 0; colum < 放料IC目标值.Length; colum++)
            {
                for (int dut = 0; dut < 放料IC目标值[colum].Length; dut++)
                {
                    if (放料IC目标值[colum][dut] != 修改前入料目标值[colum][dut])
                    {
                        if (!放料IC目标值[colum][dut] && 修改前入料目标值[colum][dut])
                        {
                            新关site更改[colum][dut] = true;
                            //RunLog($"放料目标值dut:{colum * 4 + dut + 1},{放料IC目标值[colum][dut]}->{save[colum][dut]}");
                        }
                        //else RunLog($"放料目标值dut:{colum * 4 + dut + 1},{放料IC目标值[colum][dut]}->{save[colum][dut]}");
                    }
                }
            }

            保存关Site更改 = 新关site更改;
        }

        public static void 更新开Site更改(bool[] 修改前开Site更改)
        {
            SiteStatus siteStatus = 设置信息.开site信息;

            bool[] siteopen = new bool[16];

            for (int site = 0; site < 16; site++)
            {
                if (siteStatus.GetSiteValue(site).Contains("1"))
                {
                    siteopen[site] = true;
                }
            }

            保存开Site更改 = siteopen;
        }


        public static bool 获取Load盘位置(int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Load盘坐标(true, out short xPoint, out short yPoint))
            {
                xPos = LoaderXPoint[xPoint] + 点位微调.入料_Load吸_X;
                yPos = (LoaderYPoint[yPoint] + index * Interval * Tray_Y_Pitch) + 点位微调.入料_Load吸_Y;
                return true;
            }
            else return false;
        }

        public static bool 获取Heat盘位置(int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Load盘坐标(true, out short xPoint, out short yPoint))
            {
                xPos = HeatXPoint[xPoint];
                yPos = HeatYPoint[yPoint] + index * HeatInterval * Heat_Y_Pitch;
                return true;
            }
            else return false;
        }

        public static void 更新Load盘Map(bool[] lsopen, int index, short xPoint, short yPoint)
        {
            int LoadNumber = 0;
            for (int i = 0; i < lsopen.Length; i++)
            {
                if (index + i == lsopen.Length) break;
                if (lsopen[i + index])
                {
                    LoaderDicPoint[$"{xPoint},{yPoint + i * Interval}"] = false;
                    RunLog($"Loader site{index}:{xPoint},{yPoint + i * Interval}");

                    //if (getiosts[i + index] == false) //如果等于False说明已经吸取到了IC
                    //{
                    //    opens[i + index] = 1;
                    LoadNumber++;
                    //}
                }
            }
        }

        public static bool 判断BIN项(int[] opens, string setbin, string[] valbin, out int index)
        {
            index = 0;
            for (int i = 0; i < valbin.Length; i++)
            {
                if (opens[i] == 1 && setbin.Split(',').Contains(valbin[i]))
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public static bool 获取吸嘴索引(int[] opens, short isic, out int index)
        {
            index = 0;
            for (short i = 0; i < opens.Length; i++)
            {
                if (opens[i] == isic)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        public static bool 获取Auto1盘位置(int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Auto1盘坐标(false, out int xPoint, out int yPoint))
            {
                xPos = Auto1XPoint[xPoint];
                yPos = Auto1YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                return true;
            }
            else return false;
        }

        public static bool 获取Auto2盘位置(int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Auto2盘坐标(false, out int xPoint, out int yPoint))
            {
                xPos = Auto2XPoint[xPoint];
                yPos = Auto2YPoint[yPoint] + index * Interval * Tray_Y_Pitch; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                return true;
            }
            else return false;
        }

        public static string 获取Load盘Open(int[] opens, int xPoint, int yPoint, out bool[] lsopen, out int index)
        {
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 0, out index)) return "获取入料吸嘴索引失败!";
            //if (!获取Load盘位置(index, out xPos, out yPos)) return "获取Load盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= LoaderYPoint.Length) break;
                if (LoaderDicPoint[$"{xPoint},{yPoint + i * Interval}"] == false) break;
                if (opens[i + index] == 0)
                {
                    lsopen[i + index] = true;
                    if (i > 0 && (lsopen[i + index] != lsopen[i + index - 1]))
                    {
                        lsopen[i + index] = false;
                        break;
                    }
                }
            }
            return "Success";
        }

        public static string 获取Heat盘Open(int[] opens, int xPoint, int yPoint, short status, bool ishave, short instatus, out bool[] lsopen, out int index)
        {
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, status, out index)) return "获取入料吸嘴索引失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * HeatInterval >= HeatYPoint.Length) break;
                if (HeatDicPoint[$"{xPoint},{yPoint + i * HeatInterval}"] != ishave || HeatDicTimer[$"{xPoint},{yPoint + i * HeatInterval}"].Status != instatus) break;
                if (opens[i + index] == status)
                {
                    lsopen[i + index] = true;
                    if (i > 0 && (lsopen[i + index] != lsopen[i + index - 1]))
                    {
                        lsopen[i + index] = false;
                        break;
                    }
                }
            }
            return "Success";
        }

        public static void 获取Auto1盘Open(int[] opens, int xPoint, int yPoint, out bool[] lsopen, int index, string[] bin)
        {
            lsopen = new bool[opens.Length];

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Auto1YPoint.Length) break;
                if (Auto1DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1 && Auto1.Split(',').Contains(bin[i + index]))
                {
                    lsopen[i + index] = true;
                    if (i > 0 && (lsopen[i + index] != lsopen[i + index - 1]))
                    {
                        lsopen[i + index] = false;
                        break;
                    }
                }
            }
        }

        public static void 获取Auto2盘Open(int[] opens, int xPoint, int yPoint, out bool[] lsopen, int index, string[] bin)
        {
            lsopen = new bool[opens.Length];

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Auto2YPoint.Length) break;
                if (Auto2DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1 && Auto2.Split(',').Contains(bin[i + index]))
                {
                    lsopen[i + index] = true;
                    if (i > 0 && (lsopen[i + index] != lsopen[i + index - 1]))
                    {
                        lsopen[i + index] = false;
                        break;
                    }
                }
            }
        }

        public static void 获取Fix1盘Open(int[] opens, int xPoint, int yPoint, string tray, string[] bin, int max, out bool[] lsopen, int index)
        {
            lsopen = new bool[opens.Length];
            //获取吸嘴索引(opens, 1, out index);
            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= max) break;
                if (Fix1DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1 && tray.Split(',').Contains(bin[i + index]))
                {
                    lsopen[i + index] = true;
                    if (i > 0 && (lsopen[i + index] != lsopen[i + index - 1]))
                    {
                        lsopen[i + index] = false;
                        break;
                    }
                }
            }
        }

        public static void 获取Fix2盘Open(int[] opens, int xPoint, int yPoint, string tray, string[] bin, int max, out bool[] lsopen, int index)
        {
            lsopen = new bool[opens.Length];
            //获取吸嘴索引(opens, 1, out index);
            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= max) break;
                if (Fix2DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1 && tray.Split(',').Contains(bin[i + index]))
                {
                    lsopen[i + index] = true;
                    if (i > 0 && (lsopen[i + index] != lsopen[i + index - 1]))
                    {
                        lsopen[i + index] = false;
                        break;
                    }
                }
            }
        }

        public static void 获取Fix3盘Open(int[] opens, int xPoint, int yPoint, string tray, string[] bin, int max, out bool[] lsopen, int index)
        {
            lsopen = new bool[opens.Length];
            //获取吸嘴索引(opens, 1, out index);
            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= max) break;
                if (Fix3DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1 && tray.Split(',').Contains(bin[i + index]))
                {
                    lsopen[i + index] = true;
                    if (i > 0 && (lsopen[i + index] != lsopen[i + index - 1]))
                    {
                        lsopen[i + index] = false;
                        break;
                    }
                }
            }
        }

        public static void FixMinMax(int bintype, int binarea, int fixleng, out int min, out int max)
        {
            min = 0;
            max = fixleng; //19
            if (bintype == 2) //分两区
            {
                if (binarea == 1) //分两区的1区
                {
                    min = 0;
                    max = fixleng / 2;
                }
                else if (binarea == 2) //分两区的2区
                {
                    min = fixleng / 2 + 1; //+1
                    max = fixleng;
                }
            }
            else if (bintype == 3)//分三区
            {
                if (binarea == 1) //分两区的1区
                {
                    min = 0;
                    max = fixleng / 3;
                }
                else if (binarea == 2) //分两区的2区
                {
                    min = fixleng / 3 + 1;
                    max = fixleng / 3 * 2 + 1;
                }
                else if (binarea == 3) //分两区的2区
                {
                    min = fixleng / 3 * 2 + 2;
                    max = fixleng;
                }
            }
        }

        public static string 判断Auto1BIN项(int[] open, string[] bin)
        {
            if (!open.Contains(1)) return "Fail"; //如果没有1说明吸嘴上已无料

            if (判断BIN项(open, Auto1, bin, out int index))
            {
                return "Success";
            }
            else return "Fail";
        }

        public static string 判断Auto2BIN项(int[] open, string[] bin)
        {
            if (!open.Contains(1)) return "Fail"; //如果没有1说明吸嘴上已无料

            if (判断BIN项(open, Auto2, bin, out int index))
            {
                return "Success";
            }
            else return "Fail";
        }

        public static string 判断Fix1BIN项(int[] open, string[] bin)
        {
            if (!open.Contains(1)) return "Fail"; //如果没有1说明吸嘴上已无料

            if (Fix1Num == 1)
            {
                if (判断BIN项(open, Fix1, bin, out int index))
                {
                    return "Success";
                }
            }
            else if (Fix1Num == 2)
            {
                if (判断BIN项(open, Fix1_1, bin, out int index))
                {
                    return "Success";
                }
                else if (判断BIN项(open, Fix1_2, bin, out index))
                {
                    return "Success";
                }
            }
            else if (Fix1Num == 3)
            {
                if (判断BIN项(open, Fix1_1, bin, out int index))
                {
                    return "Success";
                }
                else if (判断BIN项(open, Fix1_2, bin, out index))
                {
                    return "Success";
                }
                else if (判断BIN项(open, Fix1_3, bin, out index))
                {
                    return "Success";
                }
            }
            return "Fail";
        }

        public static string 判断Fix2BIN项(int[] open, string[] bin)
        {
            if (!open.Contains(1)) return "Fail"; //如果没有1说明吸嘴上已无料

            if (Fix2Num == 1)
            {
                if (判断BIN项(open, Fix2, bin, out int index))
                {
                    return "Success";
                }
            }
            else if (Fix2Num == 2)
            {
                if (判断BIN项(open, Fix2_1, bin, out int index))
                {
                    return "Success";
                }
                else if (判断BIN项(open, Fix2_2, bin, out index))
                {
                    return "Success";
                }
            }
            else if (Fix2Num == 3)
            {
                if (判断BIN项(open, Fix2_1, bin, out int index))
                {
                    return "Success";
                }
                else if (判断BIN项(open, Fix2_2, bin, out index))
                {
                    return "Success";
                }
                else if (判断BIN项(open, Fix2_3, bin, out index))
                {
                    return "Success";
                }
            }
            return "Fail";
        }

        public static string 判断Fix3BIN项(int[] open, string[] bin)
        {
            if (!open.Contains(1)) return "Fail"; //如果没有1说明吸嘴上已无料

            if (Fix3Num == 1)
            {
                if (判断BIN项(open, Fix3, bin, out int index))
                {
                    return "Success";
                }
            }
            else if (Fix3Num == 2)
            {
                if (判断BIN项(open, Fix3_1, bin, out int index))
                {
                    return "Success";
                }
                else if (判断BIN项(open, Fix3_2, bin, out index))
                {
                    return "Success";
                }
            }
            else if (Fix3Num == 3)
            {
                if (判断BIN项(open, Fix3_1, bin, out int index))
                {
                    return "Success";
                }
                else if (判断BIN项(open, Fix3_2, bin, out index))
                {
                    return "Success";
                }
                else if (判断BIN项(open, Fix3_3, bin, out index))
                {
                    return "Success";
                }
            }
            return "Fail";
        }

        public static double[] 获取梭车xyp放料点位(int colum)
        {
            double[] pos = new double[3];
            if (飞梭2位置 == 0) //梭车2在后，梭车1在前
            {
                if (colum == 0) //梭车1第1列
                {
                    pos[0] = 初始点位.入料_梭车1_X + 点位微调.入料_梭车1第1列放_X;
                    pos[1] = 初始点位.入料_梭车1_Y + 点位微调.入料_梭车1第1列放_Y;
                    pos[2] = 入料放料间距 + 点位微调.入料_梭车_变距;
                }
                else  //梭车1第2列
                {
                    pos[0] = 初始点位.入料_梭车1_X + 梭车列间距 + 点位微调.入料_梭车1第2列放_X;
                    pos[1] = 初始点位.入料_梭车1_Y + 点位微调.入料_梭车1第2列放_Y;
                    pos[2] = 入料放料间距 + 点位微调.入料_梭车_变距;
                }
            }
            else //梭车2在前，梭车1后
            {
                if (colum == 0) //梭车2第1列
                {
                    pos[0] = 初始点位.入料_梭车2_X + 点位微调.入料_梭车2第1列放_X;
                    pos[1] = 初始点位.入料_梭车2_Y + 点位微调.入料_梭车2第1列放_Y;
                    pos[2] = 入料放料间距 + 点位微调.入料_梭车_变距; ;
                }
                else  //梭车2第2列
                {
                    pos[0] = 初始点位.入料_梭车2_X + 梭车列间距 + 点位微调.入料_梭车2第2列放_X;
                    pos[1] = 初始点位.入料_梭车2_Y + 点位微调.入料_梭车2第2列放_Y;
                    pos[2] = 入料放料间距 + 点位微调.入料_梭车_变距;
                }
            }
            return pos;
        }

        public static double[] 获取梭车xyp吸料点位(int colum)
        {
            double[] pos = new double[3];
            if (飞梭2位置 == 0) //梭车2在后，梭车1在前
            {
                if (colum == 0) //梭车1第1列
                {
                    pos[0] = 初始点位.出料_梭车1_X + 点位微调.出料_梭车1第1列吸_X;
                    pos[1] = 初始点位.出料_梭车1_Y + 点位微调.出料_梭车1第1列吸_Y;
                    pos[2] = 出料吸料间距 + 点位微调.出料_梭车_变距;
                }
                else  //梭车1第2列
                {
                    pos[0] = 初始点位.出料_梭车1_X - 梭车列间距 + 点位微调.出料_梭车1第2列吸_X;
                    pos[1] = 初始点位.出料_梭车1_Y + 点位微调.出料_梭车1第2列吸_Y;
                    pos[2] = 出料吸料间距 + 点位微调.出料_梭车_变距;
                }
            }
            else //梭车2在前，梭车1后
            {
                if (colum == 0) //梭车2第1列
                {
                    pos[0] = 初始点位.出料_梭车2_X + 点位微调.出料_梭车2第1列吸_X;
                    pos[1] = 初始点位.出料_梭车2_Y + 点位微调.出料_梭车2第1列吸_Y;
                    pos[2] = 出料吸料间距 + 点位微调.出料_梭车_变距;
                }
                else  //梭车2第2列
                {
                    pos[0] = 初始点位.出料_梭车2_X - 梭车列间距 + 点位微调.出料_梭车2第2列吸_X;
                    pos[1] = 初始点位.出料_梭车2_Y + 点位微调.出料_梭车2第2列吸_Y;
                    pos[2] = 出料吸料间距 + 点位微调.出料_梭车_变距;
                }
            }
            return pos;
        }

        public static double 获取梭车Z放料点位(int colum)
        {
            if (飞梭2位置 == 0)
            {
                if (colum == 0)
                {
                    return 初始点位.入料_梭车1_Z + 点位微调.入料_梭车1第1列放_Z;
                }
                else return 初始点位.入料_梭车1_Z + 点位微调.入料_梭车1第2列放_Z;
            }
            else
            {
                if (colum == 0)
                {
                    return 初始点位.入料_梭车2_Z + 点位微调.入料_梭车2第1列放_Z;
                }
                else return 初始点位.入料_梭车2_Z + 点位微调.入料_梭车2第2列放_Z;
            }
        }

        public static double 获取梭车Z吸料点位(int colum)
        {
            if (飞梭2位置 == 0)
            {
                if (colum == 0)
                {
                    return 初始点位.出料_梭车1_Z + 点位微调.出料_梭车1第1列吸_Z;
                }
                else return 初始点位.出料_梭车1_Z + 点位微调.出料_梭车1第2列吸_Z;
            }
            else
            {
                if (colum == 0)
                {
                    return 初始点位.出料_梭车2_Z + 点位微调.出料_梭车2第1列吸_Z;
                }
                else return 初始点位.出料_梭车2_Z + 点位微调.出料_梭车2第2列吸_Z;
            }
        }

        public static double 获取出料Z放料点位(int trayid)
        {
            if (trayid == 0)
            {
                return 初始点位.出料_Auto1_Z + 点位微调.出料_Auto1放_Z;
            }
            else if (trayid == 1)
            {
                return 初始点位.出料_Auto2_Z + 点位微调.出料_Auto2放_Z;
            }
            else if (trayid == 2)
            {
                return 初始点位.出料_Fix1_Z + 点位微调.出料_Fix1放_Z;
            }
            else if (trayid == 3)
            {
                return 初始点位.出料_Fix2_Z + 点位微调.出料_Fix2放_Z;
            }
            else
            {
                return 初始点位.出料_Fix3_Z + 点位微调.出料_Fix3放_Z;
            }
        }

        public static string 获取测试X吸料补偿(int site, out double sitex)
        {
            sitex = 0;
            switch (site)
            {
                case 0: sitex = 点位微调.测试_Site1吸_X; break;
                case 1: sitex = 点位微调.测试_Site2吸_X; break;
                case 2: sitex = 点位微调.测试_Site3吸_X; break;
                case 3: sitex = 点位微调.测试_Site4吸_X; break;
                case 4: sitex = 点位微调.测试_Site5吸_X; break;
                case 5: sitex = 点位微调.测试_Site6吸_X; break;
                case 6: sitex = 点位微调.测试_Site7吸_X; break;
                case 7: sitex = 点位微调.测试_Site8吸_X; break;
                case 8: sitex = 点位微调.测试_Site9吸_X; break;
                case 9: sitex = 点位微调.测试_Site10吸_X; break;
                case 10: sitex = 点位微调.测试_Site11吸_X; break;
                case 11: sitex = 点位微调.测试_Site12吸_X; break;
                case 12: sitex = 点位微调.测试_Site13吸_X; break;
                case 13: sitex = 点位微调.测试_Site14吸_X; break;
                case 14: sitex = 点位微调.测试_Site15吸_X; break;
                case 15: sitex = 点位微调.测试_Site16吸_X; break;
                default: return "Fail";
            }
            return "Success";
        }

        public static string 获取测试Z吸料补偿(int site, out double sitez)
        {
            sitez = 0;
            switch (site)
            {
                case 0: sitez = 点位微调.测试_Site1吸_Z; break;
                case 1: sitez = 点位微调.测试_Site2吸_Z; break;
                case 2: sitez = 点位微调.测试_Site3吸_Z; break;
                case 3: sitez = 点位微调.测试_Site4吸_Z; break;
                case 4: sitez = 点位微调.测试_Site5吸_Z; break;
                case 5: sitez = 点位微调.测试_Site6吸_Z; break;
                case 6: sitez = 点位微调.测试_Site7吸_Z; break;
                case 7: sitez = 点位微调.测试_Site8吸_Z; break;
                case 8: sitez = 点位微调.测试_Site9吸_Z; break;
                case 9: sitez = 点位微调.测试_Site10吸_Z; break;
                case 10: sitez = 点位微调.测试_Site11吸_Z; break;
                case 11: sitez = 点位微调.测试_Site12吸_Z; break;
                case 12: sitez = 点位微调.测试_Site13吸_Z; break;
                case 13: sitez = 点位微调.测试_Site14吸_Z; break;
                case 14: sitez = 点位微调.测试_Site15吸_Z; break;
                case 15: sitez = 点位微调.测试_Site16吸_Z; break;
                default: return "Fail";
            }
            return "Success";
        }


        public static string 获取测试X放料补偿(int site, out double sitex)
        {
            sitex = 0;
            switch (site)
            {
                case 0: sitex = 点位微调.测试_Site1放_X; break;
                case 1: sitex = 点位微调.测试_Site2放_X; break;
                case 2: sitex = 点位微调.测试_Site3放_X; break;
                case 3: sitex = 点位微调.测试_Site4放_X; break;
                case 4: sitex = 点位微调.测试_Site5放_X; break;
                case 5: sitex = 点位微调.测试_Site6放_X; break;
                case 6: sitex = 点位微调.测试_Site7放_X; break;
                case 7: sitex = 点位微调.测试_Site8放_X; break;
                case 8: sitex = 点位微调.测试_Site9放_X; break;
                case 9: sitex = 点位微调.测试_Site10放_X; break;
                case 10: sitex = 点位微调.测试_Site11放_X; break;
                case 11: sitex = 点位微调.测试_Site12放_X; break;
                case 12: sitex = 点位微调.测试_Site13放_X; break;
                case 13: sitex = 点位微调.测试_Site14放_X; break;
                case 14: sitex = 点位微调.测试_Site15放_X; break;
                case 15: sitex = 点位微调.测试_Site16放_X; break;
                default: return "Fail";
            }
            return "Success";
        }

        public static string 获取测试Z放料补偿(int site, out double sitez)
        {
            sitez = 0;
            switch (site)
            {
                case 0: sitez = 点位微调.测试_Site1放_Z; break;
                case 1: sitez = 点位微调.测试_Site2放_Z; break;
                case 2: sitez = 点位微调.测试_Site3放_Z; break;
                case 3: sitez = 点位微调.测试_Site4放_Z; break;
                case 4: sitez = 点位微调.测试_Site5放_Z; break;
                case 5: sitez = 点位微调.测试_Site6放_Z; break;
                case 6: sitez = 点位微调.测试_Site7放_Z; break;
                case 7: sitez = 点位微调.测试_Site8放_Z; break;
                case 8: sitez = 点位微调.测试_Site9放_Z; break;
                case 9: sitez = 点位微调.测试_Site10放_Z; break;
                case 10: sitez = 点位微调.测试_Site11放_Z; break;
                case 11: sitez = 点位微调.测试_Site12放_Z; break;
                case 12: sitez = 点位微调.测试_Site13放_Z; break;
                case 13: sitez = 点位微调.测试_Site14放_Z; break;
                case 14: sitez = 点位微调.测试_Site15放_Z; break;
                case 15: sitez = 点位微调.测试_Site16放_Z; break;
                default: return "Fail";
            }
            return "Success";
        }
    }
}
