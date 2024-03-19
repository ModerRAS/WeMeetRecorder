using Coravel;
using System.Text.Json.Serialization;
using Velopack;
using Velopack.Sources;
using WeMeetRecorder.Utils;

namespace WeMeetRecorder {
    public class Program {
        private static async Task UpdateMyApp() {
            var mgr = new UpdateManager("https://github.com/ModerRAS/WeMeetRecorder/releases/download/latest/");

            // check for new version
            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion == null)
                return; // no update available

            // download new version
            await mgr.DownloadUpdatesAsync(newVersion);

            // install new version and restart app
            mgr.ApplyUpdatesAndRestart(newVersion);
        }
        public static void Main(string[] args) {
            var Log = new MemoryLogger();
            VelopackApp.Build().Run(Log);
            UpdateMyApp().Wait();
            var builder = WebApplication.CreateSlimBuilder(args);
            builder.Services.AddScheduler();
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });

            var app = builder.Build();

            app.Services.UseScheduler(scheduler => {
                scheduler.Schedule<WeMeetTask>().EveryFifteenSeconds();
            });

            var WeMeetApi = app.MapGroup("/");
            WeMeetApi.MapGet("/{id}/{password}/{obspath}", (int id, string password, string obspath) => {
                Env.MeetingId = id;
                Env.MeetingPassword = password;
                Env.ObsPath = obspath;
                return Results.Ok();
            });
            WeMeetApi.MapGet("/{id}/{obspath}", (int id, string obspath) => {
                Env.MeetingId = id;
                Env.ObsPath = obspath;
                return Results.Ok();
            });
            app.Run();
        }
    }

    public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

    [JsonSerializable(typeof(Todo[]))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext {

    }
}
