using ScreenToGif.Domain.Models.Project.Cached.Sequences;
using ScreenToGif.Util;
using ScreenToGif.ViewModel.Editor;
using ScreenToGif.ViewModel.Project.Sequences.SubSequences;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenToGif.ViewModel.Project.Sequences;

public class FrameSequenceViewModel : RasterSequenceViewModel
{
    private ObservableCollection<FrameSubSequenceViewModel> _frames;

    /// <summary>
    /// Each frame with its timings.
    /// </summary>
    public ObservableCollection<FrameSubSequenceViewModel> Frames
    {
        get => _frames;
        set => SetProperty(ref _frames, value);
    }

    public static FrameSequenceViewModel FromModel(FrameSequence sequence, EditorViewModel baseViewModel)
    {
        return new FrameSequenceViewModel
        {
            Id = sequence.Id,
            StartTime = sequence.StartTime,
            EndTime = sequence.EndTime,
            Opacity = sequence.Opacity,
            Background = sequence.Background,
            Effects = new ObservableCollection<object>(sequence.Effects), //TODO
            StreamPosition = sequence.StreamPosition,
            CachePath = sequence.CachePath,
            EditorViewModel = baseViewModel,
            Left = sequence.Left,
            Top = sequence.Top,
            Width = sequence.Width,
            Height = sequence.Height,
            Angle = sequence.Angle,
            Origin = sequence.Origin,
            OriginalWidth = sequence.OriginalWidth,
            OriginalHeight = sequence.OriginalHeight,
            ChannelCount = sequence.ChannelCount,
            BitsPerChannel = sequence.BitsPerChannel,
            HorizontalDpi = sequence.HorizontalDpi,
            VerticalDpi = sequence.VerticalDpi,
            Frames = new ObservableCollection<FrameSubSequenceViewModel>(sequence.Frames.Select(s => FrameSubSequenceViewModel.FromModel(s, baseViewModel)).ToList())
        };
    }

