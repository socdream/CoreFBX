using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreFBX.FBX
{
    public static class FBXExtensions
    {
        public static float[] FBXTimeToSeconds(this long[] times)
        {
            if (times != null)
                return times.Select(a => (float)a / 46186158000f).ToArray();

            return null;
        }
    }
}
