using ScreenToGif.Domain.Enums.Native;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Native.External;
using ScreenToGif.Native.Structs;
using ScreenToGif.Util.Settings;
using System.Windows;

namespace ScreenToGif.Util.Capture;

public class GdiCapture : ScreenCapture
{
    #region Variables

    private readonly IntPtr _desktopWindow = IntPtr.Zero;
    private IntPtr _windowDeviceContext;
    private IntPtr _compatibleDeviceContext;
    private IntPtr _compatibleBitmap;
    private IntPtr _oldBitmap;

    private BitmapInfoHeader _bitmapHeader;
    private CopyPixelOperations _pixelOperations;
    private int _cursorStep;
    private ulong _byteLength;

    #endregion

    public override void Start(bool isAutomatic, int delay, int left, int top, int width, int height, double scale, RecordingProject project)
    {
        base.Start(isAutomatic, delay, left, top, width, height, scale, project);

        #region Pointers

        //http://winprog.org/tutorial/bitmaps.html
        _windowDeviceContext = User32.GetWindowDC(_desktopWindow);
        _compatibleDeviceContext = Gdi32.CreateCompatibleDC(_windowDeviceContext);
        _compatibleBitmap = Gdi32.CreateCompatibleBitmap(_windowDeviceContext, Width, Height);
        _oldBitmap = Gdi32.SelectObject(_compatibleDeviceContext, _compatibleBitmap);

        #endregion

        #region Pixel Operation

        _pixelOperations = CopyPixelOperations.SourceCopy;

        //If not in a remote desktop connection or if the improvement was disabled, capture layered windows too.
        if (!SystemParameters.IsRemoteSession || !UserSettings.All.RemoteImprovement)
            _pixelOperations |= CopyPixelOperations.CaptureBlt;

        #endregion

        //Bitmap details for each frame being captured.
        _bitmapHeader = new BitmapInfoHeader(false)
        {
            BitCount = 32, //Was 24
            ClrUsed = 0,
            ClrImportant = 0,
            Compression = 0,
            Height = -StartHeight, //Negative, so the Y-axis will be positioned correctly.
            Width = StartWidth,
            Planes = 1
        };

        //This was working with 32 bits: 3L * Width * Height;
        _byteLength = (ulong)((StartWidth * _bitmapHeader.BitCount + 31) / 32 * 4 * StartHeight);

        //Preemptively Capture the first cursor shape.
        //CaptureCursor();
    }

    public override int Capture(RecordingFrame frame)
    {
        try
        {
            if (!Gdi32.StretchBlt(_compatibleDeviceContext, 0, 0, StartWidth, StartHeight, _windowDeviceContext, Left, Top, Width, Height, _pixelOperations))
                return FrameCount;

            //Set frame details.
            FrameCount++;

            frame.Ticks = Stopwatch.GetElapsedTicks();
            //frame.Delay = Stopwatch.GetMilliseconds(); //Resets the stopwatch. Messes up the editor.
            frame.Pixels = new byte[_byteLength];

            if (Gdi32.GetDIBits(_windowDeviceContext, _compatibleBitmap, 0, (uint)StartHeight, frame.Pixels, ref _bitmapHeader, DibColorModes.RgbColors) == 0)
                frame.WasFrameSkipped = true;

            if (IsAcceptingFrames)
                FrameCollection.Add(frame);
        }
        catch (Exception)
        {
            //LogWriter.Log(ex, "Impossible to get screenshot of the screen");
        }

        return FrameCount;
    }

    public override int CaptureWithCursor(RecordingFrame frame)
    {
        try
        {
            if (!Gdi32.StretchBlt(_compatibleDeviceContext, 0, 0, StartWidth, StartHeight, _windowDeviceContext, Left, Top, Width, Height, _pixelOperations))
                return FrameCount;

            CaptureCursor();

            //Set frame details.
            FrameCount++;

            frame.Ticks = Stopwatch.GetElapsedTicks();
            //frame.Delay = Stopwatch.GetMilliseconds(); //Resets the stopwatch. Messes up the editor.
            frame.Pixels = new byte[_byteLength];

            if (Gdi32.GetDIBits(_windowDeviceContext, _compatibleBitmap, 0, (uint)StartHeight, frame.Pixels, ref _bitmapHeader, DibColorModes.RgbColors) == 0)
                frame.WasFrameSkipped = true;

            if (IsAcceptingFrames)
                FrameCollection.Add(frame);
        }
        catch (Exception)
        {
            //LogWriter.Log(ex, "Impossible to get the screenshot of the screen");
        }

        return FrameCount;
    }
    
