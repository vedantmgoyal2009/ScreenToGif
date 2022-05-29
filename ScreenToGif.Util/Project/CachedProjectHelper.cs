using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Models.Project.Cached;
using ScreenToGif.Domain.Models.Project.Cached.Sequences;
using ScreenToGif.Domain.Models.Project.Cached.Sequences.SubSequences;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.Models.Project.Recording.Events;
using ScreenToGif.Util.Settings;
using System.IO;
using System.IO.Compression;
using System.Windows.Input;

namespace ScreenToGif.Util.Project;

public static class CachedProjectHelper
{
    public static CachedProject Create(DateTime? creationDate = null)
    {
        var date = creationDate ?? DateTime.Now;
        var path = Path.Combine(UserSettings.All.TemporaryFolderResolved, "ScreenToGif", "Projects", date.ToString("yyyy-MM-dd HH-mm-ss"));

        //What else create paths for?

        var project = new CachedProject
        {
            CacheRootPath = path,
            PropertiesCachePath = Path.Combine(path, "Properties.cache"),
            UndoCachePath = Path.Combine(path, "Undo.cache"),
            RedoCachePath = Path.Combine(path, "Redo.cache"),

            CreationDate = date,
            LastModificationDate = date
        };

        Directory.CreateDirectory(path);

        return project;
    }

    public static async Task<Track> CreateTrack(CachedProject project, string name)
    {
        var trackId = project.Tracks.Count + 1;

        var track = new Track
        {
            Id = (ushort)trackId,
            Name = name,
            CachePath = Path.Combine(project.CacheRootPath, $"Track-{trackId}.cache")
        };

        await using var trackStream = new FileStream(track.CachePath, FileMode.Create, FileAccess.Write, FileShare.None);

        //Track details.
        trackStream.WriteUInt16(track.Id); //2 bytes.
        trackStream.WritePascalString(track.Name); //1 byte + (0 -> 255) bytes.
        trackStream.WriteByte(track.IsVisible ? (byte)1 : (byte)0); //1 byte.
        trackStream.WriteByte(track.IsLocked ? (byte)1 : (byte)0); //1 byte.
        trackStream.WriteUInt16(1); //Sequence count, 2 bytes.

        return track;
    }

    public static async Task CreateFrameTrack(RecordingProject recording, CachedProject project)
    {
        //Track.
        var track = await CreateTrack(project, "Frames");

        //Sequence.
        var lastFrame = recording.Frames.Last();

        var sequence = new FrameSequence
        {
            Id = 1,
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromTicks(lastFrame.Ticks) + TimeSpan.FromMilliseconds(lastFrame.Delay), //Delay is empty for now.
            //Opacity = 1,
            //Background = null,
            //Effects = new(),
            StreamPosition = 0,
            CachePath = Path.Combine(project.CacheRootPath, $"Sequence-{track.Id}-1.cache"),
            //Left = 0,
            //Top = 0,
            Width = project.Width,
            Height = project.Height,
            //Angle = 0,
            Origin = project.CreatedBy == ProjectSources.ScreenRecorder ? RasterSequenceSources.Screen : RasterSequenceSources.Webcam,
            OriginalWidth = project.Width,
            OriginalHeight = project.Height,
            ChannelCount = project.ChannelCount,
            BitsPerChannel = project.BitsPerChannel,
            HorizontalDpi = project.HorizontalDpi,
            VerticalDpi = project.VerticalDpi
        };

        await using var readStream = new FileStream(recording.FramesCachePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using var deflateStream = new DeflateStream(readStream, CompressionMode.Decompress);
        await using var writeStream = new FileStream(sequence.CachePath, FileMode.Create, FileAccess.Write, FileShare.None);

        //Sequence details.
        writeStream.WriteUInt16(sequence.Id); //2 bytes.
        writeStream.WriteByte((byte)sequence.Type); //1 bytes.
        writeStream.WriteUInt64((ulong)sequence.StartTime.Ticks); //8 bytes.
        writeStream.WriteUInt64((ulong)sequence.EndTime.Ticks); //8 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Opacity))); //4 bytes.
        writeStream.WritePascalStringUInt32(await sequence.Background.ToXamlStringAsync()); //4 bytes + (0 - 2^32)

        //Sequence effects.
        writeStream.WriteByte(0); //Effect count, 1 bytes.

        //Rect sequence.
        writeStream.WriteInt32(sequence.Left); //4 bytes.
        writeStream.WriteInt32(sequence.Top); //4 bytes.
        writeStream.WriteUInt16(sequence.Width); //2 bytes.
        writeStream.WriteUInt16(sequence.Height); //2 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Angle))); //4 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.HorizontalDpi))); //4 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.VerticalDpi))); //4 bytes.

        //Raster sequence. Should it be type of raster?
        writeStream.WriteByte((byte)sequence.Origin); //1 byte.
        writeStream.WriteUInt16(sequence.OriginalWidth); //2 bytes.
        writeStream.WriteUInt16(sequence.OriginalHeight); //2 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.HorizontalDpi))); //4 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.VerticalDpi))); //4 bytes.
        writeStream.WriteByte(sequence.ChannelCount); //1 byte.
        writeStream.WriteByte(sequence.BitsPerChannel); //1 byte.
        writeStream.WriteUInt32((uint)sequence.Frames.Count); //1 byte.

        //Frames sub-sequence.
        foreach (var frame in recording.Frames)
        {
            var sub = new FrameSubSequence
            {
                TimeStampInTicks = (ulong)frame.Ticks,
                StreamPosition = (ulong)writeStream.Position,
                Width = sequence.Width,
                Height = sequence.Height,
                OriginalWidth = sequence.OriginalWidth,
                OriginalHeight = sequence.OriginalHeight,
                HorizontalDpi = sequence.HorizontalDpi,
                VerticalDpi = sequence.VerticalDpi,
                ChannelCount = sequence.ChannelCount,
                BitsPerChannel = sequence.BitsPerChannel,
                DataLength = frame.DataLength,
                Delay = frame.Delay
            };

            //Sub-sequence.
            writeStream.WriteByte((byte) sub.Type); //1 byte.
            writeStream.WriteUInt64(sub.TimeStampInTicks); //8 bytes.

            //Rect sub-sequence.
            writeStream.WriteInt32(sub.Left); //4 bytes.
            writeStream.WriteInt32(sub.Top); //4 bytes.
            writeStream.WriteUInt16(sub.Width); //2 bytes.
            writeStream.WriteUInt16(sub.Height); //2 bytes.
            writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sub.Angle))); //4 bytes.

