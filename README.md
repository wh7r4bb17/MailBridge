# MailBridge

A .NET solution for sending and testing emails via SMTP, consisting of a reusable core library, a WPF test client, and a console usage example.

## Projects

| Project | Description | Requirements |
|---|---|---|
| [MailBridge.Core](MailBridge.Core) | Framework-independent .NET library for sending emails via SMTP using MailKit. Handles connection, authentication, TLS, and attachments. | .NET 8.0 / .NET 10.0 |
| [MailBridge.Wpf Test Client](MailBridge.Wpf) | WPF desktop client for testing SMTP configurations and composing test emails, including a rich HTML editor and attachment support. | .NET 10.0,  Windows 10/11, [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) |
| [MailBridge.ConsoleSample](MailBridge.ConsoleSample) | Minimal console application demonstrating how to use `MailBridge.Core` to send an email programmatically. | .NET 10.0 |

## Getting Started

1. Clone the repository.
2. Open `MailBridge.sln` in Visual Studio 2022 or later.
3. Set `MailBridge.Wpf Test Client` as the startup project and run it to test SMTP sending interactively (Windows only).
4. Alternatively, run `MailBridge.ConsoleSample` for a minimal, cross-platform code example of sending an email via `MailBridge.Core`.
5. Reference `MailBridge.Core` directly in your own project if you only need the sending logic.


## License

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE.md)

This project is licensed under the [GNU General Public License v3.0](LICENSE.md).