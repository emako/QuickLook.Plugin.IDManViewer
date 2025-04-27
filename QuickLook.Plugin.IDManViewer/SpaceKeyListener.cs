using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QuickLook.Plugin.IDManViewer;

internal sealed class SpaceKeyListener : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const Keys SPACE_KEY = Keys.Space;

    private nint _hookId = IntPtr.Zero;
    private LowLevelKeyboardProc _proc;

    public event Action? SpacePressed;
    public event Action? SpaceReleased;

    public SpaceKeyListener()
    {
        _proc = HookCallback;
        _hookId = SetHook(_proc);
    }

    private nint SetHook(LowLevelKeyboardProc proc)
    {
        using Process curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
            GetModuleHandle(curModule.ModuleName), 0);
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode >= 0)
        {
            int wParamInt = (int)wParam;
            var vkCode = Marshal.ReadInt32(lParam);

            if ((wParamInt == WM_KEYDOWN) && (vkCode == (int)SPACE_KEY))
            {
                SpacePressed?.Invoke();
            }
            else if ((wParamInt == WM_KEYUP) && (vkCode == (int)SPACE_KEY))
            {
                SpaceReleased?.Invoke();
            }
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        UnhookWindowsHookEx(_hookId);
    }

    private delegate nint LowLevelKeyboardProc(int nCode, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint GetModuleHandle(string? lpModuleName);
}
