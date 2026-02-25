namespace KeyPressReplay.Input;

/// <summary>
/// Maps friendly key names (from config) to Win32 virtual-key codes.
/// Reference: https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
/// </summary>
public static class KeyMapper
{
    private static readonly Dictionary<string, ushort> NameToVk = new(StringComparer.OrdinalIgnoreCase)
    {
        // Letters A-Z (VK_A = 0x41 .. VK_Z = 0x5A)
        ["A"] = 0x41, ["B"] = 0x42, ["C"] = 0x43, ["D"] = 0x44,
        ["E"] = 0x45, ["F"] = 0x46, ["G"] = 0x47, ["H"] = 0x48,
        ["I"] = 0x49, ["J"] = 0x4A, ["K"] = 0x4B, ["L"] = 0x4C,
        ["M"] = 0x4D, ["N"] = 0x4E, ["O"] = 0x4F, ["P"] = 0x50,
        ["Q"] = 0x51, ["R"] = 0x52, ["S"] = 0x53, ["T"] = 0x54,
        ["U"] = 0x55, ["V"] = 0x56, ["W"] = 0x57, ["X"] = 0x58,
        ["Y"] = 0x59, ["Z"] = 0x5A,

        // Digits 0-9 (VK_0 = 0x30 .. VK_9 = 0x39)
        ["0"] = 0x30, ["1"] = 0x31, ["2"] = 0x32, ["3"] = 0x33,
        ["4"] = 0x34, ["5"] = 0x35, ["6"] = 0x36, ["7"] = 0x37,
        ["8"] = 0x38, ["9"] = 0x39,

        // Function keys
        ["F1"]  = 0x70, ["F2"]  = 0x71, ["F3"]  = 0x72, ["F4"]  = 0x73,
        ["F5"]  = 0x74, ["F6"]  = 0x75, ["F7"]  = 0x76, ["F8"]  = 0x77,
        ["F9"]  = 0x78, ["F10"] = 0x79, ["F11"] = 0x7A, ["F12"] = 0x7B,

        // Modifiers (use generic VK codes — games often only respond to these)
        ["Ctrl"]    = 0x11, // VK_CONTROL
        ["LCtrl"]   = 0xA2, // VK_LCONTROL
        ["RCtrl"]   = 0xA3, // VK_RCONTROL
        ["Alt"]     = 0x12, // VK_MENU
        ["LAlt"]    = 0xA4, // VK_LMENU
        ["RAlt"]    = 0xA5, // VK_RMENU
        ["Shift"]   = 0x10, // VK_SHIFT
        ["LShift"]  = 0xA0, // VK_LSHIFT
        ["RShift"]  = 0xA1, // VK_RSHIFT
        ["Win"]     = 0x5B, // VK_LWIN
        ["LWin"]    = 0x5B,
        ["RWin"]    = 0x5C,

        // Common keys
        ["Enter"]     = 0x0D,
        ["Return"]    = 0x0D,
        ["Tab"]       = 0x09,
        ["Space"]     = 0x20,
        [" "]         = 0x20,
        ["Backspace"] = 0x08,
        ["Delete"]    = 0x2E,
        ["Del"]       = 0x2E,
        ["Insert"]    = 0x2D,
        ["Ins"]       = 0x2D,
        ["Escape"]    = 0x1B,
        ["Esc"]       = 0x1B,

        // Arrow keys
        ["Left"]  = 0x25,
        ["Up"]    = 0x26,
        ["Right"] = 0x27,
        ["Down"]  = 0x28,

        // Navigation
        ["Home"]     = 0x24,
        ["End"]      = 0x23,
        ["PageUp"]   = 0x21,
        ["PgUp"]     = 0x21,
        ["PageDown"] = 0x22,
        ["PgDn"]     = 0x22,

        // Misc
        ["PrintScreen"] = 0x2C,
        ["PrtSc"]       = 0x2C,
        ["ScrollLock"]  = 0x91,
        ["Pause"]       = 0x13,
        ["CapsLock"]    = 0x14,
        ["NumLock"]     = 0x90,

        // Numpad
        ["Num0"] = 0x60, ["Num1"] = 0x61, ["Num2"] = 0x62, ["Num3"] = 0x63,
        ["Num4"] = 0x64, ["Num5"] = 0x65, ["Num6"] = 0x66, ["Num7"] = 0x67,
        ["Num8"] = 0x68, ["Num9"] = 0x69,
        ["NumMultiply"] = 0x6A, ["NumAdd"]      = 0x6B,
        ["NumSubtract"] = 0x6D, ["NumDecimal"]  = 0x6E,
        ["NumDivide"]   = 0x6F, ["NumEnter"]    = 0x0D,

        // OEM keys (US layout)
        [";"]  = 0xBA, ["="]  = 0xBB, [","]  = 0xBC,
        ["-"]  = 0xBD, ["."]  = 0xBE, ["/"]  = 0xBF,
        ["`"]  = 0xC0, ["["]  = 0xDB, ["\\"] = 0xDC,
        ["]"]  = 0xDD, ["'"]  = 0xDE,
    };

    private static readonly HashSet<string> ModifierNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ctrl", "LCtrl", "RCtrl",
        "Alt",  "LAlt",  "RAlt",
        "Shift","LShift","RShift",
        "Win",  "LWin",  "RWin",
    };

    /// <summary>
    /// Resolves a key name to its virtual-key code.
    /// </summary>
    public static ushort Resolve(string keyName)
    {
        // Trim whitespace for multi-char names, but preserve single-space input
        string normalized = keyName.Length == 1 ? keyName : keyName.Trim();
        if (NameToVk.TryGetValue(normalized, out var vk))
            return vk;

        throw new ArgumentException($"Unknown key name: '{keyName}'. Check config.json.");
    }

    /// <summary>
    /// Parses a key expression like "Ctrl+Shift+S" into modifier VK codes + main key VK code.
    /// </summary>
    public static (ushort[] modifiers, ushort mainKey) Parse(string keyExpression)
    {
        var parts = keyExpression.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            return (Array.Empty<ushort>(), Resolve(parts[0]));
        }

        var modifiers = new List<ushort>();
        ushort mainKey = 0;

        for (int i = 0; i < parts.Length; i++)
        {
            if (i < parts.Length - 1 && ModifierNames.Contains(parts[i]))
            {
                modifiers.Add(Resolve(parts[i]));
            }
            else
            {
                // Last part (or non-modifier) is the main key
                mainKey = Resolve(parts[i]);
            }
        }

        return (modifiers.ToArray(), mainKey);
    }
}
