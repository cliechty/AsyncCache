# Async Cache

.net Memory Cache wrapper that offloads refreshing cached values to background threads without blocking the caller.
This library is useful for caching operations which are time consuming and/or the results don't change often.

## Installation

Install using nuget. The Package is named AsyncCache (not yet available)

## Usage

### Adding and Retriving Values
Adding and retriving cached values uses the same function call.

``` c#
String value = Cacher.Get(cacheKey: "theKey", 
                        cachedOperation: () => someSlowFunctionCall("parameter"), 
                        refreshIn: Timespan.FromMinutes(10));
```

The code above will request the value for 'theKey' in the cache.

If the value is cached, then the cached value is returned.

If the value is not cached, the cachedOperation Func<T> will be called to supply and cache the value.

If refresh time has passed, then the currently cached value is returned and the cachedOperation Func<T> will be called in a background thread to refresh the cached value.

### Removing Values
``` c#
Cacher.Remove(cacheKey: "theKey");
```

## Todo
* Documentation
* Example project / demo
