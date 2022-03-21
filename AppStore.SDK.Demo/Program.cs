using System.Threading.Tasks;
using Maranics.AppStore.SDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AppStore.SDK.Demo
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var service = host.Services.GetRequiredService<AppStoreTest>();
            await service.RunAsync();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json", false, true).AddEnvironmentVariables().Build();
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IConfiguration>(configuration);
                    services.ConfigureAppStore(configuration);
                    services.AddTransient<AppStoreTest>();
                });
        }
    }
}