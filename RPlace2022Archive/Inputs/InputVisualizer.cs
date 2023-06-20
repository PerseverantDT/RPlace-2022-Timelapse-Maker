using System.Buffers;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using FFMpegCore.Extensions.System.Drawing.Common;
using FFMpegCore.Pipes;

using RPlace2022Archive.PostgreSQL;

namespace RPlace2022Archive.Inputs; 

public class InputVisualizer {
    private readonly InputDatabase db;

    public Image GetImageFromDateTime(DateTime dateTime, int scale = 1) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not yet supported for non-Windows OSes.");
        if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale must be greater than 0.");
        
        Image? image = null;
        Bitmap? bmp = null;

        try {
            (image, DateTime start) = db.GetKeyframe(dateTime);
            bmp = new Bitmap(2000 * scale, 2000 * scale, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp)) {
                g.Clear(Color.White);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(image, new Rectangle(Point.Empty, bmp.Size));
            }

            BitmapData bmpData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] data = ArrayPool<byte>.Shared.Rent(bytes);
            Marshal.Copy(ptr, data, 0, bytes);

            foreach (InputEntry entry in db.GetEntries(start, dateTime)) {
                for (int y = 0; y < entry.Height * scale; y++) {
                    for (int x = 0; x < entry.Width * scale; x++) {
                        nint bytePos = ((entry.Y * scale) + y) * bmpData.Stride + ((entry.X * scale) + x) * 3;
                        data[bytePos] = entry.Color.B;
                        data[bytePos + 1] = entry.Color.G;
                        data[bytePos + 2] = entry.Color.R;
                    }
                }
            }
            
            Marshal.Copy(data, 0, ptr, bytes);
            ArrayPool<byte>.Shared.Return(data);
            bmp.UnlockBits(bmpData);
            
            if (dateTime >= CanvasExpansion.SECOND_EXPANSION)
                return bmp.Clone(new Rectangle(Point.Empty, CanvasExpansion.SECOND_EXPANSION_SIZE * scale), PixelFormat.Format8bppIndexed);
            if (dateTime >= CanvasExpansion.FIRST_EXPANSION) 
                return bmp.Clone(new Rectangle(Point.Empty, CanvasExpansion.FIRST_EXPANSION_SIZE * scale), PixelFormat.Format8bppIndexed);
            return bmp.Clone(new Rectangle(Point.Empty, CanvasExpansion.START_SIZE * scale), PixelFormat.Format8bppIndexed);
        }
        finally {
            image?.Dispose();
            bmp?.Dispose();
        }
    }
    
    /// <summary>
    /// Create a snapshot of the canvas at the provided <see cref="DateTime"/>
    /// </summary>
    /// <param name="dateTime">The timestamp when the canvas snapshot should be taken</param>
    /// <param name="scale">The scale of the snapshot. Must be 1 or greater.</param>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    public async ValueTask<Image> GetImageFromDateTimeAsync(DateTime dateTime, int scale = 1) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not yet supported for non-Windows OSes.");
        if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale must be greater than 0.");

        Image? image = null;
        Bitmap? bmp = null;

        try {
            (image, DateTime start) = await db.GetKeyframeAsync(dateTime);
            bmp = new Bitmap(2000 * scale, 2000 * scale, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp)) {
                g.Clear(Color.White);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(image, new Rectangle(Point.Empty, bmp.Size));
            }

            BitmapData bmpData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] data = ArrayPool<byte>.Shared.Rent(bytes);
            Marshal.Copy(ptr, data, 0, bytes);

            await foreach (InputEntry entry in db.GetEntriesAsync(start, dateTime)) {
                for (int y = 0; y < entry.Height * scale; y++) {
                    for (int x = 0; x < entry.Width * scale; x++) {
                        nint bytePos = ((entry.Y * scale) + y) * bmpData.Stride + ((entry.X * scale) + x) * 3;
                        data[bytePos] = entry.Color.B;
                        data[bytePos + 1] = entry.Color.G;
                        data[bytePos + 2] = entry.Color.R;
                    }
                }
            }
            
            Marshal.Copy(data, 0, ptr, bytes);
            ArrayPool<byte>.Shared.Return(data);
            bmp.UnlockBits(bmpData);

            if (dateTime >= CanvasExpansion.SECOND_EXPANSION)
                return bmp.Clone(new Rectangle(Point.Empty, CanvasExpansion.SECOND_EXPANSION_SIZE * scale), PixelFormat.Format8bppIndexed);
            if (dateTime >= CanvasExpansion.FIRST_EXPANSION) 
                return bmp.Clone(new Rectangle(Point.Empty, CanvasExpansion.FIRST_EXPANSION_SIZE * scale), PixelFormat.Format8bppIndexed);
            return bmp.Clone(new Rectangle(Point.Empty, CanvasExpansion.START_SIZE * scale), PixelFormat.Format8bppIndexed);
        }
        finally {
            bmp?.Dispose();
            image?.Dispose();
        }
    }

    /// <summary>
    /// Create snapshots of the canvas at the provided <see cref="DateTimeRange"/>
    /// </summary>
    /// <param name="range">The range where the snapshots should be taken from</param>
    /// <param name="interval">The amount of time between each snapshot</param>
    /// <param name="scale">The scale of the snapshot. Must be greater than 0.</param>
    /// <param name="ct">The cancellation token</param>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    public async IAsyncEnumerable<(Image, DateTime)> GetImagesFromDateTimeRangeAsync(DateTimeRange range, TimeSpan interval, int scale = 1, [EnumeratorCancellation] CancellationToken ct = default) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not supported for non-Windows OSes.");
        if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale must be greater than 0.");

        Image? image = null;
        Bitmap? bmp = null;
        Bitmap? bmp2 = null;
        Graphics? g = null;
        
        const PixelFormat returnFormat = PixelFormat.Format8bppIndexed;
        Rectangle returnRect = (range.Contains(CanvasExpansion.SECOND_EXPANSION) || range.End > CanvasExpansion.SECOND_EXPANSION)
            ? new Rectangle(Point.Empty, CanvasExpansion.SECOND_EXPANSION_SIZE * scale)
            : (range.Contains(CanvasExpansion.FIRST_EXPANSION) || range.End > CanvasExpansion.FIRST_EXPANSION)
                ? new Rectangle(Point.Empty, CanvasExpansion.FIRST_EXPANSION_SIZE * scale)
                : new Rectangle(Point.Empty, CanvasExpansion.START_SIZE * scale);

        try {
            (image, DateTime start) = await db.GetKeyframeAsync(range.Start);

            bmp = new Bitmap(image, 2000, 2000);
            DateTime nextReturn = range.Start;
            image.Dispose();
            image = null;

            bmp2 = new Bitmap(returnRect.Width, returnRect.Height, PixelFormat.Format24bppRgb);
            g = Graphics.FromImage(bmp2);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            
            BitmapData bmpData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] data = ArrayPool<byte>.Shared.Rent(bytes);
            Marshal.Copy(ptr, data, 0, bytes);
            
            await foreach (InputEntry entry in db.GetEntriesAsync(start, range.End).WithCancellation(ct)) {
                if (entry.Timestamp > nextReturn) {
                    Marshal.Copy(data, 0, ptr, bytes);
                    bmp.UnlockBits(bmpData);
                    g.DrawImage(bmp, new Rectangle(Point.Empty, bmp.Size * scale));
                    while (entry.Timestamp > nextReturn) {
                        yield return (bmp2.Clone(returnRect, returnFormat), nextReturn);
                        nextReturn += interval;
                    }
                    bmpData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly, bmp.PixelFormat);
                    ptr = bmpData.Scan0;
                }

                for (int y = 0; y < entry.Height; y++) {
                    for (int x = 0; x < entry.Width; x++) {
                        nint bytePos = (entry.Y + y) * Math.Abs(bmpData.Stride) + (entry.X + x) * (bmpData.Stride / bmpData.Width);
                        data[bytePos] = entry.Color.B;
                        data[bytePos + 1] = entry.Color.G;
                        data[bytePos + 2] = entry.Color.R;
                    }
                }
            }

            Marshal.Copy(data, 0, ptr, bytes);
            ArrayPool<byte>.Shared.Return(data);
            bmp.UnlockBits(bmpData);
            g.DrawImage(bmp, new Rectangle(Point.Empty, bmp.Size * scale));

            while (nextReturn < range.End) {
                yield return (bmp2.Clone(returnRect, returnFormat), nextReturn);
                nextReturn += interval;
            }
            if (range.End != nextReturn) {
                yield return (bmp2.Clone(returnRect, returnFormat), nextReturn);
            }
        }
        finally {
            image?.Dispose();
            bmp?.Dispose();
            bmp2?.Dispose();
            g?.Dispose();
        }
    }
    
    /// <summary>
    /// Create snapshots of the canvas at the provided <see cref="DateTimeRange"/>
    /// </summary>
    /// <param name="range">The range where the snapshots should be taken from</param>
    /// <param name="interval">The amount of time between each snapshot</param>
    /// <param name="scale">The scale of the snapshot. Must be greater than 0.</param>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    public IEnumerable<(Image, DateTime)> GetImagesFromDateTimeRange(DateTimeRange range, TimeSpan interval, int scale = 1) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not supported for non-Windows OSes.");
        if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale must be greater than 0.");

        Image? image = null;
        Bitmap? bmp = null;
        Bitmap? bmp2 = null;
        Graphics? g = null;
        
        const PixelFormat returnFormat = PixelFormat.Format8bppIndexed;
        Rectangle returnRect = (range.Contains(CanvasExpansion.SECOND_EXPANSION) || range.End > CanvasExpansion.SECOND_EXPANSION)
            ? new Rectangle(Point.Empty, CanvasExpansion.SECOND_EXPANSION_SIZE * scale)
            : (range.Contains(CanvasExpansion.FIRST_EXPANSION) || range.End > CanvasExpansion.FIRST_EXPANSION)
                ? new Rectangle(Point.Empty, CanvasExpansion.FIRST_EXPANSION_SIZE * scale)
                : new Rectangle(Point.Empty, CanvasExpansion.START_SIZE * scale);

        try {
            (image, DateTime start) = db.GetKeyframe(range.Start);

            bmp = new Bitmap(image, 2000, 2000);
            DateTime nextReturn = range.Start;
            image.Dispose();
            image = null;

            bmp2 = new Bitmap(returnRect.Width, returnRect.Height, PixelFormat.Format24bppRgb);
            g = Graphics.FromImage(bmp2);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            
            BitmapData bmpData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] data = ArrayPool<byte>.Shared.Rent(bytes);
            Marshal.Copy(ptr, data, 0, bytes);
            
             foreach (InputEntry entry in db.GetEntries(start, range.End)) {
                if (entry.Timestamp > nextReturn) {
                    Marshal.Copy(data, 0, ptr, bytes);
                    bmp.UnlockBits(bmpData);
                    g.DrawImage(bmp, new Rectangle(Point.Empty, bmp.Size * scale));
                    while (entry.Timestamp > nextReturn) {
                        yield return (bmp2.Clone(returnRect, returnFormat), nextReturn);
                        nextReturn += interval;
                    }
                    bmpData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly, bmp.PixelFormat);
                    ptr = bmpData.Scan0;
                }

                for (int y = 0; y < entry.Height; y++) {
                    for (int x = 0; x < entry.Width; x++) {
                        nint bytePos = (entry.Y + y) * Math.Abs(bmpData.Stride) + (entry.X + x) * (bmpData.Stride / bmpData.Width);
                        data[bytePos] = entry.Color.B;
                        data[bytePos + 1] = entry.Color.G;
                        data[bytePos + 2] = entry.Color.R;
                    }
                }
            }

            Marshal.Copy(data, 0, ptr, bytes);
            ArrayPool<byte>.Shared.Return(data);
            bmp.UnlockBits(bmpData);
            g.DrawImage(bmp, new Rectangle(Point.Empty, bmp.Size * scale));

            while (nextReturn < range.End) {
                yield return (bmp2.Clone(returnRect, returnFormat), nextReturn);
                nextReturn += interval;
            }
            if (range.End != nextReturn) {
                yield return (bmp2.Clone(returnRect, returnFormat), nextReturn);
            }
        }
        finally {
            image?.Dispose();
            bmp?.Dispose();
            bmp2?.Dispose();
            g?.Dispose();
        }
    }

    /// <summary>
    /// Get video frames for a timelapse of the canvas. Should be used with FFMpegCore.
    /// </summary>
    /// <param name="range">The range from where the frames should be taken from</param>
    /// <param name="interval">The amount of time between each snapshot</param>
    /// <param name="scale">The scale of the timelapse. Must be 1 or greater.</param>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    public IEnumerable<IVideoFrame> GetTimelapseFramesForDateTimeRange(DateTimeRange range, TimeSpan interval, int scale = 1) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not supported for non-Windows OSes.");
        if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale must be greater than 0.");

        Image? image = null;
        Bitmap? bmp = null;
        Bitmap? bmp2 = null;
        Graphics? g = null;
        
        const PixelFormat returnFormat = PixelFormat.Format24bppRgb;
        Rectangle returnRect = (range.Contains(CanvasExpansion.SECOND_EXPANSION) || range.End > CanvasExpansion.SECOND_EXPANSION)
            ? new Rectangle(Point.Empty, CanvasExpansion.SECOND_EXPANSION_SIZE * scale)
            : (range.Contains(CanvasExpansion.FIRST_EXPANSION) || range.End > CanvasExpansion.FIRST_EXPANSION)
                ? new Rectangle(Point.Empty, CanvasExpansion.FIRST_EXPANSION_SIZE * scale)
                : new Rectangle(Point.Empty, CanvasExpansion.START_SIZE * scale);

        try {
            (image, DateTime start) = db.GetKeyframe(range.Start);

            bmp = new Bitmap(image, 2000, 2000);
            DateTime nextReturn = range.Start;
            image.Dispose();
            image = null;

            bmp2 = new Bitmap(returnRect.Width, returnRect.Height, PixelFormat.Format24bppRgb);
            g = Graphics.FromImage(bmp2);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            
            BitmapData bmpData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] data = ArrayPool<byte>.Shared.Rent(bytes);
            Marshal.Copy(ptr, data, 0, bytes);
            
            foreach (InputEntry entry in db.GetEntries(start, range.End)) {
                if (entry.Timestamp > nextReturn) {
                    Marshal.Copy(data, 0, ptr, bytes);
                    bmp.UnlockBits(bmpData);
                    g.DrawImage(bmp, new Rectangle(Point.Empty, bmp.Size * scale));
                    while (entry.Timestamp > nextReturn) {
                        yield return new BitmapVideoFrameWrapper(bmp2.Clone(returnRect, returnFormat));
                        nextReturn += interval;
                    }
                    bmpData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly, bmp.PixelFormat);
                    ptr = bmpData.Scan0;
                }

                for (int y = 0; y < entry.Height; y++) {
                    for (int x = 0; x < entry.Width; x++) {
                        nint bytePos = (entry.Y + y) * Math.Abs(bmpData.Stride) + (entry.X + x) * (bmpData.Stride / bmpData.Width);
                        data[bytePos] = entry.Color.B;
                        data[bytePos + 1] = entry.Color.G;
                        data[bytePos + 2] = entry.Color.R;
                    }
                }
            }

            Marshal.Copy(data, 0, ptr, bytes);
            ArrayPool<byte>.Shared.Return(data);
            bmp.UnlockBits(bmpData);
            g.DrawImage(bmp, new Rectangle(Point.Empty, bmp.Size * scale));

            while (nextReturn < range.End) {
                yield return new BitmapVideoFrameWrapper(bmp2.Clone(returnRect, returnFormat));
                nextReturn += interval;
            }
            if (range.End != nextReturn) {
                yield return new BitmapVideoFrameWrapper(bmp2.Clone(returnRect, returnFormat));
            }
        }
        finally {
            image?.Dispose();
            bmp?.Dispose();
            bmp2?.Dispose();
            g?.Dispose();
        }
    }

    /// <summary>
    /// Get the timestamps of the snapshots of a timelapse with the provided parameters
    /// </summary>
    /// <param name="range">The range from where the timelapse should be taken from</param>
    /// <param name="interval">The amount of time between each frame</param>
    /// <returns></returns>
    public IEnumerable<DateTime> GetTimestampsForTimelapse(DateTimeRange range, TimeSpan interval) {
        DateTime currentDateTime = range.Start;
        while (currentDateTime < range.End) {
            yield return currentDateTime;
            currentDateTime += interval;
        }
        if (currentDateTime != range.End) yield return range.End;
    }

    public InputVisualizer(InputDatabase db) {
        this.db = db;
    }
}