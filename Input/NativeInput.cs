using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KeyPressReplay.Input;

/// <summary>
/// P/Invoke wrapper around the Win32 SendInput API.
/// SendInput works even when the target window is elevated, as long as the caller is also elevated.
/// </summary>
internal static class NativeInput
{
    // --- SendInput structures ---
    // The union must include all three input types (MOUSEINPUT, KEYBDINPUT, HARDWAREINPUT)
    // so that Marshal.SizeOf<INPUT>() returns the correct size that SendInput expects.

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint type;
        public INPUTUNION union;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUTUNION
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    public const uint INPUT_KEYBOARD = 1;
    public const uint KEYEVENTF_SCANCODE = 0x0008;
    public const uint KEYEVENTF_KEYUP = 0x0002;
    public const uint MAPVK_VK_TO_VSC = 0;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKey(uint uCode, uint uMapType);

    /// <summary>
    /// Creates a KEYBDINPUT that sends both the VK code and the hardware scan code.
    /// Games/DirectInput apps read the scan code; normal apps read the VK code.
    /// </summary>
    private static KEYBDINPUT MakeKeyInput(ushort vkCode, uint flags)
    {
        return new KEYBDINPUT
        {
            wVk = vkCode,
            wScan = (ushort)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC),
            dwFlags = flags | KEYEVENTF_SCANCODE,
            time = 0,
            dwExtraInfo = IntPtr.Zero,
        };
    }

    /// <summary>
    /// Sends a key-down followed by key-up for the given virtual-key code.
    /// </summary>
    public static void SendKeyPress(ushort vkCode)
    {
        var inputs = new INPUT[2];

        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].union.ki = MakeKeyInput(vkCode, 0);

        inputs[1].type = INPUT_KEYBOARD;
        inputs[1].union.ki = MakeKeyInput(vkCode, KEYEVENTF_KEYUP);

        uint sent = SendInput(2, inputs, Marshal.SizeOf<INPUT>());
        if (sent != 2)
            Console.Error.WriteLine($"  [WARN] SendInput returned {sent}/2 — Error: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
    }

    /// <summary>
    /// Sends key-down for the given virtual-key code (used for modifier keys and hold).
    /// </summary>
    public static void SendKeyDown(ushort vkCode)
    {
        var inputs = new INPUT[1];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].union.ki = MakeKeyInput(vkCode, 0);

        uint sent = SendInput(1, inputs, Marshal.SizeOf<INPUT>());
        if (sent != 1)
            Console.Error.WriteLine($"  [WARN] SendKeyDown returned {sent}/1 — Error: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
    }

    /// <summary>
    /// Sends key-up for the given virtual-key code (used for modifier keys and hold).
    /// </summary>
    public static void SendKeyUp(ushort vkCode)
    {
        var inputs = new INPUT[1];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].union.ki = MakeKeyInput(vkCode, KEYEVENTF_KEYUP);

        uint sent = SendInput(1, inputs, Marshal.SizeOf<INPUT>());
        if (sent != 1)
            Console.Error.WriteLine($"  [WARN] SendKeyUp returned {sent}/1 — Error: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
    }
}
