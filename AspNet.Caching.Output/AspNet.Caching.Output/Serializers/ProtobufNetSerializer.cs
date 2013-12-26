using System.IO;
using ProtoBuf;

namespace AspNet.Caching.Output.Serializers
{
    public class ProtobufNetSerializer<TType> : ISerializer<TType> where TType : class
    {
        public TType Deserialize(byte[] value)
        {
            using (var ms = new MemoryStream(value))
            {
                return Serializer.Deserialize<TType>(ms);
            }
        }

        public byte[] Serialize(TType value)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, value);
                return ms.ToArray();
            }
        }
    }
}