using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SinovadMediaServer.Transversal.Mapping
{
    public static class DTOMapperExtension
    {
        public static T MapTo<T>(this object value)
        {
            var serialized = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<T>(serialized);
        }

    }
}
