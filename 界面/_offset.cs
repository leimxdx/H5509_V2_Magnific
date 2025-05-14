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
    public partial class _offset : UIUserControl
    {
        public _offset()
        {
            InitializeComponent();
        }

        public void 刷新入料手臂offset()
        {
            uiDoubleUpDown99.Text = 设置信息.OffsetPoint.入料_Load吸_X.ToString();
            uiDoubleUpDown97.Text = 设置信息.OffsetPoint.入料_Load吸_Y.ToString();
            uiDoubleUpDown98.Text = 设置信息.OffsetPoint.入料_Load吸_Z.ToString();
            uiDoubleUpDown108.Text = 设置信息.OffsetPoint.入料_Heat吸_X.ToString();
            uiDoubleUpDown107.Text = 设置信息.OffsetPoint.入料_Heat吸_Y.ToString();
            uiDoubleUpDown106.Text = 设置信息.OffsetPoint.入料_Heat吸_Z.ToString();
            uiDoubleUpDown4.Text = 设置信息.OffsetPoint.入料_Heat放_X.ToString();
            uiDoubleUpDown3.Text = 设置信息.OffsetPoint.入料_Heat放_Y.ToString();
            uiDoubleUpDown2.Text = 设置信息.OffsetPoint.入料_Heat放_Z.ToString();

            uiDoubleUpDown102.Text = 设置信息.OffsetPoint.入料_梭车1第1列放_X.ToString();
            uiDoubleUpDown101.Text = 设置信息.OffsetPoint.入料_梭车1第1列放_Y.ToString();
            uiDoubleUpDown100.Text = 设置信息.OffsetPoint.入料_梭车1第1列放_Z.ToString();
            uiDoubleUpDown14.Text = 设置信息.OffsetPoint.入料_梭车1第2列放_X.ToString();
            uiDoubleUpDown13.Text = 设置信息.OffsetPoint.入料_梭车1第2列放_Y.ToString();
            uiDoubleUpDown12.Text = 设置信息.OffsetPoint.入料_梭车1第2列放_Z.ToString();
            uiDoubleUpDown1.Text = 设置信息.OffsetPoint.入料_Load_变距.ToString();
            uiDoubleUpDown5.Text = 设置信息.OffsetPoint.入料_Heat_变距.ToString();
            uiDoubleUpDown15.Text = 设置信息.OffsetPoint.入料_梭车_变距.ToString();

            uiDoubleUpDown8.Text = 设置信息.OffsetPoint.入料_梭车2第1列放_X.ToString();
            uiDoubleUpDown7.Text = 设置信息.OffsetPoint.入料_梭车2第1列放_Y.ToString();
            uiDoubleUpDown6.Text = 设置信息.OffsetPoint.入料_梭车2第1列放_Z.ToString();
            uiDoubleUpDown11.Text = 设置信息.OffsetPoint.入料_梭车2第2列放_X.ToString();
            uiDoubleUpDown10.Text = 设置信息.OffsetPoint.入料_梭车2第2列放_Y.ToString();
            uiDoubleUpDown9.Text = 设置信息.OffsetPoint.入料_梭车2第2列放_Z.ToString();
        }

        public void 刷新出料手臂offset()
        {
            uiDoubleUpDown75.Text = 设置信息.OffsetPoint.出料_梭车1第1列吸_X.ToString();
            uiDoubleUpDown74.Text = 设置信息.OffsetPoint.出料_梭车1第1列吸_Y.ToString();
            uiDoubleUpDown73.Text = 设置信息.OffsetPoint.出料_梭车1第1列吸_Z.ToString();

            uiDoubleUpDown17.Text = 设置信息.OffsetPoint.出料_梭车1第2列吸_X.ToString();
            uiDoubleUpDown18.Text = 设置信息.OffsetPoint.出料_梭车1第2列吸_Y.ToString();
            uiDoubleUpDown16.Text = 设置信息.OffsetPoint.出料_梭车1第2列吸_Z.ToString();


            uiDoubleUpDown87.Text = 设置信息.OffsetPoint.出料_Fix1放_X.ToString();
            uiDoubleUpDown86.Text = 设置信息.OffsetPoint.出料_Fix1放_Y.ToString();
            uiDoubleUpDown85.Text = 设置信息.OffsetPoint.出料_Fix1放_Z.ToString();

            uiDoubleUpDown33.Text = 设置信息.OffsetPoint.出料_梭车_变距.ToString();
            uiDoubleUpDown34.Text = 设置信息.OffsetPoint.出料_AF_变距.ToString();

            uiDoubleUpDown31.Text = 设置信息.OffsetPoint.出料_梭车2第1列吸_X.ToString();
            uiDoubleUpDown32.Text = 设置信息.OffsetPoint.出料_梭车2第1列吸_Y.ToString();
            uiDoubleUpDown30.Text = 设置信息.OffsetPoint.出料_梭车2第1列吸_Z.ToString();

            uiDoubleUpDown20.Text = 设置信息.OffsetPoint.出料_梭车2第2列吸_X.ToString();
            uiDoubleUpDown21.Text = 设置信息.OffsetPoint.出料_梭车2第2列吸_Y.ToString();
            uiDoubleUpDown19.Text = 设置信息.OffsetPoint.出料_梭车2第2列吸_Z.ToString();

            uiDoubleUpDown96.Text = 设置信息.OffsetPoint.出料_Fix2放_X.ToString();
            uiDoubleUpDown95.Text = 设置信息.OffsetPoint.出料_Fix2放_Y.ToString();
            uiDoubleUpDown94.Text = 设置信息.OffsetPoint.出料_Fix2放_Z.ToString();

            uiDoubleUpDown90.Text = 设置信息.OffsetPoint.出料_Auto1放_X.ToString();
            uiDoubleUpDown89.Text = 设置信息.OffsetPoint.出料_Auto1放_Y.ToString();
            uiDoubleUpDown88.Text = 设置信息.OffsetPoint.出料_Auto1放_Z.ToString();

            uiDoubleUpDown93.Text = 设置信息.OffsetPoint.出料_Auto2放_X.ToString();
            uiDoubleUpDown92.Text = 设置信息.OffsetPoint.出料_Auto2放_Y.ToString();
            uiDoubleUpDown91.Text = 设置信息.OffsetPoint.出料_Auto2放_Z.ToString();

            uiDoubleUpDown84.Text = 设置信息.OffsetPoint.出料_Fix3放_X.ToString();
            uiDoubleUpDown83.Text = 设置信息.OffsetPoint.出料_Fix3放_Y.ToString();
            uiDoubleUpDown82.Text = 设置信息.OffsetPoint.出料_Fix3放_Z.ToString();

        }

        public void 刷新测试手臂offset()
        {
            uiDoubleUpDown62.Text = 设置信息.OffsetPoint.测试_梭车1左吸_X.ToString();
            uiDoubleUpDown61.Text = 设置信息.OffsetPoint.测试_梭车1左吸_Z.ToString();

            uiDoubleUpDown64.Text = 设置信息.OffsetPoint.测试_梭车2左吸_X.ToString();
            uiDoubleUpDown63.Text = 设置信息.OffsetPoint.测试_梭车2左吸_Z.ToString();

            uiDoubleUpDown70.Text = 设置信息.OffsetPoint.测试_梭车1右放_X.ToString();
            uiDoubleUpDown69.Text = 设置信息.OffsetPoint.测试_梭车1右放_Z.ToString();

            uiDoubleUpDown72.Text = 设置信息.OffsetPoint.测试_梭车2右放_X.ToString();
            uiDoubleUpDown71.Text = 设置信息.OffsetPoint.测试_梭车2右放_Z.ToString();

            uiDoubleUpDown66.Text = 设置信息.OffsetPoint.测试_Site1吸_X.ToString();
            uiDoubleUpDown23.Text = 设置信息.OffsetPoint.测试_Site2吸_X.ToString();
            uiDoubleUpDown26.Text = 设置信息.OffsetPoint.测试_Site3吸_X.ToString();
            uiDoubleUpDown25.Text = 设置信息.OffsetPoint.测试_Site4吸_X.ToString();
            uiDoubleUpDown36.Text = 设置信息.OffsetPoint.测试_Site5吸_X.ToString();
            uiDoubleUpDown35.Text = 设置信息.OffsetPoint.测试_Site6吸_X.ToString();
            uiDoubleUpDown28.Text = 设置信息.OffsetPoint.测试_Site7吸_X.ToString();
            uiDoubleUpDown27.Text = 设置信息.OffsetPoint.测试_Site8吸_X.ToString();
            uiDoubleUpDown44.Text = 设置信息.OffsetPoint.测试_Site9吸_X.ToString();
            uiDoubleUpDown43.Text = 设置信息.OffsetPoint.测试_Site10吸_X.ToString();
            uiDoubleUpDown42.Text = 设置信息.OffsetPoint.测试_Site11吸_X.ToString();
            uiDoubleUpDown41.Text = 设置信息.OffsetPoint.测试_Site12吸_X.ToString();
            uiDoubleUpDown40.Text = 设置信息.OffsetPoint.测试_Site13吸_X.ToString();
            uiDoubleUpDown39.Text = 设置信息.OffsetPoint.测试_Site14吸_X.ToString();
            uiDoubleUpDown38.Text = 设置信息.OffsetPoint.测试_Site15吸_X.ToString();
            uiDoubleUpDown37.Text = 设置信息.OffsetPoint.测试_Site16吸_X.ToString();
            uiDoubleUpDown137.Text = 设置信息.OffsetPoint.测试_Site1吸_Z.ToString();
            uiDoubleUpDown136.Text = 设置信息.OffsetPoint.测试_Site2吸_Z.ToString();
            uiDoubleUpDown135.Text = 设置信息.OffsetPoint.测试_Site3吸_Z.ToString();
            uiDoubleUpDown134.Text = 设置信息.OffsetPoint.测试_Site4吸_Z.ToString();
            uiDoubleUpDown133.Text = 设置信息.OffsetPoint.测试_Site5吸_Z.ToString();
            uiDoubleUpDown132.Text = 设置信息.OffsetPoint.测试_Site6吸_Z.ToString();
            uiDoubleUpDown131.Text = 设置信息.OffsetPoint.测试_Site7吸_Z.ToString();
            uiDoubleUpDown130.Text = 设置信息.OffsetPoint.测试_Site8吸_Z.ToString();
            uiDoubleUpDown129.Text = 设置信息.OffsetPoint.测试_Site9吸_Z.ToString();
            uiDoubleUpDown128.Text = 设置信息.OffsetPoint.测试_Site10吸_Z.ToString();
            uiDoubleUpDown127.Text = 设置信息.OffsetPoint.测试_Site11吸_Z.ToString();
            uiDoubleUpDown126.Text = 设置信息.OffsetPoint.测试_Site12吸_Z.ToString();
            uiDoubleUpDown125.Text = 设置信息.OffsetPoint.测试_Site13吸_Z.ToString();
            uiDoubleUpDown124.Text = 设置信息.OffsetPoint.测试_Site14吸_Z.ToString();
            uiDoubleUpDown123.Text = 设置信息.OffsetPoint.测试_Site15吸_Z.ToString();
            uiDoubleUpDown122.Text = 设置信息.OffsetPoint.测试_Site16吸_Z.ToString();
            uiDoubleUpDown81.Text = 设置信息.OffsetPoint.测试_Site1放_X.ToString();
            uiDoubleUpDown80.Text = 设置信息.OffsetPoint.测试_Site2放_X.ToString();
            uiDoubleUpDown79.Text = 设置信息.OffsetPoint.测试_Site3放_X.ToString();
            uiDoubleUpDown78.Text = 设置信息.OffsetPoint.测试_Site4放_X.ToString();
            uiDoubleUpDown77.Text = 设置信息.OffsetPoint.测试_Site5放_X.ToString();
            uiDoubleUpDown76.Text = 设置信息.OffsetPoint.测试_Site6放_X.ToString();
            uiDoubleUpDown68.Text = 设置信息.OffsetPoint.测试_Site7放_X.ToString();
            uiDoubleUpDown67.Text = 设置信息.OffsetPoint.测试_Site8放_X.ToString();
            uiDoubleUpDown65.Text = 设置信息.OffsetPoint.测试_Site9放_X.ToString();
            uiDoubleUpDown54.Text = 设置信息.OffsetPoint.测试_Site10放_X.ToString();
            uiDoubleUpDown53.Text = 设置信息.OffsetPoint.测试_Site11放_X.ToString();
            uiDoubleUpDown52.Text = 设置信息.OffsetPoint.测试_Site12放_X.ToString();
            uiDoubleUpDown48.Text = 设置信息.OffsetPoint.测试_Site13放_X.ToString();
            uiDoubleUpDown47.Text = 设置信息.OffsetPoint.测试_Site14放_X.ToString();
            uiDoubleUpDown46.Text = 设置信息.OffsetPoint.测试_Site15放_X.ToString();
            uiDoubleUpDown45.Text = 设置信息.OffsetPoint.测试_Site16放_X.ToString();
            uiDoubleUpDown121.Text = 设置信息.OffsetPoint.测试_Site1放_Z.ToString();
            uiDoubleUpDown120.Text = 设置信息.OffsetPoint.测试_Site2放_Z.ToString();
            uiDoubleUpDown119.Text = 设置信息.OffsetPoint.测试_Site3放_Z.ToString();
            uiDoubleUpDown118.Text = 设置信息.OffsetPoint.测试_Site4放_Z.ToString();
            uiDoubleUpDown117.Text = 设置信息.OffsetPoint.测试_Site5放_Z.ToString();
            uiDoubleUpDown116.Text = 设置信息.OffsetPoint.测试_Site6放_Z.ToString();
            uiDoubleUpDown115.Text = 设置信息.OffsetPoint.测试_Site7放_Z.ToString();
            uiDoubleUpDown114.Text = 设置信息.OffsetPoint.测试_Site8放_Z.ToString();
            uiDoubleUpDown113.Text = 设置信息.OffsetPoint.测试_Site9放_Z.ToString();
            uiDoubleUpDown112.Text = 设置信息.OffsetPoint.测试_Site10放_Z.ToString();
            uiDoubleUpDown111.Text = 设置信息.OffsetPoint.测试_Site11放_Z.ToString();
            uiDoubleUpDown110.Text = 设置信息.OffsetPoint.测试_Site12放_Z.ToString();
            uiDoubleUpDown109.Text = 设置信息.OffsetPoint.测试_Site13放_Z.ToString();
            uiDoubleUpDown105.Text = 设置信息.OffsetPoint.测试_Site14放_Z.ToString();
            uiDoubleUpDown104.Text = 设置信息.OffsetPoint.测试_Site15放_Z.ToString();
            uiDoubleUpDown103.Text = 设置信息.OffsetPoint.测试_Site16放_Z.ToString();
        }
        public void 刷新梭车offset()
        {
            uiDoubleUpDown57.Text = 设置信息.OffsetPoint.梭车2_前_Y.ToString();
            uiDoubleUpDown56.Text = 设置信息.OffsetPoint.梭车2_后_Y.ToString();
        }
        public void 刷新抓盘手臂offset()
        {
            uiDoubleUpDown55.Text = 设置信息.OffsetPoint.抓盘_Empty_X.ToString();
            uiDoubleUpDown58.Text = 设置信息.OffsetPoint.抓盘_Load_X.ToString();
            uiDoubleUpDown60.Text = 设置信息.OffsetPoint.抓盘_Auto1_X.ToString();
            uiDoubleUpDown59.Text = 设置信息.OffsetPoint.抓盘_Auto2_X.ToString();
        }

        public bool UpdateOffsetPointData(string icname, string otname, string ptname, double newvalue, double oldvalue)
        {
            if (newvalue != oldvalue)
            {
                int line = SQLiteHelper.runSqlNonQuery(SQLiteConnString, $"update OffsetPoint set {ptname} = '{newvalue}' where 微调点位名称 = '{otname}'");
                if (line >= 1)
                {
                    Debug.WriteLine($"档案名称-[{icname}]-偏移点位名称-[{otname}]-点位-[{ptname}]{oldvalue}->{newvalue}:更新成功");
                    return true;
                }
                else Debug.WriteLine($"档案名称-[{icname}]-偏移点位名称-[{otname}]-点位-[{ptname}]{oldvalue}->{newvalue}:更新失败");
            }
            return false;
        }

        private void uiButton3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                try
                {
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Load吸_X", Convert.ToDouble(uiDoubleUpDown99.Text), 设置信息.OffsetPoint.入料_Load吸_X)) 设置信息.OffsetPoint.入料_Load吸_X = Convert.ToDouble(uiDoubleUpDown99.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Load吸_Y", Convert.ToDouble(uiDoubleUpDown97.Text), 设置信息.OffsetPoint.入料_Load吸_Y)) 设置信息.OffsetPoint.入料_Load吸_Y = Convert.ToDouble(uiDoubleUpDown97.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Load吸_Z", Convert.ToDouble(uiDoubleUpDown98.Text), 设置信息.OffsetPoint.入料_Load吸_Z)) 设置信息.OffsetPoint.入料_Load吸_Z = Convert.ToDouble(uiDoubleUpDown98.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Heat吸_X", Convert.ToDouble(uiDoubleUpDown108.Text), 设置信息.OffsetPoint.入料_Heat吸_X)) 设置信息.OffsetPoint.入料_Heat吸_X = Convert.ToDouble(uiDoubleUpDown108.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Heat吸_Y", Convert.ToDouble(uiDoubleUpDown107.Text), 设置信息.OffsetPoint.入料_Heat吸_Y)) 设置信息.OffsetPoint.入料_Heat吸_Y = Convert.ToDouble(uiDoubleUpDown107.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Heat吸_Z", Convert.ToDouble(uiDoubleUpDown106.Text), 设置信息.OffsetPoint.入料_Heat吸_Z)) 设置信息.OffsetPoint.入料_Heat吸_Z = Convert.ToDouble(uiDoubleUpDown106.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Heat放_X", Convert.ToDouble(uiDoubleUpDown4.Text), 设置信息.OffsetPoint.入料_Heat放_X)) 设置信息.OffsetPoint.入料_Heat放_X = Convert.ToDouble(uiDoubleUpDown4.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Heat放_Y", Convert.ToDouble(uiDoubleUpDown3.Text), 设置信息.OffsetPoint.入料_Heat放_Y)) 设置信息.OffsetPoint.入料_Heat放_Y = Convert.ToDouble(uiDoubleUpDown3.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Heat放_Z", Convert.ToDouble(uiDoubleUpDown2.Text), 设置信息.OffsetPoint.入料_Heat放_Z)) 设置信息.OffsetPoint.入料_Heat放_Z = Convert.ToDouble(uiDoubleUpDown2.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车1第1列放_X", Convert.ToDouble(uiDoubleUpDown102.Text), 设置信息.OffsetPoint.入料_梭车1第1列放_X)) 设置信息.OffsetPoint.入料_梭车1第1列放_X = Convert.ToDouble(uiDoubleUpDown102.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车1第1列放_Y", Convert.ToDouble(uiDoubleUpDown101.Text), 设置信息.OffsetPoint.入料_梭车1第1列放_Y)) 设置信息.OffsetPoint.入料_梭车1第1列放_Y = Convert.ToDouble(uiDoubleUpDown101.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车1第1列放_Z", Convert.ToDouble(uiDoubleUpDown100.Text), 设置信息.OffsetPoint.入料_梭车1第1列放_Z)) 设置信息.OffsetPoint.入料_梭车1第1列放_Z = Convert.ToDouble(uiDoubleUpDown100.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车1第2列放_X", Convert.ToDouble(uiDoubleUpDown14.Text), 设置信息.OffsetPoint.入料_梭车1第2列放_X)) 设置信息.OffsetPoint.入料_梭车1第2列放_X = Convert.ToDouble(uiDoubleUpDown14.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车1第2列放_Y", Convert.ToDouble(uiDoubleUpDown13.Text), 设置信息.OffsetPoint.入料_梭车1第2列放_Y)) 设置信息.OffsetPoint.入料_梭车1第2列放_Y = Convert.ToDouble(uiDoubleUpDown13.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车1第2列放_Z", Convert.ToDouble(uiDoubleUpDown12.Text), 设置信息.OffsetPoint.入料_梭车1第2列放_Z)) 设置信息.OffsetPoint.入料_梭车1第2列放_Z = Convert.ToDouble(uiDoubleUpDown12.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Load_变距", Convert.ToDouble(uiDoubleUpDown1.Text), 设置信息.OffsetPoint.入料_Load_变距)) 设置信息.OffsetPoint.入料_Load_变距 = Convert.ToDouble(uiDoubleUpDown1.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_Heat_变距", Convert.ToDouble(uiDoubleUpDown5.Text), 设置信息.OffsetPoint.入料_Heat_变距)) 设置信息.OffsetPoint.入料_Heat_变距 = Convert.ToDouble(uiDoubleUpDown5.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车_变距", Convert.ToDouble(uiDoubleUpDown15.Text), 设置信息.OffsetPoint.入料_梭车_变距)) 设置信息.OffsetPoint.入料_梭车_变距 = Convert.ToDouble(uiDoubleUpDown15.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车2第1列放_X", Convert.ToDouble(uiDoubleUpDown8.Text), 设置信息.OffsetPoint.入料_梭车2第1列放_X)) 设置信息.OffsetPoint.入料_梭车2第1列放_X = Convert.ToDouble(uiDoubleUpDown8.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车2第1列放_Y", Convert.ToDouble(uiDoubleUpDown7.Text), 设置信息.OffsetPoint.入料_梭车2第1列放_Y)) 设置信息.OffsetPoint.入料_梭车2第1列放_Y = Convert.ToDouble(uiDoubleUpDown7.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车2第1列放_Z", Convert.ToDouble(uiDoubleUpDown6.Text), 设置信息.OffsetPoint.入料_梭车2第1列放_Z)) 设置信息.OffsetPoint.入料_梭车2第1列放_Z = Convert.ToDouble(uiDoubleUpDown6.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车2第2列放_X", Convert.ToDouble(uiDoubleUpDown11.Text), 设置信息.OffsetPoint.入料_梭车2第2列放_X)) 设置信息.OffsetPoint.入料_梭车2第2列放_X = Convert.ToDouble(uiDoubleUpDown11.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车2第2列放_Y", Convert.ToDouble(uiDoubleUpDown10.Text), 设置信息.OffsetPoint.入料_梭车2第2列放_Y)) 设置信息.OffsetPoint.入料_梭车2第2列放_Y = Convert.ToDouble(uiDoubleUpDown10.Value);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "入料_梭车2第2列放_Z", Convert.ToDouble(uiDoubleUpDown9.Text), 设置信息.OffsetPoint.入料_梭车2第2列放_Z)) 设置信息.OffsetPoint.入料_梭车2第2列放_Z = Convert.ToDouble(uiDoubleUpDown9.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败,请检查数据格式是否正确!{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void uiButton4_Click(object sender, EventArgs e)
        {
            刷新入料手臂offset();
        }

        private void uiButton6_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                try
                {
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车1第1列吸_X", Convert.ToDouble(uiDoubleUpDown75.Text), 设置信息.OffsetPoint.出料_梭车1第1列吸_X)) 设置信息.OffsetPoint.出料_梭车1第1列吸_X = Convert.ToDouble(uiDoubleUpDown75.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车1第1列吸_Y", Convert.ToDouble(uiDoubleUpDown74.Text), 设置信息.OffsetPoint.出料_梭车1第1列吸_Y)) 设置信息.OffsetPoint.出料_梭车1第1列吸_Y = Convert.ToDouble(uiDoubleUpDown74.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车1第1列吸_Z", Convert.ToDouble(uiDoubleUpDown73.Text), 设置信息.OffsetPoint.出料_梭车1第1列吸_Z)) 设置信息.OffsetPoint.出料_梭车1第1列吸_Z = Convert.ToDouble(uiDoubleUpDown73.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车1第2列吸_X", Convert.ToDouble(uiDoubleUpDown17.Text), 设置信息.OffsetPoint.出料_梭车1第2列吸_X)) 设置信息.OffsetPoint.出料_梭车1第2列吸_X = Convert.ToDouble(uiDoubleUpDown17.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车1第2列吸_Y", Convert.ToDouble(uiDoubleUpDown18.Text), 设置信息.OffsetPoint.出料_梭车1第2列吸_Y)) 设置信息.OffsetPoint.出料_梭车1第2列吸_Y = Convert.ToDouble(uiDoubleUpDown18.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车1第2列吸_Z", Convert.ToDouble(uiDoubleUpDown16.Text), 设置信息.OffsetPoint.出料_梭车1第2列吸_Z)) 设置信息.OffsetPoint.出料_梭车1第2列吸_Z = Convert.ToDouble(uiDoubleUpDown16.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Fix1放_X", Convert.ToDouble(uiDoubleUpDown87.Text), 设置信息.OffsetPoint.出料_Fix1放_X)) 设置信息.OffsetPoint.出料_Fix1放_X = Convert.ToDouble(uiDoubleUpDown87.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Fix1放_Y", Convert.ToDouble(uiDoubleUpDown86.Text), 设置信息.OffsetPoint.出料_Fix1放_Y)) 设置信息.OffsetPoint.出料_Fix1放_Y = Convert.ToDouble(uiDoubleUpDown86.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Fix1放_Z", Convert.ToDouble(uiDoubleUpDown85.Text), 设置信息.OffsetPoint.出料_Fix1放_Z)) 设置信息.OffsetPoint.出料_Fix1放_Z = Convert.ToDouble(uiDoubleUpDown85.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车_变距", Convert.ToDouble(uiDoubleUpDown33.Text), 设置信息.OffsetPoint.出料_梭车_变距)) 设置信息.OffsetPoint.出料_梭车_变距 = Convert.ToDouble(uiDoubleUpDown33.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_AF_变距", Convert.ToDouble(uiDoubleUpDown34.Text), 设置信息.OffsetPoint.出料_AF_变距)) 设置信息.OffsetPoint.出料_AF_变距 = Convert.ToDouble(uiDoubleUpDown34.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车2第1列吸_X", Convert.ToDouble(uiDoubleUpDown31.Text), 设置信息.OffsetPoint.出料_梭车2第1列吸_X)) 设置信息.OffsetPoint.出料_梭车2第1列吸_X = Convert.ToDouble(uiDoubleUpDown31.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车2第1列吸_Y", Convert.ToDouble(uiDoubleUpDown32.Text), 设置信息.OffsetPoint.出料_梭车2第1列吸_Y)) 设置信息.OffsetPoint.出料_梭车2第1列吸_Y = Convert.ToDouble(uiDoubleUpDown32.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车2第1列吸_Z", Convert.ToDouble(uiDoubleUpDown30.Text), 设置信息.OffsetPoint.出料_梭车2第1列吸_Z)) 设置信息.OffsetPoint.出料_梭车2第1列吸_Z = Convert.ToDouble(uiDoubleUpDown30.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车2第2列吸_X", Convert.ToDouble(uiDoubleUpDown20.Text), 设置信息.OffsetPoint.出料_梭车2第2列吸_X)) 设置信息.OffsetPoint.出料_梭车2第2列吸_X = Convert.ToDouble(uiDoubleUpDown20.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车2第2列吸_Y", Convert.ToDouble(uiDoubleUpDown21.Text), 设置信息.OffsetPoint.出料_梭车2第2列吸_Y)) 设置信息.OffsetPoint.出料_梭车2第2列吸_Y = Convert.ToDouble(uiDoubleUpDown21.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_梭车2第2列吸_Z", Convert.ToDouble(uiDoubleUpDown19.Text), 设置信息.OffsetPoint.出料_梭车2第2列吸_Z)) 设置信息.OffsetPoint.出料_梭车2第2列吸_Z = Convert.ToDouble(uiDoubleUpDown19.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Fix2放_X", Convert.ToDouble(uiDoubleUpDown96.Text), 设置信息.OffsetPoint.出料_Fix2放_X)) 设置信息.OffsetPoint.出料_Fix2放_X = Convert.ToDouble(uiDoubleUpDown96.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Fix2放_Y", Convert.ToDouble(uiDoubleUpDown95.Text), 设置信息.OffsetPoint.出料_Fix2放_Y)) 设置信息.OffsetPoint.出料_Fix2放_Y = Convert.ToDouble(uiDoubleUpDown95.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Fix2放_Z", Convert.ToDouble(uiDoubleUpDown94.Text), 设置信息.OffsetPoint.出料_Fix2放_Z)) 设置信息.OffsetPoint.出料_Fix2放_Z = Convert.ToDouble(uiDoubleUpDown94.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Auto1放_X", Convert.ToDouble(uiDoubleUpDown90.Text), 设置信息.OffsetPoint.出料_Auto1放_X)) 设置信息.OffsetPoint.出料_Auto1放_X = Convert.ToDouble(uiDoubleUpDown90.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Auto1放_Y", Convert.ToDouble(uiDoubleUpDown89.Text), 设置信息.OffsetPoint.出料_Auto1放_Y)) 设置信息.OffsetPoint.出料_Auto1放_Y = Convert.ToDouble(uiDoubleUpDown89.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Auto1放_Z", Convert.ToDouble(uiDoubleUpDown88.Text), 设置信息.OffsetPoint.出料_Auto1放_Z)) 设置信息.OffsetPoint.出料_Auto1放_Z = Convert.ToDouble(uiDoubleUpDown88.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Auto2放_X", Convert.ToDouble(uiDoubleUpDown93.Text), 设置信息.OffsetPoint.出料_Auto2放_X)) 设置信息.OffsetPoint.出料_Auto2放_X = Convert.ToDouble(uiDoubleUpDown93.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Auto2放_Y", Convert.ToDouble(uiDoubleUpDown92.Text), 设置信息.OffsetPoint.出料_Auto2放_Y)) 设置信息.OffsetPoint.出料_Auto2放_Y = Convert.ToDouble(uiDoubleUpDown92.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Auto2放_Z", Convert.ToDouble(uiDoubleUpDown91.Text), 设置信息.OffsetPoint.出料_Auto2放_Z)) 设置信息.OffsetPoint.出料_Auto2放_Z = Convert.ToDouble(uiDoubleUpDown91.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Fix3放_X", Convert.ToDouble(uiDoubleUpDown84.Text), 设置信息.OffsetPoint.出料_Fix3放_X)) 设置信息.OffsetPoint.出料_Fix3放_X = Convert.ToDouble(uiDoubleUpDown84.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Fix3放_Y", Convert.ToDouble(uiDoubleUpDown83.Text), 设置信息.OffsetPoint.出料_Fix3放_Y)) 设置信息.OffsetPoint.出料_Fix3放_Y = Convert.ToDouble(uiDoubleUpDown83.Text);
                    if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "出料_Fix3放_Z", Convert.ToDouble(uiDoubleUpDown82.Text), 设置信息.OffsetPoint.出料_Fix3放_Z)) 设置信息.OffsetPoint.出料_Fix3放_Z = Convert.ToDouble(uiDoubleUpDown82.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败,请检查数据格式是否正确!{ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void uiButton5_Click(object sender, EventArgs e)
        {
            刷新出料手臂offset();
        }

        private void uiButton8_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_梭车1左吸_X", Convert.ToDouble(uiDoubleUpDown62.Text), 设置信息.OffsetPoint.测试_梭车1左吸_X)) 设置信息.OffsetPoint.测试_梭车1左吸_X = Convert.ToDouble(uiDoubleUpDown62.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_梭车1左吸_Z", Convert.ToDouble(uiDoubleUpDown61.Text), 设置信息.OffsetPoint.测试_梭车1左吸_Z)) 设置信息.OffsetPoint.测试_梭车1左吸_Z = Convert.ToDouble(uiDoubleUpDown61.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_梭车2左吸_X", Convert.ToDouble(uiDoubleUpDown64.Text), 设置信息.OffsetPoint.测试_梭车2左吸_X)) 设置信息.OffsetPoint.测试_梭车2左吸_X = Convert.ToDouble(uiDoubleUpDown64.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_梭车2左吸_Z", Convert.ToDouble(uiDoubleUpDown63.Text), 设置信息.OffsetPoint.测试_梭车2左吸_Z)) 设置信息.OffsetPoint.测试_梭车2左吸_Z = Convert.ToDouble(uiDoubleUpDown63.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_梭车1右放_X", Convert.ToDouble(uiDoubleUpDown70.Text), 设置信息.OffsetPoint.测试_梭车1右放_X)) 设置信息.OffsetPoint.测试_梭车1右放_X = Convert.ToDouble(uiDoubleUpDown70.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_梭车1右放_Z", Convert.ToDouble(uiDoubleUpDown69.Text), 设置信息.OffsetPoint.测试_梭车1右放_Z)) 设置信息.OffsetPoint.测试_梭车1右放_Z = Convert.ToDouble(uiDoubleUpDown69.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_梭车2右放_X", Convert.ToDouble(uiDoubleUpDown72.Text), 设置信息.OffsetPoint.测试_梭车2右放_X)) 设置信息.OffsetPoint.测试_梭车2右放_X = Convert.ToDouble(uiDoubleUpDown72.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_梭车2右放_Z", Convert.ToDouble(uiDoubleUpDown71.Text), 设置信息.OffsetPoint.测试_梭车2右放_Z)) 设置信息.OffsetPoint.测试_梭车2右放_Z = Convert.ToDouble(uiDoubleUpDown71.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site1吸_X", Convert.ToDouble(uiDoubleUpDown66.Text), 设置信息.OffsetPoint.测试_Site1吸_X)) 设置信息.OffsetPoint.测试_Site1吸_X = Convert.ToDouble(uiDoubleUpDown66.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site2吸_X", Convert.ToDouble(uiDoubleUpDown23.Text), 设置信息.OffsetPoint.测试_Site2吸_X)) 设置信息.OffsetPoint.测试_Site2吸_X = Convert.ToDouble(uiDoubleUpDown23.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site3吸_X", Convert.ToDouble(uiDoubleUpDown26.Text), 设置信息.OffsetPoint.测试_Site3吸_X)) 设置信息.OffsetPoint.测试_Site3吸_X = Convert.ToDouble(uiDoubleUpDown26.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site4吸_X", Convert.ToDouble(uiDoubleUpDown25.Text), 设置信息.OffsetPoint.测试_Site4吸_X)) 设置信息.OffsetPoint.测试_Site4吸_X = Convert.ToDouble(uiDoubleUpDown25.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site5吸_X", Convert.ToDouble(uiDoubleUpDown36.Text), 设置信息.OffsetPoint.测试_Site5吸_X)) 设置信息.OffsetPoint.测试_Site5吸_X = Convert.ToDouble(uiDoubleUpDown36.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site6吸_X", Convert.ToDouble(uiDoubleUpDown35.Text), 设置信息.OffsetPoint.测试_Site6吸_X)) 设置信息.OffsetPoint.测试_Site6吸_X = Convert.ToDouble(uiDoubleUpDown35.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site7吸_X", Convert.ToDouble(uiDoubleUpDown28.Text), 设置信息.OffsetPoint.测试_Site7吸_X)) 设置信息.OffsetPoint.测试_Site7吸_X = Convert.ToDouble(uiDoubleUpDown28.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site8吸_X", Convert.ToDouble(uiDoubleUpDown27.Text), 设置信息.OffsetPoint.测试_Site8吸_X)) 设置信息.OffsetPoint.测试_Site8吸_X = Convert.ToDouble(uiDoubleUpDown27.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site9吸_X", Convert.ToDouble(uiDoubleUpDown44.Text), 设置信息.OffsetPoint.测试_Site9吸_X)) 设置信息.OffsetPoint.测试_Site9吸_X = Convert.ToDouble(uiDoubleUpDown44.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site10吸_X", Convert.ToDouble(uiDoubleUpDown43.Text), 设置信息.OffsetPoint.测试_Site10吸_X)) 设置信息.OffsetPoint.测试_Site10吸_X = Convert.ToDouble(uiDoubleUpDown43.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site11吸_X", Convert.ToDouble(uiDoubleUpDown42.Text), 设置信息.OffsetPoint.测试_Site11吸_X)) 设置信息.OffsetPoint.测试_Site11吸_X = Convert.ToDouble(uiDoubleUpDown42.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site12吸_X", Convert.ToDouble(uiDoubleUpDown41.Text), 设置信息.OffsetPoint.测试_Site12吸_X)) 设置信息.OffsetPoint.测试_Site12吸_X = Convert.ToDouble(uiDoubleUpDown41.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site13吸_X", Convert.ToDouble(uiDoubleUpDown40.Text), 设置信息.OffsetPoint.测试_Site13吸_X)) 设置信息.OffsetPoint.测试_Site13吸_X = Convert.ToDouble(uiDoubleUpDown40.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site14吸_X", Convert.ToDouble(uiDoubleUpDown39.Text), 设置信息.OffsetPoint.测试_Site14吸_X)) 设置信息.OffsetPoint.测试_Site14吸_X = Convert.ToDouble(uiDoubleUpDown39.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site15吸_X", Convert.ToDouble(uiDoubleUpDown38.Text), 设置信息.OffsetPoint.测试_Site15吸_X)) 设置信息.OffsetPoint.测试_Site15吸_X = Convert.ToDouble(uiDoubleUpDown38.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site16吸_X", Convert.ToDouble(uiDoubleUpDown37.Text), 设置信息.OffsetPoint.测试_Site16吸_X)) 设置信息.OffsetPoint.测试_Site16吸_X = Convert.ToDouble(uiDoubleUpDown37.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site1吸_Z", Convert.ToDouble(uiDoubleUpDown137.Text), 设置信息.OffsetPoint.测试_Site1吸_Z)) 设置信息.OffsetPoint.测试_Site1吸_Z = Convert.ToDouble(uiDoubleUpDown137.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site2吸_Z", Convert.ToDouble(uiDoubleUpDown136.Text), 设置信息.OffsetPoint.测试_Site2吸_Z)) 设置信息.OffsetPoint.测试_Site2吸_Z = Convert.ToDouble(uiDoubleUpDown136.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site3吸_Z", Convert.ToDouble(uiDoubleUpDown135.Text), 设置信息.OffsetPoint.测试_Site3吸_Z)) 设置信息.OffsetPoint.测试_Site3吸_Z = Convert.ToDouble(uiDoubleUpDown135.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site4吸_Z", Convert.ToDouble(uiDoubleUpDown134.Text), 设置信息.OffsetPoint.测试_Site4吸_Z)) 设置信息.OffsetPoint.测试_Site4吸_Z = Convert.ToDouble(uiDoubleUpDown134.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site5吸_Z", Convert.ToDouble(uiDoubleUpDown133.Text), 设置信息.OffsetPoint.测试_Site5吸_Z)) 设置信息.OffsetPoint.测试_Site5吸_Z = Convert.ToDouble(uiDoubleUpDown133.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site6吸_Z", Convert.ToDouble(uiDoubleUpDown132.Text), 设置信息.OffsetPoint.测试_Site6吸_Z)) 设置信息.OffsetPoint.测试_Site6吸_Z = Convert.ToDouble(uiDoubleUpDown132.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site7吸_Z", Convert.ToDouble(uiDoubleUpDown131.Text), 设置信息.OffsetPoint.测试_Site7吸_Z)) 设置信息.OffsetPoint.测试_Site7吸_Z = Convert.ToDouble(uiDoubleUpDown131.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site8吸_Z", Convert.ToDouble(uiDoubleUpDown130.Text), 设置信息.OffsetPoint.测试_Site8吸_Z)) 设置信息.OffsetPoint.测试_Site8吸_Z = Convert.ToDouble(uiDoubleUpDown130.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site9吸_Z", Convert.ToDouble(uiDoubleUpDown129.Text), 设置信息.OffsetPoint.测试_Site9吸_Z)) 设置信息.OffsetPoint.测试_Site9吸_Z = Convert.ToDouble(uiDoubleUpDown129.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site10吸_Z", Convert.ToDouble(uiDoubleUpDown128.Text), 设置信息.OffsetPoint.测试_Site10吸_Z)) 设置信息.OffsetPoint.测试_Site10吸_Z = Convert.ToDouble(uiDoubleUpDown128.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site11吸_Z", Convert.ToDouble(uiDoubleUpDown127.Text), 设置信息.OffsetPoint.测试_Site11吸_Z)) 设置信息.OffsetPoint.测试_Site11吸_Z = Convert.ToDouble(uiDoubleUpDown127.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site12吸_Z", Convert.ToDouble(uiDoubleUpDown126.Text), 设置信息.OffsetPoint.测试_Site12吸_Z)) 设置信息.OffsetPoint.测试_Site12吸_Z = Convert.ToDouble(uiDoubleUpDown126.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site13吸_Z", Convert.ToDouble(uiDoubleUpDown125.Text), 设置信息.OffsetPoint.测试_Site13吸_Z)) 设置信息.OffsetPoint.测试_Site13吸_Z = Convert.ToDouble(uiDoubleUpDown125.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site14吸_Z", Convert.ToDouble(uiDoubleUpDown124.Text), 设置信息.OffsetPoint.测试_Site14吸_Z)) 设置信息.OffsetPoint.测试_Site14吸_Z = Convert.ToDouble(uiDoubleUpDown124.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site15吸_Z", Convert.ToDouble(uiDoubleUpDown123.Text), 设置信息.OffsetPoint.测试_Site15吸_Z)) 设置信息.OffsetPoint.测试_Site15吸_Z = Convert.ToDouble(uiDoubleUpDown123.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site16吸_Z", Convert.ToDouble(uiDoubleUpDown122.Text), 设置信息.OffsetPoint.测试_Site16吸_Z)) 设置信息.OffsetPoint.测试_Site16吸_Z = Convert.ToDouble(uiDoubleUpDown122.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site1放_X", Convert.ToDouble(uiDoubleUpDown81.Text), 设置信息.OffsetPoint.测试_Site1放_X)) 设置信息.OffsetPoint.测试_Site1放_X = Convert.ToDouble(uiDoubleUpDown81.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site2放_X", Convert.ToDouble(uiDoubleUpDown80.Text), 设置信息.OffsetPoint.测试_Site2放_X)) 设置信息.OffsetPoint.测试_Site2放_X = Convert.ToDouble(uiDoubleUpDown80.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site3放_X", Convert.ToDouble(uiDoubleUpDown79.Text), 设置信息.OffsetPoint.测试_Site3放_X)) 设置信息.OffsetPoint.测试_Site3放_X = Convert.ToDouble(uiDoubleUpDown79.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site4放_X", Convert.ToDouble(uiDoubleUpDown78.Text), 设置信息.OffsetPoint.测试_Site4放_X)) 设置信息.OffsetPoint.测试_Site4放_X = Convert.ToDouble(uiDoubleUpDown78.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site5放_X", Convert.ToDouble(uiDoubleUpDown77.Text), 设置信息.OffsetPoint.测试_Site5放_X)) 设置信息.OffsetPoint.测试_Site5放_X = Convert.ToDouble(uiDoubleUpDown77.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site6放_X", Convert.ToDouble(uiDoubleUpDown76.Text), 设置信息.OffsetPoint.测试_Site6放_X)) 设置信息.OffsetPoint.测试_Site6放_X = Convert.ToDouble(uiDoubleUpDown76.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site7放_X", Convert.ToDouble(uiDoubleUpDown68.Text), 设置信息.OffsetPoint.测试_Site7放_X)) 设置信息.OffsetPoint.测试_Site7放_X = Convert.ToDouble(uiDoubleUpDown68.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site8放_X", Convert.ToDouble(uiDoubleUpDown67.Text), 设置信息.OffsetPoint.测试_Site8放_X)) 设置信息.OffsetPoint.测试_Site8放_X = Convert.ToDouble(uiDoubleUpDown67.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site9放_X", Convert.ToDouble(uiDoubleUpDown65.Text), 设置信息.OffsetPoint.测试_Site9放_X)) 设置信息.OffsetPoint.测试_Site9放_X = Convert.ToDouble(uiDoubleUpDown65.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site10放_X", Convert.ToDouble(uiDoubleUpDown54.Text), 设置信息.OffsetPoint.测试_Site10放_X)) 设置信息.OffsetPoint.测试_Site10放_X = Convert.ToDouble(uiDoubleUpDown54.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site11放_X", Convert.ToDouble(uiDoubleUpDown53.Text), 设置信息.OffsetPoint.测试_Site11放_X)) 设置信息.OffsetPoint.测试_Site11放_X = Convert.ToDouble(uiDoubleUpDown53.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site12放_X", Convert.ToDouble(uiDoubleUpDown52.Text), 设置信息.OffsetPoint.测试_Site12放_X)) 设置信息.OffsetPoint.测试_Site12放_X = Convert.ToDouble(uiDoubleUpDown52.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site13放_X", Convert.ToDouble(uiDoubleUpDown48.Text), 设置信息.OffsetPoint.测试_Site13放_X)) 设置信息.OffsetPoint.测试_Site13放_X = Convert.ToDouble(uiDoubleUpDown48.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site14放_X", Convert.ToDouble(uiDoubleUpDown47.Text), 设置信息.OffsetPoint.测试_Site14放_X)) 设置信息.OffsetPoint.测试_Site14放_X = Convert.ToDouble(uiDoubleUpDown47.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site15放_X", Convert.ToDouble(uiDoubleUpDown46.Text), 设置信息.OffsetPoint.测试_Site15放_X)) 设置信息.OffsetPoint.测试_Site15放_X = Convert.ToDouble(uiDoubleUpDown46.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site16放_X", Convert.ToDouble(uiDoubleUpDown45.Text), 设置信息.OffsetPoint.测试_Site16放_X)) 设置信息.OffsetPoint.测试_Site16放_X = Convert.ToDouble(uiDoubleUpDown45.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site1放_Z", Convert.ToDouble(uiDoubleUpDown121.Text), 设置信息.OffsetPoint.测试_Site1放_Z)) 设置信息.OffsetPoint.测试_Site1放_Z = Convert.ToDouble(uiDoubleUpDown121.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site2放_Z", Convert.ToDouble(uiDoubleUpDown120.Text), 设置信息.OffsetPoint.测试_Site2放_Z)) 设置信息.OffsetPoint.测试_Site2放_Z = Convert.ToDouble(uiDoubleUpDown120.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site3放_Z", Convert.ToDouble(uiDoubleUpDown119.Text), 设置信息.OffsetPoint.测试_Site3放_Z)) 设置信息.OffsetPoint.测试_Site3放_Z = Convert.ToDouble(uiDoubleUpDown119.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site4放_Z", Convert.ToDouble(uiDoubleUpDown118.Text), 设置信息.OffsetPoint.测试_Site4放_Z)) 设置信息.OffsetPoint.测试_Site4放_Z = Convert.ToDouble(uiDoubleUpDown118.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site5放_Z", Convert.ToDouble(uiDoubleUpDown117.Text), 设置信息.OffsetPoint.测试_Site5放_Z)) 设置信息.OffsetPoint.测试_Site5放_Z = Convert.ToDouble(uiDoubleUpDown117.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site6放_Z", Convert.ToDouble(uiDoubleUpDown116.Text), 设置信息.OffsetPoint.测试_Site6放_Z)) 设置信息.OffsetPoint.测试_Site6放_Z = Convert.ToDouble(uiDoubleUpDown116.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site7放_Z", Convert.ToDouble(uiDoubleUpDown115.Text), 设置信息.OffsetPoint.测试_Site7放_Z)) 设置信息.OffsetPoint.测试_Site7放_Z = Convert.ToDouble(uiDoubleUpDown115.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site8放_Z", Convert.ToDouble(uiDoubleUpDown114.Text), 设置信息.OffsetPoint.测试_Site8放_Z)) 设置信息.OffsetPoint.测试_Site8放_Z = Convert.ToDouble(uiDoubleUpDown114.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site9放_Z", Convert.ToDouble(uiDoubleUpDown113.Text), 设置信息.OffsetPoint.测试_Site9放_Z)) 设置信息.OffsetPoint.测试_Site9放_Z = Convert.ToDouble(uiDoubleUpDown113.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site10放_Z", Convert.ToDouble(uiDoubleUpDown112.Text), 设置信息.OffsetPoint.测试_Site10放_Z)) 设置信息.OffsetPoint.测试_Site10放_Z = Convert.ToDouble(uiDoubleUpDown112.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site11放_Z", Convert.ToDouble(uiDoubleUpDown111.Text), 设置信息.OffsetPoint.测试_Site11放_Z)) 设置信息.OffsetPoint.测试_Site11放_Z = Convert.ToDouble(uiDoubleUpDown111.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site12放_Z", Convert.ToDouble(uiDoubleUpDown110.Text), 设置信息.OffsetPoint.测试_Site12放_Z)) 设置信息.OffsetPoint.测试_Site12放_Z = Convert.ToDouble(uiDoubleUpDown110.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site13放_Z", Convert.ToDouble(uiDoubleUpDown109.Text), 设置信息.OffsetPoint.测试_Site13放_Z)) 设置信息.OffsetPoint.测试_Site13放_Z = Convert.ToDouble(uiDoubleUpDown109.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site14放_Z", Convert.ToDouble(uiDoubleUpDown105.Text), 设置信息.OffsetPoint.测试_Site14放_Z)) 设置信息.OffsetPoint.测试_Site14放_Z = Convert.ToDouble(uiDoubleUpDown105.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site15放_Z", Convert.ToDouble(uiDoubleUpDown104.Text), 设置信息.OffsetPoint.测试_Site15放_Z)) 设置信息.OffsetPoint.测试_Site15放_Z = Convert.ToDouble(uiDoubleUpDown104.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "测试_Site16放_Z", Convert.ToDouble(uiDoubleUpDown103.Text), 设置信息.OffsetPoint.测试_Site16放_Z)) 设置信息.OffsetPoint.测试_Site16放_Z = Convert.ToDouble(uiDoubleUpDown103.Text);

            }
        }

        private void uiButton7_Click(object sender, EventArgs e)
        {
            刷新测试手臂offset();
        }

        private void 刷新Offset(string tabpagetext)
        {
            if (tabpagetext == "入料手臂")
            {
                刷新入料手臂offset();
            }
            else if (tabpagetext == "出料手臂")
            {
                刷新出料手臂offset();
            }
            else if (tabpagetext == "测试手臂")
            {
                刷新测试手臂offset();
            }
            else if (tabpagetext == "梭车")
            {
                刷新梭车offset();
            }
            else if (tabpagetext == "抓盘手臂")
            {
                刷新抓盘手臂offset();
            }
        }

        private void uiTabControl2_Selecting(object sender, TabControlCancelEventArgs e)
        {
            刷新Offset(e.TabPage.Text);
        }

        private void uiButton10_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "梭车2_前_Y", Convert.ToDouble(uiDoubleUpDown57.Text), 设置信息.OffsetPoint.梭车2_前_Y)) 设置信息.OffsetPoint.梭车2_前_Y = Convert.ToDouble(uiDoubleUpDown57.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "梭车2_后_Y", Convert.ToDouble(uiDoubleUpDown56.Text), 设置信息.OffsetPoint.梭车2_后_Y)) 设置信息.OffsetPoint.梭车2_后_Y = Convert.ToDouble(uiDoubleUpDown56.Text);
            }
        }

        private void uiButton12_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("请确认是否保存!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "抓盘_Empty_X", Convert.ToDouble(uiDoubleUpDown55.Text), 设置信息.OffsetPoint.抓盘_Empty_X)) 设置信息.OffsetPoint.抓盘_Empty_X = Convert.ToDouble(uiDoubleUpDown55.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "抓盘_Load_X", Convert.ToDouble(uiDoubleUpDown58.Text), 设置信息.OffsetPoint.抓盘_Load_X)) 设置信息.OffsetPoint.抓盘_Load_X = Convert.ToDouble(uiDoubleUpDown58.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "抓盘_Auto1_X", Convert.ToDouble(uiDoubleUpDown60.Text), 设置信息.OffsetPoint.抓盘_Auto1_X)) 设置信息.OffsetPoint.抓盘_Auto1_X = Convert.ToDouble(uiDoubleUpDown60.Text);
                if (UpdateOffsetPointData(设置信息.档案名称, 设置信息.OffsetPoint.微调点位名称, "抓盘_Auto2_X", Convert.ToDouble(uiDoubleUpDown59.Text), 设置信息.OffsetPoint.抓盘_Auto2_X)) 设置信息.OffsetPoint.抓盘_Auto2_X = Convert.ToDouble(uiDoubleUpDown59.Text);
            }
        }

        private void uiButton11_Click(object sender, EventArgs e)
        {
            刷新抓盘手臂offset();
        }
    }
}
