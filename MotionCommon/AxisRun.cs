using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionCommon
{
    public class AxisRun
    {
        public string 轴名称 { get; set; }

        public short 核号 { get; set; }

        public short 轴号 { get; set; }

        public int 目标位置 { get; set; }

        public double 目标速度 { get; set; }

        public double 目标加速度 { get; set; }

        public double 目标减速度 { get; set; }

        public short 平滑时间 { get; set; } //范围[0,50]

        public double Jog平滑时间 { get; set; }

        public double 实际位置 { get; set; }

        public double 实时速度 { get; set; }

        public double 实时扭矩 { get; set; }

        public bool 轴运行中 { get; set; }

        public int 轴状态 { get; set; }

        public bool 绝对定位完成信号 { get; set; }

        public bool 相对定位完成信号 { get; set; }

        public bool 达到目标速度值信号 { get; set; }

        public bool 达到目标转矩值信号 { get; set; }

        public bool 同步周期速度值 { get; set; }

        public bool 点动中信号 { get; set; }

        public bool 位置设置完成信号 { get; set; }

        public bool 停止完成信号 { get; set; }

        public bool 轴报警汇总 { get; set; }

        public bool 原点 { get; set; }

        public bool 正极限 { get; set; }

        public bool 负极限 { get; set; }

        public bool[] 定位到位 { get; set; }

        public bool 使能OK { get; set; }

        public bool 准备好 { get; set; }

        public double 定位目标值 { get; set; }

        public int 运动方向 { get; set; }

        public int 停止类型 { get; set; } //0:平滑停止 1:急停停止 2:点动停止

        public int 跟随误差 { get; set; } = 500;

        public bool 跟随误差检测 { get; set; } = false;

        public int 二段速方式 { get; set; } = 0; //0：无  1：终点慢 2：起点慢 3:终点和起点都慢

        public int 暂停类型 { get; set; } //0:允许暂停 1:不允许暂停

        public double 二段速结束时减速速度 { get; set; } = 0.1;

        public int 二段速结束时减速距离 { get; set; }

        public double 二段速启动结束后恢复速度 { get; set; } = 0.1;

        public int 二段速启动时减速距离 { get; set; }

        public bool 二段速统一更改 { get; set; } //出入料Z吸嘴专用

        public int 轴停后等待超时时间 { get; set; } = 2000; //2S

        public double 轴动作时间 { get; set; }

        public int 轴动作超时时间 { get; set; }

    }
}
