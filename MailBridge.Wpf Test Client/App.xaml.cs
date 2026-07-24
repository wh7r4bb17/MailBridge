using System.IO;
using System.Windows;
using MailBridge.Core;
using Serilog;

namespace MailBridge.Wpf
    {
    public partial class App : Application
        {
        protected override void OnStartup(StartupEventArgs e)
            {
            string logDirectory = Path.Combine(AppSettings.GetSettingsDirectory(), "Logs");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LoggingConfig.LevelSwitch)
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "mailbridge_.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30)
                .CreateLogger();

            base.OnStartup(e);
            }

        protected override void OnExit(ExitEventArgs e)
            {
            Log.CloseAndFlush();
            base.OnExit(e);
            }
        }
    }