param(
    [Parameter(Mandatory=$true)] [int] $ProcId,
    [Parameter(Mandatory=$true)] [string] $OutDir
)

if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir | Out-Null }

Add-Type -AssemblyName System.Drawing

Add-Type @"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

public static class WinShot {
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint procId);
    [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr hWnd, StringBuilder sb, int max);
    [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);
    [DllImport("user32.dll")] public static extern IntPtr SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int L, T, R, B; }

    public class WinInfo {
        public IntPtr Hwnd;
        public string Title;
        public int Width;
        public int Height;
    }

    public static List<WinInfo> List(uint targetPid) {
        var result = new List<WinInfo>();
        EnumWindows((h, l) => {
            uint p; GetWindowThreadProcessId(h, out p);
            if (p != targetPid) return true;
            if (!IsWindowVisible(h)) return true;
            int len = GetWindowTextLength(h);
            if (len == 0) return true;
            var sb = new StringBuilder(len + 1);
            GetWindowText(h, sb, sb.Capacity);
            RECT rc; GetWindowRect(h, out rc);
            int w = rc.R - rc.L, hgt = rc.B - rc.T;
            if (w <= 0 || hgt <= 0) return true;
            result.Add(new WinInfo { Hwnd = h, Title = sb.ToString(), Width = w, Height = hgt });
            return true;
        }, IntPtr.Zero);
        return result;
    }

    // PW_RENDERFULLCONTENT = 0x00000002 — required for WPF/DirectComposition windows
    public static bool Shot(IntPtr hWnd, int w, int h, string path) {
        using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(bmp)) {
            IntPtr hdc = g.GetHdc();
            bool ok = PrintWindow(hWnd, hdc, 0x2);
            g.ReleaseHdc(hdc);
            bmp.Save(path, ImageFormat.Png);
            return ok;
        }
    }
}
"@ -ReferencedAssemblies System.Drawing

$windows = [WinShot]::List([uint32]$ProcId)
$idx = 0
foreach ($w in $windows) {
    $idx++
    $safe = ($w.Title -replace '[^a-zA-Z0-9_-]', '_')
    $file = Join-Path $OutDir ("{0:D2}_{1}_{2}x{3}.png" -f $idx, $safe, $w.Width, $w.Height)
    $ok = [WinShot]::Shot($w.Hwnd, $w.Width, $w.Height, $file)
    "$idx`tok=$ok`t$($w.Title)`t$($w.Width)x$($w.Height)`t$file"
}
