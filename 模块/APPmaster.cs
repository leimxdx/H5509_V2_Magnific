using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class APPmaster
    {
        /// <summary>
        /// 板卡初始成功标志
        /// </summary>
        public static bool InitedOK = false;

        public static List<IOInfo> IOList;

        public static List<AxisInfo> AxisInfoList;

        public static List<AxisRunInfo> AxisRunInfoList;

        public static List<AxisHomeInfo> AxisHomeInfoList;

        public static bool IsStart = false; //开始

        public static bool ProgramStart = false; //程序开始

        public static bool ProgramBtnStart = false; //程序按钮开始

        public static bool IsPause = false; //停止

        public static bool ProgramPause = false; //程序暂停

        public static bool ProgramBtnPause = false; //程序按钮暂停

        public static bool IsForecePause = false; //强制停止

        public static bool H5509初始化 = false;

        public static bool H5509正在初始化 = false;

        public static bool 定点停止标记 = false;

        public static bool 定点停止允许 = false;

        public static int 气缸动作超时时间 = 3000; //ms

        public static int 跟随误差 = 150;

        public static Communication Com = new Communication();

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

        public static Dictionary<string, bool> Auto1DicPoint = new Dictionary<string, bool>(); //自动盘1Map图

        public static Dictionary<string, bool> Auto2DicPoint = new Dictionary<string, bool>(); //自动盘2Map图

        public static Dictionary<string, bool> Fix1DicPoint = new Dictionary<string, bool>(); //手动盘1Map图

        public static Dictionary<string, bool> Fix2DicPoint = new Dictionary<string, bool>(); //手动盘2Map图

        public static Dictionary<string, bool> Fix3DicPoint = new Dictionary<string, bool>(); //手动盘3Map图

        public static Dictionary<string, string> HeatDicBin { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, string> Auto1DicBin { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, string> Auto2DicBin { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, string> Fix1DicBin { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, string> Fix2DicBin { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, string> Fix3DicBin { get; set; } = new Dictionary<string, string>();

        public static string Auto1 { get; set; } = "01";
        public static string Auto2 { get; set; } = "02,03,04,05,06";
        public static string Fix1 { get; set; } = "07";
        public static string Fix2 { get; set; } = "08";
        public static string Fix3 { get; set; } = "09";

        public static int 飞梭2位置 = 0; //0:后面 1:前面

        public static bool 调机标记 = true;

        public static int Tray平移左右标记 = 0;

        public static bool Tray平移运动标记; 

        public static int[] SiteTestState = new int[16];

        public static List<bool[][]> SiteStatus = new List<bool[][]>(); //保存Site测试开关状态
        public static List<bool[][]> SiteTestIC = new List<bool[][]>(); //保存Site放入的IC状态
        public static List<string[][]> SiteTestBIN = new List<string[][]>(); //保存

        public static ConcurrentQueue<string[]> BinSaveQueue = new ConcurrentQueue<string[]>(); //用来保存接收的Bin数据的队列
        public static ConcurrentQueue<string> SiteSaveQueue = new ConcurrentQueue<string>(); //用来保存接收的Site数据的队列

        public static ConcurrentQueue<int> InSiteQueue = new ConcurrentQueue<int>(); //入料队列
        public static ConcurrentQueue<int> OutSiteQueue = new ConcurrentQueue<int>(); //出料队列

        public static short Interval = 2; //吸料间隔
        public static bool 结批标记;

        public static short 入料手臂吸料状态;
        public static bool 入料手臂抓料等待标记;

        public static short 飞梭移动状态;
        public static short 飞梭前入料状态;
        public static short 飞梭前出料状态;
        public static short 飞梭后入料状态;
        public static short 飞梭后出料状态;

        public static bool[][] 飞梭前入料IC;
        public static bool[][] 飞梭前出料IC;
        public static bool[][] 飞梭后入料IC;
        public static bool[][] 飞梭后出料IC;

        public static short Empty上顶标记;
        public static short Loader上顶标记;
        public static short Auto1上顶标记;
        public static short Auto2上顶标记;

        public static bool[][] 放料IC目标值;

        public static string[][] 飞梭前出料BIN项;
        public static string[][] 飞梭后出料BIN项;

        public static ST_ProcessManage PS入料手臂吸放料流程;

        public static ST_ProcessManage PS出料手臂吸放料流程;

        public static ST_ProcessManage PS测试手臂取待测料流程;

        public static ST_ProcessManage PS测试手臂取已测料流程;

        public static ST_ProcessManage PS测试区气缸下压动作流程;

        public static ST_ProcessManage PS测试区气缸上抬动作流程;

        public static ST_ProcessManage PS空盘上顶流程;

        public static ST_ProcessManage PS待测盘上顶流程;

        public static ST_ProcessManage PS手动盘1上顶流程;

        public static ST_ProcessManage PS手动盘2上顶流程;

        public static ST_ProcessManage PS入料手臂放料流程;

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

        /// <summary>
        /// Loader TrayIC点位初始化
        /// </summary>
        public static void LoaderTrayICPointInit()
        {
            double xStart = 15.45;//(int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_InputArm_X_Precision);
            double xPitch = 15;//(int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_InputArm_X_Precision);
            double yStart = 10.8;//(int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_InputArm_Y_Precision);
            double yPitch = 16.3;//(int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_InputArm_Y_Precision);
            int xNum = 8;//(int)motionEx.trayParm.X_Quantity;
            int yNum = 19;//(int)motionEx.trayParm.Y_Quantity;
            double loaderx = 120;//motionEx.InitPointParam.InputArm_Loader_X + motionEx.OffsetParam.InputArm.InputArm_Loader_X;
            double loadery = 40;// motionEx.InitPointParam.InputArm_Loader_Y + motionEx.OffsetParam.InputArm.InputArm_Loader_Y;
            double total = 41 - 10 * 2; //move 900
            //double total = 38.4 - motionEx.trayParm.Y_Pitch * 2; //move 580
            int ymove = (int)(total);// / motionEx.advancedParam.Axis_InputArm_Y_Precision);
            int pmove = (int)(total);// / motionEx.advancedParam.Axis_InputArm_P_Precision);
            LoaderXPoint = new double[xNum];
            LoaderYPoint = new double[yNum];

            LoaderXPoint[0] = loaderx + xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                LoaderXPoint[i] = LoaderXPoint[i - 1] + xPitch; //累加X轴坐标
            }
            LoaderYPoint[0] = loadery - yStart + Interval * ymove; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标 33.900000000000006 50
            for (int i = 1; i < yNum; i++)
            {
                LoaderYPoint[i] = LoaderYPoint[i - 1] - yPitch; //累加Y轴坐标
            }
        }

        /// <summary>
        /// Heat TrayIC点位初始化
        /// </summary>
        public static void HeatTrayICPointInit()
        {
            double xStart = 15.45;//(int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double xPitch = 15;//(int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double yStart = 10.8;//(int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            double yPitch = 16.3;//(int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            int xNum = 8;//(int)motionEx.trayParm.X_Quantity;
            int yNum = 19;//(int)motionEx.trayParm.Y_Quantity;
            double heatx = 240;// motionEx.InitPointParam.OutputArm_Auto1_X + motionEx.OffsetParam.OutputArm.OutputArm_Auto1_X;
            double heaty = 100;// motionEx.InitPointParam.OutputArm_Auto1_Y + motionEx.OffsetParam.OutputArm.OutputArm_Auto1_Y; ;
            double total = 41 - 10 * 2; //move 580
            int move = (int)(total);
            HeatXPoint = new double[xNum];
            HeatYPoint = new double[yNum];

            HeatXPoint[0] = heatx - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                HeatXPoint[i] = HeatXPoint[i - 1] - xPitch; //累加X轴坐标
            }
            HeatYPoint[0] = heaty - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
            for (int i = 1; i < yNum; i++)
            {
                HeatYPoint[i] = HeatYPoint[i - 1] - yPitch; //累加Y轴坐标
            }
        }

        /// <summary>
        /// Auto1 TrayIC点位初始化
        /// </summary>
        public static void Auto1TrayICPointInit()
        {
            double xStart = 15.45;// (int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double xPitch = 15;//(int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double yStart = 10.8;//(int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            double yPitch = 16.3;//(int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            int xNum = 8;//(int)motionEx.trayParm.X_Quantity;
            int yNum = 19;//(int)motionEx.trayParm.Y_Quantity;
            double auto1x = 500;// motionEx.InitPointParam.OutputArm_Auto1_X + motionEx.OffsetParam.OutputArm.OutputArm_Auto1_X;
            double auto1y = 500;// motionEx.InitPointParam.OutputArm_Auto1_Y + motionEx.OffsetParam.OutputArm.OutputArm_Auto1_Y; ;
            double total = 41 - 10 * 2; //move 580
            int move = (int)(total);
            Auto1XPoint = new double[xNum];
            Auto1YPoint = new double[yNum];

            Auto1XPoint[0] = auto1x - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                Auto1XPoint[i] = Auto1XPoint[i - 1] - xPitch; //累加X轴坐标
            }
            Auto1YPoint[0] = auto1y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
            for (int i = 1; i < yNum; i++)
            {
                Auto1YPoint[i] = Auto1YPoint[i - 1] - yPitch; //累加Y轴坐标
            }
        }

        /// <summary>
        /// Auto2 TrayIC点位初始化
        /// </summary>
        public static void Auto2TrayICPointInit()
        {
            double xStart = 15.45;//(int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double xPitch = 15;//int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double yStart = 10.8;//(int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            double yPitch = 16.3;//(int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            int xNum = 8;//(int)motionEx.trayParm.X_Quantity;
            int yNum = 19;//(int)motionEx.trayParm.Y_Quantity;
            double auto2x = 400;// motionEx.InitPointParam.OutputArm_Auto2_X + motionEx.OffsetParam.OutputArm.OutputArm_Auto2_X;
            double auto2y = 400;// motionEx.InitPointParam.OutputArm_Auto2_Y + motionEx.OffsetParam.OutputArm.OutputArm_Auto2_Y;
            double total = 41 - 10 * 2; //move 580
            int move = (int)(total);
            Auto2XPoint = new double[xNum];
            Auto2YPoint = new double[yNum];

            Auto2XPoint[0] = auto2x - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                Auto2XPoint[i] = Auto2XPoint[i - 1] - xPitch; //累加X轴坐标
            }
            Auto2YPoint[0] = auto2y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
            for (int i = 1; i < yNum; i++)
            {
                Auto2YPoint[i] = Auto2YPoint[i - 1] - yPitch; //累加Y轴坐标
            }
        }

        /// <summary>
        /// Fix1 TrayIC点位初始化
        /// </summary>
        public static void Fix1TrayICPointInit()
        {
            double xStart = 15.45;//(int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double xPitch = 15;//(int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double yStart = 10.8;//(int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            double yPitch = 16.3;//(int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            int xNum = 8;//(int)motionEx.trayParm.X_Quantity;
            int yNum = 19;//(int)motionEx.trayParm.Y_Quantity;
            double fix1x = 300;// motionEx.InitPointParam.OutputArm_Fix1_X + motionEx.OffsetParam.OutputArm.OutputArm_Fix1_X;
            double fix1y = 300;// motionEx.InitPointParam.OutputArm_Fix1_Y + motionEx.OffsetParam.OutputArm.OutputArm_Fix1_Y;
            double total = 41 - 10 * 2; //move 580
            int move = (int)(total);
            Fix1XPoint = new double[xNum];
            Fix1YPoint = new double[yNum];

            Fix1XPoint[0] = fix1x - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                Fix1XPoint[i] = Fix1XPoint[i - 1] - xPitch; //累加X轴坐标
            }
            Fix1YPoint[0] = fix1y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
            for (int i = 1; i < yNum; i++)
            {
                Fix1YPoint[i] = Fix1YPoint[i - 1] - yPitch; //累加Y轴坐标
            }
        }

        /// <summary>
        /// Fix2 TrayIC点位初始化
        /// </summary>
        public static void Fix2TrayICPointInit()
        {
            double xStart = 15.45;//(int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double xPitch = 15;//(int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double yStart = 10.8;//(int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            double yPitch = 16.3;//(int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            int xNum = 8;//(int)motionEx.trayParm.X_Quantity;
            int yNum = 19;//(int)motionEx.trayParm.Y_Quantity;
            double fix2x = 200;// motionEx.InitPointParam.OutputArm_Fix2_X + motionEx.OffsetParam.OutputArm.OutputArm_Fix2_X;
            double fix2y = 200;// motionEx.InitPointParam.OutputArm_Fix2_Y + motionEx.OffsetParam.OutputArm.OutputArm_Fix2_Y;
            double total = 41 - 10 * 2; //move 580
            int move = (int)(total);
            Fix2XPoint = new double[xNum];
            Fix2YPoint = new double[yNum];

            Fix2XPoint[0] = fix2x - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                Fix2XPoint[i] = Fix2XPoint[i - 1] - xPitch; //累加X轴坐标
            }
            Fix2YPoint[0] = fix2y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
            for (int i = 1; i < yNum; i++)
            {
                Fix2YPoint[i] = Fix2YPoint[i - 1] - yPitch; //累加Y轴坐标
            }
        }

        /// <summary>
        /// Fix3 TrayIC点位初始化
        /// </summary>
        public static void Fix3TrayICPointInit()
        {
            double xStart = 15.45;//(int)(motionEx.trayParm.X_Start / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double xPitch = 15;// (int)(motionEx.trayParm.X_Pitch / motionEx.advancedParam.Axis_OutputArm_X_Precision);
            double yStart = 10.8;//(int)(motionEx.trayParm.Y_Start / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            double yPitch = 16.3;//(int)(motionEx.trayParm.Y_Pitch / motionEx.advancedParam.Axis_OutputArm_Y_Precision);
            int xNum = 8;//(int)motionEx.trayParm.X_Quantity;
            int yNum = 19;//(int)motionEx.trayParm.Y_Quantity;
            double fix3x = 100;// motionEx.InitPointParam.OutputArm_Fix3_X + motionEx.OffsetParam.OutputArm.OutputArm_Fix3_X;
            double fix3y = 100;// motionEx.InitPointParam.OutputArm_Fix3_Y + motionEx.OffsetParam.OutputArm.OutputArm_Fix3_Y;
            double total = 41 - 10 * 2; //move 580
            int move = (int)(total);
            Fix3XPoint = new double[xNum];
            Fix3YPoint = new double[yNum];

            Fix3XPoint[0] = fix3x - xStart; //获取左上角第一颗芯片x轴坐标
            for (int i = 1; i < xNum; i++)
            {
                Fix3XPoint[i] = Fix3XPoint[i - 1] - xPitch; //累加X轴坐标
            }
            Fix3YPoint[0] = fix3y - yStart + 2 * move; //以第一根吸嘴为点位 //获取左上角第一颗芯片y轴坐标
            for (int i = 1; i < yNum; i++)
            {
                Fix3YPoint[i] = Fix3YPoint[i - 1] - yPitch; //累加Y轴坐标
            }
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
            double testx1pos = Testx1pos;
            double testx2pos = Testx2pos;
            double testx3pos = Testx3pos;
            double testx4pos = Testx4pos;
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

        public static bool 获取Heat盘坐标(bool ishave, out short xPoint, out short yPoint)
        {
            xPoint = yPoint = 0;
            for (short x = 0; x < HeatXPoint.Length; x++)
            {
                for (short y = 0; y < HeatYPoint.Length; y++)
                {
                    if (HeatDicPoint[$"{x},{y}"] == ishave)
                    {
                        xPoint = x;
                        yPoint = y;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool 获取Auto1盘坐标(bool ishave, out short xPoint, out short yPoint)
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

        public static bool 获取Auto2盘坐标(bool ishave, out short xPoint, out short yPoint)
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

        public static bool 获取Fix1盘坐标(bool ishave, out short xPoint, out short yPoint)
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

        public static bool 获取Fix2盘坐标(bool ishave, out short xPoint, out short yPoint)
        {
            xPoint = yPoint = 0;
            for (short x = 0; x < Fix2XPoint.Length; x++)
            {
                for (short y = 0; y < Fix2YPoint.Length; y++)
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

        public static bool 获取Fix3盘坐标(bool ishave, out short xPoint, out short yPoint)
        {
            xPoint = yPoint = 0;
            for (short x = 0; x < Fix3XPoint.Length; x++)
            {
                for (short y = 0; y < Fix3YPoint.Length; y++)
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
        }

        public static void HeatTrayICMapInit()
        {
            HeatDicPoint = new Dictionary<string, bool>();
            for (int i = 0; i < HeatXPoint.Length; i++)
            {
                for (int j = 0; j < HeatYPoint.Length; j++)
                {
                    HeatDicPoint.Add($"{i},{j}", false);
                    HeatDicBin.Add($"{i},{j}", "");
                }
            }
        }

        public static void Auto1TrayICMapInit()
        {
            Auto1DicPoint = new Dictionary<string, bool>();
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
            //trayTemp = "";
            for (int i = 0; i < Auto2XPoint.Length; i++)
            {
                for (int j = 0; j < Auto2YPoint.Length; j++)
                {
                    Auto2DicPoint[$"{i},{j}"] = false;
                    //trayTemp += Auto2DicBin[$"{i},{j}"] + ",";
                    Auto2DicBin[$"{i},{j}"] = "";
                }
            }
            //trayTemp = trayTemp.TrimEnd(',');
            //if (trayTemp == "") return;
            //string sql = $"insert into o_{textBoxFlowcaedno.Text + "_" + machinestarttime.ToString("yyyyMMdd_HH_mm_ss")}(时间,名称,大小,数据) values('{DateTime.Now}','auto2','{Auto2XPoint.Length.ToString() + "x" + Auto2YPoint.Length.ToString()}','{trayTemp}')";
            //SqlServer.runSqlNonQueryAsync(sqlServerConnectionString, sql);

        }

        public static void Fix1TrayClear()
        {
            //trayTemp = "";
            for (int i = 0; i < Fix1XPoint.Length; i++)
            {
                for (int j = 0; j < Fix1YPoint.Length; j++)
                {
                    Fix1DicPoint[$"{i},{j}"] = false;
                    //trayTemp += Fix1DicBin[$"{i},{j}"] + ",";
                    Fix1DicBin[$"{i},{j}"] = "";
                }
            }
            //trayTemp = trayTemp.TrimEnd(',');
            //if (trayTemp == "") return;
            //string sql = $"insert into o_{textBoxFlowcaedno.Text + "_" + machinestarttime.ToString("yyyyMMdd_HH_mm_ss")}(时间,名称,大小,数据) values('{DateTime.Now}','fix1','{Fix1XPoint.Length.ToString() + "x" + Fix1YPoint.Length.ToString()}','{trayTemp}')";
            //SqlServer.runSqlNonQueryAsync(sqlServerConnectionString, sql);

        }

        public static void Fix2TrayClear()
        {
            //trayTemp = "";
            for (int i = 0; i < Fix2XPoint.Length; i++)
            {
                for (int j = 0; j < Fix2YPoint.Length; j++)
                {
                    Fix2DicPoint[$"{i},{j}"] = false;
                    //trayTemp += Fix2DicBin[$"{i},{j}"] + ",";
                    Fix2DicBin[$"{i},{j}"] = "";
                }
            }
            //trayTemp = trayTemp.TrimEnd(',');
            //if (trayTemp == "") return;
            //string sql = $"insert into o_{textBoxFlowcaedno.Text + "_" + machinestarttime.ToString("yyyyMMdd_HH_mm_ss")}(时间,名称,大小,数据) values('{DateTime.Now}','fix2','{Fix2XPoint.Length.ToString() + "x" + Fix2YPoint.Length.ToString()}','{trayTemp}')";
            //SqlServer.runSqlNonQueryAsync(sqlServerConnectionString, sql);
        }

        public static void Fix3TrayClear()
        {
            //trayTemp = "";
            for (int i = 0; i < Fix3XPoint.Length; i++)
            {
                for (int j = 0; j < Fix3YPoint.Length; j++)
                {
                    Fix3DicPoint[$"{i},{j}"] = false;
                    //trayTemp += Fix3DicBin[$"{i},{j}"] + ",";
                    Fix3DicBin[$"{i},{j}"] = "";
                }
            }
            //trayTemp = trayTemp.TrimEnd(',');
            //if (trayTemp == "") return;
            //string sql = $"insert into o_{textBoxFlowcaedno.Text + "_" + machinestarttime.ToString("yyyyMMdd_HH_mm_ss")}(时间,名称,大小,数据) values('{DateTime.Now}','fix3','{Fix3XPoint.Length.ToString() + "x" + Fix3YPoint.Length.ToString()}','{trayTemp}')";
            //SqlServer.runSqlNonQueryAsync(sqlServerConnectionString, sql);

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

        public static bool 获取Load盘位置(int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Load盘坐标(true, out short xPoint, out short yPoint))
            {
                xPos = LoaderXPoint[xPoint];
                yPos = LoaderYPoint[yPoint] + index * Interval * 16.3;
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
                    Debug.WriteLine($"Loader site{index}:{xPoint},{yPoint + i * Interval}");

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
            if (获取Auto1盘坐标(false, out short xPoint, out short yPoint))
            {
                xPos = Auto1XPoint[xPoint];
                yPos = Auto1YPoint[yPoint] + index * Interval * 16.3; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                return true;
            }
            else return false;
        }

        public static bool 获取Auto2盘位置(int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Auto2盘坐标(false, out short xPoint, out short yPoint))
            {
                xPos = Auto2XPoint[xPoint];
                yPos = Auto2YPoint[yPoint] + index * Interval * 16.3; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                return true;
            }
            else return false;
        }

        public static bool 获取Fix1盘位置(int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Fix1盘坐标(false, out short xPoint, out short yPoint))
            {
                xPos = Fix1XPoint[xPoint];
                yPos = Fix1YPoint[yPoint] + index * Interval * 16.3; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                return true;
            }
            else return false;
        }

        public static bool 获取Fix2盘位置(int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Fix2盘坐标(false, out short xPoint, out short yPoint))
            {
                xPos = Fix2XPoint[xPoint];
                yPos = Fix2YPoint[yPoint] + index * Interval * 16.3; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch;
                return true;
            }
            else return false;
        }

        public static bool 获取Fix3盘位置(int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Fix3盘坐标(false, out short xPoint, out short yPoint))
            {
                xPos = Fix3XPoint[xPoint];
                yPos = Fix3YPoint[yPoint] + index * Interval * 16.3; //坐标位置 + 哪个吸嘴 * 吸料间隔 * yPitch
                return true;
            }
            else return false;
        }

        public static string 获取Load盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 0, out index)) return "获取入料吸嘴索引失败!";
            if (!获取Load盘位置(index, out xPos, out yPos)) return "获取Load盘位置失败!";

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

        public static string 获取Auto1盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 1, out index)) return "获取出料吸嘴索引失败!";
            if (!获取Auto1盘位置(index, out xPos, out yPos)) return "获取Auto1盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Auto1YPoint.Length) break;
                if (Auto1DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1)
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

        public static string 获取Auto2盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 1, out index)) return "获取出料吸嘴索引失败!";
            if (!获取Auto2盘位置(index, out xPos, out yPos)) return "获取Auto2盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Auto2YPoint.Length) break;
                if (Auto2DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1)
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

        public static string 获取Fix1盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 1, out index)) return "获取出料吸嘴索引失败!";
            if (!获取Fix1盘位置(index, out xPos, out yPos)) return "获取Fix1盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Fix1YPoint.Length) break;
                if (Fix1DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1)
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

        public static string 获取Fix2盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 1, out index)) return "获取出料吸嘴索引失败!";
            if (!获取Fix2盘位置(index, out xPos, out yPos)) return "获取Fix2盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Fix2YPoint.Length) break;
                if (Fix2DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1)
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

        public static string 获取Fix3盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 1, out index)) return "获取出料吸嘴索引失败!";
            if (!获取Fix3盘位置(index, out xPos, out yPos)) return "获取Fix3盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Fix3YPoint.Length) break;
                if (Fix3DicPoint[$"{xPoint},{yPoint + i * Interval}"] == true) break;
                if (opens[i + index] == 1)
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

    }
}
