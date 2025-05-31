using System;
using System.Collections.Generic;
using PairsGame.Domain.Models;
using PairsGame.Domain.Commands;
using UniRx;
using Cysharp.Threading.Tasks;
using PairsGame.Application.Presenters;

namespace PairsGame.Domain.Services
{
    /// <summary>
    /// События игры для интеграции с Naninovel
    /// </summary>
    public enum GameEventType
    {
        GameStarted,
        CardFlipped,
        MatchFound,
        MismatchFound,
        GameCompleted,
        MoveUndone
    }
    
    public class GameEvent
    {
        public GameEventType Type { get; }
        public object Data { get; }
        
        public GameEvent(GameEventType type, object data = null)
        {
            Type = type;
            Data = data;
        }
    }
    
    /// <summary>
    /// Реализация игрового сервиса с паттерном Command
    /// </summary>
    public sealed class GameService : IGameService, IDisposable
    {
        private readonly ICardRepository _cardRepository;
        private readonly ReactiveProperty<GameState> _gameState;
        private readonly Subject<GameEvent> _gameEvents;
        private readonly Stack<IGameCommand> _commandHistory;
        private readonly CompositeDisposable _disposables;
        
        public IReadOnlyReactiveProperty<GameState> CurrentGameState => _gameState;
        public IObservable<GameEvent> GameEvents => _gameEvents;
        
        public GameService(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
            _gameState = new ReactiveProperty<GameState>();
            _gameEvents = new Subject<GameEvent>();
            _commandHistory = new Stack<IGameCommand>();
            _disposables = new CompositeDisposable();
        }
        
        public async UniTask StartNewGame(int gridSize = 4)
        {
            // Очищаем историю команд
            _commandHistory.Clear();
            
            // Получаем карты из репозитория
            var cards = await _cardRepository.GenerateCards(gridSize);
            
            // Создаем новое состояние игры
            var newState = new GameState(cards, gridSize);
            _gameState.Value = newState;
            
            // Подписываемся на изменения состояния
            SetupGameStateSubscriptions(newState);
            
            // Отправляем событие начала игры
            _gameEvents.OnNext(new GameEvent(GameEventType.GameStarted, gridSize));
        }
        
        public async UniTask<bool> TryFlipCard(Card card)
        {
            var state = _gameState.Value;
            if (state == null || state.IsProcessing.Value || !card.IsInteractable.Value)
                return false;
            
            // Создаем и выполняем команду
            var command = new FlipCardCommand(card, state);
            await ExecuteCommand(command);
            
            // Отправляем событие
            _gameEvents.OnNext(new GameEvent(GameEventType.CardFlipped, card));
            
            // Если открыты 2 карты, проверяем совпадение
            if (state.FlippedCards.Count == 2)
            {
                state.IsProcessing.Value = true;
                await CheckForMatch();
            }
            
            return true;
        }
        
        public async UniTask CheckForMatch()
        {
            var state = _gameState.Value;
            if (state.FlippedCards.Count != 2) return;
            
            var firstCard = state.FlippedCards[0];
            var secondCard = state.FlippedCards[1];
            
            // Даем время увидеть обе карты
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            
            if (firstCard.PairId == secondCard.PairId)
            {
                // Найдена пара!
                var command = new MatchCardsCommand(firstCard, secondCard, state);
                await ExecuteCommand(command);
                
                _gameEvents.OnNext(new GameEvent(GameEventType.MatchFound,
                    new MatchFoundData(firstCard, secondCard)));
            }
            else
            {
                // Не совпало - переворачиваем обратно
                var command = new HideCardsCommand(firstCard, secondCard);
                await ExecuteCommand(command);
                
                _gameEvents.OnNext(new GameEvent(GameEventType.MismatchFound));
            }
            
            state.FlippedCards.Clear();
            state.IsProcessing.Value = false;
        }
        
        public async UniTask UndoLastMove()
        {
            if (_commandHistory.Count == 0) return;
            
            var command = _commandHistory.Pop();
            await command.Undo();
            
            _gameEvents.OnNext(new GameEvent(GameEventType.MoveUndone));
        }
        
        private async UniTask ExecuteCommand(IGameCommand command)
        {
            await command.Execute();
            _commandHistory.Push(command);
        }
        
        private void SetupGameStateSubscriptions(GameState state)
        {
            // Очищаем предыдущие подписки
            _disposables.Clear();
            
            // Подписываемся на завершение игры
            state.IsGameCompleted
                .Where(completed => completed)
                .Subscribe(_ => 
                {
                    _gameEvents.OnNext(new GameEvent(GameEventType.GameCompleted,
                        new GameCompletedData(state.MovesCount.Value)));
                })
                .AddTo(_disposables);
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _gameState?.Dispose();
            _gameEvents?.Dispose();
        }
    }
}