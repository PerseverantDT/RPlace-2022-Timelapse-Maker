using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;

using Npgsql;

namespace RPlace2022Archive.PostgreSQL; 

public class InputDatabase : IDisposable, IAsyncDisposable {
    private readonly CancellationToken ct;
    private readonly NpgsqlDataSource dataSource;
    private readonly List<(DateTimeRange range, string tableName)> tableMapping = new List<(DateTimeRange, string)>() {
        (new DateTimeRange(new DateTime(2022, 4, 1, 12, 0, 0), new DateTime(2022, 4, 1, 14, 0, 0), true, false), "inputs_part1"),
        (new DateTimeRange(new DateTime(2022, 4, 1, 14, 0, 0), new DateTime(2022, 4, 1, 16, 0, 0), true, false), "inputs_part2"),
        (new DateTimeRange(new DateTime(2022, 4, 1, 16, 0, 0), new DateTime(2022, 4, 1, 18, 0, 0), true, false), "inputs_part3"),
        (new DateTimeRange(new DateTime(2022, 4, 1, 18, 0, 0), new DateTime(2022, 4, 1, 20, 0, 0), true, false), "inputs_part4"),
        (new DateTimeRange(new DateTime(2022, 4, 1, 20, 0, 0), new DateTime(2022, 4, 1, 22, 0, 0), true, false), "inputs_part5"),
        (new DateTimeRange(new DateTime(2022, 4, 1, 22, 0, 0), new DateTime(2022, 4, 2, 0, 0, 0), true, false), "inputs_part6"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 0, 0, 0), new DateTime(2022, 4, 2, 2, 0, 0), true, false), "inputs_part7"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 2, 0, 0), new DateTime(2022, 4, 2, 4, 0, 0), true, false), "inputs_part8"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 4, 0, 0), new DateTime(2022, 4, 2, 6, 0, 0), true, false), "inputs_part9"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 6, 0, 0), new DateTime(2022, 4, 2, 8, 0, 0), true, false), "inputs_part10"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 8, 0, 0), new DateTime(2022, 4, 2, 10, 0, 0), true, false), "inputs_part11"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 10, 0, 0), new DateTime(2022, 4, 2, 12, 0, 0), true, false), "inputs_part12"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 12, 0, 0), new DateTime(2022, 4, 2, 14, 0, 0), true, false), "inputs_part13"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 14, 0, 0), new DateTime(2022, 4, 2, 16, 0, 0), true, false), "inputs_part14"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 16, 0, 0), new DateTime(2022, 4, 2, 18, 0, 0), true, false), "inputs_part15"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 18, 0, 0), new DateTime(2022, 4, 2, 20, 0, 0), true, false), "inputs_part16"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 20, 0, 0), new DateTime(2022, 4, 2, 22, 0, 0), true, false), "inputs_part17"),
        (new DateTimeRange(new DateTime(2022, 4, 2, 22, 0, 0), new DateTime(2022, 4, 3, 0, 0, 0), true, false), "inputs_part18"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 0, 0, 0), new DateTime(2022, 4, 3, 2, 0, 0), true, false), "inputs_part19"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 2, 0, 0), new DateTime(2022, 4, 3, 4, 0, 0), true, false), "inputs_part20"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 4, 0, 0), new DateTime(2022, 4, 3, 6, 0, 0), true, false), "inputs_part21"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 6, 0, 0), new DateTime(2022, 4, 3, 8, 0, 0), true, false), "inputs_part22"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 8, 0, 0), new DateTime(2022, 4, 3, 10, 0, 0), true, false), "inputs_part23"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 10, 0, 0), new DateTime(2022, 4, 3, 12, 0, 0), true, false), "inputs_part24"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 12, 0, 0), new DateTime(2022, 4, 3, 14, 0, 0), true, false), "inputs_part25"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 14, 0, 0), new DateTime(2022, 4, 3, 16, 0, 0), true, false), "inputs_part26"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 16, 0, 0), new DateTime(2022, 4, 3, 18, 0, 0), true, false), "inputs_part27"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 18, 0, 0), new DateTime(2022, 4, 3, 20, 0, 0), true, false), "inputs_part28"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 20, 0, 0), new DateTime(2022, 4, 3, 22, 0, 0), true, false), "inputs_part29"),
        (new DateTimeRange(new DateTime(2022, 4, 3, 22, 0, 0), new DateTime(2022, 4, 4, 0, 0, 0), true, false), "inputs_part30"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 0, 0, 0), new DateTime(2022, 4, 4, 2, 0, 0), true, false), "inputs_part31"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 2, 0, 0), new DateTime(2022, 4, 4, 4, 0, 0), true, false), "inputs_part32"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 4, 0, 0), new DateTime(2022, 4, 4, 6, 0, 0), true, false), "inputs_part33"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 6, 0, 0), new DateTime(2022, 4, 4, 8, 0, 0), true, false), "inputs_part34"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 8, 0, 0), new DateTime(2022, 4, 4, 10, 0, 0), true, false), "inputs_part35"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 10, 0, 0), new DateTime(2022, 4, 4, 12, 0, 0), true, false), "inputs_part36"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 12, 0, 0), new DateTime(2022, 4, 4, 14, 0, 0), true, false), "inputs_part37"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 14, 0, 0), new DateTime(2022, 4, 4, 16, 0, 0), true, false), "inputs_part38"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 16, 0, 0), new DateTime(2022, 4, 4, 18, 0, 0), true, false), "inputs_part39"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 18, 0, 0), new DateTime(2022, 4, 4, 20, 0, 0), true, false), "inputs_part40"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 20, 0, 0), new DateTime(2022, 4, 4, 22, 0, 0), true, false), "inputs_part41"),
        (new DateTimeRange(new DateTime(2022, 4, 4, 22, 0, 0), new DateTime(2022, 4, 5, 0, 0, 0), true, false), "inputs_part42"),
        (new DateTimeRange(new DateTime(2022, 4, 5, 0, 0, 0), new DateTime(2022, 4, 5, 2, 0, 0), true, false), "inputs_part43")
    };

    private bool disposed = false;

    /// <summary>
    /// Add <see cref="InputEntry"/> records to the database.
    /// </summary>
    /// <returns>The number of entries added to the database</returns>
    public async ValueTask<ulong> AddEntriesAsync(IAsyncEnumerable<InputEntry> inputs) {
        long start = Stopwatch.GetTimestamp();
        ulong count = 0;
        NpgsqlConnection conn = await dataSource.OpenConnectionAsync(ct);
        NpgsqlBinaryImporter importer = await conn.BeginBinaryImportAsync("COPY inputs (timestamp, x, y, width, height, color, hashed_user_id) FROM STDIN WITH BINARY", ct);
        try {
            importer.Timeout = TimeSpan.FromMinutes(30);

            await foreach (InputEntry input in inputs.WithCancellation(ct)) {
                await importer.StartRowAsync(ct);
                await importer.WriteAsync(input.Timestamp, ct);
                await importer.WriteAsync(input.X, ct);
                await importer.WriteAsync(input.Y, ct);
                await importer.WriteAsync(input.Width, ct);
                await importer.WriteAsync(input.Height, ct);
                await importer.WriteAsync(input.Color.ToArgb(), ct);
                await importer.WriteAsync(input.HashedUserId, ct);
            }

            if (!ct.IsCancellationRequested) count = await importer.CompleteAsync(ct);

            return count;
        }
        finally {
            TimeSpan elapsed = Stopwatch.GetElapsedTime(start, Stopwatch.GetTimestamp());
            Console.WriteLine($"Took {elapsed:g} to add {count} entries.");
            
            await importer.DisposeAsync();
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }
    }

    /// <summary>
    /// Get the number of entries in the database
    /// </summary>
    /// <remarks>
    /// If all the dataset entries are in the database, there should be roughly
    /// 160 million entries plus/minus 1 million (I forgot how much exactly).
    /// </remarks>
    public async ValueTask<int> GetTotalCountAsync() {
        await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(ct);
        await using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM inputs", conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
        
        #if DEBUG
        int totalCount = 0;
        CancellationTokenSource trackerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = Task.Run(async () => {
            PeriodicTimer ticker = new PeriodicTimer(TimeSpan.FromSeconds(1));
            Console.WriteLine();
            while (await ticker.WaitForNextTickAsync(trackerCts.Token)) {
                (_, int top) = Console.GetCursorPosition();
                Console.SetCursorPosition(0, top - 1);
                Console.WriteLine($"Found {totalCount:#,0} entries so far.");
            }
        }, trackerCts.Token);
        #endif
        

        while (await reader.ReadAsync(ct)) {
            totalCount++;
        }
        
        #if DEBUG
        trackerCts.Cancel();
        #endif
        return totalCount;
    }

    public async ValueTask<DateTimeRange> GetTimestampRangeAsync() {
        DateTimeRange range = default;
        await using NpgsqlConnection conn = await dataSource.OpenConnectionAsync(ct);
        await using NpgsqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT timestamp FROM inputs";
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);

        bool rangeInitialized = false;
        
        #if DEBUG
        int totalCount = 0;
        Stopwatch timer = Stopwatch.StartNew();
        CancellationTokenSource trackerCts = new CancellationTokenSource();
        #pragma warning disable CS4014
        // ReSharper disable once AccessToModifiedClosure
        Task.Run(async () => {
            PeriodicTimer ticker = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
            while (await ticker.WaitForNextTickAsync(trackerCts.Token)) {
                Console.WriteLine($"[{timer.Elapsed:c}] Found {totalCount:#,0} entries");
            }
        }, trackerCts.Token);
        #pragma warning restore CS4014
        #endif
        
        while (await reader.ReadAsync(ct)) {
            DateTime timestamp = reader.GetDateTime(0);
            if (!rangeInitialized) {
                range = new DateTimeRange(timestamp, timestamp, true, true);
                rangeInitialized = true;
            }
            else {
                range.Extend(timestamp);
            }
            
            #if DEBUG
            totalCount++;
            #endif
        }

        #if DEBUG
        trackerCts.Cancel();
        timer.Stop();
        #endif
        return range;
    }

    /// <summary>
    /// Get all entries between <paramref name="startTime"/> and <paramref name="endTime"/>, sorted
    /// in ascending order based on the <see cref="InputEntry.Timestamp"/>.
    /// </summary>
    public async IAsyncEnumerable<InputEntry> GetEntriesAsync(DateTime startTime, DateTime endTime) {
        if (endTime < startTime) yield break;
        
        DateTimeRange queryRange = new DateTimeRange(startTime, endTime, true, true);
        IEnumerable<string> viewsToQuery = tableMapping
            .Where(map => map.range.Overlaps(queryRange))
            .Select(map => map.tableName);

        await using NpgsqlConnection dsConn = await dataSource.OpenConnectionAsync(ct);
        await using NpgsqlTransaction tx = await dsConn.BeginTransactionAsync(ct);
        foreach (string view in viewsToQuery) {
            Debug.WriteLine($"Querying {view}.");
            await using NpgsqlCommand cmd = dsConn.CreateCommand();
            cmd.CommandText = $"SELECT * from {view} WHERE timestamp BETWEEN @start AND @end ORDER BY timestamp";
            cmd.Parameters.Add(new NpgsqlParameter<DateTime>("start", startTime));
            cmd.Parameters.Add(new NpgsqlParameter<DateTime>("end", endTime));
            
            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) {
                if (ct.IsCancellationRequested) yield break;

                yield return reader.GetInputEntry();
            }
        }
    }

    /// <summary>
    /// Get all entries between <paramref name="startTime"/> and <paramref name="endTime"/>, sorted
    /// in ascending order based on the <see cref="InputEntry.Timestamp"/>.
    /// </summary>
    public IEnumerable<InputEntry> GetEntries(DateTime startTime, DateTime endTime) {
        if (endTime < startTime) yield break;
        
        DateTimeRange range = new DateTimeRange(startTime, endTime, true, true);
        IEnumerable<string> viewsToQuery = tableMapping
            .Where(map => map.range.Overlaps(range))
            .Select(map => map.tableName);

        using NpgsqlConnection dsConn = dataSource.OpenConnection();
        using NpgsqlTransaction tx = dsConn.BeginTransaction();
        foreach (string view in viewsToQuery) {
            Debug.WriteLine($"Querying {view}.");

            using NpgsqlCommand cmd = dsConn.CreateCommand();
            cmd.CommandText = $"SELECT * from {view} WHERE timestamp BETWEEN @start AND @end ORDER BY timestamp";
            cmd.Parameters.Add(new NpgsqlParameter<DateTime>("start", startTime));
            cmd.Parameters.Add(new NpgsqlParameter<DateTime>("end", endTime));

            using NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                yield return reader.GetInputEntry();
            }
        }
    }

    /// <summary>
    /// Get all entries in the database, sorted in ascending order based on the
    /// <see cref="InputEntry.Timestamp"/>.
    /// </summary>
    public async IAsyncEnumerable<InputEntry> GetAllEntriesAsync() {
        await using NpgsqlConnection dsConn = await dataSource.OpenConnectionAsync(ct);
        await using NpgsqlTransaction tx = await dsConn.BeginTransactionAsync(ct);
        foreach ((_, string viewName) in tableMapping) {
            await using NpgsqlCommand cmd = dsConn.CreateCommand();
            cmd.CommandText = $"SELECT * from {viewName} ORDER BY timestamp";

            await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) {
                if (ct.IsCancellationRequested) yield break;
                yield return new InputEntry(
                    reader.GetDateTime(0),
                    reader.GetInt16(1),
                    reader.GetInt16(2),
                    reader.GetInt16(3),
                    reader.GetInt16(4),
                    Color.FromArgb(reader.GetInt32(5)),
                    await reader.GetFieldValueAsync<byte[]>(6, ct)
                );
            }
        }
    }

    /// <summary>
    /// Get all entries in the dataset
    /// </summary>
    public async IAsyncEnumerable<InputEntry> GetAllEntriesUnorderedAsync() {
        await using NpgsqlConnection dsConn = await dataSource.OpenConnectionAsync(ct);
        await using NpgsqlTransaction tx = await dsConn.BeginTransactionAsync(ct);
        await using NpgsqlCommand cmd = dsConn.CreateCommand();
        cmd.CommandText = $"SELECT * from inputs";
        
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) {
            if (ct.IsCancellationRequested) yield break;

            yield return reader.GetInputEntry();
        }
    }

    /// <summary>
    /// Add a keyframe to the database, to be used for ease of timelapse making.
    /// </summary>
    /// <param name="image">The keyframe to be added to the database</param>
    /// <param name="timestamp">The time when the keyframe was taken from</param>
    /// <returns>The number of entries added. Will always be <c>1</c> unless an error happens.</returns>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    public async ValueTask<int> AddKeyframeAsync(Image image, DateTime timestamp) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not supported for non-Windows OSes.");
        
        string filePath = $"E:\\Databases\\RPlace2022\\external_files\\keyframes\\{timestamp:yyyy-M-d_HH-mm-ss}.png";
        await using (FileStream fs = File.Create(filePath)) {
            image.Save(fs, ImageFormat.Png);
        }
        
        await using NpgsqlConnection dsConn = await dataSource.OpenConnectionAsync(ct);
        await using NpgsqlCommand cmd = dsConn.CreateCommand();
        cmd.CommandText = $"INSERT INTO keyframes (timestamp, image_path) VALUES (@timestamp, @image_path)";
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>("timestamp", timestamp));
        cmd.Parameters.Add(new NpgsqlParameter<string>("image_path", filePath));

        return await cmd.ExecuteNonQueryAsync(ct);
    }

    /// <summary>
    /// Add keyframes to the database, to be used for ease of timelapse making.
    /// </summary>
    /// <returns>The number of entries added</returns>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    /// <seealso cref="InputDatabase.AddKeyframeAsync"/>
    public async ValueTask<int> AddKeyframesAsync(IAsyncEnumerable<(Image, DateTime)> keyframes) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not supported for non-Windows OSes.");
        await using NpgsqlConnection dsConn = await dataSource.OpenConnectionAsync(ct);
        await using NpgsqlTransaction transaction = await dsConn.BeginTransactionAsync(ct);

        int count = 0;
        await foreach ((Image image, DateTime timestamp) in keyframes.WithCancellation(ct)) {
            string filePath = $"E:\\Databases\\RPlace2022\\external_files\\keyframes\\{timestamp:yyyy-M-d_HH-mm-ss}.png";
            await using (FileStream fs = File.Create(filePath)) {
                image.Save(fs, ImageFormat.Png);
            }
            
            await using NpgsqlCommand cmd = dsConn.CreateCommand();
            cmd.CommandText = $"INSERT INTO keyframes (timestamp, image_path) VALUES (@timestamp, @image_path)";
            cmd.Parameters.Add(new NpgsqlParameter<DateTime>("timestamp", timestamp));
            cmd.Parameters.Add(new NpgsqlParameter<string>("image_path", filePath));
            
            count += await cmd.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
        return count;
    }

    /// <summary>
    /// Get a keyframe from the database, to be used when making a snapshot of the canvas.
    /// </summary>
    /// <param name="targetTime">The target timestamp when the keyframe should be added.</param>
    /// <returns>
    /// A tuple containing the keyframe and the time of the keyframe. The time of
    /// the keyframe will never be later than <paramref name="targetTime"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    /// <seealso cref="InputDatabase.GetKeyframe"/>
    public async ValueTask<(Image, DateTime)> GetKeyframeAsync(DateTime targetTime) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not supported for non-Windows OSes.");
        
        await using NpgsqlConnection dsConn = await dataSource.OpenConnectionAsync(ct);
        await using NpgsqlCommand cmd = dsConn.CreateCommand();
        cmd.CommandText = $"SELECT image_path, timestamp FROM keyframes WHERE timestamp < @timestamp ORDER BY timestamp DESC LIMIT 1";
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>("timestamp", targetTime));
        
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct)) return (Image.FromFile(reader.GetString(0)), reader.GetDateTime(1));

        Bitmap bmp = new Bitmap(2000, 2000, PixelFormat.Format24bppRgb);
        using Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        return (bmp, new DateTime(2022, 4, 1, 12, 40, 0));
    }

    /// <summary>
    /// Get a keyframe from the database, to be used when making a snapshot of the canvas.
    /// </summary>
    /// <param name="targetTime">The target timestamp when the keyframe should be added.</param>
    /// <returns>
    /// A tuple containing the keyframe and the time of the keyframe. The time of
    /// the keyframe will never be later than <paramref name="targetTime"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">The operating system where the program is being run is not a Windows OS.</exception>
    /// <seealso cref="InputDatabase.GetKeyframeAsync"/>
    public (Image, DateTime) GetKeyframe(DateTime targetTime) {
        if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("Image manipulation is not supported for non-Windows OSes.");
        
        using NpgsqlConnection dsConn = dataSource.OpenConnection();
        using NpgsqlCommand cmd = dsConn.CreateCommand();
        cmd.CommandText = $"SELECT image_path, timestamp FROM keyframes WHERE timestamp < @timestamp ORDER BY timestamp DESC LIMIT 1";
        cmd.Parameters.Add(new NpgsqlParameter<DateTime>("timestamp", targetTime));
        
        using NpgsqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read()) return (Image.FromFile(reader.GetString(0)), reader.GetDateTime(1));
        
        Bitmap bmp = new Bitmap(2000, 2000, PixelFormat.Format24bppRgb);
        using Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.White);
        return (bmp, new DateTime(2022, 4, 1, 12, 40, 0));
    }

    /// <summary>
    /// Create materialized views for the database. This must be run before any getter functions are
    /// called, as they all use the materialized views for faster querying.
    /// </summary>
    /// <seealso cref="InputDatabase.CreateIndexOnMaterializedViews"/>
    public async ValueTask CreateMaterializedViews() {
        await using NpgsqlConnection dsConn = await dataSource.OpenConnectionAsync(ct);
        
        foreach ((DateTimeRange range, string viewName) in tableMapping) {
            await using NpgsqlCommand cmd = dsConn.CreateCommand();
            cmd.CommandText = $"CREATE IF NOT EXISTS MATERIALIZED VIEW {viewName} AS SELECT timestamp, x, y, width, height, color, hashed_user_id FROM inputs WHERE timestamp BETWEEN '{range.Start:yyyy-MM-dd HH:mm:ss}' AND '{range.End:yyyy-MM-dd HH:mm:ss}'";

            await cmd.ExecuteNonQueryAsync(ct);
            Console.WriteLine($"Created {viewName}.");
        }
    }

    /// <summary>
    /// Create an index on the timestamp on every materialized view in the database. This is not required,
    /// but running this at least once will speed up the getter functions.
    /// <see cref="InputDatabase.CreateMaterializedViews"/> must be run before this function is called.
    /// </summary>
    public async ValueTask CreateIndexOnMaterializedViews() {
        await using NpgsqlConnection dsConn = await dataSource.OpenConnectionAsync(ct);
        
        foreach((_, string viewName) in tableMapping) {
            await using NpgsqlCommand cmd = dsConn.CreateCommand();
            cmd.CommandText = $"CREATE INDEX ON {viewName} (timestamp)";
            
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
    
    public InputDatabase(CancellationToken ct) {
        NpgsqlDataSourceBuilder dsBuilder = new NpgsqlDataSourceBuilder();
        NpgsqlConnectionStringBuilder csBuilder = dsBuilder.ConnectionStringBuilder;

        csBuilder.Host = "127.0.0.1";
        csBuilder.Port = 26257;
        csBuilder.Username = "root";
        csBuilder.Database = "rplace2022";
        csBuilder.SslMode = SslMode.Disable;
        csBuilder.IncludeErrorDetail = true;

        csBuilder.ApplicationName = "r/place 2022 Archive";
        csBuilder.CommandTimeout = 12 * 60 * 60;
        csBuilder.InternalCommandTimeout = 12 * 60 * 60;
        csBuilder.Pooling = true;

        dataSource = dsBuilder.Build();

        this.ct = ct;
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool freeManagedResources) {
        if (disposed) return;
        disposed = true;

        if (!freeManagedResources) return;
        dataSource.Dispose();
    }

    public async ValueTask DisposeAsync() {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeAsync(bool freeManagedResources) {
        if (disposed) return;
        disposed = true;

        if (!freeManagedResources) return;
        await dataSource.DisposeAsync();
    }
    
    ~InputDatabase() {
        Dispose(false);
    }
}

public static class NpgsqlDataReaderExtensions {
    public static InputEntry GetInputEntry(this NpgsqlDataReader reader) {
        return new InputEntry(
            reader.GetDateTime(0),
            reader.GetInt16(1),
            reader.GetInt16(2),
            reader.GetInt16(3),
            reader.GetInt16(4),
            Color.FromArgb(reader.GetInt32(5)),
            reader.GetFieldValue<byte[]>(6)
        );
    }
}