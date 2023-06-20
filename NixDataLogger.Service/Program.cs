using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace NixDataLogger.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .CreateLogger();
            
            try
            {
                Log.Information("Starting Nix...");
                
                if (!File.Exists("nix.json"))
                {
                    Log.Warning("Configuration file (nix.json) not found. Using default configuration file...");
                }

                IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices(services =>
                    {
                        services.Configure<ServiceConfiguration>(config => config.LoadFromFile("nix.json"));
                        services.AddHostedService<DataLoggerWorker>();
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