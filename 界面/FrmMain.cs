using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;
using Sunny.UI.Win32;
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

        _main main;
        _setting setting;
        _offset offset;
        _speed speed;
        _IO io;
        _log log;
        MotionEx motionEx = MotionEx.GetInstance();
        Handler handler = new Handler();

        public ForMain()
        {
            InitializeComponent();

            log = new _log();

            string result = Init_APPmaster();
            if (result != "Success")
            {
                log.AddLog("error", "run", "软件加载失败,请关闭软件重开!");
                MessageBox.Show("软件加载失败,请关闭软件重开!", result);
            }

        }
        private void ForMain_Load(object sender, EventArgs e)
        {
            main = new _main();
            setting = new _setting();
            offset = new _offset();
            speed = new _speed();
            io = new _IO();

            setting.handler = handler;

            //防止控件过多产生闪烁现象
            SetControlStyle();
            SetTablayoutDoubleBuffer();

            changeMainPanel(main);

            string msg = motionEx.GSNInitCard();
            if (msg != "Success")
            {
                MessageBox.Show("系统加载失败！详细信息：\n" + msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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
                //退出标记 = true; //让报警弹窗也退出，否则主程序退出不了
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

        private string GetIOInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString,"select * from IOInfo", out DataTable dataTable);
            if (result == "Success")
            {
                APPmaster.IOList = new List<IOInfo>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    APPmaster.IOList.Add(new IOInfo()
                    {
                        模块类型 = dr["模块型号"].ToString(),
                        输入 = Convert.ToInt16(dr["输入"].ToString()),
                        输出 = Convert.ToInt16(dr["输出"].ToString()),
                        核号 = Convert.ToInt16(dr["核号"].ToString()),
                        模块号 = Convert.ToInt16(dr["模块号"].ToString()),
                        硬件索引 = Convert.ToInt16(dr["硬件索引"].ToString()),
                        软件索引 = Convert.ToInt16(dr["软件索引"].ToString()),
                        IO编号 = dr["IO编号"].ToString(),
                        IO名称 = dr["IO名称"].ToString(),
                        IO区域 = dr["区域"].ToString()
                    });
                }
            }
            return result;
        }

        private string GetInitPoint()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from InitPoint", out DataTable dataTable);
            if (result == "Success")
            {
                InitPoint initPoint = new InitPoint();
                foreach (DataRow item in dataTable.Rows)
                {
                    double value = Convert.ToDouble(item["value"].ToString());
                    switch (item["ptname"].ToString())
                    {
                        case "入料_Load_X": initPoint.入料_Load_X = value; break;
                        case "入料_Load_Y": initPoint.入料_Load_Y = value; break;
                        case "入料_Load_Z": initPoint.入料_Load_Z = value; break;
                        case "入料_Heat_X": initPoint.入料_Heat_X = value; break;
                        case "入料_Heat_Y": initPoint.入料_Heat_Y = value; break;
                        case "入料_Heat_Z": initPoint.入料_Heat_Z = value; break;
                        case "入料_梭车1_X": initPoint.入料_梭车1_X = value; break;
                        case "入料_梭车1_Y": initPoint.入料_梭车1_Y = value; break;
                        case "入料_梭车1_Z": initPoint.入料_梭车1_Z = value; break;
                        case "入料_梭车2_X": initPoint.入料_梭车2_X = value; break;
                        case "入料_梭车2_Y": initPoint.入料_梭车2_Y = value; break;
                        case "入料_梭车2_Z": initPoint.入料_梭车2_Z = value; break;
                        case "入料_梭车列间隔": initPoint.入料_梭车列间隔 = value; break;
                        case "入料_变距最大值": initPoint.入料_变距最大值 = value; break;
                        case "入料_变距最小值": initPoint.入料_变距最小值 = value; break;
                        case "入料_Z1_硬件补偿": initPoint.入料_Z1_硬件补偿 = value; break;
                        case "入料_Z2_硬件补偿": initPoint.入料_Z2_硬件补偿 = value; break;
                        case "入料_Z3_硬件补偿": initPoint.入料_Z3_硬件补偿 = value; break;
                        case "入料_Z4_硬件补偿": initPoint.入料_Z4_硬件补偿 = value; break;
                        case "出料_梭车1_X": initPoint.出料_梭车1_X = value; break;
                        case "出料_梭车1_Y": initPoint.出料_梭车1_Y = value; break;
                        case "出料_梭车1_Z": initPoint.出料_梭车1_Z = value; break;
                        case "出料_梭车2_X": initPoint.出料_梭车2_X = value; break;
                        case "出料_梭车2_Y": initPoint.出料_梭车2_Y = value; break;
                        case "出料_梭车2_Z": initPoint.出料_梭车2_Z = value; break;
                        case "出料_Auto1_X": initPoint.出料_Auto1_X = value; break;
                        case "出料_Auto1_Y": initPoint.出料_Auto1_Y = value; break;
                        case "出料_Auto1_Z": initPoint.出料_Auto1_Z = value; break;
                        case "出料_Auto2_X": initPoint.出料_Auto2_X = value; break;
                        case "出料_Auto2_Y": initPoint.出料_Auto2_Y = value; break;
                        case "出料_Auto2_Z": initPoint.出料_Auto2_Z = value; break;
                        case "出料_Fix1_X": initPoint.出料_Fix1_X = value; break;
                        case "出料_Fix1_Y": initPoint.出料_Fix1_Y = value; break;
                        case "出料_Fix1_Z": initPoint.出料_Fix1_Z = value; break;
                        case "出料_Fix2_X": initPoint.出料_Fix2_X = value; break;
                        case "出料_Fix2_Y": initPoint.出料_Fix2_Y = value; break;
                        case "出料_Fix2_Z": initPoint.出料_Fix2_Z = value; break;
                        case "出料_Fix3_X": initPoint.出料_Fix3_X = value; break;
                        case "出料_Fix3_Y": initPoint.出料_Fix3_Y = value; break;
                        case "出料_Fix3_Z": initPoint.出料_Fix3_Z = value; break;
                        case "出料_梭车列间隔": initPoint.出料_梭车列间隔 = value; break;
                        case "出料_变距最大值": initPoint.出料_变距最大值 = value; break;
                        case "出料_变距最小值": initPoint.出料_变距最小值 = value; break;
                        case "出料_Fix3_右移动": initPoint.出料_Fix3_右移动 = value; break;
                        case "出料_Z1_硬件补偿": initPoint.出料_Z1_硬件补偿 = value; break;
                        case "出料_Z2_硬件补偿": initPoint.出料_Z2_硬件补偿 = value; break;
                        case "出料_Z3_硬件补偿": initPoint.出料_Z3_硬件补偿 = value; break;
                        case "出料_Z4_硬件补偿": initPoint.出料_Z4_硬件补偿 = value; break;
                        case "测试_梭车1吸_X": initPoint.测试_梭车1吸_X = value; break;
                        case "测试_梭车1吸_Z": initPoint.测试_梭车1吸_Z = value; break;
                        case "测试_梭车2吸_X": initPoint.测试_梭车2吸_X = value; break;
                        case "测试_梭车2吸_Z": initPoint.测试_梭车2吸_Z = value; break;
                        case "测试_梭车1放_X": initPoint.测试_梭车1放_X = value; break;
                        case "测试_梭车1放_Z": initPoint.测试_梭车1放_Z = value; break;
                        case "测试_梭车2放_X": initPoint.测试_梭车2放_X = value; break;
                        case "测试_梭车2放_Z": initPoint.测试_梭车2放_Z = value; break;
                        case "测试_梭车列间隔": initPoint.测试_梭车列间隔 = value; break;
                        case "测试_Test1区_X": initPoint.测试_Test1区_X = value; break;
                        case "测试_Test2区_X": initPoint.测试_Test2区_X = value; break;
                        case "测试_Test3区_X": initPoint.测试_Test3区_X = value; break;
                        case "测试_Test4区_X": initPoint.测试_Test4区_X = value; break;
                        case "测试_Test区Site间隔": initPoint.测试_Test区Site间隔 = value; break;
                        case "测试_Test区吸_Z": initPoint.测试_Test区吸_Z = value; break;
                        case "测试_Test区放_Z": initPoint.测试_Test区放_Z = value; break;
                        case "梭车2_前_Y": initPoint.梭车2_前_Y = value; break;
                        case "梭车2_后_Y": initPoint.梭车2_后_Y = value; break;
                        case "抓盘_Empty_X": initPoint.抓盘_Empty_X = value; break;
                        case "抓盘_Load_X": initPoint.抓盘_Load_X = value; break;
                        case "抓盘_Auto1_X": initPoint.抓盘_Auto1_X = value; break;
                        case "抓盘_Auto2_X": initPoint.抓盘_Auto2_X = value; break;
                    }
                }
                初始点位 = initPoint;
            }
            return result;
        }

        private string GetOffsetPoint()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from OffsetPoint where 微调点位名称 = '{设置信息.OffsetPoint.微调点位名称}'", out SQLiteDataReader reader);
            if (result == "Success")
            {
                if (reader.Read())
                {
                    OffsetPoint offsetPoint = new OffsetPoint()
                    {
                        微调点位名称 = reader["微调点位名称"].ToString(),
                        入料_Load吸_X = Convert.ToDouble(reader["入料_Load吸_X"].ToString()),
                        入料_Load吸_Y = Convert.ToDouble(reader["入料_Load吸_Y"].ToString()),
                        入料_Load吸_Z = Convert.ToDouble(reader["入料_Load吸_Z"].ToString()),
                        入料_Load_变距 = Convert.ToDouble(reader["入料_Load_变距"].ToString()),
                        入料_Heat吸_X = Convert.ToDouble(reader["入料_Heat吸_X"].ToString()),
                        入料_Heat吸_Y = Convert.ToDouble(reader["入料_Heat吸_Y"].ToString()),
                        入料_Heat吸_Z = Convert.ToDouble(reader["入料_Heat吸_Z"].ToString()),
                        入料_Heat放_X = Convert.ToDouble(reader["入料_Heat放_X"].ToString()),
                        入料_Heat放_Y = Convert.ToDouble(reader["入料_Heat放_Y"].ToString()),
                        入料_Heat放_Z = Convert.ToDouble(reader["入料_Heat放_Z"].ToString()),
                        入料_Heat_变距 = Convert.ToDouble(reader["入料_Heat_变距"].ToString()),
                        入料_梭车1第1列放_X = Convert.ToDouble(reader["入料_梭车1第1列放_X"].ToString()),
                        入料_梭车1第1列放_Y = Convert.ToDouble(reader["入料_梭车1第1列放_Y"].ToString()),
                        入料_梭车1第1列放_Z = Convert.ToDouble(reader["入料_梭车1第1列放_Z"].ToString()),
                        入料_梭车1第2列放_X = Convert.ToDouble(reader["入料_梭车1第2列放_X"].ToString()),
                        入料_梭车1第2列放_Y = Convert.ToDouble(reader["入料_梭车1第2列放_Y"].ToString()),
                        入料_梭车1第2列放_Z = Convert.ToDouble(reader["入料_梭车1第2列放_Z"].ToString()),
                        入料_梭车2第1列放_X = Convert.ToDouble(reader["入料_梭车2第1列放_X"].ToString()),
                        入料_梭车2第1列放_Y = Convert.ToDouble(reader["入料_梭车2第1列放_Y"].ToString()),
                        入料_梭车2第1列放_Z = Convert.ToDouble(reader["入料_梭车2第1列放_Z"].ToString()),
                        入料_梭车2第2列放_X = Convert.ToDouble(reader["入料_梭车2第2列放_X"].ToString()),
                        入料_梭车2第2列放_Y = Convert.ToDouble(reader["入料_梭车2第2列放_Y"].ToString()),
                        入料_梭车2第2列放_Z = Convert.ToDouble(reader["入料_梭车2第2列放_Z"].ToString()),
                        入料_梭车_变距 = Convert.ToDouble(reader["入料_梭车_变距"].ToString()),
                        出料_梭车1第1列吸_X = Convert.ToDouble(reader["出料_梭车1第1列吸_X"].ToString()),
                        出料_梭车1第1列吸_Y = Convert.ToDouble(reader["出料_梭车1第1列吸_Y"].ToString()),
                        出料_梭车1第1列吸_Z = Convert.ToDouble(reader["出料_梭车1第1列吸_Z"].ToString()),
                        出料_梭车1第2列吸_X = Convert.ToDouble(reader["出料_梭车1第2列吸_X"].ToString()),
                        出料_梭车1第2列吸_Y = Convert.ToDouble(reader["出料_梭车1第2列吸_Y"].ToString()),
                        出料_梭车1第2列吸_Z = Convert.ToDouble(reader["出料_梭车1第2列吸_Z"].ToString()),
                        出料_梭车2第1列吸_X = Convert.ToDouble(reader["出料_梭车2第1列吸_X"].ToString()),
                        出料_梭车2第1列吸_Y = Convert.ToDouble(reader["出料_梭车2第1列吸_Y"].ToString()),
                        出料_梭车2第1列吸_Z = Convert.ToDouble(reader["出料_梭车2第1列吸_Z"].ToString()),
                        出料_梭车2第2列吸_X = Convert.ToDouble(reader["出料_梭车2第2列吸_X"].ToString()),
                        出料_梭车2第2列吸_Y = Convert.ToDouble(reader["出料_梭车2第2列吸_Y"].ToString()),
                        出料_梭车2第2列吸_Z = Convert.ToDouble(reader["出料_梭车2第2列吸_Z"].ToString()),
                        出料_梭车_变距 = Convert.ToDouble(reader["出料_梭车_变距"].ToString()),
                        出料_Auto1放_X = Convert.ToDouble(reader["出料_Auto1放_X"].ToString()),
                        出料_Auto1放_Y = Convert.ToDouble(reader["出料_Auto1放_Y"].ToString()),
                        出料_Auto1放_Z = Convert.ToDouble(reader["出料_Auto1放_Z"].ToString()),
                        出料_Auto2放_X = Convert.ToDouble(reader["出料_Auto2放_X"].ToString()),
                        出料_Auto2放_Y = Convert.ToDouble(reader["出料_Auto2放_Y"].ToString()),
                        出料_Auto2放_Z = Convert.ToDouble(reader["出料_Auto2放_Z"].ToString()),
                        出料_Fix1放_X = Convert.ToDouble(reader["出料_Fix1放_X"].ToString()),
                        出料_Fix1放_Y = Convert.ToDouble(reader["出料_Fix1放_Y"].ToString()),
                        出料_Fix1放_Z = Convert.ToDouble(reader["出料_Fix1放_Z"].ToString()),
                        出料_Fix2放_X = Convert.ToDouble(reader["出料_Fix2放_X"].ToString()),
                        出料_Fix2放_Y = Convert.ToDouble(reader["出料_Fix2放_Y"].ToString()),
                        出料_Fix2放_Z = Convert.ToDouble(reader["出料_Fix2放_Z"].ToString()),
                        出料_Fix3放_X = Convert.ToDouble(reader["出料_Fix3放_X"].ToString()),
                        出料_Fix3放_Y = Convert.ToDouble(reader["出料_Fix3放_Y"].ToString()),
                        出料_Fix3放_Z = Convert.ToDouble(reader["出料_Fix3放_Z"].ToString()),
                        出料_AF_变距 = Convert.ToDouble(reader["出料_AF_变距"].ToString()),
                        测试_梭车1左吸_X = Convert.ToDouble(reader["测试_梭车1左吸_X"].ToString()),
                        测试_梭车1左吸_Z = Convert.ToDouble(reader["测试_梭车1左吸_Z"].ToString()),
                        测试_梭车2左吸_X = Convert.ToDouble(reader["测试_梭车2左吸_X"].ToString()),
                        测试_梭车2左吸_Z = Convert.ToDouble(reader["测试_梭车2左吸_Z"].ToString()),
                        测试_梭车1右放_X = Convert.ToDouble(reader["测试_梭车1右放_X"].ToString()),
                        测试_梭车1右放_Z = Convert.ToDouble(reader["测试_梭车1右放_Z"].ToString()),
                        测试_梭车2右放_X = Convert.ToDouble(reader["测试_梭车2右放_X"].ToString()),
                        测试_梭车2右放_Z = Convert.ToDouble(reader["测试_梭车2右放_Z"].ToString()),
                        测试_Site1吸_X = Convert.ToDouble(reader["测试_Site1吸_X"].ToString()),
                        测试_Site2吸_X = Convert.ToDouble(reader["测试_Site2吸_X"].ToString()),
                        测试_Site3吸_X = Convert.ToDouble(reader["测试_Site3吸_X"].ToString()),
                        测试_Site4吸_X = Convert.ToDouble(reader["测试_Site4吸_X"].ToString()),
                        测试_Site5吸_X = Convert.ToDouble(reader["测试_Site5吸_X"].ToString()),
                        测试_Site6吸_X = Convert.ToDouble(reader["测试_Site6吸_X"].ToString()),
                        测试_Site7吸_X = Convert.ToDouble(reader["测试_Site7吸_X"].ToString()),
                        测试_Site8吸_X = Convert.ToDouble(reader["测试_Site8吸_X"].ToString()),
                        测试_Site9吸_X = Convert.ToDouble(reader["测试_Site9吸_X"].ToString()),
                        测试_Site10吸_X = Convert.ToDouble(reader["测试_Site10吸_X"].ToString()),
                        测试_Site11吸_X = Convert.ToDouble(reader["测试_Site11吸_X"].ToString()),
                        测试_Site12吸_X = Convert.ToDouble(reader["测试_Site12吸_X"].ToString()),
                        测试_Site13吸_X = Convert.ToDouble(reader["测试_Site13吸_X"].ToString()),
                        测试_Site14吸_X = Convert.ToDouble(reader["测试_Site14吸_X"].ToString()),
                        测试_Site15吸_X = Convert.ToDouble(reader["测试_Site15吸_X"].ToString()),
                        测试_Site16吸_X = Convert.ToDouble(reader["测试_Site16吸_X"].ToString()),
                        测试_Site1吸_Z = Convert.ToDouble(reader["测试_Site1吸_Z"].ToString()),
                        测试_Site2吸_Z = Convert.ToDouble(reader["测试_Site2吸_Z"].ToString()),
                        测试_Site3吸_Z = Convert.ToDouble(reader["测试_Site3吸_Z"].ToString()),
                        测试_Site4吸_Z = Convert.ToDouble(reader["测试_Site4吸_Z"].ToString()),
                        测试_Site5吸_Z = Convert.ToDouble(reader["测试_Site5吸_Z"].ToString()),
                        测试_Site6吸_Z = Convert.ToDouble(reader["测试_Site6吸_Z"].ToString()),
                        测试_Site7吸_Z = Convert.ToDouble(reader["测试_Site7吸_Z"].ToString()),
                        测试_Site8吸_Z = Convert.ToDouble(reader["测试_Site8吸_Z"].ToString()),
                        测试_Site9吸_Z = Convert.ToDouble(reader["测试_Site9吸_Z"].ToString()),
                        测试_Site10吸_Z = Convert.ToDouble(reader["测试_Site10吸_Z"].ToString()),
                        测试_Site11吸_Z = Convert.ToDouble(reader["测试_Site11吸_Z"].ToString()),
                        测试_Site12吸_Z = Convert.ToDouble(reader["测试_Site12吸_Z"].ToString()),
                        测试_Site13吸_Z = Convert.ToDouble(reader["测试_Site13吸_Z"].ToString()),
                        测试_Site14吸_Z = Convert.ToDouble(reader["测试_Site14吸_Z"].ToString()),
                        测试_Site15吸_Z = Convert.ToDouble(reader["测试_Site15吸_Z"].ToString()),
                        测试_Site16吸_Z = Convert.ToDouble(reader["测试_Site16吸_Z"].ToString()),
                        测试_Site1放_X = Convert.ToDouble(reader["测试_Site1放_X"].ToString()),
                        测试_Site2放_X = Convert.ToDouble(reader["测试_Site2放_X"].ToString()),
                        测试_Site3放_X = Convert.ToDouble(reader["测试_Site3放_X"].ToString()),
                        测试_Site4放_X = Convert.ToDouble(reader["测试_Site4放_X"].ToString()),
                        测试_Site5放_X = Convert.ToDouble(reader["测试_Site5放_X"].ToString()),
                        测试_Site6放_X = Convert.ToDouble(reader["测试_Site6放_X"].ToString()),
                        测试_Site7放_X = Convert.ToDouble(reader["测试_Site7放_X"].ToString()),
                        测试_Site8放_X = Convert.ToDouble(reader["测试_Site8放_X"].ToString()),
                        测试_Site9放_X = Convert.ToDouble(reader["测试_Site9放_X"].ToString()),
                        测试_Site10放_X = Convert.ToDouble(reader["测试_Site10放_X"].ToString()),
                        测试_Site11放_X = Convert.ToDouble(reader["测试_Site11放_X"].ToString()),
                        测试_Site12放_X = Convert.ToDouble(reader["测试_Site12放_X"].ToString()),
                        测试_Site13放_X = Convert.ToDouble(reader["测试_Site13放_X"].ToString()),
                        测试_Site14放_X = Convert.ToDouble(reader["测试_Site14放_X"].ToString()),
                        测试_Site15放_X = Convert.ToDouble(reader["测试_Site15放_X"].ToString()),
                        测试_Site16放_X = Convert.ToDouble(reader["测试_Site16放_X"].ToString()),
                        测试_Site1放_Z = Convert.ToDouble(reader["测试_Site1放_Z"].ToString()),
                        测试_Site2放_Z = Convert.ToDouble(reader["测试_Site2放_Z"].ToString()),
                        测试_Site3放_Z = Convert.ToDouble(reader["测试_Site3放_Z"].ToString()),
                        测试_Site4放_Z = Convert.ToDouble(reader["测试_Site4放_Z"].ToString()),
                        测试_Site5放_Z = Convert.ToDouble(reader["测试_Site5放_Z"].ToString()),
                        测试_Site6放_Z = Convert.ToDouble(reader["测试_Site6放_Z"].ToString()),
                        测试_Site7放_Z = Convert.ToDouble(reader["测试_Site7放_Z"].ToString()),
                        测试_Site8放_Z = Convert.ToDouble(reader["测试_Site8放_Z"].ToString()),
                        测试_Site9放_Z = Convert.ToDouble(reader["测试_Site9放_Z"].ToString()),
                        测试_Site10放_Z = Convert.ToDouble(reader["测试_Site10放_Z"].ToString()),
                        测试_Site11放_Z = Convert.ToDouble(reader["测试_Site11放_Z"].ToString()),
                        测试_Site12放_Z = Convert.ToDouble(reader["测试_Site12放_Z"].ToString()),
                        测试_Site13放_Z = Convert.ToDouble(reader["测试_Site13放_Z"].ToString()),
                        测试_Site14放_Z = Convert.ToDouble(reader["测试_Site14放_Z"].ToString()),
                        测试_Site15放_Z = Convert.ToDouble(reader["测试_Site15放_Z"].ToString()),
                        测试_Site16放_Z = Convert.ToDouble(reader["测试_Site16放_Z"].ToString()),
                        梭车2_前_Y = Convert.ToDouble(reader["梭车2_前_Y"].ToString()),
                        梭车2_后_Y = Convert.ToDouble(reader["梭车2_后_Y"].ToString()),
                        抓盘_Empty_X = Convert.ToDouble(reader["抓盘_Empty_X"].ToString()),
                        抓盘_Load_X = Convert.ToDouble(reader["抓盘_Load_X"].ToString()),
                        抓盘_Auto1_X = Convert.ToDouble(reader["抓盘_Auto1_X"].ToString()),
                        抓盘_Auto2_X = Convert.ToDouble(reader["抓盘_Auto2_X"].ToString()),
                    };

                    设置信息.OffsetPoint = offsetPoint;
                }
                else result = $"无点位微调({设置信息.OffsetPoint.微调点位名称})信息";

                reader.Close();
            }
            return result;
        }

        private string GetAxisInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from AxisInfo", out DataTable dataTable);
            if (result == "Success")
            {
                APPmaster.AxisInfoList = new List<AxisInfo>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    APPmaster.AxisInfoList.Add(new AxisInfo()
                    {
                        轴名称 = dr["轴名称"].ToString(),
                        核号 = Convert.ToInt16(dr["核号"].ToString()),
                        轴号 = Convert.ToInt16(dr["轴号"].ToString()),
                    });
                }
            }
            return result;
        }

        private string GetAxisRunInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from AxisRunInfo", out DataTable dataTable);
            if (result == "Success")
            {
                APPmaster.AxisRunInfoList = new List<AxisRunInfo>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    APPmaster.AxisRunInfoList.Add(new AxisRunInfo()
                    {
                        轴名称 = dr["轴名称"].ToString(),
                        最大速度 = Convert.ToDouble(dr["最大速度"].ToString()),
                        加速度 = Convert.ToDouble(dr["加速度"].ToString()),
                        减速度 = Convert.ToDouble(dr["减速度"].ToString()),
                        步长mm = Convert.ToDouble(dr["步长mm"].ToString()),
                        速度百分比 = Convert.ToDouble(dr["速度百分比"].ToString()),
                        平滑时间 = Convert.ToInt16(dr["平滑时间"].ToString()),
                    });
                }
                AxisRunGetCoreAxis();
            }
            return result;
        }

        private string GetAxisHomeInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from AxisHomeInfo", out DataTable dataTable);
            if (result == "Success")
            {
                APPmaster.AxisHomeInfoList = new List<AxisHomeInfo>();
                foreach (DataRow dr in dataTable.Rows)
                {
                    APPmaster.AxisHomeInfoList.Add(new AxisHomeInfo()
                    {
                        轴名称 = dr["轴名称"].ToString(),
                        模式 = Convert.ToInt16(dr["模式"].ToString()),
                        回零启动方向 = Convert.ToInt16(dr["回零启动方向"].ToString()),
                        Index搜索方向 = Convert.ToInt16(dr["Index搜索方向"].ToString()),
                        捕获触发沿 = Convert.ToInt16(dr["捕获触发沿"].ToString()),
                        捕获触发器 = Convert.ToInt16(dr["捕获触发器"].ToString()),
                        搜索速度 = Convert.ToDouble(dr["搜索速度"].ToString()),
                        搜索Index速度 = Convert.ToDouble(dr["搜索Index速度"].ToString()),
                        定位速度 = Convert.ToDouble(dr["定位速度"].ToString()),
                        加速度 = Convert.ToDouble(dr["加速度"].ToString()),
                        减速度 = Convert.ToDouble(dr["减速度"].ToString()),
                        平滑时间 = Convert.ToInt16(dr["平滑时间"].ToString()),
                        零点偏置 = Convert.ToInt32(dr["零点偏置"].ToString()),
                        Home最大搜索距离 = Convert.ToInt32(dr["Home最大搜索距离"].ToString()),
                        Index最大搜索距离 = Convert.ToInt32(dr["Index最大搜索距离"].ToString()),
                        反向脱离步长 = Convert.ToInt32(dr["反向脱离步长"].ToString()),
                    });
                }
                AxisHomeGetCoreAxis();
            }
            return result;
        }

        private string GetconfigData()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, "select * from config", out DataTable dataTable);
            if (result == "Success")
            {
                foreach (DataRow item in dataTable.Rows)
                {
                    string value = item["value"].ToString();
                    switch (item["name"].ToString())
                    {
                        case "当前档案": 当前档案 = value; break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取设置信息
        /// </summary>
        /// <returns></returns>
        private string GetSetupInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from SetupInfo where 档案名称 = '{当前档案}'", out SQLiteDataReader reader);
            if (result == "Success")
            {
                if (reader.Read())
                {
                    设置信息.设置信息名称 = 当前档案;
                    设置信息.Tray盘信息.TrayName = reader["tray盘名称"].ToString();
                    设置信息.OffsetPoint.微调点位名称 = reader["偏移点位名称"].ToString();
                    设置信息.开site信息.Site名称 = reader["site名称"].ToString();
                }
                else result = "Fail";
                reader.Close();
            }
            return result;
        }

        private string GetTrayInfo()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from TrayInfo where name = '{设置信息.Tray盘信息.TrayName}'", out SQLiteDataReader reader);
            if (result == "Success")
            {
                TrayInfo trayInfo = new TrayInfo();
                if (reader.Read())
                {
                    trayInfo.TrayName = reader["name"].ToString();
                    trayInfo.X_Num = Convert.ToInt32(reader["xnum"].ToString());
                    trayInfo.Y_Num = Convert.ToInt32(reader["ynum"].ToString());
                    trayInfo.X_Start = Convert.ToDouble(reader["xstart"].ToString());
                    trayInfo.Y_Start = Convert.ToDouble(reader["ystart"].ToString());
                    trayInfo.X_Pitch = Convert.ToDouble(reader["xpitch"].ToString());
                    trayInfo.Y_Pitch = Convert.ToDouble(reader["ypitch"].ToString());
                    trayInfo.Width = Convert.ToDouble(reader["width"].ToString());
                    trayInfo.Length = Convert.ToDouble(reader["length"].ToString());
                    trayInfo.Height = Convert.ToDouble(reader["height"].ToString());

                    设置信息.Tray盘信息 = trayInfo;
                }
                else result = $"无Tray盘{设置信息.Tray盘信息.TrayName}信息";

                reader.Close();
            }
            return result;
        }

        private string GetSiteStatus()
        {
            string result = SQLiteHelper.runSql(SQLiteConnString, $"select * from SiteStatus where Site名称 = '{设置信息.开site信息.Site名称}'", out DataTable dataTable);
            if (result == "Success")
            {
                DataRow dr = dataTable.Rows[0];

                SiteStatus siteStatus = new SiteStatus()
                {
                    Site名称 = dr["Site名称"].ToString(),
                    Site1 = dr["Site1"].ToString().ToCharArray().Select(c => c.ToString()) .ToArray(),
                    Site2 = dr["Site2"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site3 = dr["Site3"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site4 = dr["Site4"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site5 = dr["Site5"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site6 = dr["Site6"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site7 = dr["Site7"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site8 = dr["Site8"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site9 = dr["Site9"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site10 = dr["Site10"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site11 = dr["Site11"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site12 = dr["Site12"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site13 = dr["Site13"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site14 = dr["Site14"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site15 = dr["Site15"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                    Site16 = dr["Site16"].ToString().ToCharArray().Select(c => c.ToString()).ToArray(),
                };

                设置信息.开site信息 = siteStatus;
            }
            return result;
        }

        private string Init_APPmaster()
        {
            ///IO信息
            string result = GetIOInfo();
            if (result != "Success") return result;

            ///轴信息
            result = GetAxisInfo();
            if (result != "Success") return result;

            ///轴运行信息
            result = GetAxisRunInfo();
            if (result != "Success") return result;

            ///轴回零信息
            result = GetAxisHomeInfo();
            if (result != "Success") return result;

            ///设备config
            result = GetconfigData();
            if (result != "Success") return result;

            ///设备档案
            result = GetSetupInfo();
            if (result == "Success")
            {
                ///tray盘信息
                result = GetTrayInfo();
                if (result != "Success") return result;

                ///点位微调
                result = GetOffsetPoint();
                if (result != "Success") return result;

                ///开Site信息
                result = GetSiteStatus();
                if (result != "Success") return result;
            }
            else return result;

            ///初始点位
            result = GetInitPoint();
            if (result != "Success") return result;

            GetSiteStatus1(); //临时参数

            APPmaster.TestPointInit();
            APPmaster.LoaderTrayICPointInit();
            APPmaster.HeatTrayICPointInit();
            APPmaster.Auto1TrayICPointInit();
            APPmaster.Auto2TrayICPointInit();
            APPmaster.Fix1TrayICPointInit();
            APPmaster.Fix2TrayICPointInit();
            APPmaster.Fix3TrayICPointInit();

            APPmaster.LoaderTrayICMapInit();
            APPmaster.HeatTrayICMapInit();
            APPmaster.Auto1TrayICMapInit();
            APPmaster.Auto2TrayICMapInit();
            APPmaster.Fix1TrayICMapInit();
            APPmaster.Fix2TrayICMapInit();
            APPmaster.Fix3TrayICMapInit();

            handler.参数初始化();
            int num = 45;
            int[] intArray = new int[8];
            for (int i = 0; i < 8; i++)
            {
                intArray[i] = ((num >> i) & 1); //00101101
            }
            StartTask = Task.Run(() => //通过线程去开启线程，防止卡顿
            {
                cts = new CancellationTokenSource();
                
                Task.Run(() =>
                {
                    handler.初始化流程();
                });
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
                    handler.系统按钮检测流程();
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
                    handler.Load顶盘流程();
                });
                Task.Run(() =>
                {
                    handler.Auto1顶盘流程2();
                });
                Task.Run(() =>
                {
                    handler.Auto2顶盘流程2();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂放空盘到Auto1();
                });
                Task.Run(() =>
                {
                    handler.抓盘手臂放空盘到Auto2();
                });
                //Task.Run(() =>
                //{
                //    handler.抓盘手臂放空盘到Auto1();
                //});
                //Task.Run(() =>
                //{
                //    handler.抓盘手臂放空盘到Auto2();
                //});
            });

            return "Success";
        }

        private void GetCoreAxis(string axisname, out short core, out short axis)
        {
            AxisInfo axisInfo = new AxisInfo();
            for (int i = 0; i < APPmaster.AxisInfoList.Count; i++)
            {
                if (APPmaster.AxisInfoList[i].轴名称 == axisname)
                {
                    axisInfo = APPmaster.AxisInfoList[i];
                }
            }
            core = axisInfo.核号;
            axis = axisInfo.轴号;
        }

        private void AxisRunGetCoreAxis()
        {
            for (int i = 0; i < APPmaster.AxisRunInfoList.Count; i++)
            {
                GetCoreAxis(APPmaster.AxisRunInfoList[i].轴名称, out short core, out short axis);
                APPmaster.AxisRunInfoList[i].核号 = core;
                APPmaster.AxisRunInfoList[i].轴号 = axis;
            }
        }

        private void AxisHomeGetCoreAxis()
        {
            for (int i = 0; i < APPmaster.AxisHomeInfoList.Count; i++)
            {
                GetCoreAxis(APPmaster.AxisHomeInfoList[i].轴名称, out short core, out short axis);
                APPmaster.AxisHomeInfoList[i].核号 = core;
                APPmaster.AxisHomeInfoList[i].轴号 = axis;
            }
        }

        private void GetSiteStatus1()
        {
            for (int i = 0; i < 16; i++)
            {
                //APPmaster.SiteStatus.Add(new bool[2][]
                //{
                //    new bool[] { true, true, true, true },
                //    new bool[] { true, true, true, true },
                //});

                APPmaster.SiteTestIC.Add(new bool[2][]
                {
                    new bool[] { true, true, true, true },
                    new bool[] { true, true, true, true },
                });

                APPmaster.SiteTestBIN.Add(new string[2][]
                {
                    new string[] { "", "", "", "" },
                    new string[] { "", "", "", "" },
                });



                //APPmaster.SiteSave.Add(new bool[2][]
                //{
                //    new bool[] { false, false, false, false },
                //    new bool[] { false, false, false, false },
                //});

                //APPmaster.SiteTestOpen.Add(new bool[2][]
                //{
                //    new bool[] { false, false, false, false },
                //    new bool[] { false, false, false, false },
                //});
            }
            APPmaster.InSiteQueue.Enqueue(4);
            //APPmaster.InSiteQueue.Enqueue(5);
            //APPmaster.InSiteQueue.Enqueue(6);
            //APPmaster.InSiteQueue.Enqueue(7);
            //APPmaster.InSiteQueue.Enqueue(8);
            //APPmaster.InSiteQueue.Enqueue(9);
            //APPmaster.InSiteQueue.Enqueue(10);
            //APPmaster.InSiteQueue.Enqueue(11);
        }

    }
}
