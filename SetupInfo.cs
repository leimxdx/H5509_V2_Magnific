using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    /// <summary>
    /// 设置信息
    /// </summary>
    public class SetupInfo
    {
        public string 设置信息名称 { get; set; }

        public TrayInfo Tray盘信息 { get; set; } = new TrayInfo();

        public OffsetPoint OffsetPoint { get; set; } = new OffsetPoint();
    }
}
