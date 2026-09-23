using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TLicense;

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
            // 必须在创建任何窗体/控件之前调用
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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

            // 授权：未授权/过期也允许启动（否则无法登录超级管理员去激活），
            // 但 LicenseValid=false 会挡住“开始运行”，并触发“软件未授权”报警。
            初始化授权();

            Application.Run(new ForMain());
        }

        /// <summary>
        /// 初始化授权状态（不阻塞启动）：
        /// 校验通过 → 允许运行；不通过 → 只禁止运行，界面照常打开。
        /// </summary>
        private static void 初始化授权()
        {
            try
            {
                // 授权配置（必须在第一次访问 LicenseService.Current 之前）
                // 已用授权码台账：默认三处保持不动（ProgramData / LocalAppData / 注册表），
                // 再追加第 4 处放到软件根目录，并设置隐藏属性。
                LicenseOptions options = LicenseOptions.CreateDefault();
                options.UsedCodeFileName = "used.dat";
                options.UsedCodeHidden = true;
                options.UsedCodeExtraFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "used.dat");
                options.UsedCodeExtraHidden = true;
                LicenseService.Configure(options);

                LicenseService service = LicenseService.Current;

                APPmaster.LicenseValid = service.Refresh().IsValid;

                // 后台每分钟复核；失效时关闸并报警（系统异常-软件未授权）
                service.LicenseInvalidated += (s, result) =>
                {
                    APPmaster.LicenseValid = false;
                    try { APPmaster.form_alarm?.RaiseLicenseAlarm(); } catch { }
                };

                service.StartMonitor(TimeSpan.FromMinutes(1));
            }
            catch (Exception ex)
            {
                APPmaster.LicenseValid = false;
                Debug.WriteLine("授权初始化失败：" + ex.Message);
            }
        }
    }
}
