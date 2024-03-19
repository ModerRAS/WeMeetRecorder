using Coravel;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Velopack;
using Velopack.Sources;
using WeMeetRecorder.Utils;

namespace WeMeetRecorder {
    public class Program {
        private static async Task UpdateMyApp() {
            var mgr = new UpdateManager(new GithubSource("https://github.com/ModerRAS/WeMeetRecorder", null, false));

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
            var builder = WebApplication.CreateSlimBuilder(args);
            builder.Services.AddScheduler();
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });

            var app = builder.Build();

            app.Services.UseScheduler(scheduler => {
                scheduler.ScheduleAsync(async () => {
                    try {
                        await UpdateMyApp();
                    } catch (Exception ex) {
                        Console.WriteLine(ex.ToString());
                    }
                }).DailyAt(4, 0).RunOnceAtStart();
                scheduler.ScheduleAsync(async () => {
                    if (!Env.IsStart) {
                        return;
                    }
                    Env.IsStart = false;
                    DateTime now = DateTime.Now;
                    if (now < Env.Time) {
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Env.MeetingId)) {
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Env.MeetingPassword)) {
                        TencentMeetingUtil.JoinMeeting(Env.MeetingId);
                    } else {
                        TencentMeetingUtil.JoinMeeting(Env.MeetingId, Env.MeetingPassword);
                    }
                    await Task.Delay(5000);
                    if (!string.IsNullOrWhiteSpace(Env.ObsPath)) {
                        //var command = $"cd \"{Path.GetDirectoryName(Env.ObsPath)}\" ;.\"{Path.GetFileName(Env.ObsPath)} --startrecording";
                        Cmd.ExecuteProgram(Env.ObsPath, "--startrecording");
                    }
                    return;
                }).EveryTenSeconds();
            });

            var WeMeetApi = app.MapGroup("/");
            WeMeetApi.MapGet("/{id}/{password}/{obspath}", (string id, string password, string obspath) => {
                Env.MeetingId = id;
                Env.MeetingPassword = password;
                Env.ObsPath = obspath;
                return Results.Ok();
            });
            WeMeetApi.MapGet("/{id}/{obspath}", (string id, string obspath) => {
                Env.MeetingId = id;
                Env.ObsPath = obspath;
                return Results.Ok();
            });
            WeMeetApi.MapPost("/", async data => {
                using (var reader = new StreamReader(data.Request.Body)) {
                    var body = await reader.ReadToEndAsync();
                    // 处理 POST 请求的内容
                    var sessionData = JsonConvert.DeserializeObject<SessionData>(body);
                    Env.StartTime = sessionData.Time;
                    Env.MeetingId = sessionData.MeetingId;
                    Env.MeetingPassword = sessionData.MeetingPassword;
                    Env.ObsPath = sessionData.ObsPath;
                    Env.IsStart = true;
                }
                data.Response.StatusCode = 200;
                
            });
            app.Run();
        }
    }

    public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

    [JsonSerializable(typeof(Todo[]))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext {

    }
    public class SessionData {
        public string Token { get; set; }
        public string Time { get; set; }
        public string MeetingId { get; set; }
        public string MeetingPassword { get; set; }
        public string ObsPath { get; set; }
    }
}
