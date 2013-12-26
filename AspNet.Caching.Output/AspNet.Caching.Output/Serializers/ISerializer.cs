using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AspNet.Caching.Output.Serializers
{
    public interface ISerializer<TType> where TType : class 
    {
        TType Deserialize(byte[] value);

        byte[] Serialize(TType value);
    }
}
