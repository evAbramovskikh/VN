using System;
using PairsGame.Domain.Models;
using Cysharp.Threading.Tasks;

namespace PairsGame.Domain.Commands
{
    /// <summary>
    /// Интерфейс команды (паттерн Command)
    /// </summary>
    public interface IGameCommand
    {
        UniTask Execute();
        UniTask Undo();
    }
    
    /// <summary>
    /// Команда переворота карты
    /// </summary>
    public sealed class FlipCardCommand : IGameCommand
    {
        private readonly Card _card;
        private readonly GameState _gameState;
        private bool _wasExecuted;
        
        public FlipCardCommand(Card card, GameState gameState)
        {
            _card = card;
            _gameState = gameState;
        }
        
        public UniTask Execute()
        {
            if (!_wasExecuted)
            {
                _card.Flip();
                _gameState.FlippedCards.Add(_card);
                _gameState.MovesCount.Value++;
                _wasExecuted = true;
            }
            return UniTask.CompletedTask;
        }
        
        public UniTask Undo()
        {
            if (_wasExecuted)
            {
                _card.Hide();
                _gameState.FlippedCards.Remove(_card);
                _gameState.MovesCount.Value--;
                _wasExecuted = false;
            }
            return UniTask.CompletedTask;
        }
    }
    
    /// <summary>
    /// Команда для пары найденных карт
    /// </summary>
    public sealed class MatchCardsCommand : IGameCommand
    {
        private readonly Card _firstCard;
        private readonly Card _secondCard;
        private readonly GameState _gameState;
        private bool _wasExecuted;
        
        public MatchCardsCommand(Card firstCard, Card secondCard, GameState gameState)
        {
            _firstCard = firstCard;
            _secondCard = secondCard;
            _gameState = gameState;
        }
        
        public UniTask Execute()
        {
            if (!_wasExecuted)
            {
                _firstCard.MarkAsMatched();
                _secondCard.MarkAsMatched();
                _gameState.MatchesFound.Value++;
                _wasExecuted = true;
            }
            return UniTask.CompletedTask;
        }
        
        public UniTask Undo()
        {
            if (_wasExecuted)
            {
                _firstCard.IsMatched.Value = false;
                _secondCard.IsMatched.Value = false;
                _firstCard.Hide();
                _secondCard.Hide();
                _gameState.MatchesFound.Value--;
                _wasExecuted = false;
            }
            return UniTask.CompletedTask;
        }
    }
    
    /// <summary>
    /// Команда скрытия карт при несовпадении
    /// </summary>
    public sealed class HideCardsCommand : IGameCommand
    {
        private readonly Card _firstCard;
        private readonly Card _secondCard;
        
        public HideCardsCommand(Card firstCard, Card secondCard)
        {
            _firstCard = firstCard;
            _secondCard = secondCard;
        }
        
        public UniTask Execute()
        {
            _firstCard.Hide();
            _secondCard.Hide();
            return UniTask.CompletedTask;
        }
        
        public UniTask Undo()
        {
            _firstCard.Flip();
            _secondCard.Flip();
            return UniTask.CompletedTask;
        }
    }
}