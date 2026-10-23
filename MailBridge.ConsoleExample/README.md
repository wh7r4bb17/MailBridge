# MailBridge.ConsoleExample

A minimal console application demonstrating that [`MailBridge.Core`](../MailBridge.Core/README.md) can be used from a plain console application, with no dependency on WPF or any other UI framework.

## What this example shows

- Loading an external `NLog.config` file and controlling the minimum log level via `LoggingConfig.SetMinLevel` from `MailBridge.Core`, using both a console target and a rolling daily file target
- Building an `SmtpSendRequest` with STARTTLS, a display name, authentication, and a custom timeout
- Sending the email via `SmtpService.SendAsync` and handling success/failure with a simple `try`/`catch`/`finally`
- Flushing the logger on exit via `Log.CloseAndFlush()`

## Prerequisites

- .NET SDK (matching the target framework of the project)
- A reachable SMTP server and valid credentials
- The `SMTP_PASSWORD` environment variable set to the sender account's password (the password is intentionally not hardcoded)

## Configuration

Before running, update the following values in `Program.cs` to match your own SMTP server:

| Field | Description |
|---|---|
| `Host` | SMTP server hostname |
| `Port` | SMTP server port (587 for STARTTLS) |
| `FromEmail` / `FromName` | Sender address and display name |
| `ToEmail` | Recipient address |
| `Username` | SMTP account used for authentication |

The password is read from the `SMTP_PASSWORD` environment variable rather than being stored in code.

### Setting the password (Windows)

```powershell
$env:SMTP_PASSWORD = "your-password"
```

### Setting the password (macOS/Linux)

```bash
export SMTP_PASSWORD="your-password"
```

## Running the example

```bash
dotnet run --project MailBridge.ConsoleExample
```

On success, the console prints `Email sent successfully.`. On failure, the error message is printed and full details are written to the log.

## Logging configuration

Logging is configured via an external `NLog.config` file included in the project (copied to the output directory on build):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      throwConfigExceptions="true"
      internalLogLevel="Off">

  <targets>
    <target xsi:type="File" name="logfile"
            fileName="${specialfolder:folder=LocalApplicationData}/MailBridge/Logs/mailbridge_${shortdate}.log"
            layout="${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}"
            archiveEvery="Day"
            maxArchiveFiles="30" />

    <target xsi:type="Console" name="logconsole"
            layout="${longdate}|${level:uppercase=true}|${logger}|${message}" />
  </targets>

  <rules>
    <logger name="*" minlevel="Trace" writeTo="logfile,logconsole" />
  </rules>
</nlog>
```

`Program.cs` only needs to load this file and set the initial level:

```csharp
LogManager.Setup().LoadConfigurationFromFile("NLog.config");
LoggingConfig.SetMinLevel(LogLevel.Debug);
```

## Logs

Logs are written to both the console and to a rolling daily log file at:

```
%LocalAppData%\MailBridge\Logs\mailbridge_<date>.log
```

The minimum log level is set to `Debug` in this example (`LoggingConfig.SetMinLevel(LogLevel.Debug);`), so connection, authentication, and send steps are all logged in detail.

## License

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE.md)

This project is licensed under the [GNU General Public License v3.0](LICENSE.md).