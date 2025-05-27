using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class HeatInfo
    {
        public string HeatName { get; set; }
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

        public double 设定温度 { get; set; }
        public double 波动温度 { get; set; }
        public int 波动延时报警 { get; set; }
        public int 达温波动报警 { get; set; }

        public double 波动温度上限
        {
            get { return 设定温度 + 波动温度; }
        }
        public double 波动温度下限
        {
            get { return 设定温度 - 波动温度; }
        }

        public double 报警温度 { get; set; }
        public double 超温温度 { get; set; }
        public int 升温超时报警 { get; set; }
        public string 达温标准 { get; set; }
        public string 探头类型 { get; set; }
        public double 温控器温度补偿 { get; set; }
        public double Site1温度补偿 { get; set; }
        public double Site2温度补偿 { get; set; }
        public double Site3温度补偿 { get; set; }
        public double Site4温度补偿 { get; set; }
        public double Site5温度补偿 { get; set; }
        public double Site6温度补偿 { get; set; }
        public double Site7温度补偿 { get; set; }
        public double Site8温度补偿 { get; set; }
        public double Site9温度补偿 { get; set; }
        public double Site10温度补偿 { get; set; }
        public double Site11温度补偿 { get; set; }
        public double Site12温度补偿 { get; set; }
        public double Site13温度补偿 { get; set; }
        public double Site14温度补偿 { get; set; }
        public double Site15温度补偿 { get; set; }
        public double Site16温度补偿 { get; set; }

        public double Site温度补偿(int site)
        {
            switch (site)
            {
                case 0: return Site1温度补偿;
                case 1: return Site2温度补偿;
                case 2: return Site3温度补偿;
                case 3: return Site4温度补偿;
                case 4: return Site5温度补偿;
                case 5: return Site6温度补偿;
                case 6: return Site7温度补偿;
                case 7: return Site8温度补偿;
                case 8: return Site9温度补偿;
                case 9: return Site10温度补偿;
                case 10: return Site11温度补偿;
                case 11: return Site12温度补偿;
                case 12: return Site13温度补偿;
                case 13: return Site14温度补偿;
                case 14: return Site15温度补偿;
                case 15: return Site16温度补偿;
            }
            return 0;
        }
    }
}
