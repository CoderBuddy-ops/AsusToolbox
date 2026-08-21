using Asus.UI;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class ControlHelper
{

    [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

    static bool _invert = false;
    static bool _darkMode = false;
    static float _scale = 1;

    public static float Scale => _scale;
    public static bool DarkMode => _darkMode;

    public static void Adjust(RForm container, bool invert = false)
    {

        container.BackColor = RForm.formBack;
        container.ForeColor = RForm.foreMain;

        _invert = invert;
        _darkMode = container.darkTheme;
        AdjustControls(container.Controls);
        _invert = false;

    }

    public static void Resize(RForm container, float baseScale = 2)
    {
        _scale = GetDpiScale(container).Value / baseScale;
        if (Math.Abs(_scale - 1) > 0.2) ResizeControls(container.Controls);

    }

    private static void ResizeControls(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            var button = control as RButton;
            if (button != null && button.Image is not null)
                button.Image = ResizeImage(button.Image);

            /*
            var pictureBox = control as PictureBox;
            if (pictureBox != null && pictureBox.BackgroundImage is not null)
                pictureBox.BackgroundImage = ResizeImage(pictureBox.BackgroundImage);
            */

            ResizeControls(control.Controls);
        }
    }


    private static void AdjustControls(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {

            AdjustControls(control.Controls);

            var button = control as RButton;
            if (button != null)
            {
                button.BackColor = button.Secondary ? RForm.buttonSecond : RForm.buttonMain;
                button.ForeColor = RForm.foreMain;

                button.FlatStyle = FlatStyle.Flat;
                if (!button.Borderless)
                    button.FlatAppearance.BorderColor = button.Secondary ? RForm.borderSecond : RForm.borderMain;

                if (button.Image is not null && _invert)
                    button.Image = AdjustImage(button.Image);
            }

            var pictureBox = control as PictureBox;
            if (pictureBox != null && pictureBox.BackgroundImage is not null && _invert)
                pictureBox.BackgroundImage = AdjustImage(pictureBox.BackgroundImage);


            var combo = control as RComboBox;
            if (combo != null)
            {
                combo.BackColor = RForm.buttonMain;
                combo.ForeColor = RForm.foreMain;
                combo.BorderColor = RForm.borderMain;
                combo.ButtonColor = RForm.buttonMain;
                combo.ArrowColor = RForm.foreMain;
            }
            var rNumeric = control as RNumericUpDown;
            if (rNumeric is not null)
            {
                rNumeric.ApplyTheme(_darkMode);
            }
            else if (control is NumericUpDown numbericUpDown)
            {
                numbericUpDown.ForeColor = RForm.foreMain;
                numbericUpDown.BackColor = RForm.buttonMain;
            }

            var rText = control as RTextBox;
            if (rText is not null)
            {
                rText.ApplyTheme(_darkMode);
            }

            var gb = control as GroupBox;
            if (gb != null)
            {
                gb.ForeColor = RForm.foreMain;
            }

            var pn = control as Panel;
            if (pn != null && pn.Name.Contains("Header"))
            {
                pn.BackColor = RForm.buttonSecond;
            }

            var sl = control as Slider;
            if (sl != null)
            {
                sl.borderColor = RForm.buttonMain;
            }

            var chk = control as CheckBox;
            if (chk != null)
            {
                if (chk.BackColor != RForm.formBack)
                {
                    chk.BackColor = RForm.buttonSecond;
                    if (chk is RCheckBox)
                        chk.FlatAppearance.BorderColor = RForm.borderSecond;
                }
                SetWindowTheme(chk.Handle, _darkMode ? "DarkMode_Explorer" : "Explorer", null);
            }


        }
    }

    public static Lazy<float> GetDpiScale(Control control)
    {
        return new Lazy<float>(() =>
        {
            using (var graphics = control.CreateGraphics())
                return graphics.DpiX / 96.0f;
        });
    }

    /// <summary>Rounded rectangular window region (borderless window corners).</summary>
    public static Region CreateRoundedRegion(int width, int height, int radius)
    {
        if (width <= 0 || height <= 0 || radius <= 0) return new Region(new Rectangle(0, 0, width, height));
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(0, 0, d, d, 180, 90);
        path.AddArc(width - d, 0, d, d, 270, 90);
        path.AddArc(width - d, height - d, d, d, 0, 90);
        path.AddArc(0, height - d, d, d, 90, 90);
        path.CloseFigure();
        return new Region(path);
    }

    /// <summary>Gets a rounded GraphicsPath for card panel drawing.</summary>
    public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        float d = radius * 2f;
        if (rect.Width <= 0 || rect.Height <= 0) return path;
        path.StartFigure();
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>Paints a clean rounded card background with subtle border.</summary>
    public static void PaintCard(Control control, Graphics g, int radius = 8)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle rect = new Rectangle(0, 0, control.Width - 1, control.Height - 1);
        if (rect.Width <= 1 || rect.Height <= 1) return;
        using GraphicsPath path = GetRoundedPath(rect, radius);
        using SolidBrush bgBrush = new SolidBrush(RForm.buttonSecond);
        using Pen borderPen = new Pen(RForm.borderMain, 1f);
        g.FillPath(bgBrush, path);
        g.DrawPath(borderPen, path);
    }

    /// <summary>Tints a monochromatic bitmap image to the target color.</summary>
    public static Image TintImage(Image image, Color targetColor)
    {
        var bmp = new Bitmap(image.Width, image.Height);
        using (var g = Graphics.FromImage(bmp))
        {
            var attributes = new ImageAttributes();
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
            {
                new float[] { 0, 0, 0, 0, 0 },
                new float[] { 0, 0, 0, 0, 0 },
                new float[] { 0, 0, 0, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { targetColor.R / 255f, targetColor.G / 255f, targetColor.B / 255f, 0, 1 }
            });
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
        }
        return bmp;
    }

    private static Image ResizeImage(Image image)
    {
        return ResizeImage(image, _scale);
    }

    public static Image ResizeImage(Image image, float scale)
    {
        if (Math.Abs(scale - 1) < 0.1) return image;

        var newSize = new Size((int)(image.Width * scale), (int)(image.Height * scale));
        var pic = new Bitmap(newSize.Width, newSize.Height);

        using (var g = Graphics.FromImage(pic))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, new Rectangle(new Point(), newSize));
        }
        return pic;
    }

    // Design tokens
    private const float GradientHeightFraction = 0.3f;
    private const float LightGradientHeightFraction = 0.9f;
    private const int TopFadeAlpha = 64;

    public static void DrawGradientBorder(Graphics g, Rectangle bounds, Color sideColor, int radius, float strokeWidth = 1f, PenAlignment alignment = PenAlignment.Center, float topLighten = 0.1f)
    {
        Color topColor = !_darkMode && strokeWidth <= 1f
            ? Color.FromArgb(TopFadeAlpha, sideColor)
            : Color.FromArgb(sideColor.A,
                (int)(sideColor.R + (255 - sideColor.R) * topLighten),
                (int)(sideColor.G + (255 - sideColor.G) * topLighten),
                (int)(sideColor.B + (255 - sideColor.B) * topLighten));

        float flatHeight = Math.Max(1f, strokeWidth);
        float gradHeight = (float)Math.Round(bounds.Height * (_darkMode ? GradientHeightFraction : LightGradientHeightFraction));
        float pad = strokeWidth;
        float axisStart = bounds.Y - pad;
        float axisEnd = bounds.Y + bounds.Height + pad;
        float axisLen = axisEnd - axisStart;
        float p1 = Math.Max(0f, Math.Min(0.98f, (pad + flatHeight) / axisLen));
        float p2 = Math.Max(p1 + 0.01f, Math.Min(1f, (pad + flatHeight + gradHeight) / axisLen));

        using (GraphicsPath path = RComboBox.RoundedRect(bounds, radius, radius))
        using (LinearGradientBrush brush = new LinearGradientBrush(
            new PointF(0, axisStart), new PointF(0, axisEnd),
            topColor, sideColor))
        {
            brush.InterpolationColors = new ColorBlend(4)
            {
                Colors = new[] { topColor, topColor, sideColor, sideColor },
                Positions = new[] { 0f, p1, p2, 1f }
            };
            using (Pen pen = new Pen(brush, strokeWidth) { Alignment = alignment })
            {
                SmoothingMode prev = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(pen, path);
                g.SmoothingMode = prev;
            }
        }
    }

    private static readonly ImageAttributes _invertAttributes = CreateInvertAttributes();

    private static ImageAttributes CreateInvertAttributes()
    {
        var matrix = new ColorMatrix(new[]
        {
            new float[] { -1,  0,  0, 0, 0 },
            new float[] {  0, -1,  0, 0, 0 },
            new float[] {  0,  0, -1, 0, 0 },
            new float[] {  0,  0,  0, 1, 0 },
            new float[] {  1,  1,  1, 0, 1 }
        });
        var attr = new ImageAttributes();
        attr.SetColorMatrix(matrix);
        return attr;
    }

    private static Image AdjustImage(Image image)
    {
        var pic = new Bitmap(image.Width, image.Height);
        using (var g = Graphics.FromImage(pic))
        {
            g.DrawImage(image,
                new Rectangle(0, 0, image.Width, image.Height),
                0, 0, image.Width, image.Height,
                GraphicsUnit.Pixel, _invertAttributes);
        }
        return pic;
    }



    public static Image RecolorDarkPixels(Image image, Color targetColor, byte luminanceThreshold = 128)
    {
        var pic = new Bitmap(image);
        for (int y = 0; y < pic.Height; y++)
        {
            for (int x = 0; x < pic.Width; x++)
            {
                Color col = pic.GetPixel(x, y);
                if (col.A == 0) continue;
                int lum = (col.R + col.G + col.B) / 3;
                if (lum < luminanceThreshold)
                    pic.SetPixel(x, y, Color.FromArgb(col.A, targetColor));
            }
        }
        return pic;
    }

    public static Image OverlayBadge(Image baseImage, Image badge, Color circleColor,
        float badgeScale = 0.5f, float shiftFraction = 0.18f,
        int? iconWidth = null, int? iconHeight = null)
    {
        int iw = iconWidth ?? baseImage.Width;
        int ih = iconHeight ?? baseImage.Height;

        int badgeSize = (int)(iw * badgeScale);
        int shift = (int)(badgeSize * shiftFraction);

        int newW = Math.Max(baseImage.Width, iw + shift);
        int newH = Math.Max(baseImage.Height, ih + shift);

        var pic = new Bitmap(newW, newH);
        using (var g = Graphics.FromImage(pic))
        using (var coloredBadge = (Bitmap)RecolorDarkPixels(badge, circleColor))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawImage(baseImage, 0, 0, baseImage.Width, baseImage.Height);

            int badgeX = iw - badgeSize + shift;
            int badgeY = ih - badgeSize + shift;
            g.DrawImage(coloredBadge, badgeX, badgeY, badgeSize, badgeSize);
        }
        return pic;
    }

    public static Image OverlayChargeBars(Image baseImage, int level, int max, Color color,
        int? iconWidth = null, int? iconHeight = null)
    {
        if (max <= 0) return baseImage;

        int iw = iconWidth ?? baseImage.Width;
        int ih = iconHeight ?? baseImage.Height;

        float s = iw / 48f;
        int barHeight = Math.Max(2, (int)Math.Round(10 * s));
        int barWidth = Math.Max(1, (int)Math.Round(4 * s));
        int barGap = Math.Max(1, (int)Math.Round(2 * s));
        int totalGap = barGap * (max - 1);
        int usedW = barWidth * max + totalGap;
        int xStart = (iw - usedW) / 2;

        // If a previous overlay extended the canvas below the icon (e.g. a corner badge),
        // start the bars below that extension; otherwise sit them right under the icon.
        int yStart = baseImage.Height > ih
            ? baseImage.Height + Math.Max(2, (int)Math.Round(2 * s))
            : ih + Math.Max(2, (int)Math.Round(3 * s));

        int newH = Math.Max(baseImage.Height, yStart + barHeight);
        int newW = Math.Max(baseImage.Width, iw);

        var pic = new Bitmap(newW, newH);
        using (var g = Graphics.FromImage(pic))
        using (var filled = new SolidBrush(color))
        using (var empty = new SolidBrush(Color.FromArgb(72, color)))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(baseImage, 0, 0, baseImage.Width, baseImage.Height);

            for (int i = 0; i < max; i++)
            {
                var rect = new Rectangle(xStart + i * (barWidth + barGap), yStart, barWidth, barHeight);
                g.FillRectangle(i < level ? filled : empty, rect);
            }
        }
        return pic;
    }

}
