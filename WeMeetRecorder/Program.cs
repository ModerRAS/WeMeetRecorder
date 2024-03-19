using Coravel;
using FuckMeetingPlus.Utils;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Velopack;
using Velopack.Sources;
using WeMeetRecorder.Utils;

namespace WeMeetRecorder {
    public class Program {
        static async Task<string> GetRedirectedUrl(string url) {
            using HttpClient client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false});
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Found)
            {
                return response.Headers.Location.ToString();
            }
            return url;
        }
        static string GetLatestVersion(string url) {
            string[] segments = url.Split('/');
            string version = segments[segments.Length - 1];
            return version;
        }
        private static async Task UpdateMyApp() {
            var version = GetLatestVersion(await GetRedirectedUrl("https://github.com/ModerRAS/WeMeetRecorder/releases/latest"));
            var mgr = new UpdateManager($"https://github.com/ModerRAS/WeMeetRecorder/releases/download/{version}/");

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
            Task.Run(() => {
                try {
                    UpdateMyApp().Wait();
                } catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }
            });
            var builder = WebApplication.CreateSlimBuilder(args);
            builder.Services.AddScheduler();
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });

            var app = builder.Build();

            app.Services.UseScheduler(scheduler => {
                scheduler.ScheduleAsync(async () => {
                    Console.Write("Running");
                    if (!Env.IsStart) {
                        return;
                    }
                    DateTime now = DateTime.Now;
                    if (now < Env.Time) {
                        return;
                    }
                    if (Env.MeetingId < 0) {
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(Env.MeetingPassword)) {
                        TencentMeetingUtil.JoinMeeting($"{Env.MeetingId}");
                    } else {
                        TencentMeetingUtil.JoinMeeting($"{Env.MeetingId}", Env.MeetingPassword);
                    }
                    await Task.Delay(5000);
                    if (!string.IsNullOrWhiteSpace(Env.ObsPath)) {
                        Cmd.RunCommand($"start /d \"{Path.GetDirectoryName(Env.ObsPath)}\" \"\" {Path.GetFileName(Env.ObsPath)} --startrecording");
                    }
                    Env.IsStart = false;
                    return;
                }).EveryTenSeconds();
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
        public int MeetingId { get; set; }
        public string MeetingPassword { get; set; }
        public string ObsPath { get; set; }
    }
}
