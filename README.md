aspnet-outputcache
========================

ASP.NET Output Cache Providers

Simple Usage
-------------

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