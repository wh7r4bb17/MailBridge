# MailBridge.Core

`MailBridge.Core` is a framework-independent .NET library for sending emails via SMTP using [MailKit](https://github.com/jstedfast/MailKit). It has no dependencies on any UI framework and can therefore be used from WPF applications, console applications, background services, or any other .NET host.

## Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
  - [Simple text email](#simple-text-email)
  - [HTML email with plaintext fallback](#html-email-with-plaintext-fallback)
  - [CC, BCC, and display names](#cc-bcc-and-display-names)
  - [Attachments](#attachments)
  - [Reply-to, priority, and timeout](#reply-to-priority-and-timeout)
  - [Authentication](#authentication)
- [Logging](#logging)
  - [Setting up LoggingConfig](#setting-up-loggingconfig)
  - [Controlling the log level at runtime](#controlling-the-log-level-at-runtime)
- [Application settings](#application-settings)
  - [Storage location](#storage-location)
  - [Loading and saving settings](#loading-and-saving-settings)
  - [Combining with LoggingConfig](#combining-with-loggingconfig)
- [Error handling](#error-handling)
- [License](#license)

## Features

- Sending text and HTML emails (including multipart-alternative with fallback)
- CC and BCC recipients
- Display names for sender, recipient, and reply-to address
- File attachments
- Reply-to address
- Configurable message priority (Normal/High/Low)
- Configurable connection timeout
- Optional SMTP authentication
- Configurable socket security options (SSL/TLS)
- Structured logging via Serilog with a centrally controllable log level
- No dependency on any UI framework — reusable across arbitrary .NET hosts

## Installation

`MailBridge.Core` is referenced by the host application either as a project reference or a NuGet package. The following NuGet packages are referenced by the project itself:

```
MailKit
Serilog
```

The host application (WPF, console, etc.) is responsible for the concrete Serilog sink configuration (e.g. `Serilog.Sinks.File`, `Serilog.Sinks.Console`) — see the [Logging](#logging) section.

## Usage

### Simple text email

with one recipient:
```csharp
using MailBridge.Core;

SmtpSendRequest request = new()
    {
    Host = "smtp.example.com",
    Port = 587,
    FromEmail = "sender@example.com",
    ToEmail = "recipient@example.com",
    Subject = "Test message",
    Body = "This is a simple text message."
    };

SmtpService smtpService = new();
await smtpService.SendAsync(request);
```

with several recipients:
```csharp
using MailBridge.Core;

SmtpSendRequest request = new()
    {
    Host = "smtp.example.com",
    Port = 587,
    FromEmail = "sender@example.com",
    ToEmails = ["a@example.com", "b@example.com"],
    Subject = "Test message",
    Body = "This is a simple text message."
    };

SmtpService smtpService = new();
await smtpService.SendAsync(request);
```

### HTML email with plaintext fallback

If both `HtmlBody` and `Body` are set, `SmtpService` automatically builds a `multipart/alternative` message. Email clients that cannot render HTML will then automatically display the plaintext content.

```csharp
SmtpSendRequest request = new()
    {
    Host = "smtp.example.com",
    Port = 587,
    FromEmail = "sender@example.com",
    ToEmail = "recipient@example.com",
    Subject = "Welcome",
    HtmlBody = "<h1>Welcome!</h1><p>Thank you for signing up.</p>",
    Body = "Welcome! Thank you for signing up."
    };

await smtpService.SendAsync(request);
```

> At least one of `Body` or `HtmlBody` must be set, otherwise `SendAsync` throws an `ArgumentException`.

### CC, BCC, and display names

```csharp
SmtpSendRequest request = new()
    {
    Host = "smtp.example.com",
    Port = 587,
    FromEmail = "sender@example.com",
    FromName = "MailBridge Support",
    ToEmail = "recipient@example.com",
    CcEmails = ["cc1@example.com", "cc2@example.com"],
    BccEmails = ["bcc@example.com"],
    Subject = "Status update",
    Body = "Please find the current status update attached."
    };

await smtpService.SendAsync(request);
```

> `ToEmail` and `ToEmails` can be combined — both are merged into a single recipient list when sending.

### Attachments

Attachments are referenced via file paths. Missing files are skipped and logged as a warning without aborting the send operation.

```csharp
SmtpSendRequest request = new()
    {
    Host = "smtp.example.com",
    Port = 587,
    FromEmail = "sender@example.com",
    ToEmail = "recipient@example.com",
    Subject = "Report",
    Body = "The report is attached.",
    Attachments = [@"C:\Reports\report.pdf"]
    };

await smtpService.SendAsync(request);
```

### Reply-to, priority, and timeout

```csharp
using MimeKit;

SmtpSendRequest request = new()
    {
    Host = "smtp.example.com",
    Port = 587,
    FromEmail = "noreply@example.com",
    FromName = "MailBridge",
    ReplyToEmail = "support@example.com",
    ReplyToName = "MailBridge Support",
    ToEmail = "recipient@example.com",
    Subject = "Important notice",
    Body = "Please do not reply to this address.",
    Priority = MessagePriority.Urgent,
    TimeoutMilliseconds = 30_000
    };

await smtpService.SendAsync(request);
```

### Authentication

```csharp
SmtpSendRequest request = new()
    {
    Host = "smtp.example.com",
    Port = 587,
    FromEmail = "sender@example.com",
    ToEmail = "recipient@example.com",
    Subject = "Test message",
    Body = "Authenticated send.",
    UseAuthentication = true,
    Username = "smtp-user",
    Password = "smtp-password"
    };

await smtpService.SendAsync(request);
```

If `UseAuthentication` is set to `true`, `Username` and `Password` must both be set, otherwise `SendAsync` throws an `ArgumentException`.

## Logging

`MailBridge.Core` uses [Serilog](https://serilog.net/) for structured logging. The library itself does not configure any sinks (e.g. file, console) — that responsibility is intentionally left to the host application, so each application can decide for itself where logs are written to.

### Setting up LoggingConfig

`LoggingConfig` provides a shared `LoggingLevelSwitch` that the host application wires into its Serilog configuration:

```csharp
using Serilog;
using MailBridge.Core;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(LoggingConfig.LevelSwitch)
    .WriteTo.File(
        path: Path.Combine(logDirectory, "mailbridge_.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();
```

### Controlling the log level at runtime

The log level can be adjusted at runtime, e.g. driven by a setting such as `EnableLogging`:

```csharp
using Serilog.Events;
using MailBridge.Core;

LoggingConfig.LevelSwitch.MinimumLevel = enableLogging
    ? LogEventLevel.Debug
    : LogEventLevel.Error;
```

This keeps control centralized, while each host application decides for itself which sinks (file, console, etc.) are actually used.

## Application settings

`AppSettings` provides simple JSON-based persistence for connection-related settings (server, port, credentials, TLS usage, logging toggle, etc.). It contains no UI-specific state (such as a selected ComboBox index) — that belongs in the host application's view model instead.

### Storage location

Settings are stored as `settings.json` in the current user's local application data folder:

```
%LocalAppData%\MailBridge\settings.json
```

On Windows, this typically resolves to a path such as:

```
C:\Users\<username>\AppData\Local\MailBridge\settings.json
```

No secrets (such as the SMTP password) are persisted; only the username is stored, and the password must be supplied at runtime by the user or a secure credential store.

### Loading and saving settings

```csharp
using MailBridge.Core;

AppSettings settings = AppSettings.Load();

settings.SmtpServer = "smtp.example.com";
settings.Port = 587;
settings.UseTls = true;
settings.EnableLogging = true;

settings.Save();
```

`AppSettings.Load()` returns default values if no settings file exists yet, or if loading fails (e.g. due to a corrupted file or missing permissions); failures are logged via Serilog rather than throwing an exception.

### Combining with LoggingConfig

The `EnableLogging` property is typically read once at startup and whenever the user changes it, to drive `LoggingConfig.LevelSwitch`:

```csharp
using Serilog.Events;
using MailBridge.Core;

AppSettings settings = AppSettings.Load();

LoggingConfig.LevelSwitch.MinimumLevel = settings.EnableLogging
    ? LogEventLevel.Debug
    : LogEventLevel.Error;
```


## Error handling

`SmtpService.SendAsync` throws the following exceptions:

| Exception | Cause |
|---|---|
| `ArgumentException` | Required fields (`Host`, `FromEmail`) are missing, no recipient is set via `ToEmail` or `ToEmails`, `Username`/`Password` are missing when `UseAuthentication` is `true`, or neither `Body` nor `HtmlBody` is set |
| `MailKit.Security.AuthenticationException` | Authentication with the SMTP server failed |
| `OperationCanceledException` | The operation was cancelled via the supplied `CancellationToken` |
| `Exception` (general) | Other errors during connection, sending, or disconnection |

All errors are logged in a structured way via Serilog before being re-thrown (`throw`).



## License

Shield: [![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]

This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][cc-by-nc-sa], see [license](/license).

[![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg