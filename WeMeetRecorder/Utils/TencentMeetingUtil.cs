using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuckMeetingPlus.Utils {
    public class TencentMeetingUtil {
        public static string GetTencentMeetingUrl(string meetingId) {
            return $"start wemeet://page/inmeeting?meeting_code={meetingId}";
        }
        public static void TypePassword(string password) {
            var point = Detector.MatchTemplateOnScreen("Assets/PleaseTypePassword.png");
            if (point == null) {
                throw new Exception("未找到输入密码的位置");
            }
            NativeMethod.LeftMouseClick(point);
            NativeMethod.KeyInput(password);
        }
        public static void ConnectToAudio() {
            var point = Detector.MatchTemplateOnScreen("Assets/AutoConnectedToAudio.png");
            if (point == null) {
                throw new Exception("未找到连接到音频");
            }
            NativeMethod.LeftMouseClick(point);
        }
        public static void ClickJoinMeeting() {
            IntPtr hwnd = NativeMethod.FindWindow(null, "腾讯会议"); // 通过窗口标题查找窗口句柄
            NativeMethod.GetWindowRect(hwnd, out var rect);
            //NativeMethod.SetForegroundWindow(hwnd); // 将窗口提到前台
            NativeMethod.LeftMouseClick(rect.Right - 100, rect.Top + 300);
        }
        public static void IKnowRecording() {
            var point = Detector.MatchTemplateOnScreen("Assets/IKnow.png");
            if (point == null) {
                return;
            }
            NativeMethod.LeftMouseClick(point);
        }
        public static void FullScreen() {
            var point = Detector.MatchTemplateOnScreen("Assets/FullScreen.png");
            if (point == null) {
                throw new Exception("未找到全屏按钮");
            }
            NativeMethod.LeftMouseClick(point);
        }

        public static void JoinMeeting(string meetingId, string password) {
            var url = GetTencentMeetingUrl(meetingId);
            Cmd.RunCommand(url);
            Task.Delay(5000).Wait();
            TypePassword(password);
            Task.Delay(1000).Wait();
            ConnectToAudio();
            Task.Delay(1000).Wait();
            ClickJoinMeeting();
            Task.Delay(5000).Wait();
            IKnowRecording();
            Task.Delay(5000).Wait();
            FullScreen();
        }
        public static void JoinMeeting(string meetingId) {
            var url = GetTencentMeetingUrl(meetingId);
            Cmd.RunCommand(url);
            Task.Delay(5000).Wait();
            IKnowRecording();
            Task.Delay(1000).Wait();
            FullScreen();
        }
    }
}
