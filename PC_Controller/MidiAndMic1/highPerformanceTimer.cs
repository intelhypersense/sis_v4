using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
namespace highPerformanceTimer
{
    public class high_performance_timer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long start_time, end_time;
        private long now_time, pre_now_time;
        private long freq;
        private double nextTimer = 0.0;
        private double delay = 0;
        public high_performance_timer()
        {
            start_time = 0;
            end_time = 0;

            pre_now_time = now_time = 0;

            if(QueryPerformanceFrequency(out freq) == false) {
                throw new Win32Exception();
            }
        }

        public void enable()
        {
            Thread.Sleep(0);

            QueryPerformanceCounter(out start_time);
            QueryPerformanceCounter(out now_time);
            nextTimer = (double)now_time;
        }
        public void reset()
        {
            QueryPerformanceCounter(out start_time);
            QueryPerformanceCounter(out now_time);
            nextTimer = (double)now_time;
        }
        public void disable()
        {
            QueryPerformanceCounter(out end_time);
        }

        public int get_ms_igap()
        {
            pre_now_time = now_time;
            QueryPerformanceCounter(out now_time);
            return (int)(((double)(now_time - pre_now_time) / (double)freq) * 1000);
        }

        public double get_ms_dgap()
        {
            pre_now_time = now_time;
            QueryPerformanceCounter(out now_time);
            return ((double)(now_time - pre_now_time) / (double)freq) * 1000;
        }

        public double get_igap()
        {
            QueryPerformanceCounter(out now_time);
            return (int)(((double)(now_time - start_time) / (double)freq) * 1000);
        }
        public double get_dgap()
        {
            QueryPerformanceCounter(out now_time);
            return (((double)(now_time - start_time) / (double)freq) * 1000);
        }
        public void get_ugap(double time)
        {
            nextTimer += time*0.001* (double)freq;
            QueryPerformanceCounter(out now_time);
            while(((double)now_time - delay) < nextTimer) {
                QueryPerformanceCounter(out now_time);
            }
        }
        public void setDelay(int delayInt)
        {
            delay = (double)delayInt * 0.001 * (double)freq;
        }
    }
}