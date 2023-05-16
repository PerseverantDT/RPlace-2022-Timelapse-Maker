using RPlace2022Archive.Classes;

namespace RPlace2022Archive.Tests; 

[TestFixture]
public class DateTimeRangeTest {
    private readonly DateTime start = new DateTime(2023, 4, 1);
    private readonly DateTime end = new DateTime(2023, 4, 30);
    
    
    /// <summary>
    /// Tests if an exception is thrown when the start date is after the end date.
    /// </summary>
    [Test]
    public void InvalidDateTimeRange() {
        Assert.Throws<ArgumentException>(() => _ = new DateTimeRange(end, start));
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Contains(DateTime)" /> returns <c>false</c> when the given
    /// <see cref="DateTime" /> is between <see cref="DateTimeRange.Start" /> and <see cref="DateTimeRange.End" />.
    /// </summary>
    [Test]
    public void Contains_InsideRange() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, false);
        DateTime testDateTime = new DateTime(2023, 4, 15);

        // Act
        bool result = range.Contains(testDateTime);

        // Assert
        Assert.That(result, Is.True);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Contains(DateTime)" /> returns <c>false</c> when the given
    /// <see cref="DateTime" /> is before <see cref="DateTimeRange.Start" />.
    /// </summary>
    [Test]
    public void Contains_OutsideRange_BeforeStart() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, false);
        DateTime testDateTime = new DateTime(2023, 3, 15);
        
        // Act
        bool result = range.Contains(testDateTime);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Contains(DateTime)" /> returns <c>false</c> when the given
    /// <see cref="DateTime" /> is after <see cref="DateTimeRange.End" />.
    /// </summary>
    [Test]
    public void Contains_OutsideRange_AfterEnd() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, false);
        DateTime testDateTime = new DateTime(2023, 5, 15);
        
        // Act
        bool result = range.Contains(testDateTime);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Contains(DateTime)" /> returns <c>true</c> when the given
    /// <see cref="DateTime" /> is equal to <see cref="DateTimeRange.Start" /> and
    /// <see cref="DateTimeRange.StartInclusive" /> is <c>true</c>.
    /// </summary>
    [Test]
    public void Contains_OnStart_Inclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, false);
        DateTime testDateTime = start;
        
        // Act
        bool result = range.Contains(testDateTime);
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Contains(DateTime)" /> returns <c>false</c> when the given
    /// <see cref="DateTime" /> is equal to <see cref="DateTimeRange.Start" /> and
    /// <see cref="DateTimeRange.StartInclusive" /> is false.
    /// </summary>
    [Test]
    public void Contains_OnStart_Exclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, false, false);
        DateTime testDateTime = start;
        
        // Act
        bool result = range.Contains(testDateTime);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Contains(DateTime)" /> returns <c>true</c> when the given
    /// <see cref="DateTime" /> is equal to <see cref="DateTimeRange.End" /> and
    /// <see cref="DateTimeRange.EndInclusive" /> is <c>true</c>.
    /// </summary>
    [Test]
    public void Contains_OnEnd_Inclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, true);
        DateTime testDateTime = end;
        
        // Act
        bool result = range.Contains(testDateTime);
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Contains(DateTime)" /> returns <c>false</c> when the given
    /// <see cref="DateTime" /> is equal to <see cref="DateTimeRange.End" /> and
    /// <see cref="DateTimeRange.EndInclusive" /> is false.
    /// </summary>
    [Test]
    public void Contains_OnEnd_Exclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, false);
        DateTime testDateTime = end;
        
        // Act
        bool result = range.Contains(testDateTime);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>false</c> when the given
    /// <see cref="DateTimeRange" /> does not overlap.
    /// </summary>
    [Test]
    public void Overlaps_CompletelySeparate() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end);
        DateTimeRange other = new DateTimeRange(new DateTime(2015, 1, 1), new DateTime(2015, 12, 31));
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>true</c> when the given
    /// <see cref="DateTimeRange" /> overlaps.
    /// </summary>
    [Test]
    public void Overlaps_Overlapping() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end);
        DateTimeRange other = new DateTimeRange(new DateTime(2023, 3, 1), new DateTime(2023, 4, 15));
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>true</c> when the given
    /// <see cref="DateTimeRange" /> is inside the range.
    /// </summary>
    [Test]
    public void Overlaps_OneInsideAnother() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end);
        DateTimeRange other = new DateTimeRange(new DateTime(2023, 4, 2), new DateTime(2023, 4, 29));
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>true</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.Start" /> as the range.
    /// </summary>
    [Test]
    public void Overlaps_SameStart() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end);
        DateTimeRange other = new DateTimeRange(start, new DateTime(2023, 6, 1));
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>true</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.End" /> as the range.
    /// </summary>
    [Test]
    public void Overlaps_SameEnd() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end);
        DateTimeRange other = new DateTimeRange(new DateTime(2022, 12, 1), end);
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>true</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.End" /> as the range's
    /// <see cref="DateTimeRange.Start" /> and both the given DateTimeRange's
    /// <see cref="DateTimeRange.EndInclusive" /> and the range's <see cref="DateTimeRange.StartInclusive" />
    /// are <c>true</c>.
    /// </summary>
    [Test]
    public void Overlaps_StartOnOtherEnd_BothInclusive() {
        DateTimeRange range = new DateTimeRange(start, end, true, true);
        DateTimeRange other = new DateTimeRange(new DateTime(2023, 2, 1), start, true, true);
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.True);
    }

    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>false</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.End" /> as the range's
    /// <see cref="DateTimeRange.Start" /> but the given DateTimeRange's <see cref="DateTimeRange.EndInclusive" />
    /// is <c>false</c>.
    /// </summary>
    [Test]
    public void Overlaps_StartOnOtherEnd_OtherEndExclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, true);
        DateTimeRange other = new DateTimeRange(new DateTime(2023, 2, 1), start, true, false);
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>false</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.End" /> as the range's
    /// <see cref="DateTimeRange.Start" /> but the range's <see cref="DateTimeRange.StartInclusive" />
    /// is <c>false</c>.
    /// </summary>
    [Test]
    public void Overlaps_StartOnOtherEnd_StartExclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, false, true);
        DateTimeRange other = new DateTimeRange(new DateTime(2023, 2, 1), start, false, true);
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>false</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.End" /> as the range's
    /// <see cref="DateTimeRange.Start" /> but both the range's <see cref="DateTimeRange.StartInclusive" />
    /// and the given DateTimeRange's <see cref="DateTimeRange.EndInclusive" /> are <c>false</c>.
    /// </summary>
    [Test]
    public void Overlaps_StartOnOtherEnd_BothExclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, false, false);
        DateTimeRange other = new DateTimeRange(new DateTime(2023, 2, 1), start, false, false);
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>true</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.Start" /> as the range's
    /// <see cref="DateTimeRange.End" /> and both the range's <see cref="DateTimeRange.EndInclusive" />
    /// and the given DateTimeRange's <see cref="DateTimeRange.StartInclusive" /> are <c>true</c>.
    /// </summary>
    [Test]
    public void Overlaps_EndOnOtherStart_BothInclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, true);
        DateTimeRange other = new DateTimeRange(end, new DateTime(2023, 6, 1), true, true);
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>false</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.Start" /> as the range's
    /// <see cref="DateTimeRange.End" /> but the given DateTimeRange's <see cref="DateTimeRange.StartInclusive" />
    /// is <c>false</c>.
    /// </summary>
    [Test]
    public void Overlaps_EndOnOtherStart_OtherStartExclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, true);
        DateTimeRange other = new DateTimeRange(end, new DateTime(2023, 6, 1), false, true);
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>false</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.Start" /> as the range's
    /// <see cref="DateTimeRange.End" /> but the range's <see cref="DateTimeRange.EndInclusive" />
    /// is <c>false</c>.
    /// </summary>
    [Test]
    public void Overlaps_EndOnOtherStart_EndExclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, true, false);
        DateTimeRange other = new DateTimeRange(end, new DateTime(2023, 6, 1), true, false);
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.False);
    }
    
    /// <summary>
    /// Test that <see cref="DateTimeRange.Overlaps(DateTimeRange)" /> returns <c>false</c> when the given
    /// <see cref="DateTimeRange" /> has the same <see cref="DateTimeRange.Start" /> as the range's
    /// <see cref="DateTimeRange.End" /> but both the range's <see cref="DateTimeRange.EndInclusive" />
    /// and the given DateTimeRange's <see cref="DateTimeRange.StartInclusive" /> are <c>false</c>.
    [Test]
    public void Overlaps_EndOnOtherStart_BothExclusive() {
        // Arrange
        DateTimeRange range = new DateTimeRange(start, end, false, false);
        DateTimeRange other = new DateTimeRange(end, new DateTime(2023, 6, 1), false, false);
        
        // Act
        bool result = range.Overlaps(other);
        
        // Assert
        Assert.That(result, Is.False);
    }
}
