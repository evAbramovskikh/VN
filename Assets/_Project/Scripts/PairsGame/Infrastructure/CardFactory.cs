using PairsGame.Domain.Models;

namespace PairsGame.Infrastructure
{
    /// <summary>
    /// Фабрика для создания карт
    /// </summary>
    public sealed class CardFactory
    {
        public Card CreateCard(int position, int pairId)
        {
            var id = $"card_{position}";
            return new Card(id, pairId, position);
        }
    }
}