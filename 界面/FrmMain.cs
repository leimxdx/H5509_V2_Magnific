using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

            string msg = motionEx.Motion.InitCard();
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
                    IoModule = dr["模块型号"].ToString(),
                    Core =Convert.ToInt16( dr["核号"].ToString()),
                    ModuleNum = Convert.ToInt16(dr["模块号"].ToString()),
                    IoIndex = Convert.ToInt16(dr["IO索引"].ToString()),
                    IoNum = dr["IO编号"].ToString(),
                    IoName = dr["IO名称"].ToString(),
                    Area = dr["区域"].ToString()
                });
            }

            //APPmaster.list = new
        }
    }
}
