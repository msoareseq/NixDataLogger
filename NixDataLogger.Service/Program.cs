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
                
                IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices(services =>
                    {
                        services.AddHostedService<DataLoggerWorker>();
                    })
                    .UseSerilog()
                    .Build();

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