    public override void Save(RecordingFrame info)
    {
        if (UserSettings.All.PreventBlackFrames && info.Pixels != null && !info.WasFrameSkipped && info.Pixels[0] == 0)
        {
            if (!info.Pixels.Any(a => a > 0))
                info.WasFrameSkipped = true;
        }

        //If the frame skipped, just increase the delay to the previous frame.
        if (info.WasFrameSkipped || info.Pixels == null)
        {
            info.Pixels = null;

            //Pass the duration to the previous frame, if any.
            if (Project.Frames.Count > 0)
                Project.Frames[^1].Delay += info.Delay;

            return;
        }

        CompressStream.WriteByte(1); //1 byte, Frame event type.
        CompressStream.WriteInt64(info.Ticks); //8 bytes.
        CompressStream.WriteInt64(info.Delay); //8 bytes.
        CompressStream.WriteInt64(info.Pixels.LongLength); //8 bytes.
        CompressStream.WriteBytes(info.Pixels);

        info.DataLength = (ulong) info.Pixels.LongLength;
        info.Pixels = null;

        Project.Frames.Add(info);
    }

    public override async Task Stop()
    {
        if (!WasFrameCaptureStarted)
            return;

        //Stop the recording first.
        await base.Stop();

        //Release resources.
        try
        {
            Gdi32.SelectObject(_compatibleDeviceContext, _oldBitmap);
            Gdi32.DeleteObject(_compatibleBitmap);
            Gdi32.DeleteDC(_compatibleDeviceContext);
            User32.ReleaseDC(_desktopWindow, _windowDeviceContext);
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Impossible to stop and clean resources used by the recording.");
        }
    }

