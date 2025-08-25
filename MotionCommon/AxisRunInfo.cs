using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionCommon
{
    public class AxisRunInfo
    {
        public string 轴名称 { get; set; }

        public short 核号 { get; set; }

        public short 轴号 { get; set; }

        public double 最大速度 { get; set; }

        public double 加速度 { get; set; }

        public double 减速度 { get; set; }

        public double 步长mm { get; set; }

        public double 速度百分比 { get; set; }

        public short 平滑时间 { get; set; }

        public double 运行速度
        {
            get { return 最大速度 * 速度百分比 / 100; }
        }
        public int 停止类型 { get; set; }

        public int 跟随误差 { get; set; }

        public bool 跟随误差检测 { get; set; }

        public int 轴二段速方式 { get; set; }

        public double 二段速启动时减速速度 { get; set; }

        public double 启动时减速速度
        {
            get { return 运行速度 * 二段速启动时减速速度; }
        }
        public int 二段速启动时减速距离 { get; set; }

        public double 二段速结束时减速速度 { get; set; }

        public double 结束时减速速度
        {
            get { return 运行速度 * 二段速结束时减速速度; }
        }

        public int 二段速结束时减速距离 { get; set; }

        public int 轴停后等待超时时间 { get; set; }

    }
}
