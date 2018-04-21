using Grains;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace Silo
{
    public class Program
    {
        private static ISiloHost _silo;
        private static readonly ManualResetEvent SiloStopped = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            _silo = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "AspNetSampleApp";
                })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ValueGrain).Assembly).WithReferences())
                //.ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning).AddConsole())
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();

            Task.Run(StartSilo);

            AssemblyLoadContext.Default.Unloading += context =>
            {
                Task.Run(StopSilo);
                SiloStopped.WaitOne();
            };

            SiloStopped.WaitOne();
        }

        private static async Task StartSilo()
        {
            await _silo.StartAsync();
            Console.WriteLine("Silo started");
        }

        private static async Task StopSilo()
        {
            await _silo.StopAsync();
            Console.WriteLine("Silo stopped");
            SiloStopped.Set();
        }
    }
}
