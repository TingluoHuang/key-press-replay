namespace KeyPressReplay.Models;

/// <summary>
/// Root config loaded from config.json
/// </summary>
public sealed class KeyPressConfig
{
    /// <summary>
    /// How many times to repeat the full sequence. Use 0 for infinite looping.
    /// </summary>
    public int LoopCount { get; set; } = 0;

    /// <summary>
    /// Delay in milliseconds before the first key press (gives you time to focus the target window).
    /// </summary>
    public int InitialDelayMs { get; set; } = 3000;

    /// <summary>
    /// Ordered list of key actions to replay.
    /// </summary>
    public List<KeyAction> Actions { get; set; } = new();
}

/// <summary>
/// A single key-press action followed by an optional wait.
/// </summary>
public sealed class KeyAction
{
    /// <summary>
    /// The key to press. Supports:
    ///   - Single characters: "A", "1", " " (space)
    ///   - Virtual-key names: "Enter", "Tab", "Escape", "F5", "Left", "Right", etc.
    ///   - Modifier combos:  "Ctrl+C", "Alt+F4", "Ctrl+Shift+S"
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Milliseconds to hold the key down before releasing it.
    /// 0 means press and immediately release (default tap behavior).
    /// </summary>
    public int HoldMs { get; set; } = 0;

    /// <summary>
    /// Milliseconds to wait AFTER this key press before the next action.
    /// </summary>
    public int WaitAfterMs { get; set; } = 500;
}
