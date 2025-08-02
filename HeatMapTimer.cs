using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ttimer = System.Timers.Timer;
using static H5509_V2.APPmaster;
using System.Diagnostics;

namespace H5509_V2
{
    public class HeatMapTimer
    {
        public Ttimer Timer { get; set; }

        public Ttimer WaitTimer { get; set; }

        public HeatMapTimer()
        {
            Timer = new Ttimer();
            Timer.Interval = IC加热时间 * 1000; //2分钟 达温后延时时间  IC加热时间
            Timer.Elapsed += TimerHeatTick;

            WaitTimer = new Ttimer();
            WaitTimer.Interval = 500;
            WaitTimer.Elapsed += WaitTimerHeatTick; //每500ms检测一次是否达温
        }

        public DateTime StartHeatTime = DateTime.Now;

        public DateTime HeatTime = DateTime.Now;

        public TimeSpan CountDownTime
        {
            get 
            { 
                TimeSpan time = StartHeatTime.AddSeconds(Timer.Interval / 1000) - DateTime.Now;
                if (time.TotalSeconds < 0) return new TimeSpan(0);
                else return time;
            }
        }

        private int status;

        public int Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                if (status == 1)
                {
                    if (温度模式 == "常温")
                    {
                        Timer.Start();
                        StartHeatTime = DateTime.Now;
                        //RunLog("StartHeatTime:" + StartHeatTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                    }
                    else
                    {
                        if (温控达温) //如果达温了直接开始倒计时
                        {
                            Timer.Start();
                            StartHeatTime = DateTime.Now;
                            //RunLog("StartHeatTime:" + StartHeatTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        }
                        else //如果未达温则等达温后再开始倒计时
                        {
                            WaitTimer.Start();
                        }
                    }
                }
            }
        }

        private void TimerHeatTick(object sender, EventArgs e)
        {
            Timer.Stop();
            Status = 2;
        }

        private void WaitTimerHeatTick(object sender, EventArgs e)
        {
            if (温控达温)
            {
                WaitTimer.Stop();
                Timer.Start();
                StartHeatTime = DateTime.Now;
                //RunLog("StartHeatTime:" + StartHeatTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
        }
    }
}
