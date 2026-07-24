# MailBridge.ConsoleExample

A minimal console application demonstrating that [`MailBridge.Core`](../MailBridge.Core/README.md) can be used from a plain console application, with no dependency on WPF or any other UI framework.

## What this example shows

- Wiring up Serilog with `LoggingConfig.LevelSwitch` from `MailBridge.Core`, using both a console sink and a rolling file sink
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

## Logs

Logs are written to both the console and to a rolling daily log file at:

```
%LocalAppData%\MailBridge\Logs\mailbridge_<date>.log
```

The minimum log level is set to `Debug` in this example (`LoggingConfig.LevelSwitch.MinimumLevel = LogEventLevel.Debug;`), so connection, authentication, and send steps are all logged in detail.


## License

Shield: [![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]

This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][cc-by-nc-sa], see [license](/license).

[![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg