using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class TesterIFParam
    {
        public string 测试机名称 { get; set; }

        public int Dout连续不良个数 { get; set; }

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
    }
}
