using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ttimer = System.Timers.Timer;

namespace H5509_V2
{
    public class HeatMapTimer
    {
        public Ttimer Timer { get; set; }

        public HeatMapTimer()
        {
            Timer = new Ttimer();
            Timer.Interval = 120 * 1000; //2分钟
            Timer.Elapsed += TimerHeatTick;
        }

        public DateTime StartHeatTime = DateTime.Now;

        public DateTime HeatTime = DateTime.Now;

        public TimeSpan CountDownTime
        {
            get { return StartHeatTime.AddSeconds(Timer.Interval / 1000) - DateTime.Now; }
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
                    Timer.Start();
                }
            }
        }

        private void TimerHeatTick(object sender, EventArgs e)
        {
            if (sender is Ttimer timer)
            {
                timer.Stop();
                Status = 2;
            }
        }
    }
}
