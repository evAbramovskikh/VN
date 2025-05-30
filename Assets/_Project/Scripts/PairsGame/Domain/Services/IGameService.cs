using System;
using PairsGame.Domain.Models;
using UniRx;
using Cysharp.Threading.Tasks;

namespace PairsGame.Domain.Services
{
    /// <summary>
    /// Интерфейс игрового сервиса
    /// </summary>
    public interface IGameService
    {
        IReadOnlyReactiveProperty<GameState> CurrentGameState { get; }
        IObservable<GameEvent> GameEvents { get; }
        
        UniTask StartNewGame(int gridSize = 4);
        UniTask<bool> TryFlipCard(Card card);
        UniTask CheckForMatch();
        UniTask UndoLastMove();
        void Dispose();
    }
}