using System.IO;
using ServiceStack.Text;

namespace AspNet.Caching.Output.Serializers
{
    public class ServiceStackSerializer<TType> : ISerializer<TType> where TType : class
    {
        public TType Deserialize(byte[] value)
        {
            using (var ms = new MemoryStream(value))
            {
                return JsonSerializer.DeserializeFromStream(typeof (TType), ms) as TType;
            }
        }

        public byte[] Serialize(TType value)
        {
            using (var ms = new MemoryStream())
            {
                JsonSerializer.SerializeToStream(value, typeof (TType), ms);
                return ms.ToArray();
            }
        }
    }
}