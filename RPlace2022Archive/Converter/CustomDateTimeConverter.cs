using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace RPlace2022Archive.Converter; 

public class CustomDateTimeConverter : DateTimeConverter {
    private static readonly string[] FORMATS = {
        "yyyy-MM-dd HH:mm:ss.fff UTC",
        "yyyy-MM-dd HH:mm:ss.ff UTC",
        "yyyy-MM-dd HH:mm:ss.f UTC",
        "yyyy-MM-dd HH:mm:ss UTC"
    }; 
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) {
        return DateTime.TryParseExact(text, FORMATS, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result) 
            ? result 
            : base.ConvertFromString(text, row, memberMapData);
    }
}