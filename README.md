# MailBridge

A .NET solution for sending and testing emails via SMTP, consisting of a reusable core library, a WPF test client, and a console usage example.

## Projects

| Project | Description | Requirements |
|---|---|---|
| [MailBridge.Core](MailBridge.Core) | Framework-independent .NET library for sending emails via SMTP using MailKit. Handles connection, authentication, TLS, and attachments. | .NET 8.0 |
| [MailBridge.Wpf Test Client](MailBridge.Wpf) | WPF desktop client for testing SMTP configurations and composing test emails, including a rich HTML editor and attachment support. | .NET 8.0, Windows 10/11, [WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) |
| [MailBridge.ConsoleSample](MailBridge.ConsoleSample) | Minimal console application demonstrating how to use `MailBridge.Core` to send an email programmatically. | .NET 8.0 |

## Getting Started

1. Clone the repository.
2. Open `MailBridge.sln` in Visual Studio 2022 or later.
3. Set `MailBridge.Wpf Test Client` as the startup project and run it to test SMTP sending interactively (Windows only).
4. Alternatively, run `MailBridge.ConsoleSample` for a minimal, cross-platform code example of sending an email via `MailBridge.Core`.
5. Reference `MailBridge.Core` directly in your own project if you only need the sending logic.


## License

Shield: [![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]

This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][cc-by-nc-sa], see [license](MailBridge.Core/LICENSE.md).

[![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg