using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace RPlace2022Archive.Converter; 

public class RGBAToColorConverter : DefaultTypeConverter {
    public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) {
        if (text is null) return Color.Empty;
        
        if (text.Length != 7 && text.Length != 9)
        {
            throw new TypeConverterException(this, memberMapData, text, row.Context);
        }

        byte r = byte.Parse(text.AsSpan()[1..3], NumberStyles.HexNumber);
        byte g = byte.Parse(text.AsSpan()[3..5], NumberStyles.HexNumber);
        byte b = byte.Parse(text.AsSpan()[5..7], NumberStyles.HexNumber);
        byte a = 255;

        if (text.Length == 9)
        {
            a = byte.Parse(text.AsSpan()[7..9], NumberStyles.HexNumber);
        }

        return Color.FromArgb(a, r, g, b);
    }
}