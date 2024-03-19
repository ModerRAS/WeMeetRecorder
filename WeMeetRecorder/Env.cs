namespace WeMeetRecorder {
    public class Env {
        public static bool IsStart { get; set; } = false;
        public static string StartTime { get; set; }
        public static string ObsPath { get; set; }
        public static int MeetingId { get; set; }
        public static string MeetingPassword { get; set; }
        public static DateTime Time { get => DateTime.ParseExact(StartTime, "yyyy/MM/dd/HH/mm", null); }
    }
}
