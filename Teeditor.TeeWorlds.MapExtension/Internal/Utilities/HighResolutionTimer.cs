using System;
using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Extensions
{
    internal static class HighResolutionTimer
    {
        [DllImport("KERNEL32")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        /*
	        Function: time_get
		        Fetches a sample from a high resolution timer.
	        Returns:
		        Current value of the timer.
	        Remarks:
		        To know how fast the timer is ticking, see <time_freq>.
        */
        public static Int64 TimeGet()
        {
            Int64 last = 0;
            Int64 t;

            QueryPerformanceCounter(out t);

            if (t < last) /* for some reason, QPC can return values in the past */
                return last;

            last = t;
            return t;
        }

        /*
	        Function: time_freq
		        Returns the frequency of the high resolution timer.
	        Returns:
		        Returns the frequency of the high resolution timer.
        */
        public static Int64 GetTimeFreq()
        {
            Int64 t;
            QueryPerformanceFrequency(out t);
            return t;
        }
    }
}
