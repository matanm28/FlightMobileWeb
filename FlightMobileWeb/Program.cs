namespace FlightMobileWeb {
    using System.IO;
    using Autofac.Extensions.DependencyInjection;
    using FlightControlWeb;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    public class Program {
        public static void Main(string[] args) {
            var host = Host.CreateDefaultBuilder(args).UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureWebHostDefaults(
                    webHostBuilder => {
                        webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory()).UseIISIntegration().UseStartup<Startup>();
                    }).Build();
            

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(
                        webBuilder => {
                            webBuilder.UseStartup<Startup>();
                        });
    }
}
