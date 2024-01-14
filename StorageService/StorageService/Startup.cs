namespace StorageService
{
    using StorageService.Application.Messaging.Handlers;
    using StorageService.Infrastructure.CrossCutting.Helpers;
    using StorageService.Infrastructure.CrossCutting.Settings;
    using StorageService.Messaging;
    using StorageService.Messaging.Tracks;

    using Confluent.Kafka;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(this.Configuration);
            services.AddSingleton(GetKafkaSettings);
            services.AddSingleton(GetServiceSettings);

            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            services.AddSingleton<ITrackEventHandler, TrackEventHandler>();

            AddConsumers(services);
            services.AddHostedService<TrackEventsConsumer>();
        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            var serviceSettings = app.ApplicationServices.GetService<ServiceSettings>();

            string filePath = serviceSettings.StorageFilePath;
            string directoryPath = Path.GetDirectoryName(filePath);
            bool fileExists = File.Exists(filePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!fileExists)
            {
                File.Create(filePath).Close();
            }

            logger.LogInformation("StorageService started.");
        }

        private void AddConsumers(IServiceCollection services)
        {
            var kafkaSettings = GetKafkaSettings(services.BuildServiceProvider());
            var consumerSettings = kafkaSettings.Consumers[typeof(TrackEvent).Name.ToString()];

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = string.Join(',', kafkaSettings.BootstrapServers),
                AutoOffsetReset = consumerSettings.AutoOffsetReset,
                GroupId = consumerSettings.GroupId
            };

            var consumer = new ConsumerBuilder<Ignore, TrackEvent>(consumerConfig)
                   .SetValueDeserializer(new JsonDeserializer<TrackEvent>())
                   .Build();

            services.AddSingleton<IConsumer<Ignore, TrackEvent>>(consumer);
        }

        private static KafkaSettings GetKafkaSettings(IServiceProvider provider)
        {
            return provider.GetService<IConfiguration>()
                .GetSection("Kafka")
                .Get<KafkaSettings>();
        }

        private static ServiceSettings GetServiceSettings(IServiceProvider provider)
        {
            return provider.GetService<IConfiguration>()
                .GetSection("ServiceSettings")
                .Get<ServiceSettings>();
        }
    }
}
