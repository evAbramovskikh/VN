using PairsGame.Application.Presenters;
using PairsGame.Domain.Services;
using PairsGame.Infrastructure;
using PairsGame.UI.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DI
{
    /// <summary>
    /// DI конфигурация для мини-игры
    /// </summary>
    public sealed class PairsGameLifetimeScope : LifetimeScope
    {
        [SerializeField] private PairsGameView _gameViewPrefab;
        [SerializeField] private GameSettings _gameSettings;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // Регистрируем настройки
            builder.RegisterInstance(_gameSettings).As<GameSettings>();
            
            // Domain Layer
            builder.Register<ICardRepository, CardRepository>(Lifetime.Scoped);
            builder.Register<IGameService, GameService>(Lifetime.Scoped);
            
            // Infrastructure
            builder.Register<CardFactory>(Lifetime.Scoped);
            
            // Presentation Layer
            builder.RegisterComponentInNewPrefab(_gameViewPrefab, Lifetime.Scoped)
                .As<IPairsGameView>();
            
            builder.Register<IPairsGamePresenter, PairsGamePresenter>(Lifetime.Scoped);
            
            // Entry Point
            builder.RegisterEntryPoint<PairsGamePresenter>();
        }
    }
}