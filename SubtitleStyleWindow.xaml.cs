using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MakoMiniPlayer
{
    public partial class SubtitleStyleWindow : Window
    {
        private readonly AppSettings _settings;

        private readonly List<(string Name, string Hex)> _colors = new()
        {
            ("Bijela",      "#FFFFFF"),
            ("Žuta",        "#FFFF00"),
            ("Zelena",      "#00FF00"),
            ("Cyan",        "#00FFFF"),
            ("Narančasta",  "#FFA500"),
            ("Crvena",      "#FF4444"),
            ("Plava",       "#4488FF"),
        };

        private readonly List<(string Name, string Hex)> _outlineColors = new()
        {
            ("Crna",        "#000000"),
            ("Tamno siva",  "#222222"),
            ("Tamno plava", "#000044"),
            ("Tamno crvena","#440000"),
            ("Bijela",      "#FFFFFF"),
        };

        public SubtitleStyleWindow(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;

            foreach (var c in _colors)
                CmbColor.Items.Add(c.Name);
            foreach (var c in _outlineColors)
                CmbOutlineColor.Items.Add(c.Name);

            // Postavi trenutne vrijednosti
            SliderSize.Value = _settings.SubtitleSize;
            SliderMargin.Value = _settings.SubtitleBottomMarginPct;
            SliderShadow.Value = _settings.SubtitleShadowRadius;
            SliderOutline.Value = _settings.SubtitleOutlineWidth;

            ChkBold.IsChecked = _settings.SubtitleBold;
            ChkShadow.IsChecked = _settings.SubtitleShadow;
            ChkOutline.IsChecked = _settings.SubtitleOutline;

            var colorIdx = _colors.FindIndex(c => c.Hex == _settings.SubtitleColor);
            CmbColor.SelectedIndex = colorIdx >= 0 ? colorIdx : 0;

            var outlineIdx = _outlineColors.FindIndex(
                c => c.Hex == _settings.SubtitleOutlineColor);
            CmbOutlineColor.SelectedIndex = outlineIdx >= 0 ? outlineIdx : 0;

            // Eventi za live preview
            ChkBold.Checked += (s, e) => UpdatePreview();
            ChkBold.Unchecked += (s, e) => UpdatePreview();
            ChkShadow.Checked += (s, e) => UpdatePreview();
            ChkShadow.Unchecked += (s, e) => UpdatePreview();
            ChkOutline.Checked += (s, e) => UpdatePreview();
            ChkOutline.Unchecked += (s, e) => UpdatePreview();
            CmbColor.SelectionChanged += (s, e) => UpdatePreview();
            CmbOutlineColor.SelectionChanged += (s, e) => UpdatePreview();

            UpdatePreview();
        }

        private void SliderSize_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtSizeLabel == null) return;
            TxtSizeLabel.Text = ((int)SliderSize.Value).ToString();
            UpdatePreview();
        }

        private void SliderMargin_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtMarginLabel == null) return;
            TxtMarginLabel.Text = ((int)SliderMargin.Value).ToString();
        }

        private void SliderShadow_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtShadowLabel == null) return;
            TxtShadowLabel.Text = ((int)SliderShadow.Value).ToString();
            UpdatePreview();
        }

        private void SliderOutline_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtOutlineLabel == null) return;
            TxtOutlineLabel.Text = $"{SliderOutline.Value:F1}";
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (TxtPreview == null) return;

            TxtPreview.FontSize = SliderSize.Value;
            TxtPreview.FontWeight = ChkBold.IsChecked == true
                ? FontWeights.Bold : FontWeights.Normal;

            // Boja teksta
            if (CmbColor.SelectedIndex >= 0)
            {
                var color = (Color)ColorConverter.ConvertFromString(
                    _colors[CmbColor.SelectedIndex].Hex);
                TxtPreview.Foreground = new SolidColorBrush(color);
            }

            // Pozadina uvijek transparentna
            TxtPreview.Background = Brushes.Transparent;

            // Shadow ili outline za preview
            if (ChkOutline.IsChecked == true && CmbOutlineColor.SelectedIndex >= 0)
            {
                var outHex = _outlineColors[CmbOutlineColor.SelectedIndex].Hex;
                var outColor = (Color)ColorConverter.ConvertFromString(outHex);
                TxtPreview.Effect = new DropShadowEffect
                {
                    Color = outColor,
                    ShadowDepth = 0,
                    BlurRadius = SliderOutline.Value * 2,
                    Opacity = 1
                };
            }
            else if (ChkShadow.IsChecked == true)
            {
                TxtPreview.Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    ShadowDepth = 3,
                    BlurRadius = SliderShadow.Value,
                    Opacity = 0.9
                };
            }
            else
            {
                TxtPreview.Effect = null;
            }
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            _settings.SubtitleSize = (float)SliderSize.Value;
            _settings.SubtitleBottomMarginPct = SliderMargin.Value;
            _settings.SubtitleShadowRadius = SliderShadow.Value;
            _settings.SubtitleOutlineWidth = SliderOutline.Value;

            _settings.SubtitleBold = ChkBold.IsChecked == true;
            _settings.SubtitleBackground = false;
            _settings.SubtitleShadow = ChkShadow.IsChecked == true;
            _settings.SubtitleOutline = ChkOutline.IsChecked == true;

            if (CmbColor.SelectedIndex >= 0)
                _settings.SubtitleColor = _colors[CmbColor.SelectedIndex].Hex;

            if (CmbOutlineColor.SelectedIndex >= 0)
                _settings.SubtitleOutlineColor =
                    _outlineColors[CmbOutlineColor.SelectedIndex].Hex;

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}