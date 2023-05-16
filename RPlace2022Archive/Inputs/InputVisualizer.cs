using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

using FFMpegCore.Extensions.System.Drawing.Common;
using FFMpegCore.Pipes;

using RPlace2022Archive.PostgreSQL;

namespace RPlace2022Archive.Inputs; 

public class InputVisualizer {
    private readonly InputDatabase db;
    
    /// <summary>
    /// Create a snapshot of the canvas at the provided <see cref="DateTime"/>
    /// </summary>
    /// <param name="dateTime">The timestamp when the canvas snapshot should be taken</param>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    public async ValueTask<Image> GetImageFromDateTimeAsync(DateTime dateTime) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not yet supported for non-Windows OSes.");
        (Image image, DateTime start) = await db.GetKeyframeAsync(dateTime);

        Bitmap bmp = new Bitmap(image, 2000, 2000);

        await foreach (InputEntry entry in db.GetEntriesAsync(start, dateTime)) {
            for (int y = 0; y < entry.Height; y++) {
                for (int x = 0; x < entry.Width; x++) { bmp.SetPixel(entry.X + x, entry.Y + y, entry.Color); }
            }
        }

        if (dateTime >= CanvasExpansion.SECOND_EXPANSION) return bmp.Clone(CanvasExpansion.SECOND_EXPANSION_RECT, PixelFormat.Format24bppRgb);
        if (dateTime >= CanvasExpansion.FIRST_EXPANSION) return bmp.Clone(CanvasExpansion.FIRST_EXPANSION_RECT, PixelFormat.Format24bppRgb);
        return bmp.Clone(CanvasExpansion.START_RECT, PixelFormat.Format24bppRgb);
    }

    /// <summary>
    /// Create snapshots of the canvas at the provided <see cref="DateTimeRange"/>
    /// </summary>
    /// <param name="range">The range where the snapshots should be taken from</param>
    /// <param name="interval">The amount of time between each snapshot</param>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    public async IAsyncEnumerable<(Image, DateTime)> GetImagesFromDateTimeRangeAsync(DateTimeRange range, TimeSpan interval, [EnumeratorCancellation] CancellationToken ct = default) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not supported for non-Windows OSes.");
        (Image image, DateTime start) = await db.GetKeyframeAsync(range.Start);

        using Bitmap bmp = new Bitmap(image, 2000, 2000);
        DateTime nextReturn = range.Start;

        await foreach (InputEntry entry in db.GetEntriesAsync(start, range.End).WithCancellation(ct)) {
            while (entry.Timestamp > nextReturn) {
                if (range.End >= CanvasExpansion.SECOND_EXPANSION) yield return (bmp.Clone(CanvasExpansion.SECOND_EXPANSION_RECT, PixelFormat.Format24bppRgb), nextReturn);
                else if (range.End >= CanvasExpansion.FIRST_EXPANSION) yield return (bmp.Clone(CanvasExpansion.FIRST_EXPANSION_RECT, PixelFormat.Format24bppRgb), nextReturn);
                else yield return (bmp.Clone(CanvasExpansion.START_RECT, PixelFormat.Format24bppRgb), nextReturn);
                nextReturn += interval;
            }
            for (int y = 0; y < entry.Height; y++) {
                for (int x = 0; x < entry.Width; x++) { bmp.SetPixel(entry.X + x, entry.Y + y, entry.Color); }
            }
        }

        while (nextReturn < range.End) {
            if (range.End >= CanvasExpansion.SECOND_EXPANSION) yield return (bmp.Clone(CanvasExpansion.SECOND_EXPANSION_RECT, PixelFormat.Format24bppRgb), nextReturn);
            else if (range.End >= CanvasExpansion.FIRST_EXPANSION) yield return (bmp.Clone(CanvasExpansion.FIRST_EXPANSION_RECT, PixelFormat.Format24bppRgb), nextReturn);
            else yield return (bmp.Clone(CanvasExpansion.START_RECT, PixelFormat.Format24bppRgb), nextReturn);
            nextReturn += interval;
        }
        if (range.End != nextReturn) {
            if (range.End >= CanvasExpansion.SECOND_EXPANSION) yield return (bmp.Clone(CanvasExpansion.SECOND_EXPANSION_RECT, PixelFormat.Format24bppRgb), nextReturn);
            else if (range.End >= CanvasExpansion.FIRST_EXPANSION) yield return (bmp.Clone(CanvasExpansion.FIRST_EXPANSION_RECT, PixelFormat.Format24bppRgb), nextReturn);
            else yield return (bmp.Clone(CanvasExpansion.START_RECT, PixelFormat.Format24bppRgb), nextReturn);
        }

        image.Dispose();
    }

    /// <summary>
    /// Get video frames for a timelapse of the canvas. Should be used with FFMpegCore.
    /// </summary>
    /// <param name="range">The range from where the frames should be taken from</param>
    /// <param name="interval">The amount of time between each snapshot</param>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    public IEnumerable<IVideoFrame> GetTimelapseFramesForDateTimeRange(DateTimeRange range, TimeSpan interval) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not supported for non-Windows OSes.");
        (Image image, DateTime start) = db.GetKeyframe(range.Start);
        
        using Bitmap bmp = new Bitmap(image, 2000, 2000);
        DateTime nextReturn = range.Start;
        
        foreach (InputEntry entry in db.GetEntries(start, range.End)) {
            while (entry.Timestamp > nextReturn) {
                if (range.Contains(CanvasExpansion.SECOND_EXPANSION) || range.End > CanvasExpansion.SECOND_EXPANSION) yield return new BitmapVideoFrameWrapper(bmp.Clone(CanvasExpansion.SECOND_EXPANSION_RECT, PixelFormat.Format24bppRgb));
                else if (range.Contains(CanvasExpansion.FIRST_EXPANSION) || range.End > CanvasExpansion.FIRST_EXPANSION) yield return new BitmapVideoFrameWrapper(bmp.Clone(CanvasExpansion.FIRST_EXPANSION_RECT, PixelFormat.Format24bppRgb));
                else yield return new BitmapVideoFrameWrapper(bmp.Clone(CanvasExpansion.START_RECT, PixelFormat.Format24bppRgb));
                nextReturn += interval;
            }
            for (int y = 0; y < entry.Height; y++) {
                for (int x = 0; x < entry.Width; x++) { bmp.SetPixel(entry.X + x, entry.Y + y, entry.Color); }
            }
        }
        
        while (nextReturn < range.End) {
            if (range.Contains(CanvasExpansion.SECOND_EXPANSION) || range.End > CanvasExpansion.SECOND_EXPANSION) yield return new BitmapVideoFrameWrapper(bmp.Clone(CanvasExpansion.SECOND_EXPANSION_RECT, PixelFormat.Format24bppRgb));
            else if (range.Contains(CanvasExpansion.FIRST_EXPANSION) || range.End > CanvasExpansion.FIRST_EXPANSION) yield return new BitmapVideoFrameWrapper(bmp.Clone(CanvasExpansion.FIRST_EXPANSION_RECT, PixelFormat.Format24bppRgb));
            else yield return new BitmapVideoFrameWrapper(bmp.Clone(CanvasExpansion.START_RECT, PixelFormat.Format24bppRgb));
        }
        if (range.End != nextReturn) {
            if (range.Contains(CanvasExpansion.SECOND_EXPANSION) || range.End > CanvasExpansion.SECOND_EXPANSION) yield return new BitmapVideoFrameWrapper(bmp.Clone(CanvasExpansion.SECOND_EXPANSION_RECT, PixelFormat.Format24bppRgb));
            else if (range.Contains(CanvasExpansion.FIRST_EXPANSION) || range.End > CanvasExpansion.FIRST_EXPANSION) yield return new BitmapVideoFrameWrapper(bmp.Clone(CanvasExpansion.FIRST_EXPANSION_RECT, PixelFormat.Format24bppRgb));
            else yield return new BitmapVideoFrameWrapper(bmp.Clone(CanvasExpansion.START_RECT, PixelFormat.Format24bppRgb));
        }
        
        image.Dispose();
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