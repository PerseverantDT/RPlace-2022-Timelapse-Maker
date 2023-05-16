using FFMpegCore;

using Microsoft.Extensions.DependencyInjection;

using RPlace2022Archive.Inputs;
using RPlace2022Archive.PostgreSQL;

namespace RPlace2022Archive;

internal class Program {
    public static async Task Main(string[] args) {
        const int numBeeps = 3;
        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) => {
            cts.Cancel();
        };

        IServiceProvider serviceProvider = CreateServiceProvider(cts.Token);
        InputDatabase db = serviceProvider.GetRequiredService<InputDatabase>();
        InputVisualizer visualizer = serviceProvider.GetRequiredService<InputVisualizer>();
        
        // Write your code to create a timelapse in this region
        #region Your code here
        #endregion
    }
    
    private static IServiceProvider CreateServiceProvider(CancellationToken ct = default) {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<InputDatabase>(_ => new InputDatabase(ct));
        services.AddSingleton<InputVisualizer>();
        
        GlobalFFOptions.Configure(new FFOptions() {
            BinaryFolder = @"C:\ProgramData\chocolatey\lib\ffmpeg-full\tools\ffmpeg\bin",
            TemporaryFilesFolder = Path.GetTempPath()
        });

        return services.BuildServiceProvider();
    }
}