using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H5509_V2
{
    public class log
    {
        public DateTime Time { get; set; }
        public string LogLevel { get; set; }
        public string LogType { get; set; }
        public string LogContent { get; set; }
    }

    public class LogManager
    {
        public string Msg { get; set; }

        public short TxtLogtype { get; set; }

        public string ExcelType { get; set; }

        public string ExcelLevel { get; set; }
    }
}
