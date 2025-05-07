using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class AxisRunInfo
    {
        public string 轴名称 { get; set; }
        public short 核号 { get; set; }
        public short 轴号 { get; set; }
        public double 最大速度 { get; set; }
        public double 加速度 { get; set;}
        public double 减速度 { get; set;}
        public double 步长mm { get; set; }
        public double 速度百分比 { get; set;}
        public short 平滑时间 { get; set; }
        public double 运行速度
        {
            get { return 最大速度 * 速度百分比 / 100; }
        }
    }
}
