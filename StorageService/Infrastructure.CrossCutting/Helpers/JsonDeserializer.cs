namespace StorageService.Infrastructure.CrossCutting.Helpers
{
    using System;
    using System.Text;

    using Confluent.Kafka;
    using Newtonsoft.Json;

    public class JsonDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull)
            {
                return default;
            }

            var jsonString = Encoding.UTF8.GetString(data.ToArray());
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
