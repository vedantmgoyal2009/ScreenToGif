using ScreenToGif.Domain.Models.Project.Cached.Sequences;
using ScreenToGif.Util;
using ScreenToGif.ViewModel.Project.Sequences.SubSequences;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace ScreenToGif.ViewModel.Project.Sequences;

public class CursorSequenceViewModel : RasterSequenceViewModel
{
    private ObservableCollection<CursorSubSequenceViewModel> _cursorEvents;

    /// <summary>
    /// Each frame with its timings.
    /// </summary>
    public ObservableCollection<CursorSubSequenceViewModel> CursorEvents
    {
        get => _cursorEvents;
        set => SetProperty(ref _cursorEvents, value);
    }

    public static CursorSequenceViewModel FromModel(CursorSequence sequence)
    {
        return new CursorSequenceViewModel
        {
            Id = sequence.Id,
            StartTime = sequence.StartTime,
            EndTime = sequence.EndTime,
            Opacity = sequence.Opacity,
            Background = sequence.Background,
            Effects = new ObservableCollection<object>(sequence.Effects), //TODO
            StreamPosition = sequence.StreamPosition,
            CachePath = sequence.CachePath,
            Left = sequence.Left,
            Top = sequence.Top,
            Width = sequence.Width,
            Height = sequence.Height,
            Angle = sequence.Angle,
            CursorEvents = new ObservableCollection<CursorSubSequenceViewModel>(sequence.CursorEvents.Select(CursorSubSequenceViewModel.FromModel).ToList())
        };
    }

