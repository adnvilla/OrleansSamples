using GrainInterfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IClusterClient>(CreateClusterClient);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        private IClusterClient CreateClusterClient(IServiceProvider serviceProvider)
        {
            var client = new ClientBuilder()
                .UseLocalhostClustering()
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IValueGrain).Assembly))
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "AspNetSampleApp";
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            StartClientWithRetries(client).Wait();

            return client;
        }

        private static async Task StartClientWithRetries(IClusterClient client)
        {
            for (var i = 0; i < 5; i++)
            {
                try
                {
                    await client.Connect();
                    return;
                }
                catch (Exception)
                { }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}