/*
 
"Get" (BookSleeve) returns a deferred byte[]. Wait to get the actual byte[], then use a MemoryStream over this byte[] to call Deserialize via protobuf-net.

BookSleeve is entirely async via Task, hence the need for either Wait or ContinueWith to access the byte[]
protobuf-net is entirely Stream-based, hence the need for MemoryStream to sit on top of a byte[]
 
 */

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Caching;

using AspNet.Caching.Output.Connection;
using AspNet.Caching.Output.Model;

using BookSleeve;

//using ProtoBuf;


namespace AspNet.Caching.Output.Providers
{
    public class RedisOutputCachingProvider : OutputCacheProvider
    {
        private RedisConnectionManager redisConnectionManager;
        private string host = "localhost";
        private int port = 6379;

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config["host"] != null && !String.IsNullOrWhiteSpace(config["host"]))
                host = config["host"];

            if (config["port"] != null && !String.IsNullOrWhiteSpace(config["port"]))
                port = Convert.ToInt32(config["port"]);

            redisConnectionManager = new RedisConnectionManager(host, port);

            base.Initialize(name, config);
        }

        public override object Get(string key)
        {
            RedisConnection redisConnection = redisConnectionManager.GetConnection();

            Task<byte[]> tb = redisConnection.Strings.Get(1, key);
            if (tb == null)
                return null;

            var value = redisConnection.Wait(tb);

            if (value == null)
                return null;

            // var cacheItem = Serializer.Deserialize<CacheItem>(new MemoryStream(value));
            
            var cacheItem = (CacheItem) new BinaryFormatter().Deserialize(new MemoryStream(value));

            //var cacheItem = (CacheItem) ServiceStack.Text.JsonSerializer.DeserializeFromStream(typeof(CacheItem), new MemoryStream(value));

            return cacheItem == null ? null : cacheItem.Data;

        }

        public override object Add(string key, object entry, DateTime utcExpiry)
        {
            this.Set(key, entry, utcExpiry);
            return entry;
        }

        public override void Set(string key, object entry, DateTime utcExpiry)
        {
            byte[] raw;
            using (var ms = new MemoryStream())
            {
                var cacheItem = new CacheItem { Key = key, Data = entry, Expiry = utcExpiry };

                // ProtoBuf.Serializer.Serialize(ms, new CacheItem { Key = key, Data = entry, Expiry = utcExpiry });
                
                new BinaryFormatter().Serialize(ms, cacheItem);

                // ServiceStack.Text.JsonSerializer.SerializeToStream(cacheItem, typeof(CacheItem), ms);

                raw = ms.ToArray();
            }

            RedisConnection redisConnection = redisConnectionManager.GetConnection();
            //TODO: discuss if acquire remote lock here is really required or not
            int retry = 0;
            while (retry < 3)
            {
                Task<bool> lockTakenTask = redisConnection.Strings.TakeLock(1, "lock:" + key, "L", 2);
                bool lockTaken = redisConnection.Wait(lockTakenTask);
                if (lockTaken)
                {
                    redisConnection.Strings.Set(1, key, raw);
                    redisConnection.Strings.ReleaseLock(1, "lock:" + key);
                    redisConnection.WaitAll();
                    break;
                }

                Thread.Sleep(500);
                retry++;
            }
        }

        public override void Remove(string key)
        {
            RedisConnection redisConnection = redisConnectionManager.GetConnection();

            Task<bool> remove = redisConnection.Keys.Remove(1, key);
            bool removed = redisConnection.Wait(remove);

            // we may need an exception handling in case of failures on "remove"
        }
    }

}
