using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NLog;

namespace MailBridge.Core
    {
    /// <summary>
    /// Sends emails via SMTP using MailKit. This class has no dependency on any UI
    /// framework and can be used from WPF, console applications, background services,
    /// or any other .NET host.
    /// </summary>
    public sealed class SmtpService
        {
        #region Fields
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Public Methods

        /// <summary>
        /// Connects to the SMTP server described by <paramref name="request"/>, authenticates
        /// if requested, and sends the email.
        /// </summary>
        /// <param name="request">The SMTP connection, credential, and message details.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation is cancelled.</exception>
        /// <exception cref="MailKit.Security.AuthenticationException">Thrown if authentication fails.</exception>
        public async Task SendAsync(SmtpSendRequest request, CancellationToken cancellationToken = default)
            {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Host, nameof(request.Host));
            ArgumentException.ThrowIfNullOrWhiteSpace(request.FromEmail, nameof(request.FromEmail));

            List<string> toEmails = [.. request.ToEmails];

            if (!string.IsNullOrWhiteSpace(request.ToEmail))
                { toEmails.Insert(0, request.ToEmail); }

            if (toEmails.Count == 0)
                { throw new ArgumentException("At least one recipient must be set via ToEmail or ToEmails.", nameof(request)); }

            if (string.IsNullOrWhiteSpace(request.Body) && string.IsNullOrWhiteSpace(request.HtmlBody))
                { throw new ArgumentException("Either Body or HtmlBody must be set.", nameof(request)); }

            string host = request.Host;
            string fromEmail = request.FromEmail;
            string subject = request.Subject ?? string.Empty;

            string? username = null;
            string? password = null;

            if (request.UseAuthentication)
                {
                ArgumentException.ThrowIfNullOrWhiteSpace(request.Username, nameof(request.Username));
                ArgumentException.ThrowIfNullOrWhiteSpace(request.Password, nameof(request.Password));

                username = request.Username;
                password = request.Password;
                }

            using SmtpClient client = new()
                {
                Timeout = request.TimeoutMilliseconds
                };

            try
                {
                Logger.Debug($"Connecting to SMTP server {host}:{request.Port} using {request.SocketOptions}");
                await client.ConnectAsync(host, request.Port, request.SocketOptions, cancellationToken);
                Logger.Info($"Connected to SMTP server {host}:{request.Port}");

                if (username is not null && password is not null)
                    {
                    Logger.Debug($"Authenticating with SMTP server as {username}");
                    await client.AuthenticateAsync(username, password, cancellationToken);
                    Logger.Info($"Authenticated with SMTP server as {username}");
                    }

                MimeMessage message = new()
                    {
                    Priority = request.Priority
                    };

                message.From.Add(new MailboxAddress(request.FromName ?? string.Empty, fromEmail));

                foreach (string to in toEmails)
                    {
                    if (!string.IsNullOrWhiteSpace(to))
                        { message.To.Add(new MailboxAddress(string.Empty, to)); }
                    }

                foreach (string cc in request.CcEmails)
                    {
                    if (!string.IsNullOrWhiteSpace(cc))
                        { message.Cc.Add(new MailboxAddress(string.Empty, cc)); }
                    }

                foreach (string bcc in request.BccEmails)
                    {
                    if (!string.IsNullOrWhiteSpace(bcc))
                        { message.Bcc.Add(new MailboxAddress(string.Empty, bcc)); }
                    }

                if (!string.IsNullOrWhiteSpace(request.ReplyToEmail))
                    { message.ReplyTo.Add(new MailboxAddress(request.ReplyToName ?? string.Empty, request.ReplyToEmail)); }

                message.Subject = subject;

                BodyBuilder builder = new();

                if (!string.IsNullOrWhiteSpace(request.HtmlBody))
                    { builder.HtmlBody = request.HtmlBody; }

                if (!string.IsNullOrWhiteSpace(request.Body))
                    { builder.TextBody = request.Body; }

                foreach (string attachmentPath in request.Attachments)
                    {
                    if (!string.IsNullOrWhiteSpace(attachmentPath) && File.Exists(attachmentPath))
                        {
                        Logger.Debug($"Attaching file {attachmentPath}");
                        await builder.Attachments.AddAsync(attachmentPath, cancellationToken);
                        }
                    else
                        { Logger.Warn($"Attachment not found, skipping: {attachmentPath}"); }
                    }

                message.Body = builder.ToMessageBody();

                Logger.Debug($"Sending email from {fromEmail} to {string.Join(", ", toEmails)} with subject {subject}");
                await client.SendAsync(message, cancellationToken);

                Logger.Info($"Email sent successfully from {fromEmail} to {string.Join(", ", toEmails)}");
                await client.DisconnectAsync(true, cancellationToken);
                }
            catch (OperationCanceledException)
                {
                Logger.Warn($"SMTP operation to {host}:{request.Port} was cancelled");
                throw;
                }
            catch (AuthenticationException ex)
                {
                Logger.Error(ex, $"SMTP authentication failed for user {username} on {host}:{request.Port}");
                throw;
                }
            catch (Exception ex)
                {
                Logger.Error(ex, $"Failed to send email from {fromEmail} to {string.Join(", ", toEmails)} via {host}:{request.Port}");
                throw;
                }
            }

        #endregion Public Methods
        }
    }