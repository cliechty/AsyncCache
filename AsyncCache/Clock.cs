using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCache
{
    public static class Clock
    {
        public static Func<DateTime> Now = () => DateTime.Now;

        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;
    }
}
