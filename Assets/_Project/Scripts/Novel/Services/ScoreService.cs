using System;
using Naninovel;
using UniRx;
using UniTask = Naninovel.UniTask; 

[InitializeAtRuntime]
public sealed class ScoreService : IEngineService, IStatefulService<GameStateMap>
{
    public readonly ReactiveProperty<int> Current = new(0);

    public UniTask InitializeService()
    {
        return UniTask.CompletedTask;
    }

    public void ResetService()
    {
        Current.Value = 0;
    }

    public void DestroyService()
    {
    }

    public void SaveServiceState(GameStateMap map)
    {
        map.SetState(new ScoreState { Value = Current.Value });
    }

    public UniTask LoadServiceState(GameStateMap map)
    {
        var snap = map.GetState<ScoreState>();
        if (snap != null) Current.Value = snap.Value;
        return UniTask.CompletedTask;
    }

    public void Add(int v)
    {
        Current.Value += v;
    }

    public int Get()
    {
        return Current.Value;
    }
}

[Serializable]
internal class ScoreState
{
    public int Value;
}