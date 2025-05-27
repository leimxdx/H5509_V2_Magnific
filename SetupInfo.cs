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
        public string 档案名称 { get; set; }

        public string 温度模式 { get; set; }

        public TrayInfo Tray盘信息 { get; set; } = new TrayInfo();

        public HeatInfo Heat盘信息 { get; set; } = new HeatInfo();

        public OffsetPoint OffsetPoint { get; set; } = new OffsetPoint();

        public SiteStatus 开site信息 { get; set; } = new SiteStatus();

        public BinParam 分BIN信息 { get; set; } = new BinParam();

        public TesterIFParam 测试机信息 { get; set; } = new TesterIFParam();
    }
}
