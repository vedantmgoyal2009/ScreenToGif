using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenToGif.Util;

public static class ImageHelper
{
    /// <summary>
    /// Creates a solid color BitmapSource.
    /// </summary>
    /// <param name="color">The Background color.</param>
    /// <param name="width">The Width of the image.</param>
    /// <param name="height">The Height of the image.</param>
    /// <param name="dpi">The dpi of the image.</param>
    /// <param name="pixelFormat">The PixelFormat.</param>
    /// <returns>A BitmapSource of the given parameters.</returns>
    public static BitmapSource CreateEmtpyBitmapSource(System.Windows.Media.Color color, int width, int height, double dpi, PixelFormat pixelFormat)
    {
        var rawStride = (width * pixelFormat.BitsPerPixel + 7) / 8;
        var rawImage = new byte[rawStride * height];

        var colors = new List<System.Windows.Media.Color> { color };
        var myPalette = new BitmapPalette(colors);

        return BitmapSource.Create(width, height, dpi, dpi, pixelFormat, myPalette, rawImage, rawStride);
    }

    /// <summary>
    /// Converts a BitmapSource to a BitmapImage.
    /// </summary>
    /// <typeparam name="T">A BitmapEncoder derived class.</typeparam>
    /// <param name="bitmapSource">The source to convert.</param>
    /// <returns>A converted BitmapImage.</returns>
    private static BitmapImage GetBitmapImage<T>(BitmapSource bitmapSource) where T : BitmapEncoder, new()
    {
        var frame = BitmapFrame.Create(bitmapSource);
        var encoder = new T();
        encoder.Frames.Add(frame);

        var bitmapImage = new BitmapImage();
        bool isCreated;

        try
        {
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                isCreated = true;
            }
        }
        catch
        {
            isCreated = false;
        }

        return isCreated ? bitmapImage : null;
    }

    public static BitmapSource FromArray(byte[] data, int width, int height, int channels, int bitsPerPixel = 32)
    {
        var format = PixelFormats.Default;
        var stride = channels * width;

        if (channels == 1)
        {
            if (bitsPerPixel == 1)
            {
                format = PixelFormats.BlackWhite;
                stride = width / 8;
            }
            else
                format = PixelFormats.Gray8; //Grey scale image 0-255.
        }
        else if (channels == 3)
        {
            format = PixelFormats.Bgr24; //RGB.
            stride = 3 * ((bitsPerPixel * width + 31) / 32);
        }
        else if (channels == 4)
        {
            format = PixelFormats.Bgr32; //RGB + alpha.
            stride = 4 * ((bitsPerPixel * width + 31) / 32);
        }

        //for (var i = data.Count; i < width * height * ch; i++) //data.Count - 1
        //    data.Add(0);

        var wbm = new WriteableBitmap(width, height, 96, 96, format, null);
        wbm.WritePixels(new Int32Rect(0, 0, width, height), data, stride, 0);

        return wbm;
    }

    public static BitmapSource FromArray(List<byte> data, int w, int h, int ch, int bitsPerPixel = 8)
    {
        var format = PixelFormats.Default;

        if (ch == 1)
        {
            if (bitsPerPixel == 1)
                format = PixelFormats.BlackWhite;
            else
                format = PixelFormats.Gray8; //Grey scale image 0-255.
        }
        else if (ch == 3)
            format = PixelFormats.Bgr24; //RGB.
        else if (ch == 4)
            format = PixelFormats.Bgr32; //RGB + alpha.

        for (var i = data.Count; i < w * h * ch; i++) //data.Count - 1
            data.Add(0);

        var wbm = new WriteableBitmap(w, h, 96, 96, format, null);
        wbm.WritePixels(new Int32Rect(0, 0, w, h), data.ToArray(), ch * w, 0);

        return wbm;
    }

    public static void SavePixelArrayToFile(IReadOnlyList<byte> pixels, int width, int height, int channels, string filePath)
    {
        //var img = BitmapSource.Create(project.Frames[index].Rect.Width, project.Frames[index].Rect.Height, 96, 96, PixelFormats.Bgra32, null, newPixels, 4 * project.Frames[index].Rect.Width);

        //using (var stream = new FileStream(project.ChunkPath + index + ".png", FileMode.Create))
        //{
        //    var encoder = new PngBitmapEncoder();
        //    encoder.Frames.Add(BitmapFrame.Create(img));
        //    encoder.Save(stream);
        //    stream.Close();
        //}

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(FromArray(pixels.ToList(), width, height, channels)));
            encoder.Save(fileStream);
        }
    }
}