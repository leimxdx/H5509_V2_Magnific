using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static GTN.mc;

namespace H5509_V2
{
    public class MotionEx
    {
        //创建一个Motion对象
        public Motion Motion = new Motion();

        //创建一个静态对象保存当前的实例
        private static volatile MotionEx instance;

        //定义一个锁
        private static readonly object objlock = new object();

        //创建一个私有的构造方法
        private MotionEx() { }

        public static MotionEx GetInstance()
        {
            if (instance == null)
            {
                lock (objlock)
                {
                    if (instance == null)
                    {
                        instance = new MotionEx();
                    }
                }
            }
            return instance;
        }

        #region 轴回零


        //public string Wait

        public string WaitAxisHome(AxisHome axisHome)
        {
            string result = Motion.CommonInitedValidate();

            if(result != "Success") return result;

            try
            {
                THomeStatus tHomeSts;
                do
                {
                    short error = GTN_GetHomeStatus(axisHome.核号, axisHome.轴号, out tHomeSts);
                    Motion.ErrorHandler($"GTN_GetHomeStatus({axisHome.核号}, {axisHome.轴号}, out tHomeSts)", error);

                }
                while (tHomeSts.run == 1);

                if (tHomeSts.error != HOME_ERROR_NONE) //未发生错误
                {
                    if (tHomeSts.error == HOME_ERROR_NOT_TRAP_MODE) return "执行Smart Home回原点的轴不是处于点位运动模式";
                    else if (tHomeSts.error == HOME_ERROR_DISABLE) return "执行Smart Home回原点的轴未使能";
                    else if (tHomeSts.error == HOME_ERROR_ALARM) return "执行Smart Home回原点的轴驱动报警";
                    else if (tHomeSts.error == HOME_ERROR_STOP) return "未完成回原点，轴停止运动";
                    else if (tHomeSts.error == HOME_ERROR_STAGE) return "回原点阶段错误";
                    else if (tHomeSts.error == HOME_ERROR_HOME_MODE) return "模式错误，如已经启动Smart Home，勿重复调用回原点指令";
                    else if (tHomeSts.error == HOME_ERROR_SET_CAPTURE_HOME) return "设置Home捕获模式失败";
                    else if (tHomeSts.error == HOME_ERROR_NO_HOME) return "未找到Home";
                    else if (tHomeSts.error == HOME_ERROR_SET_CAPTURE_INDEX) return "设置Index捕获模式失败";
                    else if (tHomeSts.error == HOME_ERROR_NO_INDEX) return "未找到Index";
                    else return "回零异常";
                }
            }
            catch (Exception ex) 
            {
                return ex.Message;
            }

            return "Success";
        }

        public Task<string> InputAxisZHome()
        {
            List<AxisHome> axisHomes = new List<AxisHome>();

            axisHomes.Add(new AxisHome() { 核号 = 1, 轴号 = 1, 模式 = 11, Home搜索方向 = -1, 捕获触发沿 = 1, 搜索Home速度 = 10, 加速度 = 10, 减速度 = 0.2, 平滑时间 = 0, });
            axisHomes.Add(new AxisHome() { 核号 = 1, 轴号 = 1, 模式 = 11, Home搜索方向 = -1, 捕获触发沿 = 1, 搜索Home速度 = 10, 加速度 = 10, 减速度 = 0.2, 平滑时间 = 0, });
            axisHomes.Add(new AxisHome() { 核号 = 1, 轴号 = 1, 模式 = 11, Home搜索方向 = -1, 捕获触发沿 = 1, 搜索Home速度 = 10, 加速度 = 10, 减速度 = 0.2, 平滑时间 = 0, });
            axisHomes.Add(new AxisHome() { 核号 = 1, 轴号 = 1, 模式 = 11, Home搜索方向 = -1, 捕获触发沿 = 1, 搜索Home速度 = 10, 加速度 = 10, 减速度 = 0.2, 平滑时间 = 0, });

            string result = Motion.MultiaxisHome(axisHomes);

            Task<string> task = Task.Run(() =>
            {
                if (result == "Success")
                {
                    //等待回零完成
                }
                return result;
            });

            return task;
        }

        public Task<string> OutputAxisZHome()
        {
            List<AxisHome> axisHomes = new List<AxisHome>();

            axisHomes.Add(new AxisHome() { 核号 = 1, 轴号 = 1, 模式 = 11, Home搜索方向 = -1, 捕获触发沿 = 1, 搜索Home速度 = 10, 加速度 = 10, 减速度 = 0.2, 平滑时间 = 0, });
            axisHomes.Add(new AxisHome() { 核号 = 1, 轴号 = 1, 模式 = 11, Home搜索方向 = -1, 捕获触发沿 = 1, 搜索Home速度 = 10, 加速度 = 10, 减速度 = 0.2, 平滑时间 = 0, });
            axisHomes.Add(new AxisHome() { 核号 = 1, 轴号 = 1, 模式 = 11, Home搜索方向 = -1, 捕获触发沿 = 1, 搜索Home速度 = 10, 加速度 = 10, 减速度 = 0.2, 平滑时间 = 0, });
            axisHomes.Add(new AxisHome() { 核号 = 1, 轴号 = 1, 模式 = 11, Home搜索方向 = -1, 捕获触发沿 = 1, 搜索Home速度 = 10, 加速度 = 10, 减速度 = 0.2, 平滑时间 = 0, });

            string result = Motion.MultiaxisHome(axisHomes);

            Task<string> task = Task.Run(() =>
            {
                if (result == "Success")
                {
                    //等待回零完成
                }
                return result;
            });

            return task;
        }

        //public Task<string> InputAxisXHome()
        //{
        //    List<AxisHome> axisHomes = new List<AxisHome>()
        //    {

        //    };
        //    string result = Motion.MultiaxisHome(axisHomes);
        //}

        #endregion

    }
}