    internal override void RenderAt(IntPtr current, int canvasWidth, int canvasHeight, TimeSpan timestamp, double quality, string cachePath)
    {
        var ticks = (ulong)timestamp.Ticks;

        //Get first cursor after timestamp.
        var cursor = CursorEvents.FirstOrDefault(f => ticks >= f.TimeStampInTicks);

        if (cursor == null)
            return;

        using var readStream = new FileStream(cachePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        readStream.Position = (long)cursor.DataStreamPosition;

        var data = readStream.ReadBytes((uint)cursor.DataLength);

        //Missing features:
        //Sequence background.
        //Sequence effects.
        //Sequence angle.
        //Sequence resize. (act as cut or resize when size is different?)
        //Sequence opacity.
        //Cursor angle.
        //Cursor resize.

        //Cursors are special, because they hold more data (only the masked monochrome variant).
        //Rotation and resize need special care.
        
        //Alter size.
        if (cursor.OriginalHeight != cursor.Height || cursor.OriginalWidth != cursor.Width)
        {
            //If not the same size, do a resize run.
            //Need an aux buffer to hold new data.
        }

        //Alter Angle
        if (cursor.Angle != 0)
        {
            //If not Angle = 0, do a reangle run.
            //Is this a 0,5/0,5 rotation?
            //x' = x * cos(a) + y * sin(a)
            //y' = y * cos(a) - x * sin(a)

            var newLeft = Left * Math.Cos(Angle) + Top * Math.Sin(Angle);
            var newTop = Top * Math.Cos(Angle) - Left * Math.Sin(Angle);

            //Altering the angle in non-square ways, makes the size of the rectangle to be different.
        }

        DrawCursor(current, canvasWidth, canvasHeight, cursor, data);
    }

    private void DrawCursor(IntPtr current, int canvasWidth, int canvasHeight, CursorSubSequenceViewModel cursor, byte[] data)
    {
        //Adjust cursor position to its hotspot (actual click position).
        var cursorLeft = cursor.Left - cursor.XHotspot + Left;
        var cursorTop = cursor.Top - cursor.YHotspot + Top;

        //Cut any part of the cursor that overflows outside of the top/left bounds.
        var offsetX = 0 > cursorLeft ? cursorLeft * -1 : 0;
        var offsetY = 0 > cursorTop ? cursorTop * -1 : 0;

        //Cut any part of the cursor that overflows outside of the bottom/right bounds.
        var cursorWidthOffset = cursorLeft + cursor.Width - Math.Min(Width, canvasWidth);
        var cursorHeightOffset = cursorTop + cursor.Height - Math.Min(Height, canvasHeight);

        var cursorWidth = cursorWidthOffset > 0 ? cursor.Width - cursorWidthOffset : cursor.Width;
        var cursorHeight = cursorHeightOffset > 0 ? cursor.Height - cursorHeightOffset : cursor.Height;

        var stride = Width * ((BitsPerChannel * ChannelCount + 7) / 8); //TODO: get the Project details.
        var cursorStride = (int)(cursor.DataLength / cursor.Width);
        
        //Cursors can be divided into 3 types:
        switch (cursor.CursorType)
        {
            //Masked monochrome, a cursor which reacts with the background.
            case 1:
                DrawMonochromeCursor(current, stride, data, cursorLeft, cursorTop, offsetX, offsetY, cursorWidth, cursorHeight, cursorStride, cursor.Height);
                break;

            //Color, a full color cursor which supports transparency.
            case 2:
                DrawColorCursor(current, stride, data, cursorLeft, cursorTop, offsetX, offsetY, cursorWidth, cursorHeight, cursorStride);
                break;

            //Masked color, a mix of both previous types.
            case 4:
                DrawMaskedColorCursor(current, stride, data, cursorLeft, cursorTop, offsetX, offsetY, cursorWidth, cursorHeight, cursorStride);
                break;
        }
    }

    private void DrawMonochromeCursor(IntPtr address, int targetPitch, IReadOnlyList<byte> buffer, int posX, int posY, int offsetX, int offsetY, int width, int height, int cursorPitch, int fullHeight)
    {
        cursorPitch /= 2;

        for (var row = offsetY; row < height; row++)
        {
            //128 in binary.
            byte mask = 0x80;

            //Simulate the offset, adjusting the mask.
            for (var off = 0; off < offsetX; off++)
            {
                if (mask == 0x01)
                    mask = 0x80;
                else
                    mask = (byte)(mask >> 1);
            }

            for (var col = offsetX; col < width; col++)
            {
                var targetIndex = (row - offsetY + posY) * targetPitch + (col - offsetX + posX) * 4;

                //AND mask is taken from the first half of the cursor image.
                //XOR mask is taken from the second half of the cursor image, hence the "+ actualHeight * cursorPitch". 
                var and = (buffer[row * cursorPitch + col / 8] & mask) == mask; 
                var xor = (buffer[row * cursorPitch + col / 8 + fullHeight * cursorPitch] & mask) == mask;

                //Reads current pixel and applies AND and XOR. (AND/XOR ? White : Black)
                Marshal.WriteByte(address, targetIndex, (byte)((Marshal.ReadByte(address, targetIndex) & (and ? 255 : 0)) ^ (xor ? 255 : 0)));
                Marshal.WriteByte(address, targetIndex + 1, (byte)((Marshal.ReadByte(address, targetIndex + 1) & (and ? 255 : 0)) ^ (xor ? 255 : 0)));
                Marshal.WriteByte(address, targetIndex + 2, (byte)((Marshal.ReadByte(address, targetIndex + 2) & (and ? 255 : 0)) ^ (xor ? 255 : 0)));
                Marshal.WriteByte(address, targetIndex + 3, (byte)((Marshal.ReadByte(address, targetIndex + 3) & 255) ^ 0));
                
                //Shifts the mask around until it reaches 1, then resets it back to 128.
                if (mask == 0x01)
                    mask = 0x80;
                else
                    mask = (byte)(mask >> 1);
            }
        }
    }

    private void DrawColorCursor(IntPtr address, int targetPitch, IReadOnlyList<byte> buffer, int posX, int posY, int offsetX, int offsetY, int width, int height, int cursorPitch)
    {
        for (var row = offsetY; row < height; row++)
        {
            for (var col = offsetX; col < width; col++)
            {
                var targetIndex = (row - offsetY + posY) * targetPitch + (col - offsetX + posX) * 4;
                var bufferIndex = row * cursorPitch + col * 4;

                if (bufferIndex > buffer.Count)
                    continue;

                var alpha = buffer[bufferIndex + 3] + 1;

                if (alpha == 1)
                    continue;

                //Premultiplied alpha values.
                var invAlpha = 256 - alpha;
                alpha += 1;

                Marshal.WriteByte(address, targetIndex, (byte)((alpha * buffer[bufferIndex] + invAlpha * Marshal.ReadByte(address, targetIndex)) >> 8));
                Marshal.WriteByte(address, targetIndex + 1, (byte)((alpha * buffer[bufferIndex + 1] + invAlpha * Marshal.ReadByte(address, targetIndex + 1)) >> 8));
                Marshal.WriteByte(address, targetIndex + 2, (byte)((alpha * buffer[bufferIndex + 2] + invAlpha * Marshal.ReadByte(address, targetIndex + 2)) >> 8));
            }
        }
    }

    private void DrawMaskedColorCursor(IntPtr address, int targetPitch, IReadOnlyList<byte> buffer, int posX, int posY, int offsetX, int offsetY, int width, int height, int cursorPitch)
    {
        //ImageHelper.SavePixelArrayToFile(buffer, width, height, 4, System.IO.Path.GetFullPath(".\\MaskedColor.png"));

        for (var row = offsetY; row < height; row++)
        {
            for (var col = offsetX; col < width; col++)
            {
                var surfaceIndex = (row + posY) * targetPitch + (col + posX) * 4;
                var bufferIndex = row * cursorPitch + col * 4;

                if (bufferIndex > buffer.Count)
                    continue;

                var maskFlag = buffer[bufferIndex + 3];

                //Just copies the pixel color.
                if (maskFlag == 0)
                {
                    Marshal.WriteByte(address, surfaceIndex, buffer[bufferIndex]);
                    Marshal.WriteByte(address, surfaceIndex + 1, buffer[bufferIndex + 1]);
                    Marshal.WriteByte(address, surfaceIndex + 2, buffer[bufferIndex + 2]);
                    continue;
                }

                //Applies the XOR opperation with the current color.
                Marshal.WriteByte(address, surfaceIndex, (byte)(buffer[bufferIndex] ^ Marshal.ReadByte(address, surfaceIndex)));
                Marshal.WriteByte(address, surfaceIndex + 1, (byte)(buffer[bufferIndex + 1] ^ Marshal.ReadByte(address, surfaceIndex + 1)));
                Marshal.WriteByte(address, surfaceIndex + 2, (byte)(buffer[bufferIndex + 2] ^ Marshal.ReadByte(address, surfaceIndex + 2)));
            }
        }
    }
}