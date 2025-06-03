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
using static H5509_V2.APPmaster;

namespace H5509_V2
{
    public partial class _speed: UIUserControl
    {
        public _speed()
        {
            InitializeComponent();
        }

        public void 刷新速度值()
        {
            APPmaster.GetAxisRunInfo("M22_入料手臂X轴", out AxisRunInfo axisRun);
            uiDoubleUpDown99.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M16_入料手臂Y轴", out axisRun);
            uiDoubleUpDown97.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M1_入料手臂Z1轴", out axisRun);
            uiDoubleUpDown98.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M5_入料手臂P轴", out axisRun);
            uiDoubleUpDown108.Value = axisRun.速度百分比;

            APPmaster.GetAxisRunInfo("M23_出料手臂X轴", out axisRun);
            uiDoubleUpDown14.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M17_出料手臂Y轴", out axisRun);
            uiDoubleUpDown18.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M6_出料手臂Z1轴", out axisRun);
            uiDoubleUpDown17.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M10_出料手臂P轴", out axisRun);
            uiDoubleUpDown16.Value = axisRun.速度百分比;

            APPmaster.GetAxisRunInfo("M21_测试手臂X轴", out axisRun);
            uiDoubleUpDown107.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M19_测试手臂Z轴", out axisRun);
            uiDoubleUpDown106.Value = axisRun.速度百分比;

            APPmaster.GetAxisRunInfo("M20_飞梭轴", out axisRun);
            uiDoubleUpDown13.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M18_放Tary盘轴", out axisRun);
            uiDoubleUpDown12.Value = axisRun.速度百分比;

            APPmaster.GetAxisRunInfo("M12_Empty顶盘轴", out axisRun);
            uiDoubleUpDown4.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M13_Loader顶盘轴", out axisRun);
            uiDoubleUpDown3.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M14_Auto1顶盘轴", out axisRun);
            uiDoubleUpDown1.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M15_Auto2顶盘轴", out axisRun);
            uiDoubleUpDown5.Value = axisRun.速度百分比;
            APPmaster.GetAxisRunInfo("M11_Fix3放IC轴", out axisRun);
            uiDoubleUpDown2.Value = axisRun.速度百分比;

        }

        public bool UpdateRunInfoData(string pointname, string valuename, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = SQLiteHelper.runSqlNonQuery(APPmaster.SQLiteConnString, $"update AxisRunInfo set {valuename} = '{newvalue}' where 轴名称 = '{pointname}'");
                if (line >= 1)
                {
                    RunLog($"运行参数:[{pointname}:{valuename}]{oldvalue}->{newvalue}:更新成功");
                    return true;
                }
                else RunLog($"运行参数:[{pointname}:{valuename}]{oldvalue}->{newvalue}:更新失败");
            }
            return false;
        }

