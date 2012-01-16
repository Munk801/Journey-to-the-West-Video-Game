using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace GameLoop
{
    class PreciseTimer
    {
        // Since we are calling C, Don't manage security checks
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport ("kernel32")]
        // Retrieves the frequency of the high resolution performance counter
        private static extern bool QueryPerformanceFrequency(ref long PerformanceFrequency);
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32")]
        // Retrieves the current value of the high-resolution performance counter.
        private static extern bool QueryPerformanceCounter(ref long PerformanceCount);
        long _ticksPerSecond = 0;
        long _previousElapsedTime = 0;

        public PreciseTimer()
        {
            QueryPerformanceFrequency(ref _ticksPerSecond);
            GetElapsedTime();
        }

        public double GetElapsedTime()
        {
            long time = 0;
            QueryPerformanceCounter(ref time);
            double elapsedTime = (double)(time - _previousElapsedTime);
            _previousElapsedTime = time;
            return elapsedTime;
        }
    }
}
