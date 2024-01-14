namespace PixelService
{
    using Confluent.Kafka;

    using PixelService.Application.Messaging;
    using PixelService.Infrastructure.CrossCutting.Helpers;
    using PixelService.Infrastructure.CrossCutting.Settings;
    using PixelService.Messaging.Events.Tracks;

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

            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddScoped<ITrackMessagingService, TrackMessagingService>();

            AddProducers(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void AddProducers(IServiceCollection services)
        {
            var kafkaSettings = GetKafkaSettings(services.BuildServiceProvider());
            var producerSettings = kafkaSettings.Producers[typeof(TrackEvent).Name.ToString()];
            
            var producerconfig = new ProducerConfig
            {
                BootstrapServers = string.Join(',', kafkaSettings.BootstrapServers),
                Acks = producerSettings.Acks,
                MessageSendMaxRetries = producerSettings.MessageSendMaxRetries
            };

            var producer = new ProducerBuilder<Null, TrackEvent>(producerconfig)
                   .SetKeySerializer(Serializers.Null)
                   .SetValueSerializer(new JsonSerializer<TrackEvent>())
                   .Build();

            services.AddSingleton<IProducer<Null, TrackEvent>>(producer);
        }

        private static KafkaSettings GetKafkaSettings(IServiceProvider provider)
        {
            return provider.GetService<IConfiguration>()
                .GetSection("Kafka")
                .Get<KafkaSettings>();
        }
    }
}
