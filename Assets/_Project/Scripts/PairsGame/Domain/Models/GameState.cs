using System;
using System.Collections.Generic;
using UniRx;

namespace PairsGame.Domain.Models
{
    /// <summary>
    /// Состояние игры
    /// </summary>
    public sealed class GameState
    {
        // Реактивные свойства для отслеживания состояния игры
        public IReactiveProperty<int> MovesCount { get; }
        public IReactiveProperty<int> MatchesFound { get; }
        public IReactiveProperty<bool> IsProcessing { get; }
        public IReactiveProperty<bool> IsGameCompleted { get; }
        
        // Карты на поле
        public IReadOnlyList<Card> Cards { get; }
        
        // Текущие открытые карты
        public ReactiveCollection<Card> FlippedCards { get; }
        
        // Настройки игры
        public int TotalPairs { get; }
        public int GridSize { get; }
        
        public GameState(IReadOnlyList<Card> cards, int gridSize = 4)
        {
            Cards = cards;
            GridSize = gridSize;
            TotalPairs = cards.Count / 2;
            
            MovesCount = new ReactiveProperty<int>(0);
            MatchesFound = new ReactiveProperty<int>(0);
            IsProcessing = new ReactiveProperty<bool>(false);
            IsGameCompleted = new ReactiveProperty<bool>(false);
            
            FlippedCards = new ReactiveCollection<Card>();
            
            MatchesFound
                .Where(matches => matches == TotalPairs)
                .Subscribe(_ => IsGameCompleted.Value = true);
        }
        
        public void Reset()
        {
            MovesCount.Value = 0;
            MatchesFound.Value = 0;
            IsProcessing.Value = false;
            IsGameCompleted.Value = false;
            FlippedCards.Clear();
            
            foreach (var card in Cards)
            {
                card.IsFlipped.Value = false;
                card.IsMatched.Value = false;
            }
        }
    }
}