using MailKit.Security;
using MimeKit;

namespace MailBridge.Core
    {
    /// <summary>
    /// Encapsulates all parameters required to send a single email via SMTP.
    /// </summary>
    public sealed class SmtpSendRequest
        {
        #region Properties

        /// <summary>
        /// File paths of attachments to include with the email.
        /// </summary>
        public IReadOnlyList<string> Attachments { get; init; } = [];

        /// <summary>
        /// Additional BCC (blind carbon copy) recipient email addresses.
        /// </summary>
        public IReadOnlyList<string> BccEmails { get; init; } = [];

        /// <summary>
        /// The plain-text email body. Used as a fallback for clients that cannot render HTML,
        /// or as the sole body if <see cref="HtmlBody"/> is not set.
        /// </summary>
        public string? Body { get; init; }

        /// <summary>
        /// Additional CC (carbon copy) recipient email addresses.
        /// </summary>
        public IReadOnlyList<string> CcEmails { get; init; } = [];

        /// <summary>
        /// The sender email address.
        /// </summary>
        public string? FromEmail { get; init; }

        /// <summary>
        /// The display name shown for the sender (e.g. "MailBridge Support").
        /// </summary>
        public string? FromName { get; init; }

        /// <summary>
        /// The SMTP server host name or IP address.
        /// </summary>
        public string? Host { get; init; }

        /// <summary>
        /// The HTML email body. If set, this takes precedence over <see cref="Body"/> for
        /// clients that support HTML rendering.
        /// </summary>
        public string? HtmlBody { get; init; }

        /// <summary>
        /// The password used for SMTP authentication, if <see cref="UseAuthentication"/> is true.
        /// </summary>
        public string? Password { get; init; }

        /// <summary>
        /// The SMTP server port.
        /// </summary>
        public int Port { get; init; }

        /// <summary>
        /// The message priority (importance) shown by the recipient's mail client.
        /// Defaults to <see cref="MessagePriority.Normal"/>.
        /// </summary>
        public MessagePriority Priority { get; init; } = MessagePriority.Normal;

        /// <summary>
        /// The email address that replies should be sent to, if different from
        /// <see cref="FromEmail"/>.
        /// </summary>
        public string? ReplyToEmail { get; init; }

        /// <summary>
        /// The display name shown for the reply-to address.
        /// </summary>
        public string? ReplyToName { get; init; }

        /// <summary>
        /// The socket security options to use when connecting (SSL/TLS behavior).
        /// Defaults to <see cref="SecureSocketOptions.Auto"/>.
        /// </summary>
        public SecureSocketOptions SocketOptions { get; init; } = SecureSocketOptions.Auto;

        /// <summary>
        /// The email subject.
        /// </summary>
        public string? Subject { get; init; }

        /// <summary>
        /// The timeout, in milliseconds, for network operations against the SMTP server.
        /// Defaults to 100000 (MailKit's default).
        /// </summary>
        public int TimeoutMilliseconds { get; init; } = 100_000;

        /// <summary>
        /// A single primary ("To") recipient email address. Combined with
        /// <see cref="ToEmails"/> when sending — use whichever is more convenient
        /// for the caller.
        /// </summary>
        public string? ToEmail { get; init; }

        /// <summary>
        /// Additional primary ("To") recipient email addresses. Combined with
        /// <see cref="ToEmail"/> when sending — use whichever is more convenient
        /// for the caller.
        /// </summary>
        public IReadOnlyList<string> ToEmails { get; init; } = [];

        /// <summary>
        /// Whether SMTP authentication should be performed before sending.
        /// </summary>
        public bool UseAuthentication { get; init; }

        /// <summary>
        /// The username used for SMTP authentication, if <see cref="UseAuthentication"/> is true.
        /// </summary>
        public string? Username { get; init; }

        #endregion Properties
        }
    }