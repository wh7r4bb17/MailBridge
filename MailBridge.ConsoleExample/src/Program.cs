using MailKit.Security;
using MailBridge.Core;
using Serilog;
using Serilog.Events;

namespace MailBridge.ConsoleExample
    {
    /// <summary>
    /// Example demonstrating that MailBridge.Core can be used from a plain
    /// console application, with no dependency on WPF or any other UI framework.
    /// </summary>
    internal class Program
        {
        private static async Task Main(string[] args)
            {
            string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MailBridge", "Logs");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LoggingConfig.LevelSwitch)
                .WriteTo.Console()
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "mailbridge_.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30)
                .CreateLogger();

            LoggingConfig.LevelSwitch.MinimumLevel = LogEventLevel.Debug;

            SmtpService smtpService = new();

            SmtpSendRequest request = new()
                {
                Host = "smtp.example.com",
                Port = 587,
                SocketOptions = SecureSocketOptions.StartTls,
                FromEmail = "sender@example.com",
                FromName = "MailBridge Console Example",
                ToEmail = "recipient@example.com",
                Subject = "Test from console app",
                Body = "Hello from the console example project.",
                UseAuthentication = true,
                Username = "sender@example.com",
                Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? string.Empty,
                TimeoutMilliseconds = 30_000
                };

            try
                {
                await smtpService.SendAsync(request);
                Console.WriteLine("Email sent successfully.");
                }
            catch (Exception ex)
                {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                }
            finally
                {
                Log.CloseAndFlush();
                }
            }
        }
    }