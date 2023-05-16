using CsvHelper.Configuration.Attributes;

using RPlace2022Archive.Converter;

namespace RPlace2022Archive.Classes;

/// <summary>
/// A record containing input data from the original r/Place 2022 dataset.
/// </summary>
/// <param name="Timestamp">The time when the tile was placed</param>
/// <param name="UserId">The hashed identifier of the user who sent the string</param>
/// <param name="PixelColor">The color of the placed tile</param>
/// <param name="Coordinate">The position where the tile was placed. For rectangle inputs (from moderators), the top-left and the bottom-right corners are provided</param>
public record struct RawInput (
    [property: Name("timestamp"), TypeConverter(typeof(CustomDateTimeConverter))] DateTime Timestamp,
    [property: Name("user_id")] string UserId,
    [property: Name("pixel_color"), TypeConverter(typeof(RGBAToColorConverter))] Color PixelColor,
    [property: Name("coordinate"), TypeConverter(typeof(CoordinateConverter))] (short L, short T, short? R, short? B) Coordinate
) {
    public RawInput() : this(default, default!, default, default!) { }
}