using Naninovel;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ScoreHUD : MonoBehaviour
{
    private Label _label;
    private readonly CompositeDisposable _disposables = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var ui = GetComponent<UIDocument>();
        _label = ui.rootVisualElement.Q<Label>("score-label");

        if (Engine.Initialized)
            Bind();
        else
            Engine.OnInitializationFinished += HandleEngineInitialized;
    }

    private void HandleEngineInitialized ()
    {
        Engine.OnInitializationFinished -= HandleEngineInitialized;
        Bind();
    }

    private void Bind ()
    {
        var scoreService = Engine.GetService<ScoreService>();

        UpdateUI(scoreService.Get());

        scoreService.Current
            .Subscribe(UpdateUI)
            .AddTo(_disposables);
    }

    private void UpdateUI (int value)
    {
        _label.text = $"Очки: {value}";
    }

    private void OnDestroy ()
    {
        _disposables.Dispose();
        Engine.OnInitializationFinished -= HandleEngineInitialized;
    }
}