namespace StorageService
{
    using Microsoft.AspNetCore;
    public class Program
    {
        private static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var builder = WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .UseStartup<Startup>();

            var app = builder.Build();

            app.Run();
        }
    }
}