using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;

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

        public ForMain()
        {
            InitializeComponent();

            Init_APPmaster();

        }
        private void ForMain_Load(object sender, EventArgs e)
        {


            main = new _main();


            setting = new _setting();
            offset = new _offset();
            speed = new _speed();
            io = new _IO();
            log = new _log();

            //防止控件过多产生闪烁现象
            SetControlStyle();
            SetTablayoutDoubleBuffer();


            changeMainPanel(main);

            string msg = motionEx.GSNInitCard();
            if (msg != "Success")
            {
                MessageBox.Show("系统加载失败！详细信息：\n" + msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// MainPanel界面切换
        /// </summary>
        /// <param name="uc"></param>
        private void changeMainPanel(UserControl uc)
        {
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
        }

        private void button_offset_Click(object sender, EventArgs e)
        {
            changeMainPanel(offset);
        }

        private void button_speed_Click(object sender, EventArgs e)
        {
            changeMainPanel(speed);
        }

        private void button_io_Click(object sender, EventArgs e)
        {
            changeMainPanel(io);
        }

        private void button_log_Click(object sender, EventArgs e)
        {
            changeMainPanel(log);
        }

        private void Init_APPmaster()
        {
            DataTable dataTable = SQLiteHelper.GetDataSet("select * from IOInfo").Tables[0];
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

            dataTable = SQLiteHelper.GetDataSet("select * from AxisInfo").Tables[0];
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

            dataTable = SQLiteHelper.GetDataSet("select * from AxisRunInfo").Tables[0];
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
                    速度百分比 = Convert.ToInt16(dr["速度百分比"].ToString()),
                    平滑时间 = Convert.ToInt16(dr["平滑时间"].ToString()),
                });
            }
            AxisRunGetCoreAxis();

            dataTable = SQLiteHelper.GetDataSet("select * from AxisHomeInfo").Tables[0];
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
    }
}
