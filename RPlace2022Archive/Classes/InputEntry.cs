namespace RPlace2022Archive.Classes;

/// <summary>
/// A record containing input data from the local database
/// </summary>
/// <param name="Timestamp">The time when the tile was placed</param>
/// <param name="X">The x-coordinate where the tile was placed</param>
/// <param name="Y">The y-coordinate where the tile was placed</param>
/// <param name="Width">The width of the rectangle of the input. For most inputs, this will be 1.</param>
/// <param name="Height">The height of the rectangle of the input. For most inputs, this will be 1</param>
/// <param name="R">The red component of the color of the placed tile</param>
/// <param name="G">The green component of the color of the placed tile</param>
/// <param name="B">The blue component of the color of the placed tile</param>
/// <param name="HashedUserId">The hashed identifier of the user who placed the input</param>
public record struct InputEntry (
    DateTime Timestamp,
    short X,
    short Y,
    short Width,
    short Height,
    byte R,
    byte G,
    byte B,
    byte[] HashedUserId
) {
    /// <summary>
    /// The color of the placed tile
    /// </summary>
    public Color Color => Color.FromArgb(R, G, B);
    /// <summary>
    /// The rectangle indicating the position of the placed tile
    /// </summary>
    public Rectangle Position => new Rectangle(X, Y, Width, Height);

    public InputEntry(DateTime timestamp, short x, short y, short width, short height, Color color, byte[] hashedUserId) 
        : this(timestamp, x, y, width, height, color.R, color.G, color.B, hashedUserId) { }
    public InputEntry(DateTime timestamp, short x, short y, byte r, byte g, byte b, byte[] hashedUserId) 
        : this(timestamp, x, y, 1, 1, r, g, b, hashedUserId) { }
    public InputEntry(DateTime timestamp, short x, short y, Color color, byte[] hashedUserId) 
        : this(timestamp, x, y, 1, 1, color.R, color.G, color.B, hashedUserId) { }

    /// <summary>
    /// Create an instance of <see cref="InputEntry"/> from a <see cref="RawInput"/>
    /// </summary>
    public static InputEntry FromRawInput(RawInput input) {
        byte[] hashedUserId = Convert.FromBase64String(input.UserId);
        return new InputEntry(
            input.Timestamp,
            input.Coordinate.L,
            input.Coordinate.T,
            input.Coordinate.R.HasValue ? (short)(input.Coordinate.R.Value - input.Coordinate.L + 1) : (short)1,
            input.Coordinate.B.HasValue ? (short)(input.Coordinate.B.Value - input.Coordinate.T + 1) : (short)1,
            input.PixelColor,
            hashedUserId
        );
    }
}

public static class InputEntryMappings {
    /// <summary>
    /// Convert the <see cref="RawInput"/> instance to an <see cref="InputEntry"/>.
    /// </summary>
    public static InputEntry ToInputEntry(this RawInput input) {
        byte[] hashedUserId = Convert.FromBase64String(input.UserId);
        return new InputEntry(
            input.Timestamp,
            input.Coordinate.L,
            input.Coordinate.T,
            input.Coordinate.R.HasValue ? (short)(input.Coordinate.R.Value - input.Coordinate.L + 1) : (short)1,
            input.Coordinate.B.HasValue ? (short)(input.Coordinate.B.Value - input.Coordinate.T + 1) : (short)1,
            input.PixelColor,
            hashedUserId
        );
    }
}