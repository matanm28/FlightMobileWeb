using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlightControlWeb {
    using System;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using FlightMobileWeb.Controllers;
    using FlightSimulatorApp.Model;

    public class Startup {
        private const int SecondsToTimeOut = 5;

        public Startup(IWebHostEnvironment env) {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                                                                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddJsonFile(
                                                                               $"appsettings.{env.EnvironmentName}.json",
                                                                               optional: true).AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; private set; }

        public ILifetimeScope AutofacContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpClient<ScreenshotController>(
                    client => {
                        client.BaseAddress = new Uri(Configuration.GetSection("FlightGear")["http_url"]);
                        client.Timeout = TimeSpan.FromSeconds(SecondsToTimeOut);
                        client.DefaultRequestHeaders.Add("Accept", "image/jpeg");
                    });
            services.AddMvc().AddControllersAsServices();
            services.AddOptions();
        }

        public void ConfigureContainer(ContainerBuilder builder) {
            builder.RegisterType<TelnetClient>().As<ITelnetClient>();
            FlightGearClient client = new FlightGearClient(new TelnetClient())
                                              {
                                                      IP = this.Configuration.GetSection("FlightGear")["tcp_ip"],
                                                      Port = int.Parse(this.Configuration.GetSection("FlightGear")["tcp_port"])
                                              };
            client.Start();
            builder.Register(c => client).As<IFlightGearClient>().SingleInstance();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                //ScreenshotController.debugMode = true;
            }

            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
