namespace RPlace2022Archive.Classes; 

/// <summary>
/// Represents an interval of two <see cref="DateTime"/> instances.
/// </summary>
public struct DateTimeRange {
    public DateTime Start { get; private set; }
    /// <summary>
    /// Whether the <see cref="DateTimeRange.Start"/> is included in the range or not.
    /// </summary>
    public bool StartInclusive { get; private set; }
    public DateTime End { get; private set; }
    /// <summary>
    /// Whether the <see cref="DateTimeRange.End"/> is included in the range or not.
    /// </summary>
    public bool EndInclusive { get; private set; }
    public TimeSpan Length => End - Start;

    /// <summary>
    /// Check if the provided <see cref="DateTime"/> is contained within the <see cref="DateTimeRange"/>
    /// </summary>
    /// <returns><c>true</c> if the timestamp is inside the range, <c>false</c> otherwise</returns>
    public bool Contains(DateTime dateTime) {
        if (dateTime < Start) return false;
        if (dateTime > End) return false;
        if (!StartInclusive && dateTime == Start) return false;
        if (!EndInclusive && dateTime == End) return false;

        return true;
    }

    /// <summary>
    /// Whether the <see cref="DateTimeRange"/> overlaps with the provided <see cref="DateTimeRange"/>
    /// </summary>
    /// <returns><c>true</c> if the ranges overlap, <c>false</c> otherwise</returns>
    public bool Overlaps(DateTimeRange other) {
        if (other.Start > End) return false;
        if (Start > other.End) return false;
        if (other.Start == End && (!other.StartInclusive || !EndInclusive)) return false;
        if (Start == other.End && (!StartInclusive || !other.EndInclusive)) return false;

        return true;
    }

    /// <summary>
    /// Extend the <see cref="DateTimeRange"/> so that it contains the provided <see cref="DateTime"/>
    /// </summary>
    /// <returns><c>true</c> if the range was extended, <c>false</c> otherwise.</returns>
    public bool Extend(DateTime dateTime) {
        if (Contains(dateTime)) return false;
        if (dateTime < Start) {
            Start = dateTime;
            StartInclusive = true;
            return true;
        }
        if (dateTime > End) {
            End = dateTime;
            EndInclusive = true;
            return true;
        }
        if (!StartInclusive && dateTime == Start) {
            StartInclusive = true;
            return true;
        }
        if (!EndInclusive && dateTime == End) {
            EndInclusive = true;
            return true;
        }

        return false;
    } 

    public override string ToString() {
        return $"{Start:s}-{End:s}";
    }

    public DateTimeRange(DateTime start, DateTime end, bool startInclusive = true, bool endInclusive = false) {
        if (start > end) throw new ArgumentException("The end DateTime cannot be smaller than the start DateTime");
        
        this.Start = start;
        this.End = end;
        this.StartInclusive = startInclusive;
        this.EndInclusive = endInclusive;
    }
}