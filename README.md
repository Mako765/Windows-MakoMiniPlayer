# MakoMiniPlayer

A lightweight Windows video player with full custom subtitle support — a port of the Android MakoMiniPlayer to Windows, built with WPF and LibVLCSharp.

## Features

- Plays virtually any video format (MP4, MKV, AVI, MOV, WMV, FLV, TS, WebM, HEVC, and more) — codecs are built in, no K-Lite or external codec packs needed
- Custom subtitle overlay with adjustable size, color, outline, and shadow
- Subtitle timing offset adjustment
- Online subtitle search via OpenSubtitles.com and SubDL
- Load subtitles directly from disk (SRT, ASS, SUB, SSA)
- Embedded subtitle track selection
- Fullscreen playback (over the taskbar) with auto-hiding controls
- Double-click to toggle fullscreen
- Aspect ratio control
- Audio track selection
- Screenshot capture
- Resume playback from last position
- Recent files list
- Always-on-top mode
- Mute toggle
- Interface available in 13 languages: Croatian, English, German, French, Spanish, Italian, Portuguese, Russian, Polish, Czech, Dutch, Swedish, and Turkish
- Drag and drop files to play

## Installation

Download the latest installer from the [Releases](../../releases) page, run `MakoMiniPlayer-Setup.exe`, and follow the setup wizard.

> Note: Since the installer is not code-signed, Windows SmartScreen may show a warning. Click "More info" → "Run anyway" to proceed.

## Requirements

- Windows 10 or 11 (64-bit)

## Building from source

1. Open the project in Visual Studio 2022
2. Restore NuGet packages (LibVLCSharp, LibVLCSharp.WPF, VideoLAN.LibVLC.Windows)
3. Build in Release mode

## Subtitle API keys

To use the online subtitle search, enter your own OpenSubtitles.com and/or SubDL API credentials in the app's API settings. Keys are stored locally in your AppData folder and are never bundled with the application.

## Built with

- [LibVLCSharp](https://code.videolan.org/videolan/LibVLCSharp) — video playback engine
- [OpenSubtitles.com](https://www.opensubtitles.com) — online subtitles
- [SubDL](https://subdl.com) — online subtitles

## Support

MakoMiniPlayer is free and open source. If you find it useful, you can support development:

☕ [paypal.me/makominiplayer](https://paypal.me/makominiplayer)

## License

This project is licensed under the GNU General Public License v3.0 — see the [LICENSE](LICENSE) file for details.

---

Developed by Mako © 2026

<img width="909" height="685" alt="Snimka zaslona 2026-06-07 214446" src="https://github.com/user-attachments/assets/8629ebe6-53b4-46f0-bad7-8e2425fb7097" />
<img width="1143" height="960" alt="Snimka zaslona 2026-06-07 214407" src="https://github.com/user-attachments/assets/c69eb089-d894-4884-94c8-f9e73d8cfc31" />
<img width="2507" height="1376" alt="Snimka zaslona 2026-06-07 214343" src="https://github.com/user-attachments/assets/d470d188-06c3-49b6-9b81-94abcb59b218" />

