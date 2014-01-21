using System;

using ProtoBuf;

namespace AspNet.Caching.Output.Model
{
    [ProtoContract, Serializable]
    public class CacheItem
    {
        [ProtoMember(1)]
        public string Key { get; set; }

        [ProtoMember(2, DynamicType = true)]
        public object  Data { get; set; }

        //

        //berkay git test aasdasd
        [ProtoMember(3)]
        public DateTime Expiry { get; set; }
    }
}