        private void uiButton3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                APPmaster.GetAxisRunInfo("M22_入料手臂X轴", out AxisRunInfo axisRun);
                if (UpdateRunInfoData("M22_入料手臂X轴", "速度百分比", uiDoubleUpDown99.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown99.Value;

                APPmaster.GetAxisRunInfo("M16_入料手臂Y轴", out axisRun);
                if (UpdateRunInfoData("M16_入料手臂Y轴", "速度百分比", uiDoubleUpDown97.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown97.Value;

                APPmaster.GetAxisRunInfo("M5_入料手臂P轴", out axisRun);
                if (UpdateRunInfoData("M5_入料手臂P轴", "速度百分比", uiDoubleUpDown108.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown108.Value;

                APPmaster.GetAxisRunInfo("M1_入料手臂Z1轴", out axisRun);
                if (UpdateRunInfoData("M1_入料手臂Z1轴", "速度百分比", uiDoubleUpDown98.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown98.Value;
                APPmaster.GetAxisRunInfo("M2_入料手臂Z2轴", out axisRun);
                if (UpdateRunInfoData("M2_入料手臂Z2轴", "速度百分比", uiDoubleUpDown98.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown98.Value;
                APPmaster.GetAxisRunInfo("M3_入料手臂Z3轴", out axisRun);
                if (UpdateRunInfoData("M3_入料手臂Z3轴", "速度百分比", uiDoubleUpDown98.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown98.Value;
                APPmaster.GetAxisRunInfo("M4_入料手臂Z4轴", out axisRun);
                if (UpdateRunInfoData("M4_入料手臂Z4轴", "速度百分比", uiDoubleUpDown98.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown98.Value;

                APPmaster.GetAxisRunInfo("M23_出料手臂X轴", out axisRun);
                if (UpdateRunInfoData("M23_出料手臂X轴", "速度百分比", uiDoubleUpDown14.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown14.Value;

                APPmaster.GetAxisRunInfo("M17_出料手臂Y轴", out axisRun);
                if (UpdateRunInfoData("M17_出料手臂Y轴", "速度百分比", uiDoubleUpDown18.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown18.Value;

                APPmaster.GetAxisRunInfo("M10_出料手臂P轴", out axisRun);
                if (UpdateRunInfoData("M10_出料手臂P轴", "速度百分比", uiDoubleUpDown16.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown16.Value;

                APPmaster.GetAxisRunInfo("M6_出料手臂Z1轴", out axisRun);
                if (UpdateRunInfoData("M6_出料手臂Z1轴", "速度百分比", uiDoubleUpDown17.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown17.Value;
                APPmaster.GetAxisRunInfo("M7_出料手臂Z2轴", out axisRun);
                if (UpdateRunInfoData("M7_出料手臂Z2轴", "速度百分比", uiDoubleUpDown17.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown17.Value;
                APPmaster.GetAxisRunInfo("M8_出料手臂Z3轴", out axisRun);
                if (UpdateRunInfoData("M8_出料手臂Z3轴", "速度百分比", uiDoubleUpDown17.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown17.Value;
                APPmaster.GetAxisRunInfo("M9_出料手臂Z4轴", out axisRun);
                if (UpdateRunInfoData("M9_出料手臂Z4轴", "速度百分比", uiDoubleUpDown17.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown17.Value;

                APPmaster.GetAxisRunInfo("M21_测试手臂X轴", out axisRun);
                if (UpdateRunInfoData("M21_测试手臂X轴", "速度百分比", uiDoubleUpDown107.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown107.Value;

                APPmaster.GetAxisRunInfo("M19_测试手臂Z轴", out axisRun);
                if (UpdateRunInfoData("M19_测试手臂Z轴", "速度百分比", uiDoubleUpDown106.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown106.Value;

                APPmaster.GetAxisRunInfo("M20_飞梭轴", out axisRun);
                if (UpdateRunInfoData("M20_飞梭轴", "速度百分比", uiDoubleUpDown13.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown13.Value;

                APPmaster.GetAxisRunInfo("M18_放Tary盘轴", out axisRun);
                if (UpdateRunInfoData("M18_放Tary盘轴", "速度百分比", uiDoubleUpDown12.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown12.Value;

                APPmaster.GetAxisRunInfo("M12_Empty顶盘轴", out axisRun);
                if (UpdateRunInfoData("M12_Empty顶盘轴", "速度百分比", uiDoubleUpDown4.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown4.Value;

                APPmaster.GetAxisRunInfo("M13_Loader顶盘轴", out axisRun);
                if (UpdateRunInfoData("M13_Loader顶盘轴", "速度百分比", uiDoubleUpDown3.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown3.Value;

                APPmaster.GetAxisRunInfo("M14_Auto1顶盘轴", out axisRun);
                if (UpdateRunInfoData("M14_Auto1顶盘轴", "速度百分比", uiDoubleUpDown1.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown1.Value;

                APPmaster.GetAxisRunInfo("M15_Auto2顶盘轴", out axisRun);
                if (UpdateRunInfoData("M15_Auto2顶盘轴", "速度百分比", uiDoubleUpDown5.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown5.Value;

                APPmaster.GetAxisRunInfo("M11_Fix3放IC轴", out axisRun);
                if (UpdateRunInfoData("M11_Fix3放IC轴", "速度百分比", uiDoubleUpDown2.Value, axisRun.速度百分比)) axisRun.速度百分比 = uiDoubleUpDown2.Value;
            }
        }

        private void uiButton4_Click(object sender, EventArgs e)
        {
            刷新速度值();
        }
    }
}
