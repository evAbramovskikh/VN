using System.Collections.Generic;
using PairsGame.Domain.Models;
using PairsGame.Domain.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PairsGame.Infrastructure
{
    /// <summary>
    /// Репозиторий карт - отвечает за генерацию и перемешивание карт
    /// </summary>
    public sealed class CardRepository : ICardRepository
    {
        private readonly CardFactory _cardFactory;
        private readonly GameSettings _settings;
        
        public CardRepository(CardFactory cardFactory, GameSettings settings)
        {
            _cardFactory = cardFactory;
            _settings = settings;
        }
        
        public async UniTask<Card[]> GenerateCards(int gridSize)
        {
            var totalCards = gridSize * gridSize;
            var pairsCount = totalCards / 2;
            
            var cards = new Card[totalCards];
            var pairIds = GenerateShuffledPairIds(pairsCount);
            
            // Создаем карты
            for (int i = 0; i < totalCards; i++)
            {
                cards[i] = _cardFactory.CreateCard(i, pairIds[i]);
            }
            
            // Имитация асинхронной загрузки (например, загрузка спрайтов)
            await UniTask.Delay(100);
            
            return cards;
        }
        
        private List<int> GenerateShuffledPairIds(int pairsCount)
        {
            var pairIds = new List<int>(pairsCount * 2);
            
            // Создаем пары ID
            for (int i = 0; i < pairsCount; i++)
            {
                pairIds.Add(i);
                pairIds.Add(i);
            }
            
            // Перемешиваем используя Fisher-Yates shuffle
            for (int i = pairIds.Count - 1; i > 0; i--)
            {
                var randomIndex = Random.Range(0, i + 1);
                (pairIds[i], pairIds[randomIndex]) = (pairIds[randomIndex], pairIds[i]);
            }
            
            return pairIds;
        }
    }
}