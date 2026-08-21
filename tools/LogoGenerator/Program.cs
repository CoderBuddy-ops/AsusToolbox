using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// Generates the application icons from the supplied Asus logo:
//   tools/LogoGenerator/source/asus.jpg   (canonical source asset)
//
// Writes:
//   Asus/favicon.ico                      app/exe/window icon (16..256 px)
//   Asus/Resources/standard.ico           tray icon (colour)
//   Asus/Resources/dark-standard.ico      tray icon (grayscale)
//   Asus/Resources/light-standard.ico     tray icon (grayscale)
//   Asus/Resources/eco.ico                GPU-mode tray variant (same logo)
//   Asus/Resources/dark-eco.ico           GPU-mode tray variant (grayscale)
//   Asus/Resources/light-eco.ico          GPU-mode tray variant (grayscale)
//   Asus/Resources/ultimate.ico           GPU-mode tray variant (same logo)
//
// The supplied artwork is used as-is:
// 1. Red emblem is detected and extracted with exact bounds.
// 2. Circular dark badge is drawn on transparent canvas.
// 3. Red emblem is centered inside the circular badge with balanced padding.
// 4. Frames 16..128 are encoded as standard uncompressed Win32 DIBs with 32bpp alpha.
// 5. Frame 256 is encoded as PNG.
//
// Run: dotnet run --project tools/LogoGenerator

static class Program
{
    static readonly int[] AppSizes = { 16, 20, 24, 32, 48, 64, 128, 256 };
    static readonly int[] TraySizes = { 16, 20, 24, 32, 48, 64 };

    static string Root => FindRoot();

    static string FindRoot()
    {
        var candidates = new List<string>
        {
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")),
        };
        foreach (var c in candidates)
            if (Directory.Exists(Path.Combine(c, "Asus")) &&
                File.Exists(Path.Combine(c, "tools", "LogoGenerator", "source", "asus.jpg")))
                return c;
        return candidates[0];
    }

    static void Main()
    {
        string sourcePath = Path.Combine(Root, "tools", "LogoGenerator", "source", "asus.jpg");
        if (!File.Exists(sourcePath))
        {
            Console.Error.WriteLine("Logo source not found: " + sourcePath);
            Environment.Exit(1);
        }

        using var srcImg = Image.FromFile(sourcePath);
        using var srcBmp = new Bitmap(srcImg);

        Color bg = srcBmp.GetPixel(0, 0);
        int minX = srcBmp.Width, minY = srcBmp.Height, maxX = 0, maxY = 0;
        for (int y = 0; y < srcBmp.Height; y++)
        {
            for (int x = 0; x < srcBmp.Width; x++)
            {
                Color p = srcBmp.GetPixel(x, y);
                // Strict red emblem detection (isolates the stylized A ribbon from faint background arcs)
                if (p.R > 80 && (p.R - p.G) > 35 && (p.R - p.B) > 35)
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }

        int emblemW = maxX - minX + 1;
        int emblemH = maxY - minY + 1;

        using var emblem = new Bitmap(emblemW, emblemH, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(emblem))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(srcBmp, new Rectangle(0, 0, emblemW, emblemH),
                new Rectangle(minX, minY, emblemW, emblemH), GraphicsUnit.Pixel);
        }

        string asus = Path.Combine(Root, "Asus");
        string res = Path.Combine(asus, "Resources");

        WriteIco(Path.Combine(asus, "favicon.ico"), AppSizes, emblem, bg, grayscale: false);
        WriteIco(Path.Combine(res, "standard.ico"), TraySizes, emblem, bg, grayscale: false);
        WriteIco(Path.Combine(res, "dark-standard.ico"), TraySizes, emblem, bg, grayscale: true);
        WriteIco(Path.Combine(res, "light-standard.ico"), TraySizes, emblem, bg, grayscale: true);
        WriteIco(Path.Combine(res, "eco.ico"), TraySizes, emblem, bg, grayscale: false);
        WriteIco(Path.Combine(res, "dark-eco.ico"), TraySizes, emblem, bg, grayscale: true);
        WriteIco(Path.Combine(res, "light-eco.ico"), TraySizes, emblem, bg, grayscale: true);
        WriteIco(Path.Combine(res, "ultimate.ico"), TraySizes, emblem, bg, grayscale: false);

        Console.WriteLine("Icons generated successfully from " + sourcePath);
    }