    public override void RenderAt(IntPtr current, int canvasWidth, int canvasHeight, TimeSpan timestamp, double quality, string cachePath)
    {
        var ticks = (ulong)timestamp.Ticks;

        //Get first frame after timestamp. TODO: I should probably get the frames at timestamp + 60fps(16.6ms) and merge at opacity/n
        var frame = Frames.FirstOrDefault(f => f.TimeStampInTicks >= ticks);

        if (frame == null)
            return;

        using var readStream = new FileStream(cachePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        readStream.Position = (long) frame.DataStreamPosition;

        var data = readStream.ReadBytes((uint) frame.DataLength);

        Draw(current, canvasWidth, canvasHeight, frame);

        var stride = frame.Width * (( frame.BitsPerChannel * frame.ChannelCount + 7) / 8);

        //Cut any part of the sequence that overflows outside of the top/left bounds.
        var offsetX = 0 > Left ? Left * -1 : 0;
        var offsetY = 0 > Top ? Top * -1 : 0;

        //Cut any part of the frame that overflows outside of the bottom/right bounds.
        var widthOffset = Left + Width - canvasWidth;
        var heightOffset = Top + Height - canvasHeight;

        var finalLeft = 0 > Left ? 0 : Left;
        var finalTop = 0 > Top ? 0 : Top;
        var finalWidth = widthOffset > 0 ? Width - widthOffset : Width;
        var finalHeight = heightOffset > 0 ? Height - heightOffset : Height;

        //Merge with target canvas (opacity, position).
        DrawOld(finalLeft, finalTop, offsetX, offsetY, finalWidth, finalHeight, Opacity, current, stride, data);
    }

    private void Draw(IntPtr current, int canvasWidth, int canvasHeight, FrameSubSequenceViewModel frame)
    {
        //TODO:
        //Sequences can have a background.
        //Sequences have angles.
        //Sequences have sizes/positions.
        //Sequences can have effects (shadow, border).
        //Frames have angles.
        //Frames have sizes/positions.

        //Maybe do a single run.
        //  Maybe into an aux buffer?
        //  Avoid using aux buffers, because it requires a copy run.
        //Apply frame resize.
        //Apply frame re-angle.
        //Draw sequence background (possibly store in aux buffer cache).
        //Apply sequence resize.
        //Apply sequence re-angle.

        //Maybe instead of WidthxHeight work with scale?

        //Alter size.
        if (frame.OriginalHeight != frame.Height || frame.OriginalWidth != frame.Width)
        {
            //If not the same size, do a resize run.
            //Need an aux buffer to hold new data.
        }
        
        Angle = 25;
        frame.Angle = 25;

        //Alter Angle
        if (frame.Angle != 0)
        {
            //If not Angle = 0, do a reangle run.
            //Is this a 0,5/0,5 rotation?
            //x' = x * cos(a) + y * sin(a)
            //y' = y * cos(a) - x * sin(a)

            //var newLeft = Left * Math.Cos(Angle) + Top * Math.Sin(Angle);
            //var newTop = Top * Math.Cos(Angle) - Left * Math.Sin(Angle);

            //Altering the angle in non-square ways, makes the size of the rectangle to be different.
        }
    }

    private static void DrawOld(int posX, int posY, int offsetX, int offsetY, int width, int height, double opacity, IntPtr address, int pitch, IReadOnlyList<byte> buffer)
    {
        for (var row = offsetY; row < height; row++)
        {
            for (var col = offsetX; col < width; col++)
            {
                var surfaceIndex = (row - offsetY + posY) * pitch + (col - offsetX + posX) * 4;
                var bufferIndex = row * pitch + col * 4;

                int topAlpha = (byte)(buffer[bufferIndex + 3] * opacity);
                int bottomAlpha = Marshal.ReadByte(address, surfaceIndex + 3);

                ////Alpha = topA + (bottomA * (255 - topA) / 255)
                //var alpha = topAlpha + (bottomAlpha * (255 - topAlpha) / 255);
                //Marshal.WriteByte(address, surfaceIndex + 3, (byte)alpha);

                ////Blue = (topBlue * topA + bottomBlue * bottomA * (255 - topA) / 255) / alpha
                //Marshal.WriteByte(address, surfaceIndex,     (byte)((buffer[bufferIndex    ] * topAlpha + Marshal.ReadByte(address, surfaceIndex    ) * bottomAlpha * (255 - topAlpha) / 255) / alpha));

                ////Green = (topGreen * topA + bottomGreen * bottomA * (255 - topA) / 255) / alpha
                //Marshal.WriteByte(address, surfaceIndex + 1, (byte)((buffer[bufferIndex + 1] * topAlpha + Marshal.ReadByte(address, surfaceIndex + 1) * bottomAlpha * (255 - topAlpha) / 255) / alpha));

                ////Red = (topRed * topA + bottomRed * bottomA * (255 - topA) / 255) / alpha
                //Marshal.WriteByte(address, surfaceIndex + 2, (byte)((buffer[bufferIndex + 2] * topAlpha + Marshal.ReadByte(address, surfaceIndex + 2) * bottomAlpha + (255 - topAlpha) / 255) / alpha));

                //Blue = (topBlue * topA / 255) + (bottomBlue * bottomA * (255 - topA) / (255 * 255))
                Marshal.WriteByte(address, surfaceIndex, (byte)((buffer[bufferIndex] * topAlpha / 255) + (Marshal.ReadByte(address, surfaceIndex) * bottomAlpha * (255 - topAlpha)) / (255 * 255)));

                //Green = (topGreen * v / 255) + (bottomGreen * bottomA * (255 - topA) / (255 * 255))
                Marshal.WriteByte(address, surfaceIndex + 1, (byte)((buffer[bufferIndex + 1] * topAlpha / 255) + (Marshal.ReadByte(address, surfaceIndex + 1) * bottomAlpha * (255 - topAlpha)) / (255 * 255)));

                //Red = (topRed * topA / 255) + (bottomRed * bottomA * (255 - topA) / (255 * 255))
                Marshal.WriteByte(address, surfaceIndex + 2, (byte)((buffer[bufferIndex + 2] * topAlpha / 255) + (Marshal.ReadByte(address, surfaceIndex + 2) * bottomAlpha * (255 - topAlpha)) / (255 * 255)));

                //Alpha = topA + (bottomA * (255 - topA) / 255)
                Marshal.WriteByte(address, surfaceIndex + 3, (byte)(topAlpha + (bottomAlpha * (255 - topAlpha) / 255)));
            }
        }
    }

    public static WriteableBitmap ResizeWritableBitmap(WriteableBitmap wBitmap, int reqWidth, int reqHeight)
    {
        var stride = wBitmap.PixelWidth * ((wBitmap.Format.BitsPerPixel + 7) / 8);
        var numPixels = stride * wBitmap.PixelHeight;
        var arrayOfPixels = new ushort[numPixels];

        wBitmap.CopyPixels(arrayOfPixels, stride, 0);

        var oriWidth = wBitmap.PixelWidth;
        var oriHeight = wBitmap.PixelHeight;

        var nXFactor = oriWidth / (double)reqWidth;
        var nYFactor = oriHeight / (double)reqHeight;

        var nStride = reqWidth * ((wBitmap.Format.BitsPerPixel + 7) / 8);
        var nNumPixels = reqWidth * reqHeight;
        var newArrayOfPixels = new ushort[nNumPixels];

        /* Core Part */
        /* Code project article: Image Processing for Dummies with C# and GDI+ Part 2 - Convolution Filters By Christian Graus</a>
           href=<a href="http://www.codeproject.com/KB/GDI-plus/csharpfilters.aspx"></a>
        */

        for (var y = 0; y < reqHeight; y++)
        {
            for (var x = 0; x < reqWidth; x++)
            {
                //Setup
                var floorX = (int)Math.Floor(x * nXFactor);
                var floorY = (int)Math.Floor(y * nYFactor);

                var ceilX = floorX + 1;
                if (ceilX >= oriWidth)
                    ceilX = floorX;

                var ceilY = floorY + 1;
                if (ceilY >= oriHeight)
                    ceilY = floorY;

                var fractionX = x * nXFactor - floorX;
                var fractionY = y * nYFactor - floorY;

                var oneMinusX = 1.0 - fractionX;
                var oneMinusY = 1.0 - fractionY;

                var pix1 = arrayOfPixels[floorX + floorY * oriWidth];
                var pix2 = arrayOfPixels[ceilX + floorY * oriWidth];
                var pix3 = arrayOfPixels[floorX + ceilY * oriWidth];
                var pix4 = arrayOfPixels[ceilX + ceilY * oriWidth];

                var g1 = (ushort)(oneMinusX * pix1 + fractionX * pix2);
                var g2 = (ushort)(oneMinusX * pix3 + fractionX * pix4);
                var g = (ushort)(oneMinusY * g1 + fractionY * (double)g2);

                newArrayOfPixels[y * reqWidth + x] = g;
            }
        }

        /*End of Core Part*/
        var newWBitmap = new WriteableBitmap(reqWidth, reqHeight, 96, 96, PixelFormats.Gray16, null);
        var imagerect = new Int32Rect(0, 0, reqWidth, reqHeight);
        var newStride = reqWidth * ((PixelFormats.Gray16.BitsPerPixel + 7) / 8);

        newWBitmap.WritePixels(imagerect, newArrayOfPixels, newStride, 0);
        return newWBitmap;
    }
}