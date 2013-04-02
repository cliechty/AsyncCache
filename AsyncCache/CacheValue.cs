using System;

namespace AsyncCache
{
    class CacheValue<T>
    {
        private CacheValueState currentState = CacheValueState.Loading;

        public T Value { get; set; }
        public DateTime ExpirationTime { get; set; }
        public CacheValueState CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }
    }
}
