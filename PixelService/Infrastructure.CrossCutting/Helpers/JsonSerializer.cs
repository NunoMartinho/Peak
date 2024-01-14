namespace PixelService.Infrastructure.CrossCutting.Helpers
{
    using System.Text;
    using System.Threading.Tasks;

    using Confluent.Kafka;

    using Newtonsoft.Json;

    public class JsonSerializer<T> : IAsyncSerializer<T>
    {
        public Task<byte[]> SerializeAsync(T data, SerializationContext context)
        {
            string jsonString = JsonConvert.SerializeObject(data);
            return Task.FromResult(Encoding.UTF8.GetBytes(jsonString));
        }
    }
}
