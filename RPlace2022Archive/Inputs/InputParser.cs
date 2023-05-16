using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;

using CsvHelper;
using CsvHelper.Configuration;

namespace RPlace2022Archive.Inputs; 

public static class InputParser {
    /// <summary>
    /// The format to the path where the dataset files are located
    /// </summary>
    private const string FILEPATH_FORMAT = @"E:\RPlace2022Inputs\inputs_{0}.csv.gzip";
    /// <summary>
    /// Options used when parsing the dataset files.
    /// </summary>
    private static readonly FileStreamOptions FS_OPTIONS = new FileStreamOptions() {
        Access = FileAccess.Read,
        Mode = FileMode.Open,
        Options = FileOptions.SequentialScan,
        Share = FileShare.Read
    };

    /// <summary>
    /// Get inputs from the selected part of the dataset
    /// </summary>
    /// <param name="segmentNumber">The file number to parse</param>
    public static async IAsyncEnumerable<RawInput> GetEntriesAsync(byte segmentNumber, [EnumeratorCancellation] CancellationToken ct = default) {
        if (segmentNumber > 78) {
            Console.WriteLine($"The dataset does not contain a segment {segmentNumber}.");
            yield break;
        }
    
        await using FileStream inputFile = File.Open(string.Format(FILEPATH_FORMAT, segmentNumber.ToString("00")), FS_OPTIONS);
        await using GZipStream decompressedStream = new GZipStream(inputFile, CompressionMode.Decompress);
        
        Console.WriteLine($"Processing segment {segmentNumber}.");

        using CsvReader csvReader = new CsvReader(new StreamReader(decompressedStream), new CsvConfiguration(CultureInfo.InvariantCulture) {
            Delimiter = ",",
            Encoding = Encoding.UTF8,
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            HeaderValidated = null,
            MissingFieldFound = null
        });
    
        while (await csvReader.ReadAsync()) {
            if (ct.IsCancellationRequested) yield break;
            yield return csvReader.GetRecord<RawInput>();
        }
    }

    /// <summary>
    /// Get inputs from the selected part of the dataset
    /// </summary>
    /// <param name="segmentNumber">The file number to parse</param>
    public static IEnumerable<RawInput> GetEntries(byte segmentNumber) {
        if (segmentNumber > 78) {
            Console.WriteLine($"The dataset does not contain a segment {segmentNumber}.");
        }
        
        using FileStream inputFile = File.Open(string.Format(FILEPATH_FORMAT, segmentNumber.ToString("00")), FS_OPTIONS);
        using GZipStream decompressedStream = new GZipStream(inputFile, CompressionMode.Decompress);
        
        Console.WriteLine($"Processing segment {segmentNumber}.");

        using CsvReader csvReader = new CsvReader(new StreamReader(decompressedStream), new CsvConfiguration(CultureInfo.InvariantCulture) {
            Delimiter = ",",
            Encoding = Encoding.UTF8,
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            HeaderValidated = null,
            MissingFieldFound = null
        });
        
        while (csvReader.Read()) {
            yield return csvReader.GetRecord<RawInput>();
        }
    }

    /// <summary>
    /// Get all inputs from the dataset
    /// </summary>
    public static async IAsyncEnumerable<RawInput> GetAllEntriesAsync([EnumeratorCancellation] CancellationToken ct = default) {
        for (int i = 0; i < 79; i++) {
            await using FileStream inputFile = File.Open(string.Format(FILEPATH_FORMAT, i.ToString("00")), FS_OPTIONS);
            await using GZipStream decompressedStream = new GZipStream(inputFile, CompressionMode.Decompress);
            
            Debug.WriteLine($"Processing segment {i}.");

            using CsvReader csvReader = new CsvReader(new StreamReader(decompressedStream), new CsvConfiguration(CultureInfo.InvariantCulture) {
                Delimiter = ",",
                Encoding = Encoding.UTF8,
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                HeaderValidated = null,
                MissingFieldFound = null
            });
    
            while (await csvReader.ReadAsync()) {
                if (ct.IsCancellationRequested) yield break;
                yield return csvReader.GetRecord<RawInput>();
            }
        }
    }
    
    /// <summary>
    /// Get all inputs from the dataset
    /// </summary>
    public static IEnumerable<RawInput> GetAllEntries() {
        for (int i = 0; i < 79; i++) {
            using FileStream inputFile = File.Open(string.Format(FILEPATH_FORMAT, i.ToString("00")), FS_OPTIONS);
            using GZipStream decompressedStream = new GZipStream(inputFile, CompressionMode.Decompress);
            
            Debug.WriteLine($"Processing segment {i}.");

            using CsvReader csvReader = new CsvReader(new StreamReader(decompressedStream), new CsvConfiguration(CultureInfo.InvariantCulture) {
                Delimiter = ",",
                Encoding = Encoding.UTF8,
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim,
                HeaderValidated = null,
                MissingFieldFound = null
            });
            
            while (csvReader.Read()) {
                yield return csvReader.GetRecord<RawInput>();
            }
        }
    }
}