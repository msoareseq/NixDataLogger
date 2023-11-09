using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace NixDataLogger.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string? baseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string defaultDir = @"C:\Nix\";

            if (WindowsServiceHelpers.IsWindowsService())
            {
                Log.Information("Running as a Windows Service");
            }
            else
            {
                Log.Information("Running as a Console Application");
            }


            string configFileName = Path.Combine(baseDir ?? defaultDir, "nix.json");
            ServiceConfiguration serviceConfig;




            
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(baseDir ?? defaultDir, "logs\\log.txt"), rollingInterval: RollingInterval.Day)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();

            

            try
            {
                Log.Information("Starting Nix...");
                Log.Information("Base Directory: {0}", baseDir);

                if (!File.Exists(configFileName))
                {
                    Log.Warning("Configuration file (nix.json) not found. Using default configuration file...");
                    serviceConfig = new ServiceConfiguration();
                }

                IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices(services =>
                    {
                        services.Configure<ServiceConfiguration>(config => config.LoadFromFile("nix.json"));
                        services.AddHostedService<DataLoggerWorker>();
                        services.AddHostedService<SyncWorker>();
                    })
                    .UseSerilog()
                    .Build();

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Log.Information("Nix terminated by user");
                    host.StopAsync().Wait();
                };



                host.Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Nix terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
            
            
            
            
            
        }
    }
}