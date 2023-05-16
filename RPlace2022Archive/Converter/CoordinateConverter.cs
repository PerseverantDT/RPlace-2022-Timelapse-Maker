using System.Runtime.CompilerServices;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace RPlace2022Archive.Converter; 

public class CoordinateConverter : ITypeConverter {
    public object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData) {
        if (text is null) return null;
        
        // Split the string into individual components
        string[] components = text.Split(',');
        if (components.Length != 2 && components.Length != 4)
        {
            throw new CsvHelperException(row.Context, $"Invalid tuple format: {text}");
        }

        // Parse the components as integers
        Span<short> values = stackalloc short[components.Length];
        for (int i = 0; i < components.Length; i++)
        {
            if (!short.TryParse(components[i], out values[i]))
            {
                throw new CsvHelperException(row.Context, $"Invalid tuple format: {text}");
            }
        }

        // Create a new Tuple<int, int> or Tuple<int, int, int, int> object
        return components.Length == 2 
            ? ValueTuple.Create<short, short, short?, short?>(values[0], values[1], null, null) 
            : ValueTuple.Create<short, short, short?, short?>(values[0], values[1], values[2], values[3]);
    }

    public string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData) {
        if (value is null) return null;
        
        ITuple tuple = (ITuple)value;
        Span<int> values = stackalloc int[tuple.Length];
        for (int i = 0; i < tuple.Length; i++)
        {
            values[i] = (int)tuple[i]!;
        }
        StringBuilder sb = new StringBuilder();
        foreach (int i in values) {
            sb.Append(i).Append(", ");
        }

        return sb.ToString(0, sb.Length - 2);
    }
}