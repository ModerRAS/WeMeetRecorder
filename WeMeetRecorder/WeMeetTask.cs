using Coravel.Invocable;
using FuckMeetingPlus.Utils;

namespace WeMeetRecorder {
    public class WeMeetTask : IInvocable {
        public WeMeetTask() {

        }
        public async Task Invoke() {
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
            return;
        }
    }
}
