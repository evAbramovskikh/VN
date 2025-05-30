using System;
using UniRx;

namespace PairsGame.Domain.Models
{
    /// <summary>
    /// Модель карты в игре
    /// </summary>
    public sealed class Card
    {
        public string Id { get; }
        public int PairId { get; }
        public int Position { get; }
        
        // Реактивные свойства для отслеживания состояния
        public IReactiveProperty<bool> IsFlipped { get; }
        public IReactiveProperty<bool> IsMatched { get; }
        public IReadOnlyReactiveProperty<bool> IsInteractable { get; }
        
        public Card(string id, int pairId, int position)
        {
            Id = id;
            PairId = pairId;
            Position = position;
            
            IsFlipped = new ReactiveProperty<bool>(false);
            IsMatched = new ReactiveProperty<bool>(false);
            
            IsInteractable = IsFlipped
                .CombineLatest(IsMatched, (flipped, matched) => !flipped && !matched)
                .ToReadOnlyReactiveProperty();
        }
        
        public void Flip() => IsFlipped.Value = true;
        public void Hide() => IsFlipped.Value = false;
        public void MarkAsMatched() => IsMatched.Value = true;
    }
}