using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AspNet.Caching.Output.Serializers
{
    public class BinarySerializer<TType> : ISerializer<TType> where TType : class
    {
        private readonly BinaryFormatter _formatter;

        public BinarySerializer()
        {
            _formatter = new BinaryFormatter();
        }

        public TType Deserialize(byte[] value)
        {
            using (var ms = new MemoryStream(value))
            {
                return _formatter.Deserialize(ms) as TType;
            }
        }

        public byte[] Serialize(TType value)
        {
            using (var ms = new MemoryStream())
            {
                _formatter.Serialize(ms, value);
                return ms.ToArray();
            }
        }
    }
}