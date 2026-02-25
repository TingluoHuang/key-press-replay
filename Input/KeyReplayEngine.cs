using KeyPressReplay.Models;

namespace KeyPressReplay.Input;

/// <summary>
/// Executes a sequence of key actions by sending simulated input via SendInput.
/// </summary>
internal sealed class KeyReplayEngine
{
    private readonly KeyPressConfig _config;

    public KeyReplayEngine(KeyPressConfig config)
    {
        _config = config;
    }

    public void Run(CancellationToken ct)
    {
        Console.WriteLine($"Starting in {_config.InitialDelayMs}ms — switch to the target window now...");
        Thread.Sleep(_config.InitialDelayMs);

        bool infinite = _config.LoopCount <= 0;
        int iteration = 0;

        while (!ct.IsCancellationRequested)
        {
            iteration++;
            if (!infinite && iteration > _config.LoopCount)
                break;

            string loopLabel = infinite ? $"Loop #{iteration} (infinite)" : $"Loop {iteration}/{_config.LoopCount}";
            Console.WriteLine($"\n--- {loopLabel} ---");

            foreach (var action in _config.Actions)
            {
                if (ct.IsCancellationRequested) break;

                SendKey(action.Key, action.HoldMs);
                Console.WriteLine($"  Pressed: {action.Key,-20} | Hold: {action.HoldMs}ms | Wait: {action.WaitAfterMs}ms");

                if (action.WaitAfterMs > 0)
                    Thread.Sleep(action.WaitAfterMs);
            }
        }

        Console.WriteLine("\nDone.");
    }

    private static void SendKey(string keyExpression, int holdMs)
    {
        var (modifiers, mainKey) = KeyMapper.Parse(keyExpression);

        // Press modifiers down
        foreach (var mod in modifiers)
            NativeInput.SendKeyDown(mod);

        // Press the main key down
        NativeInput.SendKeyDown(mainKey);

        // Hold for the specified duration
        if (holdMs > 0)
            Thread.Sleep(holdMs);

        // Release the main key
        NativeInput.SendKeyUp(mainKey);

        // Release modifiers in reverse order
        for (int i = modifiers.Length - 1; i >= 0; i--)
            NativeInput.SendKeyUp(modifiers[i]);
    }
}
