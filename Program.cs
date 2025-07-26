using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H5509_V2
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            int processCount = 0;
            Process[] pa = Process.GetProcesses();//获取当前进程数组。
            var ProcessName = Process.GetCurrentProcess().ProcessName;

            foreach (Process PTest in pa)
            {
                if (PTest.ProcessName == ProcessName)
                {
                    processCount += 1;
                }
            }
            if (processCount > 1)
            {
                //如果程序已经运行，则给出提示。并退出本进程。
                System.Windows.Forms.MessageBox.Show(Process.GetCurrentProcess().ProcessName + "程序已经在运行！", "退出程序", MessageBoxButtons.OK, MessageBoxIcon.Error);//可能你不需要弹出窗口，在这里可以屏蔽掉
                return; //Exit;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ForMain());
        }
    }
}
