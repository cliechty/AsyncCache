using System;

namespace AsyncCache
{
    public class CacheSettings
    {
        private TimeSpan maxTimeInCache = TimeSpan.FromMinutes(10);

        public TimeSpan MaxTimeInCache
        {
            get { return this.maxTimeInCache; }
            set { this.maxTimeInCache = value; }
        }
    }
}
