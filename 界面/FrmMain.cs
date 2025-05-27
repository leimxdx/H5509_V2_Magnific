using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using H5509_alarm;
using Sunny.UI;
using Sunny.UI.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static GTN.mc;
using static H5509_V2.APPmaster;

namespace H5509_V2
{
    public partial class ForMain : UIForm
    {

        #region     窗体操作
        /// <summary>
        /// 防止控件过多产生闪烁现象
        /// </summary>
        private void SetControlStyle()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // 双缓冲
            this.UpdateStyles();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        private void SetTablayoutDoubleBuffer()
        {
            typeof(Panel)
               .GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
               .SetValue(MainPanel, true, null);
        }
        #endregion

        private _main main;
        private _setting setting;
        private _offset offset;
        private _speed speed;
        private _IO io;
        private _log log;
        private Handler handler;
        private MotionEx motionEx;

        public ForMain()
        {
            InitializeComponent();

            log = new _log();

            string result = Init_APPmaster();
            if (result != "Success")
            {
                log.AddLog("error", "run", "软件加载失败,请关闭软件重开!" + result);
                MessageBox.Show("软件加载失败,请关闭软件重开!", result);
            }

        }
        private void ForMain_Load(object sender, EventArgs e)
        {
            string result = "";
            main = new _main();
            setting = new _setting();
            offset = new _offset();
            speed = new _speed();
            io = new _IO();
            handler = new Handler();
            motionEx = MotionEx.GetInstance();


            setting.刷新分BIN信息 = main.刷新分BIN信息;
            setting.刷新温度信息 = main.刷新温度显示;
            handler.刷新批次信息 = main.刷新批次信息;
            setting.handler = handler;
            main.handler = handler;

            dTMHelper = new DTMHelper(TCPIP, TCPPort);//"192.168.1.5", 502

            if (!dTMHelper.Connect())
            {
                TCPConnect = false;
                MessageBox.Show("温控器连接失败!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                TCPConnect = true;
                main.刷新温度显示();
                //result = dTMHelper.ReadSv(2, 0, out short[] value);
                //if ((value[0] / 10) != 设定温度)
                //{
                //    result = dTMHelper.WriteSv(2, 0, 设定温度);
                //    if (result != "Success")
                //    {
                //        MessageBox.Show($"温度设置为【{value}】℃失败!\n详细信息：" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    }
                //}
                //else MessageBox.Show($"读取温控器温度失败!\n详细信息：" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //防止控件过多产生闪烁现象
            SetControlStyle();

            SetTablayoutDoubleBuffer();

            changeMainPanel(main);

            handler.参数初始化();

            result = motionEx.GSNInitCard();
            if (result != "Success")
            {
                MessageBox.Show("系统加载失败！详细信息：\n" + result, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            result = Init_FrMain();
            if (result != "Success")
            {
                log.AddLog("error", "run", "软件加载失败,请关闭软件重开!" + result);
                MessageBox.Show("软件加载失败,请关闭软件重开!", result);
            }

            #region 调试参数

            //温度模式 = 0;
            ////结批标记 = true;
            //放料目标值();
            //飞梭前入料IC = new bool[2][] { new bool[] { true, true, true, true }, new bool[] { true, true, true, true } };
            //HeatDicTimer["5,11"].Status = 2;
            //HeatDicTimer["5,10"].Status = 2;
            //HeatDicTimer["5,9"].Status = 2;
            //HeatDicPoint["5,11"] = true;
            //HeatDicPoint["5,10"] = true;
            //HeatDicPoint["5,9"] = true;
            //handler.debug();

            #endregion

            main.canVisable(this);

            //TimeSpan time = new TimeSpan(0,11,31);
            //int x = (int)(449 / (time.TotalMinutes / 60));
            //handler.DebugIO(out string[] iovalues);
        }

        /// <summary>
        /// MainPanel界面切换
        /// </summary>
        /// <param name="uc"></param>
        private void changeMainPanel(UserControl uc)
        {
            if (uc == io)
            {
                io.IOUpdateTimer.Start();
            }
            else
            {
                io.IOUpdateTimer.Stop();
            }

            MainPanel.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            MainPanel.Controls.Add(uc);
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否退出软件?", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                //writeDataHelper.PrintLog("软件退出");
                退出标记 = true; //让报警弹窗也退出，否则主程序退出不了
                //gg.CloseCard(); //关闭轴卡
                Application.Exit(); //退出软件
            }
        }

        private void button_main_Click(object sender, EventArgs e)
        {
            changeMainPanel(main);
        }

        private void button_setting_Click(object sender, EventArgs e)
        {
            changeMainPanel(setting);
            setting.刷新设置与维护界面();
        }

        private void button_offset_Click(object sender, EventArgs e)
        {
            changeMainPanel(offset);
            offset.刷新Offset();
        }

        private void button_speed_Click(object sender, EventArgs e)
        {
            changeMainPanel(speed);
            speed.刷新速度值();
        }

        private void button_io_Click(object sender, EventArgs e)
        {
            changeMainPanel(io);
        }

        private void button_log_Click(object sender, EventArgs e)
        {
            changeMainPanel(log);
        }

        private void 屏幕刷新()
        {
            while (true)
            {
                try
                {
                    if (退出标记) break;

                    this.Invoke(new Action(() =>
                    {
                        if (listcode.Count != 0)
                        {
                            form_alarm.Show();
                            for (int i = 0; i < listcode.Count; i++)
                            {
                                form_alarm.code(listcode[i], true);
                                listcode.Remove(listcode[i]);
                                //alarm_count++;
                            }
                        }
                    }));

                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("屏幕刷新异常!" + ex.Message);
                    Thread.Sleep(100);
                }
            }
        }

        private void 温度读取()
        {
            while (true)
            {
                try
                {
                    if (TCPConnect)
                    {
                        string result = dTMHelper.ReadPv(0, out ushort[] value);
                        if (result == "Success")
                        {
                            for (int i = 0; i < value.Length; i++)
                            {
                                if (value[i] != 32770)
                                {
                                    SiteTemp[i] = (double)value[i] / 10 + 加热盘.Site温度补偿(i);
                                }
                                else SiteTemp[i] = value[i];
                            }
                        }
                        result = dTMHelper.ReadPv(1, out ushort[] value2);
                        if (result == "Success")
                        {
                            for (int i = 0; i < value2.Length; i++)
                            {
                                if (value2[i] != 32770)
                                {
                                    SiteTemp[i + 8] = (double)value2[i] / 10 + 加热盘.Site温度补偿(i + 8);
                                }
                                else SiteTemp[i + 8] = value2[i];
                            }
                        }
                        result = dTMHelper.ReadPv(2, 0, out ushort[] value3);
                        if (result == "Success")
                        {
                            if (value3[0] != 32770)
                            {
                                HeatTrayTemp = (double)value3[0] / 10 + 加热盘.温控器温度补偿;
                            }
                            else HeatTrayTemp = value3[0];
                        }

                        if (设备状态 == 1 && 温度模式 == "高温" && !IsPause)
                        {
                            double tempwavemax = 加热盘.波动温度上限;
                            double tempwavemin = 加热盘.波动温度下限;
                            if (!温控达温)
                            {
                                bool[] tempopen = 检测温度(tempwavemax, tempwavemin);
                                if (!tempopen.Contains(false)) //有false说明还有Site没有达温
                                {
                                    温控达温 = true;
                                    达温时间 = DateTime.Now;
                                    Debug.WriteLine("加热盘已达温");
                                    Debug.WriteLine("达温时间：" + 达温时间.ToString("yyyy-MM-dd HH:mm:ss"));
                                    Debug.WriteLine("升温时间：" + 升温时间.ToString(@"hh\:mm\:ss"));
                                }
                            }
                            else
                            {
                                //bool[] tempopen = 检测温度(tempwavemax, tempwavemin);
                                //if (tempopen.Contains(false)) //有false说明还有Site超出波动范围
                                //{
                                //    Debug.WriteLine("超波动温度");
                                //    ProgramPause = true;
                                //    IsPause = true;
                                //    for (int i = 0; i < 16; i++)
                                //    {
                                //        if (SiteTemp[i] != 32770)
                                //        {
                                //            if (SiteTemp[i] > tempwavemax && SiteTemp[i] < tempwavemin)
                                //            {
                                //                Debug.WriteLine($"Site{i + 1}:【{SiteTemp[i]}】℃超波动温度");
                                //            }
                                //        }
                                //    }
                                //    if (HeatTrayTemp != 32770)
                                //    {
                                //        if (HeatTrayTemp > tempwavemax && HeatTrayTemp < tempwavemin)
                                //        {
                                //            Debug.WriteLine($"温控器:【{HeatTrayTemp}】℃超波动温度");
                                //        }
                                //    }
                                //}
                            }
                        }
                    }

                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("温度读取异常!" + ex.Message);
                    Thread.Sleep(100);
                }
            }
        }

        private string Init_FrMain()
        {
            StartTask = Task.Run(() => //通过线程去开启线程，防止卡顿
            {
                cts = new CancellationTokenSource();

                Task.Run(() =>
                {
                    handler.系统按钮检测流程();
                });
                Task.Run(() =>
                {
                    屏幕刷新();
                });
                Task.Run((() =>
                {
                    温度读取();
                }));
                Task.Run(() =>
                {
                    handler.初始化流程();
                });
                Task.Run(() =>
                {
                    handler.结批流程();
                });
                //Task.Run(() =>
                //{
                //    handler.入料手臂吸放料流程();
                //});
                Task.Run(() =>
                {
                    handler.入料手臂吸放料流程();
                });
                Task.Run(() =>
                {
                    handler.出料手臂吸放料流程();
                });
                Task.Run(() =>
                {
                    handler.飞梭移动流程();
                });
                Task.Run(() =>
                {
                    handler.测试手臂取待测料流程();
                });
                Task.Run(() =>
                {
                    handler.测试手臂取已测料流程();
                });
                Task.Run(() =>
                {
                    handler.测试区气缸下压动作流程();
                });
                Task.Run(() =>
                {
                    handler.测试区气缸上抬动作流程();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂取Empty盘();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂放空盘到Empty();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂取Load空盘();
                });
                Task.Run(() =>
                {
                    handler.Empty顶盘流程();
                });
                Task.Run(() =>
                {
                    handler.Empty下降流程();
                });
                Task.Run(() =>
                {
                    handler.Empty下降回零流程();
                });
                Task.Run(() =>
                {
                    handler.Load顶盘流程();
                });
                Task.Run(() =>
                {
                    handler.Load下降回零流程();
                });
                Task.Run(() =>
                {
                    handler.Auto1顶盘流程2();
                });
                Task.Run(() =>
                {
                    handler.Auto1下降回零流程();
                });
                Task.Run(() =>
                {
                    handler.Auto2顶盘流程2();
                });
                Task.Run(() =>
                {
                    handler.Auto2下降回零流程();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂放空盘到Auto1();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂放空盘到Auto2();
                });
            });

            return "Success";
        }
    }
}
