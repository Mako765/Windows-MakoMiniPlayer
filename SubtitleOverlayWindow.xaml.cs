#nullable disable
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MakoMiniPlayer
{
    public partial class SubtitleOverlayWindow : Window
    {
        private double _controlsHeight = 100;
        private AppSettings _currentSettings;

        public SubtitleOverlayWindow()
        {
            InitializeComponent();
        }

        public void ShowSubtitle(string text)
        {
            TxtSubtitle.Text = text;
            TxtSubtitle.VisibilityEx = Visibility.Visible;
        }

        public void HideSubtitle()
        {
            TxtSubtitle.VisibilityEx = Visibility.Collapsed;
        }

        public void SetControlsHeight(double height)
        {
            if (height > 10 && height < 500)
                _controlsHeight = height;
        }

        public void ApplyStyle(AppSettings s)
        {
            if (s == null) return;
            _currentSettings = s;

            try
            {
                var color = (Color)ColorConverter
                    .ConvertFromString(s.SubtitleColor);
                TxtSubtitle.Foreground = new SolidColorBrush(color);
                TxtSubtitle.FontSize = s.SubtitleSize;
                TxtSubtitle.FontWeight = s.SubtitleBold
                    ? FontWeights.Bold : FontWeights.Normal;

                if (s.SubtitleOutline && s.SubtitleOutlineWidth > 0)
                {
                    var outColor = (Color)ColorConverter
                        .ConvertFromString(s.SubtitleOutlineColor);
                    TxtSubtitle.Stroke =
                        new SolidColorBrush(outColor);
                    TxtSubtitle.StrokeThickness = s.SubtitleOutlineWidth;
                }
                else
                {
                    TxtSubtitle.Stroke = null;
                    TxtSubtitle.StrokeThickness = 0;
                }

                // Pozadina uvijek transparentna
                TxtSubtitle.Background = Brushes.Transparent;

                if (s.SubtitleShadow)
                    TxtSubtitle.Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        ShadowDepth = 2,
                        BlurRadius = s.SubtitleShadowRadius,
                        Opacity = s.SubtitleShadowOpacity
                    };
                else
                    TxtSubtitle.Effect = null;

                RepositionNow();
            }
            catch { }
        }

        public void RepositionNow()
        {
            if (_currentSettings == null) return;

            var winW = ActualWidth;
            var winH = ActualHeight;
            if (winW <= 0 || winH <= 0) return;

            var txtW = winW * 0.92;
            var lineH = _currentSettings.SubtitleSize * 1.4;
            var txtH = lineH * 1.5;
            var videoH = winH - _controlsHeight;
            if (videoH < 50) videoH = winH;

            var pct = _currentSettings.SubtitleBottomMarginPct / 100.0;
            var fromBottom = videoH * pct;
            if (fromBottom < 5) fromBottom = 5;
            var topMargin = winH - _controlsHeight - fromBottom - txtH;

            // VAŽNO — postavi širinu da tekst ne lomi prerano
            TxtSubtitle.Width = txtW;
            TxtSubtitle.Margin = new Thickness(0, topMargin, 0, 0);
        }
    }
}