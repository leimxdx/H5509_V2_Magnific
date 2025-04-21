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

        public static bool IsPause = false; //停止

        public static bool IsForecePause = false; //强制停止

        public static int 气缸动作超时时间 = 3000; //ms

        public static Communication Com = new Communication();

        public static List<bool[][]> SiteTestOpen = new List<bool[][]>();

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

        public static string Auto1 { get; set; }
        public static string Auto2 { get; set; }
        public static string Fix1 { get; set; }
        public static string Fix2 { get; set; }
        public static string Fix3 { get; set; }

        public static List<bool[][]> Sitestatus = new List<bool[][]>();
        public static List<bool[][]> SiteSave = new List<bool[][]>();

        public static ConcurrentQueue<string[]> BinSaveQueue = new ConcurrentQueue<string[]>();
        public static ConcurrentQueue<string> SiteSaveQueue = new ConcurrentQueue<string>();

        public static short Interval = 2;

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

        public static bool 获取Load盘位置(out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Load盘坐标(true, out short xPoint, out short yPoint))
            {
                xPos = LoaderXPoint[xPoint];
                yPos = LoaderYPoint[yPoint];
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

        public static bool 获取Auto1盘位置(out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Auto1盘坐标(true, out short xPoint, out short yPoint))
            {
                xPos = Auto1XPoint[xPoint];
                yPos = Auto1YPoint[yPoint];
                return true;
            }
            else return false;
        }
        public static bool 获取Auto2盘位置(out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Auto2盘坐标(true, out short xPoint, out short yPoint))
            {
                xPos = Auto2XPoint[xPoint];
                yPos = Auto2YPoint[yPoint];
                return true;
            }
            else return false;
        }
        public static bool 获取Fix1盘位置(out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Fix1盘坐标(true, out short xPoint, out short yPoint))
            {
                xPos = Fix1XPoint[xPoint];
                yPos = Fix1YPoint[yPoint];
                return true;
            }
            else return false;
        }
        public static bool 获取Fix2盘位置(out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Fix2盘坐标(true, out short xPoint, out short yPoint))
            {
                xPos = Fix2XPoint[xPoint];
                yPos = Fix2YPoint[yPoint];
                return true;
            }
            else return false;
        }
        public static bool 获取Fix3盘位置(out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            if (获取Fix3盘坐标(true, out short xPoint, out short yPoint))
            {
                xPos = Fix3XPoint[xPoint];
                yPos = Fix3YPoint[yPoint];
                return true;
            }
            else return false;
        }

        public static string 获取Load盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 0, out index)) return "获取入料吸嘴索引失败!";
            if (!获取Load盘位置(out xPos, out yPos)) return "获取Load盘位置失败!";

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
            if (!获取Auto1盘位置(out xPos, out yPos)) return "获取Auto1盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Auto1YPoint.Length) break;
                if (Auto1DicPoint[$"{xPoint},{yPoint + i * Interval}"] == false) break;
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
        public static string 获取Auto2盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 1, out index)) return "获取出料吸嘴索引失败!";
            if (!获取Auto2盘位置(out xPos, out yPos)) return "获取Auto2盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Auto2YPoint.Length) break;
                if (Auto2DicPoint[$"{xPoint},{yPoint + i * Interval}"] == false) break;
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
        public static string 获取Fix1盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 1, out index)) return "获取出料吸嘴索引失败!";
            if (!获取Fix1盘位置(out xPos, out yPos)) return "获取Fix1盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Fix1YPoint.Length) break;
                if (Fix1DicPoint[$"{xPoint},{yPoint + i * Interval}"] == false) break;
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
        public static string 获取Fix2盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 1, out index)) return "获取出料吸嘴索引失败!";
            if (!获取Fix2盘位置(out xPos, out yPos)) return "获取Fix2盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Fix2YPoint.Length) break;
                if (Fix2DicPoint[$"{xPoint},{yPoint + i * Interval}"] == false) break;
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
        public static string 获取Fix3盘Open(int[] opens, short xPoint, short yPoint, out bool[] lsopen, out int index, out double xPos, out double yPos)
        {
            xPos = yPos = 0;
            lsopen = new bool[opens.Length];

            if (!获取吸嘴索引(opens, 1, out index)) return "获取出料吸嘴索引失败!";
            if (!获取Fix3盘位置(out xPos, out yPos)) return "获取Fix3盘位置失败!";

            for (int i = 0; i < opens.Length; i++)
            {
                if (index + i == opens.Length) break;
                if (yPoint + i * Interval >= Fix3YPoint.Length) break;
                if (Fix3DicPoint[$"{xPoint},{yPoint + i * Interval}"] == false) break;
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

    }
}
