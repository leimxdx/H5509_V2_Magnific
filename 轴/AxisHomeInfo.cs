using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class AxisHomeInfo
    {
        public short 核号 { get; set; }

        public short 轴号 { get; set; }

        public string 轴名称 { get; set; }

        /// <summary>
        /// 回原点模式, 11：限位+Home回原点, 20：Home回原点
        /// </summary>
        public short 模式 { get; set; }

        /// <summary>
        /// 设置启动搜索原点时的运动方向：非正数-负方向，正整数-正方向
        /// </summary>
        public short 回零启动方向 { get; set; }

        /// <summary>
        /// 设置搜索Index的运动方向：非正数-负方向，正整数-正方向
        /// </summary>
        public short Index搜索方向 { get; set; }

        /// <summary>
        /// 设置捕获沿：0-下降沿，非0值-上升沿
        /// </summary>
        public short 捕获触发沿 { get; set; }

        /// <summary>
        /// 用于设置触发器：触发器需要和轴号对应，触发器用于实现高速硬件捕获，默认设置为-1即可，控制器自动将触发器和轴号对应
        /// </summary>
        public short 捕获触发器 { get; set; } = -1;

        /// <summary>
        /// 搜索Home速度（单位：pulse/ms）
        /// </summary>
        public double 搜索速度 { get; set; }

        public double 定位速度 { get; set; }

        /// <summary>
        /// 搜索Index速度（单位：pulse/ms）
        /// </summary>
        public double 搜索Index速度 { get; set; }

        /// <summary>
        /// 回原点运动的加速度（单位：pulse/ms^2）
        /// </summary>
        public double 加速度 { get; set; }

        /// <summary>
        /// 回原点运动的减速度（单位：pulse/ms^2）
        /// </summary>
        public double 减速度 { get; set; }

        /// <summary>
        /// 回原点运动的平滑时间：取值[0,50]，具体含义与GTS系列控制器点位运动相似
        /// </summary>
        public short 平滑时间 { get; set; }

        /// <summary>
        /// 最终停止的位置相对于原点的偏移量
        /// </summary>
        public int 零点偏置 { get; set; }

        /// <summary>
        /// Home最大搜索距离，0表示不限制搜索距离，默认为805306368或-805306368
        /// </summary>
        public int Home最大搜索距离 { get; set; } = 0;

        /// <summary>
        /// Index最大搜索距离，0表示不限制搜索距离，默认为805306368或-805306368
        /// </summary>
        public int Index最大搜索距离 { get; set; } = 0;

        /// <summary>
        /// 反方向离开限位的脱离步长，当移动一次设定的步长，仍未脱离限位时，会再次移动设定的步长，直到脱离限位
        /// </summary>
        public int 反向脱离步长 { get; set; } = 1000;
    }
}
