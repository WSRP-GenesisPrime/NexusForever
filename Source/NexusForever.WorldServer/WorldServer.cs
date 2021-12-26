using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Hosting.WindowsServices;
using NexusForever.Shared.Configuration;
using NLog;

namespace NexusForever.WorldServer
{
    internal static class WorldServer
    {
        #if DEBUG
        private const string Title = "NexusForever: World Server (DEBUG)";
        #else
        private const string Title = "NexusForever: World Server (RELEASE)";
        #endif

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public static TimeSpan Uptime
        {
            get
            {
                return DateTime.UtcNow.Subtract(bootTime);
            }
        }
        private static DateTime bootTime;

        /// <summary>
        /// Internal unique id of the realm.
        /// </summary>
        public static ushort RealmId { get; private set; }

        /// <summary>
        /// Realm message of the day that is shown to players on login.
        /// </summary>
        public static string RealmMotd { get; set; }

        private static void Main()
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            // TODO: this really should be refactored
            ConfigurationManager<WorldServerConfiguration>.Instance.Initialise("WorldServer.json");
            RealmId   = ConfigurationManager<WorldServerConfiguration>.Instance.Config.RealmId;
            RealmMotd = ConfigurationManager<WorldServerConfiguration>.Instance.Config.MessageOfTheDay;

            IHostBuilder builder = new HostBuilder() //Host.CreateDefaultBuilder()
                // register world server service first since it needs to execute before the web host
                .ConfigureServices(sc =>
                {
                    sc.AddHostedService<HostedService>();
                })
                .ConfigureWebHostDefaults(wb =>
                {
                    WorldServerEmbeddedWebServer.Build(wb);
                })
                .UseWindowsService()
                .UseSystemd();

            if (!WindowsServiceHelpers.IsWindowsService() && !SystemdHelpers.IsSystemdService())
                Console.Title = Title;

            try
            {
                IHost host = builder.Build();
                host.Run();
            }
            catch (Exception e)
            {
                log.Fatal(e);
            }
        }
    }
}
