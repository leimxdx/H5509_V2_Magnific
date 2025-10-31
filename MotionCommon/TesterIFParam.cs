using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionCommon
{
    public class TesterIFParam
    {
        public string 测试机名称 { get; set; }

        public int Dout连续不良个数 { get; set; }

        public int Dout连续良品个数 { get; set; }

        public bool Dout连续不良自动关闭 { get; set; }

        public bool Dout连续不良报警 { get; set; }

        public int Dut累计不良报警个数 { get; set; }

        public bool Dut累计不良报警使能 { get; set; }

        public double 总良率报警百分比 { get; set; }

        public int 总良率报警基数 { get; set; }

        public int 总良率报警间隔 { get; set; }

        public bool 总良率报警使能 { get; set; }

        public int socket使用次数 { get; set; }

        public int 测试超时时间 { get; set; }

        public int timeout超时时间 { get; set; }

        public bool 超时发送timeout { get; set; }

        //public string 串口号 { get; set; }

        //public int 波特率 { get; set; }

        //public Parity 校验位 { get; set; }

        //public int 数据位 { get; set; }

        //public StopBits 停止位 { get; set; }

        public int 入料手臂吸料超时时间 { get; set; }

        public int 出料手臂吸料超时时间 { get; set; }

        public int 测试手臂吸料超时时间 { get; set; }

        public int 入料手臂吹料时间 { get; set; }

        public int 出料手臂吹料时间 { get; set; }

        public int 测试手臂吹料时间 { get; set; }

        public int 发送开始信号延时 { get; set; }

        public int 接收测试信号上抬延时 { get; set; }

        public int 离线模式测试等待时间 { get; set; }

        public bool 入料吸提前开真空 { get; set; }

        public bool 出料吸提前开真空 { get; set; }

        public bool 测试吸提前开真空 { get; set; }

        public bool Auto侧敲使能 { get; set; }

        public bool 前侧同时敲击 { get; set; }

        public int 放置多少颗敲击 { get; set; }

        public int Auto侧敲次数 { get; set; }

        public int 侧敲间隔 { get; set; }

        public bool 放料时允许侧敲 { get; set; }

        public bool 侧敲时允许放料 { get; set; }

        public bool Auto换盘时出料Z回零 { get; set; }

        public string 通讯名称 { get; set; }

        public bool 测试超时自动关闭Dut { get; set; }

        public bool 接收超时Bin自动关闭Dut { get; set; }

        public string 超时Bin字符串显示 { get; set; }

        public string 离线模式IC放置位置 { get; set; }
    }
}
