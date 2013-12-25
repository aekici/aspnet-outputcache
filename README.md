aspnet-outputcache
==================

ASP.NET Output Cache Providers

Simple Usage
------------

Just use standard system.web/caching/outputCache config section.

```
  <system.web>
    <caching>
      <outputCache defaultProvider="RedisOutputCachingProvider">
        <providers>
          <clear />
          <add name="RedisOutputCachingProvider" type="AspNet.Caching.Output.Providers.RedisOutputCachingProvider, AspNet.Caching.Output"
              host="localhost" port="6379" />
        </providers>
      </outputCache>
    </caching>
  </system.web>
```




Note
----

First we planned to use protobuf-net for de/serialization, since protobuf-net relies on strongly-typed objects and System.Web.Caching objects are internal, we needed to go with BinaryFormatter.

Here is the [Microsoft Connect](http://social.msdn.microsoft.com/Forums/en-US/d2a7ea13-ec22-43da-b969-5193e5c3b616/internal-cachedvary-and-outputcacheentry-classes) link.

We're on it!


Serialization benchmarks
------------------------

Public servicestack [benchmarks](http://mono.servicestack.net/benchmarks/NorthwindDatabaseRowsSerialization.1000000-times.2010-02-06.html)