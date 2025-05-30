using System;
using Cysharp.Threading.Tasks;

namespace PairsGame.Application.Presenters
{
    /// <summary>
    /// Интерфейс презентера игры
    /// </summary>
    public interface IPairsGamePresenter : IDisposable
    {
        UniTask Initialize();
        UniTask StartGame();
        UniTask OnCardClicked(int cardIndex);
    }
}