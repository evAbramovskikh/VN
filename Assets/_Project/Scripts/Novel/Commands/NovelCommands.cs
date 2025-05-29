using Naninovel;
using Naninovel.Commands;
using UniTask = Naninovel.UniTask;

[CommandAlias("addScore")]
public class AddScore : Command
{
    [RequiredParameter] public IntegerParameter Value;

    public override UniTask Execute(AsyncToken _)
    {
        Engine.GetService<ScoreService>().Add(Value);
        return UniTask.CompletedTask;
    }
}

[CommandAlias("checkScore")]
public class CheckScore : Command
{
    [RequiredParameter] public NamedStringParameter bad;
    [RequiredParameter] public NamedStringParameter good;
    [ParameterAlias("MinGood")] public IntegerParameter Threshold = 10;

    public override UniTask Execute(AsyncToken token = default)
    {
        var score = Engine.GetService<ScoreService>().Get();
        var target = score >= Threshold ? good : bad;

        return new Goto { Path = target }.Execute(token);
    }
}