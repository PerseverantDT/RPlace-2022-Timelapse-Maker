namespace RPlace2022Archive.Constants; 

public static class CanvasExpansion {
    public static readonly DateTime START = new DateTime(2022, 4, 1, 12, 40, 0);
    public static readonly DateTime FIRST_EXPANSION = new DateTime(2022, 4, 2, 16, 25, 0);
    public static readonly DateTime SECOND_EXPANSION = new DateTime(2022, 4, 3, 19, 4, 0);
    
    public static readonly Size START_SIZE = new Size(1000, 1000);
    public static readonly Size FIRST_EXPANSION_SIZE = new Size(2000, 1000);
    public static readonly Size SECOND_EXPANSION_SIZE = new Size(2000, 2000);
    
    public static readonly Rectangle START_RECT = new Rectangle(0, 0, START_SIZE.Width, START_SIZE.Height);
    public static readonly Rectangle FIRST_EXPANSION_RECT = new Rectangle(0, 0, FIRST_EXPANSION_SIZE.Width, FIRST_EXPANSION_SIZE.Height);
    public static readonly Rectangle SECOND_EXPANSION_RECT = new Rectangle(0, 0, SECOND_EXPANSION_SIZE.Width, SECOND_EXPANSION_SIZE.Height);
}