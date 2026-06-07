using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace MakoMiniPlayer
{
    public class OutlinedTextBlock : FrameworkElement
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
                typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata("",
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush),
                typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(Brushes.White,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush),
                typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(Brushes.Black,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double),
                typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(2.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double),
                typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(32.0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register("FontWeight", typeof(FontWeight),
                typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(FontWeights.Normal,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush),
                typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(Brushes.Transparent,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty VisibilityExProperty =
            DependencyProperty.Register("VisibilityEx", typeof(Visibility),
                typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(Visibility.Collapsed,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }
        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }
        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }
        public Visibility VisibilityEx
        {
            get => (Visibility)GetValue(VisibilityExProperty);
            set => SetValue(VisibilityExProperty, value);
        }

        private FormattedText BuildFormattedText(double maxWidth)
        {
            var typeface = new Typeface(
                new FontFamily("Arial"),
                FontStyles.Normal,
                FontWeight,
                FontStretches.Normal);

            var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var ft = new FormattedText(
                string.IsNullOrEmpty(Text) ? " " : Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                FontSize,
                Foreground,
                dpi);

            ft.MaxTextWidth = maxWidth > 10 ? maxWidth : 800;
            ft.TextAlignment = TextAlignment.Center;
            return ft;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (VisibilityEx == Visibility.Collapsed)
                return new Size(0, 0);

            double maxW = !double.IsNaN(Width) && Width > 10
                ? Width - StrokeThickness * 2
                : (double.IsInfinity(availableSize.Width)
                    ? 800 : availableSize.Width - StrokeThickness * 2);

            var ft = BuildFormattedText(maxW);
            return new Size(
                ft.Width + StrokeThickness * 2,
                ft.Height + StrokeThickness * 2);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (VisibilityEx == Visibility.Collapsed || string.IsNullOrEmpty(Text))
                return;

            double maxW = !double.IsNaN(Width) && Width > 10
                ? Width - StrokeThickness * 2
                : ActualWidth - StrokeThickness * 2;
            if (maxW < 1) maxW = 800;

            var ft = BuildFormattedText(maxW);
            var origin = new Point(StrokeThickness, StrokeThickness);
            var geo = ft.BuildGeometry(origin);

            // Pozadina
            if (Background != null && Background != Brushes.Transparent)
                dc.DrawRectangle(Background, null,
                    new Rect(0, 0, ActualWidth, ActualHeight));

            // Outline
            if (StrokeThickness > 0 && Stroke != null)
                dc.DrawGeometry(null,
                    new Pen(Stroke, StrokeThickness * 2)
                    { LineJoin = PenLineJoin.Round },
                    geo);

            // Tekst
            dc.DrawGeometry(Foreground, null, geo);
        }
    }
}