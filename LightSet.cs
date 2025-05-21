using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class LightSet
    {
        public string 设备状态 { get; set; }
        public bool 灯颜色显示 { get; set; }
        public int 绿色显示方式 { get; set; }
        public bool 黄灯颜色显示 { get; set; }
        public int 黄灯显示方式 { get; set; }
        public bool 红灯颜色显示 { get; set; }
        public int 红灯显示方式 { get; set; }
        public bool 音乐提示 { get; set; }
        public string 报警音乐 { get; set; }
    }
}
