
using System;
using System.Linq;
using PairsGame.Domain.Models;
using PairsGame.Domain.Services;
using PairsGame.Infrastructure;
using PairsGame.UI.Views;
using UniRx;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace PairsGame.Application.Presenters
{
    /// <summary>
    /// Презентер игры с реактивным подходом
    /// </summary>
    public sealed class PairsGamePresenter : IPairsGamePresenter, IInitializable
    {
        private readonly IGameService _gameService;
        private readonly IPairsGameView _gameView;
        private readonly GameSettings _gameSettings;
        private readonly CompositeDisposable _disposables;
        
        public PairsGamePresenter(
            IGameService gameService,
            IPairsGameView gameView,
            GameSettings gameSettings)
        {
            _gameService = gameService;
            _gameView = gameView;
            _gameSettings = gameSettings;
            _disposables = new CompositeDisposable();
        }
        
        void IInitializable.Initialize()
        {
            Initialize().Forget();
        }
        
        public async UniTask Initialize()
        {
            // Подписываемся на клики по картам с throttle для предотвращения двойных кликов
            _gameView.OnCardClicked
                .ThrottleFirst(TimeSpan.FromMilliseconds(300))
                .Subscribe(index => OnCardClicked(index).Forget())
                .AddTo(_disposables);
            
            // Подписываемся на изменения состояния игры
            _gameService.CurrentGameState
                .Where(state => state != null)
                .Subscribe(BindGameState)
                .AddTo(_disposables);
            
            // Подписываемся на игровые события
            SetupEventSubscriptions();
            
            // Начинаем новую игру
            await StartGame();
        }
        
        private void SetupEventSubscriptions()
        {
            // Реагируем на различные игровые события
            _gameService.GameEvents
                .Subscribe(gameEvent =>
                {
                    switch (gameEvent.Type)
                    {
                        case GameEventType.GameStarted:
                            _gameView.ShowStartAnimation();
                            break;
                            
                        case GameEventType.MatchFound:
                            if (gameEvent.Data is { } matchData)
                            {
                                var data = (dynamic)matchData;
                                _gameView.ShowMatchAnimation(data.First.Position, data.Second.Position);
                                _gameView.PlaySound(SoundType.Match);
                            }
                            break;
                            
                        case GameEventType.MismatchFound:
                            _gameView.PlaySound(SoundType.Mismatch);
                            break;
                            
                        case GameEventType.CardFlipped:
                            _gameView.PlaySound(SoundType.Flip);
                            break;
                            
                        case GameEventType.GameCompleted:
                            OnGameCompleted(gameEvent.Data).Forget();
                            break;
                    }
                })
                .AddTo(_disposables);
            
            // Интеграция с Naninovel через глобальные события
            _gameService.GameEvents
                .Where(e => e.Type == GameEventType.GameCompleted)
                .Subscribe(_ => PairsGameBridge.NotifyGameCompleted())
                .AddTo(_disposables);
        }
        
        public async UniTask StartGame()
        {
            _gameView.ShowLoading();
            
            await _gameService.StartNewGame();
            
            _gameView.HideLoading();
        }
        
        public async UniTask OnCardClicked(int cardIndex)
        {
            var state = _gameService.CurrentGameState.Value;
            if (state == null || cardIndex >= state.Cards.Count) return;
            
            var card = state.Cards[cardIndex];
            await _gameService.TryFlipCard(card);
        }
        
        private void BindGameState(GameState state)
        {
            // Реактивно обновляем UI
            // Счетчик ходов
            state.MovesCount
                .Subscribe(moves => _gameView.SetMovesCount(moves))
                .AddTo(_disposables);
            
            // Счетчик найденных пар
            state.MatchesFound
                .CombineLatest(
                    Observable.Return(state.TotalPairs),
                    (matches, total) => new { matches, total })
                .Subscribe(data => _gameView.SetMatchesCount(data.matches, data.total))
                .AddTo(_disposables);
            
            // Блокировка UI во время обработки
            state.IsProcessing
                .Subscribe(processing => _gameView.SetInteractionEnabled(!processing))
                .AddTo(_disposables);
            
            // Обновляем состояние каждой карты
            BindCardViews(state);
        }
        
        private void BindCardViews(GameState state)
        {
            for (int i = 0; i < state.Cards.Count; i++)
            {
                var card = state.Cards[i];
                var cardView = _gameView.GetCardView(i);
                
                if (cardView != null)
                {
                    // Устанавливаем спрайт для лицевой стороны карты
                    var frontSprite = _gameSettings.GetCardSprite(card.PairId);
                    cardView.SetCardImage(frontSprite);
                    
                    // Реактивная подписка на все изменения карты
                    Observable.CombineLatest(
                        card.IsFlipped,
                        card.IsMatched,
                        card.IsInteractable,
                        (flipped, matched, interactable) => new { flipped, matched, interactable })
                        .Subscribe(cardState =>
                        {
                            cardView.SetFlipped(cardState.flipped);
                            cardView.SetMatched(cardState.matched);
                            cardView.SetInteractable(cardState.interactable);
                        })
                        .AddTo(_disposables);
                }
            }
        }
        
        private async UniTaskVoid OnGameCompleted(object data)
        {
            var completionData = (dynamic)data;
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            
            _gameView.ShowGameCompleted(completionData.Moves);
            _gameView.PlaySound(SoundType.Complete);
            
            // Ждем 2 секунды перед возвратом
            await UniTask.Delay(TimeSpan.FromSeconds(2));
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _gameService?.Dispose();
        }
    }
    
    /// <summary>
    /// Мост для коммуникации с Naninovel
    /// </summary>
    public static class PairsGameBridge
    {
        public static readonly Subject<Unit> GameCompleted = new();
        
        public static void NotifyGameCompleted()
        {
            GameCompleted.OnNext(Unit.Default);
        }
    }
    
    public enum SoundType
    {
        Flip,
        Match,
        Mismatch,
        Complete
    }
}