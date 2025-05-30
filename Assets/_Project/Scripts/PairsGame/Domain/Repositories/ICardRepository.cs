using PairsGame.Domain.Models;
using Cysharp.Threading.Tasks;

namespace PairsGame.Domain.Services
{
    public interface ICardRepository
    {
        UniTask<Card[]> GenerateCards(int gridSize);
    }
}