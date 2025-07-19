using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class ControlPerm
    {
        public string 控件名 { get; set; }

        public string 中文名 { get; set; }

        public string 权限 { get; set; }

        public string 权限描述 
        {
            get 
            {
                if (权限 == "Operator") return "#ABCD#";
                else if (权限 == "Engineer") return "#BCD#";
                else if (权限 == "Administrator") return "#CD#";
                else if (权限 == "Supplier") return "#D#";
                else return "#D#";
            }
        }

        public string 显示 { get; set; }
        public string 状态 { get; set; }
    }
}