    private void CaptureCursor()
    {
        #region Get cursor details

        //ReSharper disable once RedundantAssignment, disable once InlineOutVariableDeclaration
        var cursorInfo = new CursorInfo(false);

        if (!User32.GetCursorInfo(out cursorInfo))
            return;

        if (cursorInfo.Flags != ScreenToGif.Native.Constants.CursorShowing)
        {
            Gdi32.DeleteObject(cursorInfo.CursorHandle);
            return;
        }

        var iconHandle = User32.CopyIcon(cursorInfo.CursorHandle);

        if (iconHandle == IntPtr.Zero)
        {
            Gdi32.DeleteObject(cursorInfo.CursorHandle);
            return;
        }

        if (!User32.GetIconInfo(iconHandle, out var iconInfo))
        {
            User32.DestroyIcon(iconHandle);
            Gdi32.DeleteObject(cursorInfo.CursorHandle);
            return;
        }

        var iconInfoEx = new IconInfoEx();
        iconInfoEx.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(iconInfoEx);

        if (!User32.GetIconInfoEx(iconHandle, ref iconInfoEx))
        {
            User32.DestroyIcon(iconHandle);
            Gdi32.DeleteObject(cursorInfo.CursorHandle);
        }

        #endregion

        try
        {
            //Color.
            var colorHeader = new BitmapInfoHeader(false);
            //Gdi32.GetDIBits(_windowDeviceContext, iconInfo.Color, 0, 0, null, ref colorHeader, DibColorModes.RgbColors);
            Gdi32.GetDIBits(_windowDeviceContext, iconInfoEx.Color, 0, 0, null, ref colorHeader, DibColorModes.RgbColors);

            //Mask.
            var maskHeader = new BitmapInfoHeader(false);
            //Gdi32.GetDIBits(_windowDeviceContext, iconInfo.Mask, 0, 0, null, ref maskHeader, DibColorModes.RgbColors);
            Gdi32.GetDIBits(_windowDeviceContext, iconInfoEx.Mask, 0, 0, null, ref maskHeader, DibColorModes.RgbColors);

            if (colorHeader.Height != 0)
            {
                //Create bitmap.
                var compatibleBitmap = Gdi32.CreateCompatibleBitmap(_windowDeviceContext, colorHeader.Width, colorHeader.Height);
                var oldBitmap = Gdi32.SelectObject(_compatibleDeviceContext, compatibleBitmap);

                //Draw image.
                var ok = User32.DrawIconEx(_compatibleDeviceContext, 0, 0, cursorInfo.CursorHandle, 0, 0, _cursorStep, IntPtr.Zero, DrawIconFlags.Image);

                if (!ok)
                {
                    _cursorStep = 0;
                    User32.DrawIconEx(_compatibleDeviceContext, 0, 0, cursorInfo.CursorHandle, 0, 0, _cursorStep, IntPtr.Zero, DrawIconFlags.Image);
                }
                else
                    _cursorStep++;

                //Get color data.
                var colorBuffer = new byte[colorHeader.SizeImage];
                colorHeader.Height *= -1;
                Gdi32.GetDIBits(_windowDeviceContext, compatibleBitmap, 0, (uint)(colorHeader.Height * -1), colorBuffer, ref colorHeader, DibColorModes.RgbColors);

                //Erase bitmaps.
                Gdi32.SelectObject(_compatibleDeviceContext, oldBitmap);
                Gdi32.DeleteObject(compatibleBitmap);

                var needsMask = true;
                for (var index = 0; index < colorBuffer.Length; index += 4)
                {
                    if (colorBuffer[index] == 0)
                        continue;

                    needsMask = false;
                    break;
                }

                if (!needsMask)
                {
                    RegisterCursorDataEvent(2, colorBuffer, colorHeader.Width, colorHeader.Height * -1, cursorInfo.ScreenPosition.X - Left, cursorInfo.ScreenPosition.Y - Top, iconInfo.XHotspot, iconInfo.YHotspot);
                    return;
                }

                var colorHeight = colorHeader.Height * -1;
                var colorWidth = colorHeader.Width; //Bug: For some reason, after calling GetDIBits() for the mask, the width of the color struct shifts.

                var maskBuffer2 = new byte[maskHeader.SizeImage];
                maskHeader.Height *= -1;
                Gdi32.GetDIBits(_windowDeviceContext, iconInfo.Mask, 0, (uint)(maskHeader.Height * -1), maskBuffer2, ref maskHeader, DibColorModes.RgbColors);

                var targetPitch = colorBuffer.Length / colorHeight;
                var cursorPitch = maskBuffer2.Length / maskHeader.Height * -1;

                //Merge mask with color.
                for (var row = 0; row < colorWidth; row++)
                {
                    //128 in binary.
                    byte mask = 0x80;

                    for (var col = 0; col < colorHeight; col++)
                    {
                        var targetIndex = row * targetPitch + col * 4;
                        var xor = (maskBuffer2[row * cursorPitch + col / 8] & mask) == mask;

                        //Reads current pixel and merge with mask.
                        colorBuffer[targetIndex + 3] = (byte)(xor ? 255 : 0);

                        //Shifts the mask around until it reaches 1, then resets it back to 128.
                        if (mask == 0x01)
                            mask = 0x80;
                        else
                            mask = (byte)(mask >> 1);
                    }
                }

                RegisterCursorDataEvent(4, colorBuffer, colorWidth, colorHeight, cursorInfo.ScreenPosition.X - Left, cursorInfo.ScreenPosition.Y - Top, iconInfo.XHotspot, iconInfo.YHotspot);
                return;
            }

            //Get mask data.
            var maskBuffer = new byte[maskHeader.SizeImage];
            maskHeader.Height *= -1;
            Gdi32.GetDIBits(_windowDeviceContext, iconInfo.Mask, 0, (uint)(maskHeader.Height * -1), maskBuffer, ref maskHeader, DibColorModes.RgbColors);
            
            RegisterCursorDataEvent(1, maskBuffer, maskHeader.Width, maskHeader.Height / 2 * -1, cursorInfo.ScreenPosition.X - Left, cursorInfo.ScreenPosition.Y - Top, iconInfo.XHotspot, iconInfo.YHotspot);
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Impossible to get the cursor");
        }
        finally
        {
            Gdi32.DeleteObject(iconInfo.Color);
            Gdi32.DeleteObject(iconInfo.Mask);
            User32.DestroyIcon(iconHandle);
            Gdi32.DeleteObject(cursorInfo.CursorHandle);
        }
    }
}