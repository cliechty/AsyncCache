# Async Cache

.net Memory Cache wrapper which will refresh the cache in a background thread without blocking after a specified time if the cached value is requested.``

Installation
============

Install using nuget. The Package is named AsyncCache (not yet available)

Usage
=====

``` c#
string value = Cacher.Get(cacheKey: "theKey", 
    cachedOperation: () => someSlowFunctionCall("parameter"), 
    lifetimeInMinutes: 10);
```

The code above will cause Cacher to make 1 call to someSlowFunctionCall the first time the code is executed. If there are multiple threads calling Cacher.Get with the same key while someSlowFunction is processing the intial load they will be blocked until someSlowFunction returns. 
'someSlowFunction' will not be called again until 10 minutes have passed and there is another call to Cacher.Get with the cacheKey 'theKey'
If you call Cacher.Get with the cacheKey: 'theKey' again Cacher will return the cached value.