    static Bitmap Render(Image emblem, Color bgColor, int size, bool grayscale)
    {
        var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.Clear(Color.Transparent);

            // True circular badge occupying the canvas
            float margin = size >= 32 ? 0.75f : 0.5f;
            float diameter = size - margin * 2;
            RectangleF badgeRect = new RectangleF(margin, margin, diameter, diameter);

            // Inscribed circular clipping: guarantees 100% transparent corners outside the circle
            using (var circlePath = new GraphicsPath())
            {
                circlePath.AddEllipse(badgeRect);
                g.SetClip(circlePath);

                using (var brush = new SolidBrush(bgColor))
                {
                    g.FillEllipse(brush, badgeRect);
                }

                // Center the red emblem cleanly inside the circle (~72% diameter fill, zero clipping)
                float scale = (diameter * 0.72f) / Math.Max(emblem.Width, emblem.Height);
                float dw = emblem.Width * scale;
                float dh = emblem.Height * scale;
                float dx = (size - dw) / 2.0f;
                float dy = (size - dh) / 2.0f;

                g.DrawImage(emblem, new RectangleF(dx, dy, dw, dh));
            }
        }

        if (!grayscale) return bmp;

        var gray = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(gray))
        using (var attr = new ImageAttributes())
        {
            attr.SetColorMatrix(new ColorMatrix(new[]
            {
                new[] { 0.299f, 0.299f, 0.299f, 0f, 0f },
                new[] { 0.587f, 0.587f, 0.587f, 0f, 0f },
                new[] { 0.114f, 0.114f, 0.114f, 0f, 0f },
                new[] { 0f,     0f,     0f,     1f, 0f },
                new[] { 0f,     0f,     0f,     0f, 1f }
            }));
            g.DrawImage(bmp, new Rectangle(0, 0, size, size), 0, 0, size, size,
                GraphicsUnit.Pixel, attr);
        }
        bmp.Dispose();
        return gray;
    }

    static void WriteIco(string path, int[] sizes, Image emblem, Color bgColor, bool grayscale)
    {
        var rawFrames = new List<(int size, byte[] data)>();
        foreach (int s in sizes)
        {
            using var bmp = Render(emblem, bgColor, s, grayscale);
            byte[] frameData;
            if (s == 256)
            {
                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                frameData = ms.ToArray();
            }
            else
            {
                frameData = CreateDibFrame(bmp);
            }
            rawFrames.Add((s, frameData));
        }

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        bw.Write((ushort)0);                 // Reserved (0)
        bw.Write((ushort)1);                 // Resource Type (1 = Icon)
        bw.Write((ushort)rawFrames.Count);   // Image Count

        int dataOffset = 6 + (16 * rawFrames.Count);

        foreach (var (size, data) in rawFrames)
        {
            bw.Write((byte)(size >= 256 ? 0 : size)); // Width
            bw.Write((byte)(size >= 256 ? 0 : size)); // Height
            bw.Write((byte)0);                       // Color Count
            bw.Write((byte)0);                       // Reserved
            bw.Write((ushort)1);                     // Planes
            bw.Write((ushort)32);                    // Bit Count (32bpp)
            bw.Write((uint)data.Length);             // Size of image data in bytes
            bw.Write((uint)dataOffset);              // File offset to image data

            dataOffset += data.Length;
        }

        foreach (var (_, data) in rawFrames)
        {
            bw.Write(data);
        }

        Console.WriteLine($"  {Path.GetFileName(path)}  ({rawFrames.Count} frames, {fs.Length / 1024.0:F1} KB)");
    }

    static byte[] CreateDibFrame(Bitmap bmp)
    {
        int w = bmp.Width;
        int h = bmp.Height;

        int andStride = ((w + 31) / 32) * 4;
        int andSize = andStride * h;
        int xorSize = w * h * 4;
        int dibSize = 40 + xorSize + andSize;

        byte[] dib = new byte[dibSize];
        using var ms = new MemoryStream(dib);
        using var bw = new BinaryWriter(ms);

        // BITMAPINFOHEADER (40 bytes)
        bw.Write((uint)40);          // biSize
        bw.Write((int)w);            // biWidth
        bw.Write((int)(h * 2));      // biHeight (XOR height + AND height)
        bw.Write((ushort)1);         // biPlanes
        bw.Write((ushort)32);        // biBitCount
        bw.Write((uint)0);           // biCompression (BI_RGB)
        bw.Write((uint)xorSize);     // biSizeImage
        bw.Write((int)0);            // biXPelsPerMeter
        bw.Write((int)0);            // biYPelsPerMeter
        bw.Write((uint)0);           // biClrUsed
        bw.Write((uint)0);           // biClrImportant

        // XOR / Color Pixels (32bpp ARGB -> BGRA in bottom-up scanlines)
        byte[] andMask = new byte[andSize];

        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                Color p = bmp.GetPixel(x, y);
                bw.Write(p.B);
                bw.Write(p.G);
                bw.Write(p.R);
                bw.Write(p.A);

                // If transparent, set 1 in AND mask
                if (p.A == 0)
                {
                    int rowFromBottom = (h - 1) - y;
                    int byteIndex = (rowFromBottom * andStride) + (x / 8);
                    int bitIndex = 7 - (x % 8);
                    andMask[byteIndex] |= (byte)(1 << bitIndex);
                }
            }
        }

        // AND Mask
        bw.Write(andMask);

        return dib;
    }
}

