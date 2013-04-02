using System;

namespace AsyncCache
{
    public static class Clock
    {
        public static Func<DateTime> Now = () => DateTime.Now;

        public static Func<DateTime> UtcNow = () => DateTime.UtcNow;
    }
}
