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
        public string IoModule {  get; set; }

        /// <summary>
        /// 核号
        /// </summary>
        public short Core { get; set; }

        /// <summary>
        /// 模块号
        /// </summary>
        public short ModuleNum { get; set; }

        /// <summary>
        /// IO索引
        /// </summary>
        public short IoIndex { get; set; }

        /// <summary>
        /// IO号  X001/Y001
        /// </summary>
        public string IoNum { get; set; }

        /// <summary>
        /// IO名称 开始/暂停/安全钮
        /// </summary>
        public string IoName { get; set; }

        /// <summary>
        /// IO区域
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 当前IO信号
        /// </summary>
        public bool Value { get; set; }

        //public void duqu
    }
}
