using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    public class Time
    {
        public static readonly Time Infinite = new Time(0x7fffffffffffffffL);
        public static readonly Time Zero = new Time(0);

        public const long UnitsPerSecond = 46186158000L;

        public Time(long time)
        {
            Value = time;
        }

        public long Value;

        public long Get()
        {
            return Value;
        }

        public double GetSecondDouble()
        {
            return Value / (double)UnitsPerSecond;
        }

        public long GetFrameCount()
        {
            return Value / 1539538600L;
        }
    }
}
