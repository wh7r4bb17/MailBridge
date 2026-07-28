using MailBridge.Core;
using MailKit.Security;
using NLog;

namespace MailBridge.ConsoleExample
    {
    /// <summary>
    /// Example demonstrating that MailBridge.Core can be used from a plain
    /// console application, with no dependency on WPF or any other UI framework.
    /// </summary>
    internal class Program
        {
        #region Private Methods

        private static async Task Main(string[] args)
            {
            LogManager.Setup().LoadConfigurationFromFile("NLog.config");

            LoggingConfig.SetMinLevel(LogLevel.Debug);

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
                LogManager.Shutdown();
                }
            }

        #endregion Private Methods
        }
    }