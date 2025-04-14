using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class IOInfo
    {
        /// <summary>
        /// IO模块  0：GNM601 1：gLink200
        /// </summary>
        public string 模块类型 {  get; set; }

        /// <summary>
        /// IO模块输入数量
        /// </summary>
        public short 输入 { get; set; }

        /// <summary>
        /// IO模块输出数量
        /// </summary>
        public short 输出 { get; set; }

        /// <summary>
        /// 核号
        /// </summary>
        public short 核号 { get; set; }

        /// <summary>
        /// 模块号
        /// </summary>
        public short 模块号 { get; set; }

        /// <summary>
        /// 硬件索引
        /// </summary>
        public short 硬件索引 { get; set; }

        /// <summary>
        /// 软件索引
        /// </summary>
        public short 软件索引 { get; set; }

        /// <summary>
        /// IO号  X001/Y001
        /// </summary>
        public string IO编号 { get; set; }

        /// <summary>
        /// IO名称 开始/暂停/安全钮
        /// </summary>
        public string IO名称 { get; set; }

        /// <summary>
        /// IO区域
        /// </summary>
        public string IO区域 { get; set; }
    }
}
