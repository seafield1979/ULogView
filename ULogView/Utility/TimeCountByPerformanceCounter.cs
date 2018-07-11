using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ULogView.Utility
{
    public class TimeCountByPerformanceCounter
    {
        [DllImport("kernel32.dll")]
        static extern bool QueryPerformanceCounter(ref long lpPerformanceCount);
        [DllImport("kernel32.dll")]
        static extern bool QueryPerformanceFrequency(ref long lpFrequency);

        private long startCounter;

        public void Start()
        {
            QueryPerformanceCounter(ref startCounter);
        }

        public double GetSecTime()
        {
            long stopCounter = 0;
            QueryPerformanceCounter(ref stopCounter);
            long frequency = 0;
            QueryPerformanceFrequency(ref frequency);

            return (double)(stopCounter - startCounter) / frequency;
        }

        public string Format()
        {
            return "QueryPerformanceCounter/Frequency..{0}ms";
        }
    }
}
