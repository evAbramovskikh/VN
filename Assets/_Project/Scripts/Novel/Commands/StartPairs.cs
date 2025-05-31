using Naninovel;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using PairsGame.Application.Presenters;
using NanoUniTask = Naninovel.UniTask;
using UniRx;

/// <summary>
/// Загружает сцену мини-игры, ждёт победы, выгружает сцену.
/// Вызывается из сценария:  @startPairs
/// </summary>
[CommandAlias("startPairs")]
public sealed class StartPairs : Command
{
    public override async NanoUniTask Execute (AsyncToken token = default)
    {
        // 1. Загружаем сцену
        await SceneManager
            .LoadSceneAsync("PairsGame", LoadSceneMode.Additive)
            .ToUniTask(cancellationToken: token.CancellationToken);

        // 2. Ждём событие победы
        await PairsGameBridge.GameCompleted
            .First()
            .ToUniTask(cancellationToken: token.CancellationToken);

        // 3. Выгружаем сцену
        await SceneManager
            .UnloadSceneAsync("PairsGame")
            .ToUniTask(cancellationToken: token.CancellationToken);
    }
}