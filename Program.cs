using System.Security.Principal;
using System.Text.Json;
using KeyPressReplay.Input;
using KeyPressReplay.Models;

// ── Check admin privileges ─────────────────────────────────────────
var identity = WindowsIdentity.GetCurrent();
var principal = new WindowsPrincipal(identity);
if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("WARNING: Not running as Administrator.");
    Console.WriteLine("Key presses may not reach elevated windows.");
    Console.WriteLine("Right-click the exe and select 'Run as administrator'.\n");
    Console.ResetColor();
}

// ── Load config ────────────────────────────────────────────────────
string configPath = args.Length > 0 ? args[0] : "config.json";

if (!File.Exists(configPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Config file not found: {configPath}");
    Console.WriteLine("Place a config.json next to the exe, or pass the path as a command-line argument.");
    Console.ResetColor();
    return 1;
}

KeyPressConfig config;
try
{
    string json = File.ReadAllText(configPath);
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
    config = JsonSerializer.Deserialize<KeyPressConfig>(json, options)
             ?? throw new InvalidOperationException("Deserialized config is null.");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Failed to load config: {ex.Message}");
    Console.ResetColor();
    return 1;
}

if (config.Actions.Count == 0)
{
    Console.WriteLine("No actions defined in config. Nothing to do.");
    return 0;
}

// ── Print summary ──────────────────────────────────────────────────
Console.WriteLine("=== Key Press Replay ===");
Console.WriteLine($"  Config         : {Path.GetFullPath(configPath)}");
Console.WriteLine($"  Actions        : {config.Actions.Count}");
Console.WriteLine($"  Loop count     : {(config.LoopCount <= 0 ? "Infinite" : config.LoopCount)}");
Console.WriteLine($"  Initial delay  : {config.InitialDelayMs}ms");
Console.WriteLine();
Console.WriteLine("Actions:");
for (int i = 0; i < config.Actions.Count; i++)
{
    var a = config.Actions[i];
    Console.WriteLine($"  {i + 1,3}. Key: {a.Key,-20} Hold: {a.HoldMs}ms  Wait after: {a.WaitAfterMs}ms");
}
Console.WriteLine();
Console.WriteLine("Press Ctrl+C to stop at any time.");
Console.WriteLine();

// ── Run ────────────────────────────────────────────────────────────
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;   // prevent immediate termination so we clean up
    cts.Cancel();
    Console.WriteLine("\nStopping...");
};

var engine = new KeyReplayEngine(config);
engine.Run(cts.Token);

return 0;
