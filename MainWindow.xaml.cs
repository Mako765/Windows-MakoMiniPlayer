#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace MakoMiniPlayer
{
    public partial class MainWindow : Window
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private bool _isDraggingSeek = false;
        private DispatcherTimer _timer;
        private DispatcherTimer _hideControlsTimer;
        private DispatcherTimer _spuDisableTimer;
        private string _currentFile = "";
        private AppSettings _settings;
        private SubtitleOverlayWindow _subtitleOverlay;
        private string _pendingSubtitlePath = "";
        private bool _controlsVisible = true;
        private bool _isFullscreen = false;
        private DateTime _lastClickTime = DateTime.MinValue;
        private System.Drawing.Point _lastClickPoint;

        [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll")] static extern IntPtr WindowFromPoint(System.Drawing.Point point);
        [DllImport("user32.dll")] static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr lpdwProcessId);
        [DllImport("user32.dll")] static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll")] static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")] static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelMouseProc _hookProc;
        private IntPtr _hookHandle = IntPtr.Zero;

        const int WH_MOUSE_LL = 14;
        const int WM_RBUTTONUP = 0x0205;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_MOUSEMOVE = 0x0200;
        const uint GA_ROOT = 2;
        const int SW_SHOW = 5;
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_SHOWWINDOW = 0x0040;

        [StructLayout(LayoutKind.Sequential)]
        struct MSLLHOOKSTRUCT
        {
            public System.Drawing.Point pt;
            public uint mouseData, flags, time;
            public IntPtr dwExtraInfo;
        }

        private readonly List<(string Name, float? Ratio)> _aspectRatios = new()
        {
            ("Default", null), ("16:9", 16f/9f), ("16:10", 16f/10f),
            ("4:3", 4f/3f), ("21:9", 21f/9f), ("1:1", 1f),
            ("Fit", -1f), ("Fill", -2f),
        };
        private int _currentAspectIndex = 0;

        private List<SubtitleEntry> _subtitleEntries = new();
        private bool _subtitleEnabled = false;
        private long _subtitleOffsetMs = 0;

        public MainWindow()
        {
            Core.Initialize();
            InitializeComponent();
            _settings = AppSettings.Load();
            Translations.CurrentLang = _settings.Language;
            InitializeVLC();
            InitializeTimer();
            InitializeHideControlsTimer();
            InitializeSpuDisableTimer();
            LoadRecentFiles();
            VolumeBar.Value = _settings.Volume;
            Loaded += (s, e) =>
            {
                InitializeSubtitleOverlay();
                UpdateOverlayPosition();
                InstallMouseHook();
                ForceForeground();
                RefreshUI();
            };
            ContentRendered += (s, e) => ForceForeground();
        }

        private void ForceForeground()
        {
            var handle =
                new System.Windows.Interop.WindowInteropHelper(this).Handle;
            if (handle == IntPtr.Zero) return;

            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            var foreHwnd = GetForegroundWindow();
            var foreThread = GetWindowThreadProcessId(foreHwnd, IntPtr.Zero);
            var ourThread = GetWindowThreadProcessId(handle, IntPtr.Zero);
            AttachThreadInput(foreThread, ourThread, true);
            BringWindowToTop(handle);
            ShowWindow(handle, SW_SHOW);
            SetForegroundWindow(handle);
            AttachThreadInput(foreThread, ourThread, false);
            Activate();
            Focus();

            var t = new DispatcherTimer
            { Interval = TimeSpan.FromMilliseconds(1200) };
            t.Tick += (s, e) =>
            {
                t.Stop();
                if (!Topmost)
                    SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            };
            t.Start();
        }

        private void TitleBar_Minimize(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void TitleBar_MaxRestore(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal : WindowState.Maximized;
        }

        private void TitleBar_Close(object sender, RoutedEventArgs e)
            => Close();

        private void InitializeSpuDisableTimer()
        {
            int attempts = 0;
            _spuDisableTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMilliseconds(500) };
            _spuDisableTimer.Tick += (s, e) =>
            {
                attempts++;
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.SetSpu(-1);
                    if (!string.IsNullOrEmpty(_pendingSubtitlePath) &&
                        File.Exists(_pendingSubtitlePath))
                    {
                        var entries =
                            SubtitleParser.Parse(_pendingSubtitlePath);
                        if (entries.Count > 0)
                        {
                            _subtitleEntries = entries;
                            _subtitleEnabled = true;
                            _subtitleOffsetMs = 0;
                            TxtSubOffset.Text = "+0.0s";
                            _subtitleOverlay?.ApplyStyle(_settings);
                            _subtitleOverlay?.RepositionNow();
                            var fn = Path.GetFileName(_currentFile);
                            Title = $"MakoMiniPlayer — {fn} 💬";
                            TxtTitleFile.Text = $"— {fn} 💬";
                            _pendingSubtitlePath = "";
                        }
                    }
                }
                if (attempts >= 10) { _spuDisableTimer.Stop(); attempts = 0; }
            };
        }

        private void InitializeHideControlsTimer()
        {
            _hideControlsTimer = new DispatcherTimer
            { Interval = TimeSpan.FromSeconds(3) };
            _hideControlsTimer.Tick += (s, e) =>
            {
                _hideControlsTimer.Stop();
                if (_isFullscreen) HideControls();
            };
        }

        private void HideControls()
        {
            if (!_isFullscreen) return;
            _controlsVisible = false;
            ControlsBar.Visibility = Visibility.Collapsed;
            Cursor = Cursors.None;
            UpdateOverlayPosition();
        }

        private void ShowControls()
        {
            _controlsVisible = true;
            ControlsBar.Visibility = Visibility.Visible;
            Cursor = Cursors.Arrow;
            UpdateOverlayPosition();
            if (_isFullscreen)
            {
                _hideControlsTimer.Stop();
                _hideControlsTimer.Start();
            }
        }

        private void InstallMouseHook()
        {
            _hookProc = MouseHookCallback;
            var module = GetModuleHandle(
                System.Diagnostics.Process.GetCurrentProcess()
                    .MainModule.ModuleName);
            _hookHandle = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, module, 0);
        }

        private bool IsMouseOverMainWindow(System.Drawing.Point pt)
        {
            var winHandle =
                new System.Windows.Interop.WindowInteropHelper(this).Handle;
            return GetAncestor(WindowFromPoint(pt), GA_ROOT) == winHandle;
        }

        private bool IsMouseOverVideoArea(System.Drawing.Point screenPt)
        {
            if (!IsMouseOverMainWindow(screenPt)) return false;
            try
            {
                GetWindowRect(
                    new System.Windows.Interop.WindowInteropHelper(this).Handle,
                    out RECT winRect);
                var dpi = VisualTreeHelper.GetDpi(this);
                int titlePx = _isFullscreen ? 0 : (int)(32 * dpi.DpiScaleY);
                int ctrlPx = _controlsVisible
                    ? (int)((ActualHeight - VideoView.ActualHeight) * dpi.DpiScaleY)
                    : 0;
                int videoTop = winRect.Top + titlePx;
                int videoBottom = winRect.Bottom - ctrlPx;
                return screenPt.Y > videoTop && screenPt.Y < videoBottom
                    && screenPt.X > winRect.Left && screenPt.X < winRect.Right;
            }
            catch { return false; }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var info = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                int msg = wParam.ToInt32();

                switch (msg)
                {
                    case WM_MOUSEMOVE:
                        if (_isFullscreen && IsMouseOverMainWindow(info.pt))
                            Dispatcher.BeginInvoke(new Action(ShowControls));
                        break;

                    case WM_RBUTTONUP:
                        if (IsMouseOverMainWindow(info.pt))
                            Dispatcher.BeginInvoke(
                                new Action(ShowVideoContextMenu));
                        break;

                    case WM_LBUTTONDOWN:
                        if (IsMouseOverVideoArea(info.pt))
                        {
                            var now = DateTime.Now;
                            var diff =
                                (now - _lastClickTime).TotalMilliseconds;
                            var dist =
                                Math.Abs(info.pt.X - _lastClickPoint.X) +
                                Math.Abs(info.pt.Y - _lastClickPoint.Y);
                            if (diff < 500 && dist < 10)
                            {
                                _lastClickTime = DateTime.MinValue;
                                Dispatcher.BeginInvoke(
                                    new Action(ToggleFullscreen));
                            }
                            else
                            {
                                _lastClickTime = now;
                                _lastClickPoint = info.pt;
                            }
                        }
                        break;
                }
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private (System.Windows.Controls.Primitives.Popup popup,
            StackPanel panel) CreatePopupMenu()
        {
            var popup = new System.Windows.Controls.Primitives.Popup
            {
                AllowsTransparency = true,
                StaysOpen = false,
                PlacementTarget = this,
                Placement =
                    System.Windows.Controls.Primitives.PlacementMode.MousePoint
            };
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(18, 6, 35)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 136)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(4),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    ShadowDepth = 6,
                    BlurRadius = 16,
                    Opacity = 0.9
                }
            };
            var scroll = new ScrollViewer
            {
                MaxHeight = 560,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            var panel = new StackPanel { MinWidth = 240 };
            scroll.Content = panel;
            border.Child = scroll;
            popup.Child = border;
            return (popup, panel);
        }

        private void AddMenuItem(
            StackPanel panel,
            System.Windows.Controls.Primitives.Popup popup,
            string text, Action action,
            bool isHeader = false, bool isSeparator = false)
        {
            if (isSeparator)
            {
                panel.Children.Add(new Border
                {
                    Height = 1,
                    Background = new SolidColorBrush(
                        Color.FromArgb(60, 0, 255, 136)),
                    Margin = new Thickness(8, 3, 8, 3)
                });
                return;
            }
            var btn = new Button
            {
                Content = text,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(14, 7, 14, 7),
                Margin = new Thickness(2, 1, 2, 1),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = isHeader ? Cursors.Arrow : Cursors.Hand,
                IsEnabled = !isHeader,
                FontSize = isHeader ? 14 : 13,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                Foreground = isHeader
                    ? new SolidColorBrush(Color.FromRgb(0, 220, 110))
                    : new SolidColorBrush(Color.FromRgb(0, 255, 136)),
                Template = CreateMenuItemTemplate()
            };
            if (!isHeader && action != null)
                btn.Click += (s, e) => { popup.IsOpen = false; action(); };
            panel.Children.Add(btn);
        }

        private ControlTemplate CreateMenuItemTemplate()
        {
            var tpl = new ControlTemplate(typeof(Button));
            var bd = new FrameworkElementFactory(typeof(Border));
            bd.Name = "Bd";
            bd.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            bd.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty,
                HorizontalAlignment.Left);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty,
                VerticalAlignment.Center);
            bd.AppendChild(cp);
            tpl.VisualTree = bd;
            var tr = new Trigger
            { Property = UIElement.IsMouseOverProperty, Value = true };
            tr.Setters.Add(new Setter(Border.BackgroundProperty,
                new SolidColorBrush(Color.FromRgb(50, 25, 90)), "Bd"));
            tpl.Triggers.Add(tr);
            return tpl;
        }

        private void ShowVideoContextMenu()
        {
            try
            {
                bool hasMedia = !string.IsNullOrEmpty(_currentFile);
                var (popup, panel) = CreatePopupMenu();
                AddMenuItem(panel, popup, "🎬  MakoMiniPlayer",
                    null, isHeader: true);
                AddMenuItem(panel, popup, "", null, isSeparator: true);

                if (hasMedia)
                {
                    AddMenuItem(panel, popup,
                        (_mediaPlayer != null && _mediaPlayer.IsPlaying)
                            ? "⏸  " + Translations.Get("pause")
                            : "▶  " + Translations.Get("play"),
                        () => BtnPlayPause_Click(null, null));
                    AddMenuItem(panel, popup, "⏹  " + Translations.Get("stop"),
                        () => BtnStop_Click(null, null));
                    AddMenuItem(panel, popup, "", null, isSeparator: true);
                    AddMenuItem(panel, popup, "⏪  " + Translations.Get("minus5"),
                        () => Skip_Minus5_Click(null, null));
                    AddMenuItem(panel, popup, "⏩  " + Translations.Get("plus5"),
                        () => Skip_Plus5_Click(null, null));
                    AddMenuItem(panel, popup, "", null, isSeparator: true);
                    AddMenuItem(panel, popup, "📁  " + Translations.Get("loadSub"),
                        () => LoadSubtitleFromDisk());
                    AddMenuItem(panel, popup, "🔑  OpenSubtitles.com",
                        () => OpenSubtitleSearch(false));
                    AddMenuItem(panel, popup, "🆓  SubDL",
                        () => OpenSubtitleSearch(true));
                    AddMenuItem(panel, popup, "🎨  " + Translations.Get("subStyle"),
                        () => ShowSubtitleStyleDialog());
                    AddMenuItem(panel, popup, "🚫  " + Translations.Get("subOff"), () =>
                    {
                        StopSubtitleOverlay();
                        _mediaPlayer.SetSpu(-1);
                    });

                    var spuDesc = _mediaPlayer?.SpuDescription;
                    if (spuDesc != null && spuDesc.Length > 1)
                    {
                        AddMenuItem(panel, popup, "", null, isSeparator: true);
                        AddMenuItem(panel, popup,
                            "📺  " + Translations.Get("embeddedSub"),
                            null, isHeader: true);
                        foreach (var track in spuDesc)
                        {
                            if (track.Id == -1) continue;
                            var t2 = track;
                            AddMenuItem(panel, popup, $"   {t2.Name}", () =>
                            {
                                StopSubtitleOverlay();
                                _mediaPlayer.SetSpu(t2.Id);
                            });
                        }
                    }
                    AddMenuItem(panel, popup, "", null, isSeparator: true);
                    AddMenuItem(panel, popup, "ℹ️  " + Translations.Get("mediaInfo"),
                        () => ShowMediaInfo());
                    AddMenuItem(panel, popup, "", null, isSeparator: true);
                }
                else
                {
                    AddMenuItem(panel, popup, "🎨  " + Translations.Get("subStyle"),
                        () => ShowSubtitleStyleDialog());
                    AddMenuItem(panel, popup, "", null, isSeparator: true);
                }

                AddMenuItem(panel, popup, "🌐  " + Translations.Get("language"),
                    () => ShowLanguageMenu());
                AddMenuItem(panel, popup, "", null, isSeparator: true);

                if (hasMedia)
                {
                    AddMenuItem(panel, popup, "📷  " + Translations.Get("screenshot"),
                        () => BtnScreenshot_Click(null, null));
                    AddMenuItem(panel, popup, "", null, isSeparator: true);
                    AddMenuItem(panel, popup,
                        _isFullscreen
                            ? "⛶  " + Translations.Get("exitFullscreen")
                            : "⛶  " + Translations.Get("fullscreen") + "  (F)",
                        () => ToggleFullscreen());
                }

                AddMenuItem(panel, popup,
                    "📌  " + Translations.Get("onTop") + (Topmost ? "  ✓" : ""),
                    () =>
                    {
                        Topmost = !Topmost;
                        BtnOnTop.IsChecked = Topmost;
                        if (_subtitleOverlay != null)
                            _subtitleOverlay.Topmost = true;
                    });
                AddMenuItem(panel, popup, "", null, isSeparator: true);
                AddMenuItem(panel, popup, "📂  " + Translations.Get("openFile"),
                    () => BtnOpen_Click(null, null));
                AddMenuItem(panel, popup, "", null, isSeparator: true);
                AddMenuItem(panel, popup, "☕  " + Translations.Get("buyCoffee"),
                    () => ShowBuyMeCoffee());
                AddMenuItem(panel, popup, "ℹ️  " + Translations.Get("about"),
                    () => ShowAbout());
                if (_isFullscreen)
                {
                    AddMenuItem(panel, popup, "", null, isSeparator: true);
                    AddMenuItem(panel, popup,
                        "✕  " + Translations.Get("closePlayer"),
                        () => Close());
                }

                var t = new DispatcherTimer
                { Interval = TimeSpan.FromMilliseconds(50) };
                t.Tick += (s, e) => { t.Stop(); popup.IsOpen = true; };
                t.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška u meniju: {ex.Message}");
            }
        }

        private void ShowMediaInfo()
        {
            if (string.IsNullOrEmpty(_currentFile))
            {
                MessageBox.Show(Translations.Get("noFile"), "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"📁  {Path.GetFileName(_currentFile)}");
            sb.AppendLine();
            var fi = new FileInfo(_currentFile);
            sb.AppendLine($"{Translations.Get("size")}  {fi.Length / 1024 / 1024} MB");
            if (_mediaPlayer?.Length > 0)
                sb.AppendLine(
                    $"⏱  {Translations.Get("duration")}  {FormatTime(_mediaPlayer.Length)}");
            sb.AppendLine();
            sb.AppendLine("🔊  " + Translations.Get("audioTracks"));
            if (_mediaPlayer?.AudioTrackDescription != null)
                foreach (var t in _mediaPlayer.AudioTrackDescription)
                    if (t.Id >= 0) sb.AppendLine($"   • {t.Name}");
            sb.AppendLine();
            sb.AppendLine("💬  " + Translations.Get("subTracks"));
            if (_mediaPlayer?.SpuDescription != null)
                foreach (var t in _mediaPlayer.SpuDescription)
                    if (t.Id >= 0) sb.AppendLine($"   • {t.Name}");
            MessageBox.Show(sb.ToString(), Translations.Get("mediaInfo"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowLanguageMenu()
        {
            var (popup, panel) = CreatePopupMenu();
            popup.Placement =
                System.Windows.Controls.Primitives.PlacementMode.Center;
            AddMenuItem(panel, popup, "🌐  " + Translations.Get("language"),
                null, isHeader: true);
            AddMenuItem(panel, popup, "", null, isSeparator: true);
            var langs = new (string Label, string Code)[]
            {
                ("🇭🇷  Hrvatski", "hr"),  ("🇬🇧  English", "en"),
                ("🇩🇪  Deutsch", "de"),   ("🇫🇷  Français", "fr"),
                ("🇪🇸  Español", "es"),   ("🇮🇹  Italiano", "it"),
                ("🇵🇹  Português", "pt"), ("🇷🇺  Русский", "ru"),
                ("🇵🇱  Polski", "pl"),    ("🇨🇿  Čeština", "cs"),
                ("🇳🇱  Nederlands", "nl"),("🇸🇪  Svenska", "sv"),
                ("🇹🇷  Türkçe", "tr"),
            };
            foreach (var lang in langs)
            {
                var code = lang.Code;
                var label = lang.Label +
                    (Translations.CurrentLang == code ? "  ✓" : "");
                AddMenuItem(panel, popup, label, () =>
                {
                    Translations.CurrentLang = code;
                    _settings.Language = code;
                    _settings.Save();
                    RefreshUI();
                });
            }
            popup.IsOpen = true;
        }

        private void RefreshUI()
        {
            BtnSubtitles.Content = "💬 " + Translations.Get("subtitles");
            BtnOpen.Content = "📂 " + Translations.Get("open");
            BtnOnTop.Content = "📌 " + Translations.Get("onTop");
        }

        private void ShowAbout()
        {
            var (popup, panel) = CreatePopupMenu();
            popup.Placement =
                System.Windows.Controls.Primitives.PlacementMode.Center;
            AddMenuItem(panel, popup, "ℹ️  " + Translations.Get("about"),
                null, isHeader: true);
            AddMenuItem(panel, popup, "", null, isSeparator: true);
            AddMenuItem(panel, popup, "MakoMiniPlayer v2.0", null);
            AddMenuItem(panel, popup, Translations.Get("aboutLine1"), null);
            AddMenuItem(panel, popup, Translations.Get("aboutLine2"), null);
            AddMenuItem(panel, popup, "", null, isSeparator: true);
            AddMenuItem(panel, popup, "◆  LibVLCSharp — video engine", null);
            AddMenuItem(panel, popup, "◆  OpenSubtitles.com", null);
            AddMenuItem(panel, popup, "◆  SubDL", null);
            AddMenuItem(panel, popup, "", null, isSeparator: true);
            AddMenuItem(panel, popup, Translations.Get("aboutDev") + "  ©  2026", null);
            AddMenuItem(panel, popup, "", null, isSeparator: true);
            AddMenuItem(panel, popup, "✓  " + Translations.Get("close"), () => { });
            popup.IsOpen = true;
        }

        private void ShowBuyMeCoffee()
        {
            var (popup, panel) = CreatePopupMenu();
            popup.Placement =
                System.Windows.Controls.Primitives.PlacementMode.Center;

            void AddCentered(string text, double size, Brush color, bool bold)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = text,
                    FontSize = size,
                    Foreground = color,
                    FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 4),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            AddCentered("☕", 32,
                new SolidColorBrush(Color.FromRgb(0, 255, 136)), false);
            AddCentered(Translations.Get("buyCoffee"), 16,
                new SolidColorBrush(Color.FromRgb(0, 255, 136)), true);
            AddMenuItem(panel, popup, "", null, isSeparator: true);
            AddCentered(Translations.Get("coffeeFree"), 13,
                Brushes.White, false);
            AddCentered(Translations.Get("coffeeMsg"), 13,
                new SolidColorBrush(Color.FromRgb(180, 180, 200)), false);
            AddMenuItem(panel, popup, "", null, isSeparator: true);

            var btn = new Button
            {
                Content = "☕  " + Translations.Get("openPaypal"),
                Height = 38,
                Margin = new Thickness(20, 6, 20, 6),
                Background = new SolidColorBrush(Color.FromRgb(0, 170, 85)),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            btn.Click += (s, e) =>
            {
                popup.IsOpen = false;
                try
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "https://paypal.me/makominiplayer",
                            UseShellExecute = true
                        });
                }
                catch { }
            };
            panel.Children.Add(btn);

            var cancel = new Button
            {
                Content = Translations.Get("close"),
                Height = 30,
                Margin = new Thickness(20, 0, 20, 8),
                Background = new SolidColorBrush(Color.FromRgb(60, 42, 110)),
                Foreground = Brushes.White,
                FontSize = 12,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            cancel.Click += (s, e) => popup.IsOpen = false;
            panel.Children.Add(cancel);

            popup.IsOpen = true;
        }

        private void BtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var rootGrid = (Grid)Content;
                ControlsBar.Visibility = Visibility.Collapsed;
                rootGrid.RowDefinitions[0].Height = new GridLength(0);
                Dispatcher.Invoke(() => { },
                    System.Windows.Threading.DispatcherPriority.Render);
                System.Threading.Thread.Sleep(100);

                var dpi = VisualTreeHelper.GetDpi(this);
                var rtb = new RenderTargetBitmap(
                    (int)(ActualWidth * dpi.DpiScaleX),
                    (int)(ActualHeight * dpi.DpiScaleY),
                    dpi.PixelsPerInchX, dpi.PixelsPerInchY,
                    PixelFormats.Pbgra32);
                rtb.Render(this);
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(rtb));
                var folder = Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyPictures),
                    "MakoMiniPlayer");
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder,
                    $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                using (var fs = File.OpenWrite(path)) enc.Save(fs);

                if (!_isFullscreen)
                    rootGrid.RowDefinitions[0].Height = new GridLength(32);
                ControlsBar.Visibility = Visibility.Visible;

                MessageBox.Show($"{Translations.Get("screenshotSaved")}\n{path}",
                    Translations.Get("screenshot"), MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                var rootGrid = (Grid)Content;
                if (!_isFullscreen)
                    rootGrid.RowDefinitions[0].Height = new GridLength(32);
                ControlsBar.Visibility = Visibility.Visible;
                MessageBox.Show($"{Translations.Get("error")} {ex.Message}",
                    Translations.Get("screenshot"), MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void VideoView_MouseDoubleClick(object sender,
            MouseButtonEventArgs e)
        { }
        private void VideoOverlay_MouseEnter(object sender,
            MouseEventArgs e)
        { }
        private void VideoOverlay_MouseLeave(object sender,
            MouseEventArgs e)
        { }

        private void InitializeVLC()
        {
            _libVLC = new LibVLC("--sub-track=-1", "--no-video-title-show");
            VideoView.Background = System.Windows.Media.Brushes.Black;
            _mediaPlayer = new MediaPlayer(_libVLC);
            VideoView.MediaPlayer = _mediaPlayer;
            _mediaPlayer.Playing += OnMediaPlaying;
            _mediaPlayer.Paused += OnMediaPaused;
            _mediaPlayer.Stopped += OnMediaStopped;
            _mediaPlayer.EndReached += OnMediaEndReached;
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void InitializeSubtitleOverlay()
        {
            _subtitleOverlay = new SubtitleOverlayWindow();
            _subtitleOverlay.Topmost = true;
            _subtitleOverlay.Show();
            _subtitleOverlay.Owner = this;
            LocationChanged += (s, e) => UpdateOverlayPosition();
            SizeChanged += (s, e) => UpdateOverlayPosition();
            StateChanged += (s, e) =>
            {
                if (WindowState == WindowState.Minimized)
                    _subtitleOverlay.Hide();
                else
                {
                    if (!_subtitleOverlay.IsVisible) _subtitleOverlay.Show();
                    UpdateOverlayPosition();
                }
            };
            Activated += (s, e) => UpdateOverlayPosition();
        }

        private void UpdateOverlayPosition()
        {
            if (_subtitleOverlay == null) return;
            try
            {
                var tl = PointToScreen(new Point(0, 0));
                var dpi = VisualTreeHelper.GetDpi(this);
                _subtitleOverlay.Left = tl.X / dpi.DpiScaleX;
                _subtitleOverlay.Top = tl.Y / dpi.DpiScaleY;
                _subtitleOverlay.Width = ActualWidth;
                _subtitleOverlay.Height = ActualHeight;
            }
            catch
            {
                _subtitleOverlay.Left = Left;
                _subtitleOverlay.Top = Top;
                _subtitleOverlay.Width = Width;
                _subtitleOverlay.Height = Height;
            }
            var ch = _controlsVisible
                ? ActualHeight - VideoView.ActualHeight : 0;
            _subtitleOverlay.SetControlsHeight(ch);
            _subtitleOverlay.ApplyStyle(_settings);
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Otvori video",
                Filter = "Video fajlovi|*.mp4;*.mkv;*.avi;*.mov;*.wmv;" +
                         "*.flv;*.ts;*.m2ts;*.webm;*.m4v;*.hevc|" +
                         "Svi fajlovi|*.*"
            };
            if (dlg.ShowDialog() == true) OpenFile(dlg.FileName);
        }

        public void OpenFile(string path)
        {
            if (!File.Exists(path)) return;
            if (!string.IsNullOrEmpty(_currentFile) && _mediaPlayer.Length > 0)
                _settings.SavePosition(_currentFile, _mediaPlayer.Time);
            _currentFile = path;
            var fn = Path.GetFileName(path);
            Title = $"MakoMiniPlayer — {fn}";
            TxtTitleFile.Text = $"— {fn}";
            _settings.AddRecentFile(path);
            _settings.Save();
            LoadRecentFiles();
            StopSubtitleOverlay();
            _pendingSubtitlePath =
                SubtitleParser.FindSubtitleInDir(path) ?? "";
            var media = new Media(_libVLC, new Uri(path));
            _mediaPlayer.Play(media);
            media.Dispose();
            var saved = _settings.GetSavedPosition(path);
            if (saved > 10000)
            {
                var r = MessageBox.Show(
                    string.Format(Translations.Get("resumeQuestion"),
                        FormatTime(saved)),
                    Translations.Get("resume"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes)
                {
                    System.Threading.Thread.Sleep(500);
                    _mediaPlayer.Time = saved;
                }
                else { _settings.SavePosition(path, 0); _settings.Save(); }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length > 0) OpenFile(files[0]);
            }
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer.IsPlaying) _mediaPlayer.Pause();
            else _mediaPlayer.Play();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentFile) && _mediaPlayer.Length > 0)
                _settings.SavePosition(_currentFile, _mediaPlayer.Time);
            _settings.Save();
            _mediaPlayer.Stop();
            SeekBar.Value = 0;
            TxtTime.Text = "00:00:00 / 00:00:00";
            BtnPlayPause.Content = "▶";
            _subtitleOverlay?.HideSubtitle();
        }

        private void Skip_Minus5_Click(object sender, RoutedEventArgs e)
            => _mediaPlayer.Time = Math.Max(0, _mediaPlayer.Time - 300000);

        private void Skip_Plus5_Click(object sender, RoutedEventArgs e)
            => _mediaPlayer.Time = Math.Min(
                _mediaPlayer.Length, _mediaPlayer.Time + 300000);

        private void BtnFullscreen_Click(object sender, RoutedEventArgs e)
            => ToggleFullscreen();

        private void BtnSubtitleStyle_Click(object sender, RoutedEventArgs e)
            => ShowSubtitleStyleDialog();

        private void BtnOnTopMenu_Click(object sender, RoutedEventArgs e)
        {
            BtnOnTop.IsChecked = !BtnOnTop.IsChecked;
            Topmost = BtnOnTop.IsChecked == true;
            if (_subtitleOverlay != null) _subtitleOverlay.Topmost = true;
        }

        private void SeekBar_PreviewMouseDown(object sender,
            MouseButtonEventArgs e)
        { _isDraggingSeek = true; SeekTo(e); }

        private void SeekBar_PreviewMouseUp(object sender,
            MouseButtonEventArgs e)
        { _isDraggingSeek = false; SeekTo(e); }

        private void SeekTo(MouseButtonEventArgs e)
        {
            var p = e.GetPosition(SeekBar);
            var r = Math.Max(0, Math.Min(1, p.X / SeekBar.ActualWidth));
            if (_mediaPlayer.Length > 0) _mediaPlayer.Position = (float)r;
            SeekBar.Value = r * SeekBar.Maximum;
        }

        private void SeekBar_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isDraggingSeek && _mediaPlayer.Length > 0)
                _mediaPlayer.Position =
                    (float)(SeekBar.Value / SeekBar.Maximum);
        }

        private bool _isMuted = false;
        private double _volumeBeforeMute = 80;
        private void BtnMute_Click(object sender, RoutedEventArgs e)
        {
            if (_isMuted)
            {
                _isMuted = false;
                VolumeBar.Value = _volumeBeforeMute;
                BtnMute.Content = "🔊";
            }
            else
            {
                _isMuted = true;
                _volumeBeforeMute = VolumeBar.Value;
                VolumeBar.Value = 0;
                BtnMute.Content = "🔇";
            }
        }

        private void VolumeBar_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = (int)VolumeBar.Value;
                _settings.Volume = (int)VolumeBar.Value;
            }
        }

        private void CmbAudio_SelectionChanged(object sender,
            SelectionChangedEventArgs e)
        {
            if (CmbAudio.SelectedItem is TrackItem item)
                _mediaPlayer.SetAudioTrack(item.Id);
        }

        private void BtnOnTop_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
            if (_subtitleOverlay != null) _subtitleOverlay.Topmost = true;
        }

        private void BtnOnTop_Unchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
            if (_subtitleOverlay != null) _subtitleOverlay.Topmost = true;
        }

        private void BtnAspect_Click(object sender, RoutedEventArgs e)
        {
            _currentAspectIndex =
                (_currentAspectIndex + 1) % _aspectRatios.Count;
            var ar = _aspectRatios[_currentAspectIndex];
            BtnAspect.Content = $"Aspect: {ar.Name}";
            if (_mediaPlayer == null) return;
            if (ar.Ratio == null || ar.Ratio == -1f)
            {
                _mediaPlayer.AspectRatio = "";
                _mediaPlayer.CropGeometry = "";
            }
            else if (ar.Ratio == -2f)
            {
                _mediaPlayer.AspectRatio =
                    $"{(int)VideoView.ActualWidth}" +
                    $":{(int)VideoView.ActualHeight}";
            }
            else
            {
                var p = ar.Name.Split(':');
                _mediaPlayer.AspectRatio = $"{p[0]}:{p[1]}";
                _mediaPlayer.CropGeometry = "";
            }
        }

        private void BtnSubtitles_Click(object sender, RoutedEventArgs e)
        {
            var (popup, panel) = CreatePopupMenu();
            popup.PlacementTarget = BtnSubtitles;
            popup.Placement =
                System.Windows.Controls.Primitives.PlacementMode.Top;
            AddMenuItem(panel, popup, "📁  " + Translations.Get("loadFromDisk"),
                () => LoadSubtitleFromDisk());
            AddMenuItem(panel, popup, "🔑  OpenSubtitles.com",
                () => OpenSubtitleSearch(false));
            AddMenuItem(panel, popup, "🆓  SubDL",
                () => OpenSubtitleSearch(true));
            AddMenuItem(panel, popup, "", null, isSeparator: true);
            var spuDesc = _mediaPlayer.SpuDescription;
            if (spuDesc != null && spuDesc.Length > 1)
            {
                AddMenuItem(panel, popup,
                    "📺  " + Translations.Get("embeddedSub"),
                    null, isHeader: true);
                foreach (var track in spuDesc)
                {
                    if (track.Id == -1) continue;
                    var t = track;
                    AddMenuItem(panel, popup, $"   {t.Name}", () =>
                    {
                        StopSubtitleOverlay();
                        _mediaPlayer.SetSpu(t.Id);
                    });
                }
                AddMenuItem(panel, popup, "", null, isSeparator: true);
            }
            AddMenuItem(panel, popup, "🎨  " + Translations.Get("subStyle"),
                () => ShowSubtitleStyleDialog());
            AddMenuItem(panel, popup, "", null, isSeparator: true);
            AddMenuItem(panel, popup, "🚫  " + Translations.Get("subOff"), () =>
            {
                StopSubtitleOverlay();
                _mediaPlayer.SetSpu(-1);
            });
            popup.IsOpen = true;
        }

        private MenuItem MakeMenuItem(string h, RoutedEventHandler handler)
        {
            var item = new MenuItem { Header = h };
            item.Click += handler;
            return item;
        }

        private void LoadSubtitleFromDisk()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Otvori subtitle fajl",
                Filter = "Subtitle fajlovi|*.srt;*.ass;*.sub;*.ssa|" +
                         "Svi fajlovi|*.*"
            };
            if (!string.IsNullOrEmpty(_currentFile) &&
                File.Exists(_currentFile))
            {
                dlg.InitialDirectory =
                    Path.GetDirectoryName(_currentFile);
            }
            if (dlg.ShowDialog() == true) LoadSubtitleFile(dlg.FileName);
        }

        public void LoadSubtitleFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                MessageBox.Show($"{Translations.Get("subNotFound")}\n{path}",
                    "MakoMiniPlayer", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            var entries = SubtitleParser.Parse(path);
            if (entries.Count == 0)
            {
                MessageBox.Show(Translations.Get("subEmpty"),
                    "MakoMiniPlayer", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            _subtitleEntries = entries;
            _subtitleEnabled = true;
            _subtitleOffsetMs = 0;
            TxtSubOffset.Text = "+0.0s";
            _mediaPlayer.SetSpu(-1);
            _subtitleOverlay?.ApplyStyle(_settings);
            _subtitleOverlay?.RepositionNow();
            var fn = Path.GetFileName(_currentFile);
            Title = $"MakoMiniPlayer — {fn} 💬";
            TxtTitleFile.Text = $"— {fn} 💬";
        }

        private void StopSubtitleOverlay()
        {
            _subtitleEnabled = false;
            _subtitleEntries.Clear();
            _subtitleOffsetMs = 0;
            TxtSubOffset.Text = "+0.0s";
            _subtitleOverlay?.HideSubtitle();
        }

        private void OpenSubtitleSearch(bool useSubDL)
        {
            var suggested = string.IsNullOrEmpty(_currentFile) ? "" :
                Path.GetFileNameWithoutExtension(_currentFile);
            var win = new SubtitleSearchWindow(
                _settings, suggested, _currentFile);
            win.Owner = this;
            if (useSubDL) win.RbSubDL.IsChecked = true;
            var r = win.ShowDialog();
            if (r == true && !string.IsNullOrEmpty(win.DownloadedSubtitlePath))
                LoadSubtitleFile(win.DownloadedSubtitlePath);
        }

        private void BtnSubPlus_Click(object sender, RoutedEventArgs e)
        {
            _subtitleOffsetMs += 500;
            TxtSubOffset.Text = FormatOffset(_subtitleOffsetMs);
        }

        private void BtnSubMinus_Click(object sender, RoutedEventArgs e)
        {
            _subtitleOffsetMs -= 500;
            TxtSubOffset.Text = FormatOffset(_subtitleOffsetMs);
        }

        private string FormatOffset(long ms)
            => $"{(ms >= 0 ? "+" : "-")}{Math.Abs(ms) / 1000.0:F1}s";

        private void ShowSubtitleStyleDialog()
        {
            var win = new SubtitleStyleWindow(_settings);
            win.Owner = this;
            if (win.ShowDialog() == true)
            {
                _settings.Save();
                UpdateOverlayPosition();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_mediaPlayer == null || !_mediaPlayer.IsPlaying) return;
            if (!_isDraggingSeek)
                SeekBar.Value = _mediaPlayer.Position * SeekBar.Maximum;
            var cur = TimeSpan.FromMilliseconds(_mediaPlayer.Time);
            var tot = TimeSpan.FromMilliseconds(_mediaPlayer.Length);
            TxtTime.Text = $"{cur:hh\\:mm\\:ss} / {tot:hh\\:mm\\:ss}";
            UpdateSubtitleOverlay();
        }

        private void UpdateSubtitleOverlay()
        {
            if (!_subtitleEnabled || _subtitleEntries.Count == 0)
            {
                _subtitleOverlay?.HideSubtitle();
                return;
            }
            var ms = _mediaPlayer.Time + _subtitleOffsetMs;
            var entry = _subtitleEntries.FirstOrDefault(
                s => ms >= s.StartMs && ms <= s.EndMs);
            if (entry != null) _subtitleOverlay?.ShowSubtitle(entry.Text);
            else _subtitleOverlay?.HideSubtitle();
        }

        private void OnMediaPlaying(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                BtnPlayPause.Content = "⏸";
                VideoView.Visibility = Visibility.Visible;
                LoadAudioTracks();
            });
            _mediaPlayer.SetSpu(-1);
            _spuDisableTimer.Stop();
            _spuDisableTimer.Start();
        }

        private void OnMediaPaused(object sender, EventArgs e)
            => Dispatcher.Invoke(() => BtnPlayPause.Content = "⏸");

        private void OnMediaStopped(object sender, EventArgs e)
            => Dispatcher.Invoke(() =>
            {
                BtnPlayPause.Content = "▶";
                VideoView.Visibility = Visibility.Hidden;
            });

        private void OnMediaEndReached(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(_currentFile))
                {
                    _settings.SavePosition(_currentFile, 0);
                    _settings.Save();
                }
                BtnPlayPause.Content = "▶";
                SeekBar.Value = 0;
                _subtitleOverlay?.HideSubtitle();
            });
        }

        private void LoadAudioTracks()
        {
            CmbAudio.Items.Clear();
            var desc = _mediaPlayer?.AudioTrackDescription;
            if (desc == null) return;
            foreach (var t in desc)
                CmbAudio.Items.Add(new TrackItem { Id = t.Id, Name = t.Name });
            var pref = CmbAudio.Items.Cast<TrackItem>().FirstOrDefault(t =>
            {
                var n = t.Name.ToLower();
                return n.Contains("bos") || n.Contains("hrv") ||
                       n.Contains("srp") || n.Contains("cro") ||
                       n.Contains("ser") || n.Contains("hr") ||
                       n.Contains("bs") || n.Contains("sr");
            });
            var sel = pref ?? CmbAudio.Items.Cast<TrackItem>()
                .FirstOrDefault(t => t.Id >= 0);
            if (sel != null)
            {
                CmbAudio.SelectedItem = sel;
                _mediaPlayer.SetAudioTrack(sel.Id);
            }
        }

        private void LoadRecentFiles()
        {
            RecentPanel.Children.Clear();
            foreach (var path in _settings.RecentFiles.Take(8))
            {
                if (!File.Exists(path)) continue;
                var name = Path.GetFileNameWithoutExtension(path);
                var btn = new Button
                {
                    Content = $"▶ {name}",
                    Height = 22,
                    Padding = new Thickness(8, 0, 8, 0),
                    Margin = new Thickness(0, 0, 4, 0),
                    Background = new SolidColorBrush(
                        Color.FromRgb(45, 10, 78)),
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(0, 255, 136)),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    FontSize = 11,
                    Tag = path
                };
                btn.Click += (s, ev) => OpenFile((string)((Button)s).Tag);
                RecentPanel.Children.Add(btn);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.Key)
            {
                case Key.Space:
                    if (_mediaPlayer.IsPlaying) _mediaPlayer.Pause();
                    else _mediaPlayer.Play();
                    e.Handled = true; break;
                case Key.Left:
                    _mediaPlayer.Time =
                        Math.Max(0, _mediaPlayer.Time - 10000);
                    e.Handled = true; break;
                case Key.Right:
                    _mediaPlayer.Time += 10000;
                    e.Handled = true; break;
                case Key.Up:
                    VolumeBar.Value = Math.Min(100, VolumeBar.Value + 5);
                    e.Handled = true; break;
                case Key.Down:
                    VolumeBar.Value = Math.Max(0, VolumeBar.Value - 5);
                    e.Handled = true; break;
                case Key.F:
                    ToggleFullscreen(); e.Handled = true; break;
                case Key.Escape:
                    if (_isFullscreen) ToggleFullscreen();
                    e.Handled = true; break;
                case Key.S:
                    BtnSubtitles_Click(null, null);
                    e.Handled = true; break;
                case Key.A:
                    BtnAspect_Click(null, null);
                    e.Handled = true; break;
            }
        }

        private double _prevLeft, _prevTop, _prevWidth, _prevHeight;
        private void ToggleFullscreen()
        {
            var rootGrid = (Grid)Content;
            if (_isFullscreen)
            {
                _isFullscreen = false;
                if (BtnOnTop.IsChecked != true) Topmost = false;
                _hideControlsTimer.Stop();
                ShowControls();
                rootGrid.RowDefinitions[0].Height = new GridLength(32);
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.CanResizeWithGrip;
                WindowState = WindowState.Normal;
                Left = _prevLeft;
                Top = _prevTop;
                Width = _prevWidth;
                Height = _prevHeight;
                if (_subtitleOverlay != null)
                    _subtitleOverlay.WindowState = WindowState.Normal;
            }
            else
            {
                _isFullscreen = true;
                _prevLeft = Left;
                _prevTop = Top;
                _prevWidth = Width;
                _prevHeight = Height;
                Topmost = true;
                rootGrid.RowDefinitions[0].Height = new GridLength(0);
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Normal;
                Left = 0;
                Top = 0;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;
                ShowControls();
                if (_subtitleOverlay != null)
                {
                    _subtitleOverlay.WindowStyle = WindowStyle.None;
                    _subtitleOverlay.Topmost = true;
                }
            }
            UpdateOverlayPosition();
        }

        private string FormatTime(long ms)
        {
            var ts = TimeSpan.FromMilliseconds(ms);
            return ts.Hours > 0 ? $"{ts:hh\\:mm\\:ss}" : $"{ts:mm\\:ss}";
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_hookHandle != IntPtr.Zero)
                UnhookWindowsHookEx(_hookHandle);
            if (!string.IsNullOrEmpty(_currentFile) &&
                _mediaPlayer?.Length > 0)
                _settings.SavePosition(_currentFile, _mediaPlayer.Time);
            _settings.Save();
            _timer?.Stop();
            _hideControlsTimer?.Stop();
            _spuDisableTimer?.Stop();
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
            _subtitleOverlay?.Close();
            base.OnClosed(e);
        }
    }

    public class TrackItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }
}