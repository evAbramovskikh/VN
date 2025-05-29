using System;
using Naninovel;

/// <summary>
/// Временно имитирует мини-игру: просто ждёт 5 секунд и возвращается к сценарию.
/// </summary>
[CommandAlias("startPairs")]
public sealed class StartPairs : Command
{
    public override async Naninovel.UniTask Execute (AsyncToken token = default)
    {
        // ждём 5 секунд, поддерживая отмену (Esc, переход к save/load и т.д.)
        await Naninovel.UniTask.Delay(TimeSpan.FromSeconds(5), cancellationToken: token.CancellationToken);
    }
}