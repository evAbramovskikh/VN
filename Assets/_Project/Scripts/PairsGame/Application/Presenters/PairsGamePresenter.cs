using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using VContainer.Unity;
using PairsGame.Domain.Models;
using PairsGame.Domain.Services;
using PairsGame.Infrastructure;
using PairsGame.UI.Views;

namespace PairsGame.Application.Presenters
{
    /// <summary>Данные для события «пара найдена».</summary>
    public readonly struct MatchFoundData
    {
        public MatchFoundData(Card first, Card second)
        {
            First  = first;
            Second = second;
        }

        public Card First  { get; }
        public Card Second { get; }
    }

    /// <summary>Данные для события «игра завершена».</summary>
    public readonly struct GameCompletedData
    {
        public GameCompletedData(int moves) => Moves = moves;
        public int Moves { get; }
    }

    /// <summary>
    /// Реактивный презентер мини-игры без использования dynamic.
    /// </summary>
    public sealed class PairsGamePresenter : IPairsGamePresenter, IInitializable, IDisposable
    {
        private readonly IGameService     _gameService;
        private readonly IPairsGameView   _gameView;
        private readonly GameSettings     _settings;
        private readonly CompositeDisposable _disposables = new();

        public PairsGamePresenter(
            IGameService   gameService,
            IPairsGameView gameView,
            GameSettings   settings)
        {
            _gameService = gameService;
            _gameView    = gameView;
            _settings    = settings;
        }
        
        /// <summary>
        /// IInitializable
        /// </summary>

        void IInitializable.Initialize() => Initialize().Forget();

        public async UniTask Initialize()
        {
            SubscribeCardClicks();
            SubscribeGameState();
            SubscribeGameEvents();

            await StartGame();
        }

        /// <summary>
        /// Public API
        /// </summary>

        public async UniTask StartGame()
        {
            _gameView.ShowLoading();
            await _gameService.StartNewGame();
            _gameView.HideLoading();
        }

        public async UniTask OnCardClicked(int cardIndex)
        {
            var state = _gameService.CurrentGameState.Value;
            if (state == null || cardIndex < 0 || cardIndex >= state.Cards.Count)
                return;

            await _gameService.TryFlipCard(state.Cards[cardIndex]);
        }

        /// <summary>
        /// Subscriptions
        /// </summary>

        private void SubscribeCardClicks()
        {
            _gameView.OnCardClicked
                     .ThrottleFirst(TimeSpan.FromMilliseconds(300))
                     .Subscribe(i => OnCardClicked(i).Forget())
                     .AddTo(_disposables);
        }

        private void SubscribeGameState()
        {
            _gameService.CurrentGameState
                        .Where(s => s != null)
                        .Subscribe(BindGameState)
                        .AddTo(_disposables);
        }

        private void SubscribeGameEvents()
        {
            _gameService.GameEvents
                        .Subscribe(HandleGameEvent)
                        .AddTo(_disposables);

            _gameService.GameEvents
                        .Where(e => e.Type == GameEventType.GameCompleted)
                        .Subscribe(_ => PairsGameBridge.NotifyGameCompleted())
                        .AddTo(_disposables);
        }

        private void HandleGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case GameEventType.GameStarted:
                    _gameView.ShowStartAnimation();
                    break;

                case GameEventType.CardFlipped:
                    _gameView.PlaySound(SoundType.Flip);
                    break;

                case GameEventType.MatchFound:
                    if (e.Data is MatchFoundData m)
                    {
                        _gameView.ShowMatchAnimation(m.First.Position, m.Second.Position);
                    }
                    _gameView.PlaySound(SoundType.Match);
                    break;

                case GameEventType.MismatchFound:
                    _gameView.PlaySound(SoundType.Mismatch);
                    break;

                case GameEventType.GameCompleted:
                    HandleGameCompleted(e.Data).Forget();
                    break;
            }
        }

        
        /// <summary>
        /// Bindings
        /// </summary>

        private void BindGameState(GameState state)
        {
            state.MovesCount
                 .Subscribe(_gameView.SetMovesCount)
                 .AddTo(_disposables);

            state.MatchesFound
                 .CombineLatest(Observable.Return(state.TotalPairs),
                                (m, t) => (m, t))
                 .Subscribe(x => _gameView.SetMatchesCount(x.m, x.t))
                 .AddTo(_disposables);

            state.IsProcessing
                 .Subscribe(p => _gameView.SetInteractionEnabled(!p))
                 .AddTo(_disposables);

            BindCards(state);
        }

        private void BindCards(GameState state)
        {
            for (int i = 0; i < state.Cards.Count; i++)
            {
                var card     = state.Cards[i];
                var cardView = _gameView.GetCardView(i);
                if (cardView == null) continue;

                cardView.SetCardImage(_settings.GetCardSprite(card.PairId));

                Observable.CombineLatest(card.IsFlipped,
                                         card.IsMatched,
                                         card.IsInteractable,
                                         (f, m, iable) => (f, m, iable))
                          .Subscribe(s =>
                          {
                              cardView.SetFlipped(s.f);
                              cardView.SetMatched(s.m);
                              cardView.SetInteractable(s.iable);
                          })
                          .AddTo(_disposables);
            }
        }
        
        /// <summary>
        /// Helpers
        /// </summary>

        private async UniTaskVoid HandleGameCompleted(object data)
        {
            var moves = data is GameCompletedData g ? g.Moves : 0;

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            _gameView.ShowGameCompleted(moves);
            _gameView.PlaySound(SoundType.Complete);

            // === отправляем сигнал, что игра окончена ===
            PairsGameBridge.NotifyGameCompleted();

            await UniTask.Delay(TimeSpan.FromSeconds(2));
        }
        
        /// <summary>
        /// IDisposable
        /// </summary>

        public void Dispose()
        {
            _disposables.Dispose();
            _gameService.Dispose();
        }
    }
    
    /// <summary>
    /// мост в Naninovel
    /// </summary>
    
    public static class PairsGameBridge
    {
        public static readonly Subject<Unit> GameCompleted = new();
        public static void NotifyGameCompleted()
            => GameCompleted.OnNext(Unit.Default);
    }

    public enum SoundType { Flip, Match, Mismatch, Complete }
}
