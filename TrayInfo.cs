using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class TrayInfo
    {
        public string TrayName { get; set; }
        /// <summary>
        /// X数量
        /// </summary>
        public int X_Num { get; set; }
        /// <summary>
        /// Y数量
        /// </summary>
        public int Y_Num { get; set; }
        /// <summary>
        /// 左侧边缘到第一颗料的距离
        /// </summary>
        public double X_Start { get; set; }
        /// <summary>
        /// 上方边缘到第一个料的距离
        /// </summary>
        public double Y_Start { get; set; }
        /// <summary>
        /// X方向芯片间隔
        /// </summary>
        public double X_Pitch { get; set; }
        /// <summary>
        /// y方向芯片间隔
        /// </summary>
        public double Y_Pitch { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public double Length { get; set; }
        /// <summary>
        /// 厚度
        /// </summary>
        public double Height { get; set; }
    }
}
