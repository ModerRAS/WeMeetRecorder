using System.Drawing;
using System;
using WindowsInput;
using System.Runtime.InteropServices;

namespace FuckMeetingPlus.Utils;

public class NativeMethod
{
    [DllImport("user32.dll")]
    static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }


    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void LeftMouseClick(int Xposition, int Yposition)
    {
        SetCursorPos(Xposition, Yposition);
        mouse_event(MOUSEEVENTF_LEFTDOWN, Xposition, Yposition, 0, 0);
        mouse_event(MOUSEEVENTF_LEFTUP, Xposition, Yposition, 0, 0);
    }

    public static void LeftMouseClick(Point point) {
        LeftMouseClick(point.X, point.Y);
    }

    internal static void KeyInput(string str)
    {
        new InputSimulator().Keyboard.TextEntry(str);
    }

    public static float GetDPIScaling() {
        Graphics g = Graphics.FromHwnd(IntPtr.Zero);
        IntPtr desktop = g.GetHdc();
        int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
        int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

        float ScreenScalingFactor = PhysicalScreenHeight / (float)LogicalScreenHeight;

        return ScreenScalingFactor; // 1.25 = 125%
    }

    public static (int, int) GetScreenSize() {
        Graphics g = Graphics.FromHwnd(IntPtr.Zero);
        IntPtr desktop = g.GetHdc();
        int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
        int PhysicalScreenWidth = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPHORZRES);
        g.ReleaseHdc();
        return (PhysicalScreenWidth, PhysicalScreenHeight); // 1.25 = 125%
    }
    public static Bitmap GetScreenCapture() {
        var (width, height) = GetScreenSize();
        Rectangle tScreenRect = new Rectangle(0, 0, width, height);
        Bitmap tSrcBmp = new Bitmap(width, height); // 用于屏幕原始图片保存
        Graphics gp = Graphics.FromImage(tSrcBmp);
        gp.CopyFromScreen(0, 0, 0, 0, tScreenRect.Size);
        gp.DrawImage(tSrcBmp, 0, 0, tScreenRect, GraphicsUnit.Pixel);
        return tSrcBmp;
    }
}