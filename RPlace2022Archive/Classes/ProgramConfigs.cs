using FFMpegCore.Enums;

namespace RPlace2022Archive.Classes; 

public readonly record struct ProgramConfigs(
    Speed EncodingSpeed, 
    DateTime StartTime, 
    DateTime EndTime, 
    TimeSpan Interval
) {
    public ProgramConfigs() : this(Speed.Medium, CanvasExpansion.START, CanvasExpansion.FIRST_EXPANSION, TimeSpan.FromSeconds(1)) { }
}