            //Raster sub-sequence. 
            writeStream.WriteUInt16(sub.OriginalWidth); //2 bytes.
            writeStream.WriteUInt16(sub.OriginalHeight); //2 bytes.
            writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sub.HorizontalDpi))); //4 bytes.
            writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sub.VerticalDpi))); //4 bytes.
            writeStream.WriteByte(sub.ChannelCount); //1 byte.
            writeStream.WriteByte(sub.BitsPerChannel); //1 byte.

            //Frame sub-sequence.
            deflateStream.ReadByte(); //1 byte, Frame event type.
            deflateStream.ReadUInt64(); //8 bytes, Ticks.

            if (sub.DataStreamPosition != (ulong)writeStream.Position + 16)
                System.Diagnostics.Debugger.Break();

            //Delay, Data size, Pixels = 8 + 8 + X bytes.
            await using (var subStream = new SubStream(deflateStream, 16L + (long)sub.DataLength))
                await subStream.CopyToAsync(writeStream);
            
            sequence.Frames.Add(sub);
        }

        track.Sequences.Add(sequence);
        project.Tracks.Add(track);
    }

    public static async Task CreateCursorTrack(RecordingProject recording, CachedProject project)
    {
        var lastEvent = recording.Events.LastOrDefault(l => l.EventType is RecordingEvents.Cursor or RecordingEvents.CursorData);

        if (lastEvent == null)
            return;

        var track = await CreateTrack(project, "Cursor Events");

        var sequence = new CursorSequence
        {
            Id = 1,
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromTicks(lastEvent.TimeStampInTicks) + TimeSpan.FromMilliseconds(recording.Frames[0].Delay), //TODO: Decide for how long to display cursor.
            //Opacity = 1,
            //Background = null,
            //Effects = new(),
            StreamPosition = 0,
            CachePath = Path.Combine(project.CacheRootPath, $"Sequence-{track.Id}-1.cache"),
            //Left = 0,
            //Top = 0,
            Width = project.Width,
            Height = project.Height,
            //Angle = 0
        };

        await using var writeStream = new FileStream(sequence.CachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        
        //Sequence details.
        writeStream.WriteUInt16(sequence.Id); //2 bytes.
        writeStream.WriteByte((byte)sequence.Type); //1 bytes.
        writeStream.WriteUInt64((ulong)sequence.StartTime.Ticks); //8 bytes.
        writeStream.WriteUInt64((ulong)sequence.EndTime.Ticks); //8 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Opacity))); //4 bytes.
        writeStream.WritePascalStringUInt32(await sequence.Background.ToXamlStringAsync()); //4 bytes + (0 - 2^32)

        //Sequence effects.
        writeStream.WriteByte(0); //Effect count, 1 bytes.

        //Rect sequence.
        writeStream.WriteInt32(sequence.Left); //4 bytes.
        writeStream.WriteInt32(sequence.Top); //4 bytes.
        writeStream.WriteUInt16(sequence.Width); //2 bytes.
        writeStream.WriteUInt16(sequence.Height); //2 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Angle))); //4 bytes.

        //Cursor sequence.
        var cursorEvents = recording.Events.Where(w => w.EventType is RecordingEvents.Cursor or RecordingEvents.CursorData).ToList();
        
        writeStream.WriteUInt32((uint) cursorEvents.Count); //4 bytes.

        await using var readStream = new FileStream(recording.EventsCachePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var cursorData = cursorEvents.OfType<CursorDataEvent>().FirstOrDefault();

        //Cursor sub-sequence.
        foreach (var entry in cursorEvents)
        {
            var sub = new CursorSubSequence
            {
                TimeStampInTicks = (ulong)(entry?.TimeStampInTicks ?? 0),
                StreamPosition = (ulong)writeStream.Position
            };

            //If only data, ignore press states
            if (entry is CursorEvent state)
            {
                sub.Left = state.Left;
                sub.Top = state.Top;
                sub.Width = (ushort)(cursorData?.Width ?? 32);
                sub.Height = (ushort)Math.Abs(cursorData?.Height ?? 32);
                sub.OriginalWidth = (ushort)(cursorData?.Width ?? 32);
                sub.OriginalHeight = (ushort)Math.Abs(cursorData?.Height ?? 32);
                sub.HorizontalDpi = 96; //How to get this information? Does it change for high DPI screens?
                sub.VerticalDpi = 96;
                sub.ChannelCount = 4;
                sub.BitsPerChannel = 8;
                sub.DataLength = (ushort)(cursorData?.PixelsLength ?? 0);
                sub.CursorType = (byte)(cursorData?.CursorType ?? 0);
                sub.XHotspot = (ushort)(cursorData?.XHotspot ?? 0);
                sub.YHotspot = (ushort)(cursorData?.YHotspot ?? 0);
                sub.IsLeftButtonDown = state.LeftButton == MouseButtonState.Pressed;
                sub.IsRightButtonDown = state.RightButton == MouseButtonState.Pressed;
                sub.IsMiddleButtonDown = state.MiddleButton == MouseButtonState.Pressed;
                sub.IsFirstExtraButtonDown = state.FirstExtraButton == MouseButtonState.Pressed;
                sub.IsSecondExtraButtonDown = state.SecondExtraButton == MouseButtonState.Pressed;
                sub.MouseWheelDelta = state.MouseDelta;
            }
            else if (entry is CursorDataEvent data)
            {
                sub.Left = data.Left;
                sub.Top = data.Top;
                sub.Width = (ushort)(cursorData?.Width ?? 32);
                sub.Height = (ushort)Math.Abs(cursorData?.Height ?? 32);
                sub.OriginalWidth = (ushort)(cursorData?.Width ?? 32);
                sub.OriginalHeight = (ushort)Math.Abs(cursorData?.Height ?? 32);
                sub.HorizontalDpi = 96; //How to get this information? Does it change for high DPI screens?
                sub.VerticalDpi = 96;
                sub.ChannelCount = 4;
                sub.BitsPerChannel = 8;
                sub.DataLength = (ushort)(cursorData?.PixelsLength ?? 0);
                sub.CursorType = (byte)(cursorData?.CursorType ?? 0);
                sub.XHotspot = (ushort)(cursorData?.XHotspot ?? 0);
                sub.YHotspot = (ushort)(cursorData?.YHotspot ?? 0);

                cursorData = data;
            }

            //Sub-sequence details.
            writeStream.WriteByte((byte)sub.Type); //1 byte.
            writeStream.WriteUInt64(sub.TimeStampInTicks); //8 bytes.

            //Rect sub-sequence details.
            writeStream.WriteInt32(sub.Left); //4 bytes.
            writeStream.WriteInt32(sub.Top); //4 bytes.
            writeStream.WriteUInt16(sub.Width); //2 bytes.
            writeStream.WriteUInt16(sub.Height); //2 bytes.
            writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sub.Angle))); //4 bytes.

            //Raster sub-sequence details.
            writeStream.WriteUInt16(sub.OriginalWidth); //2 bytes.
            writeStream.WriteUInt16(sub.OriginalHeight); //2 bytes.
            writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sub.HorizontalDpi))); //4 bytes.
            writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sub.VerticalDpi))); //4 bytes.
            writeStream.WriteByte(sub.ChannelCount); //1 byte.
            writeStream.WriteByte(sub.BitsPerChannel); //1 byte.
            writeStream.WriteUInt64(sub.DataLength); //8 bytes.

            //Cursor sub-sequence details.
            writeStream.WriteByte(sub.CursorType); //1 byte.
            writeStream.WriteUInt16(sub.XHotspot); //2 bytes.
            writeStream.WriteUInt16(sub.YHotspot); //2 bytes.
            writeStream.WriteBoolean(sub.IsLeftButtonDown); //1 byte.
            writeStream.WriteBoolean(sub.IsRightButtonDown); //1 byte.
            writeStream.WriteBoolean(sub.IsMiddleButtonDown); //1 byte.
            writeStream.WriteBoolean(sub.IsFirstExtraButtonDown); //1 byte.
            writeStream.WriteBoolean(sub.IsSecondExtraButtonDown); //1 byte.
            writeStream.WriteInt16(sub.MouseWheelDelta); //2 bytes.

            if (sub.DataStreamPosition != (ulong)writeStream.Position)
                System.Diagnostics.Debugger.Break();

            //The pixel location is 42 bytes after the start of the event stream position.
            await using (var part = new SubStream(readStream, 42L + (cursorData?.StreamPosition ?? 0L), (long)sub.DataLength))
                await part.CopyToAsync(writeStream);

            sequence.CursorEvents.Add(sub);
        }

        track.Sequences.Add(sequence);
        project.Tracks.Add(track);
    }

    public static async Task CreateKeyTrack(RecordingProject recording, CachedProject project)
    {
        var lastEvent = recording.Events.LastOrDefault(l => l.EventType == RecordingEvents.Key);

        if (lastEvent == null)
            return;

        var track = await CreateTrack(project, "Key Events");

        var sequence = new KeySequence
        {
            Id = 1,
            Width = project.Width,
            Height = project.Height,
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromTicks(lastEvent.TimeStampInTicks) + TimeSpan.FromMilliseconds(recording.Frames[0].Delay),
            CachePath = Path.Combine(project.CacheRootPath, $"Sequence-{track.Id}-1.cache")
        };

        await using var writeStream = new FileStream(sequence.CachePath, FileMode.Create, FileAccess.Write, FileShare.None);

        //Sequence details.
        writeStream.WriteUInt16(sequence.Id); //2 bytes.
        writeStream.WriteByte((byte)sequence.Type); //1 bytes.
        writeStream.WriteUInt64((ulong)sequence.StartTime.Ticks); //8 bytes.
        writeStream.WriteUInt64((ulong)sequence.EndTime.Ticks); //8 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Opacity))); //4 bytes.
        writeStream.WritePascalStringUInt32(await sequence.Background.ToXamlStringAsync()); //4 bytes + (0 - 2^32)

        //Sequence effects.
        writeStream.WriteByte(0); //Effect count, 1 bytes.

        //Sizeable sequence.
        writeStream.WriteInt32(sequence.Left); //4 bytes.
        writeStream.WriteInt32(sequence.Top); //4 bytes.
        writeStream.WriteUInt16(sequence.Width); //2 bytes.
        writeStream.WriteUInt16(sequence.Height); //2 bytes.
        writeStream.WriteBytes(BitConverter.GetBytes(Convert.ToSingle(sequence.Angle))); //4 bytes.

        //Key sequence.
        var keyEvents = recording.Events.OfType<KeyEvent>().ToList();

        //TODO: More details in here.
        writeStream.WriteUInt32((uint)keyEvents.Count); //4 bytes.

        foreach (var keyEvent in keyEvents)
        {
            var sub = new KeySubSequence
            {
                TimeStampInTicks = (ulong)keyEvent.TimeStampInTicks,
                Key = keyEvent.Key,
                Modifiers = keyEvent.Modifiers,
                IsUppercase = keyEvent.IsUppercase,
                WasInjected = keyEvent.WasInjected,
                StreamPosition = (ulong)writeStream.Position
            };

            writeStream.WriteByte((byte)sub.Type); //1 byte.
            writeStream.WriteUInt64(sub.TimeStampInTicks); //8 bytes.
            writeStream.WriteByte((byte)sub.Key); //1 byte.
            writeStream.WriteByte((byte)sub.Modifiers); //1 byte.
            writeStream.WriteBoolean(sub.IsUppercase); //1 byte.
            writeStream.WriteBoolean(sub.WasInjected); //1 byte.
            
            sequence.KeyEvents.Add(sub);
        }

        track.Sequences.Add(sequence);
        project.Tracks.Add(track);
    }

    //Read from disk, to load recent projects.

    //Discard?

    //Save to StorageProject.
}