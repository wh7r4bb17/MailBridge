using NLog;
using System.Windows;

namespace MailBridge.Wpf
    {
    public partial class App : Application
        {
        #region Protected Methods

        protected override void OnExit(ExitEventArgs e)
            {
            LogManager.Shutdown();
            base.OnExit(e);
            }

        protected override void OnStartup(StartupEventArgs e)
            {
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");

            base.OnStartup(e);
            }

        #endregion Protected Methods
        }
    }