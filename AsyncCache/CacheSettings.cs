using System;

namespace AsyncCache
{
    public class CacheSettings
    {
        private TimeSpan maxTimeInCache = TimeSpan.FromHours(1);

        public TimeSpan MaxTimeInCache
        {
            get { return this.maxTimeInCache; }
            set { this.maxTimeInCache = value; }
        }
    }
}
