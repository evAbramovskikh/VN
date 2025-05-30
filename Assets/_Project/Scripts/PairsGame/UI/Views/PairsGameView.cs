using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UniRx;
using Cysharp.Threading.Tasks;
using PairsGame.Application.Presenters;
using PairsGame.Infrastructure;

namespace PairsGame.UI.Views
{
    /// <summary>
    /// Интерфейс представления игры
    /// </summary>
    public interface IPairsGameView
    {
        IObservable<int> OnCardClicked { get; }
        
        void ShowLoading();
        void HideLoading();
        void SetMovesCount(int moves);
        void SetMatchesCount(int matches, int total);
        void SetInteractionEnabled(bool enabled);
        void ShowGameCompleted(int moves);
        void ShowStartAnimation();
        void ShowMatchAnimation(int firstCardIndex, int secondCardIndex);
        void PlaySound(SoundType soundType);
        ICardView GetCardView(int index);
    }
    
    /// <summary>
    /// View игры с UI Toolkit
    /// </summary>
    public sealed class PairsGameView : MonoBehaviour, IPairsGameView
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private GameSettings _gameSettings;
        
        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        
        private VisualElement _root;
        private VisualElement _gameBoard;
        private Label _movesLabel;
        private Label _matchesLabel;
        private VisualElement _loadingOverlay;
        private VisualElement _completedOverlay;
        private VisualElement _progressFill;
        
        private readonly List<CardView> _cardViews = new();
        private readonly Subject<int> _cardClickSubject = new();
        
        public IObservable<int> OnCardClicked => _cardClickSubject;
        
        private void Awake()
        {
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            _root = _uiDocument.rootVisualElement;
            
            // Находим элементы UI
            _gameBoard = _root.Q<VisualElement>("game-board");
            _movesLabel = _root.Q<Label>("moves-count");
            _matchesLabel = _root.Q<Label>("matches-count");
            _loadingOverlay = _root.Q<VisualElement>("loading-overlay");
            _completedOverlay = _root.Q<VisualElement>("completed-overlay");
            _progressFill = _root.Q<VisualElement>("progress-fill");
            
            // Создаем карты
            CreateCardViews();
        }
        
        private void CreateCardViews()
        {
            const int gridSize = 4;
            const int totalCards = gridSize * gridSize;
            
            _gameBoard.Clear();
            _cardViews.Clear();
            
            for (int i = 0; i < totalCards; i++)
            {
                var cardView = Instantiate(_cardPrefab);
                
                // Инициализируем карту со спрайтом рубашки из настроек
                if (_gameSettings != null && _gameSettings.CardBackSprite != null)
                {
                    cardView.Initialize(_gameSettings.CardBackSprite);
                }
                
                var cardElement = cardView.CreateVisualElement();
                
                int index = i; // Захват переменной для замыкания
                cardElement.RegisterCallback<ClickEvent>(_ => OnCardClick(index));
                
                // Добавляем класс для анимации появления
                cardElement.AddToClassList($"card-{i}");
                cardElement.AddToClassList("card-appear");
                
                _gameBoard.Add(cardElement);
                _cardViews.Add(cardView);
            }
        }
        
        private void OnCardClick(int index)
        {
            _cardClickSubject.OnNext(index);
        }
        
        public void ShowLoading()
        {
            _loadingOverlay.style.display = DisplayStyle.Flex;
            _loadingOverlay.AddToClassList("fade-in");
        }
        
        public void HideLoading()
        {
            _loadingOverlay.AddToClassList("fade-out");
            _loadingOverlay.schedule.Execute(() => 
            {
                _loadingOverlay.style.display = DisplayStyle.None;
                _loadingOverlay.RemoveFromClassList("fade-in");
                _loadingOverlay.RemoveFromClassList("fade-out");
            }).StartingIn(300);
        }
        
        public void SetMovesCount(int moves)
        {
            _movesLabel.text = $"Ходов: {moves}";
            
            // Небольшая анимация при изменении
            _movesLabel.AddToClassList("pulse");
            _movesLabel.schedule.Execute(() => 
            {
                _movesLabel.RemoveFromClassList("pulse");
            }).StartingIn(300);
        }
        
        public void SetMatchesCount(int matches, int total)
        {
            _matchesLabel.text = $"Найдено пар: {matches}/{total}";
            
            // Обновляем прогресс бар
            if (_progressFill != null)
            {
                var progress = total > 0 ? (float)matches / total : 0f;
                _progressFill.style.width = Length.Percent(progress * 100);
            }
        }
        
        public void SetInteractionEnabled(bool enabled)
        {
            _gameBoard.pickingMode = enabled ? PickingMode.Position : PickingMode.Ignore;
        }
        
        public void ShowGameCompleted(int moves)
        {
            _completedOverlay.style.display = DisplayStyle.Flex;
            
            var completedMovesLabel = _completedOverlay.Q<Label>("completed-moves");
            if (completedMovesLabel != null)
            {
                completedMovesLabel.text = $"Завершено за {moves} ходов!";
            }
            
            // Анимация появления
            _completedOverlay.style.opacity = 0;
            _completedOverlay.schedule.Execute(() =>
            {
                _completedOverlay.AddToClassList("fade-in");
            }).StartingIn(100);
        }
        
        public void ShowStartAnimation()
        {
            // Анимация появления карт волной
            for (int i = 0; i < _cardViews.Count; i++)
            {
                var delay = i * 50; // мс между картами
                var cardElement = _gameBoard.Q(null, $"card-{i}");
                
                if (cardElement != null)
                {
                    cardElement.schedule.Execute(() =>
                    {
                        cardElement.AddToClassList("show");
                    }).StartingIn(delay);
                }
            }
        }
        
        public void ShowMatchAnimation(int firstCardIndex, int secondCardIndex)
        {
            // Анимация совпадения уже реализована в CardView
            // Здесь в будущем можно будет добавить эффекты дополнительные, может пригодится
            var firstCard = GetCardView(firstCardIndex);
            var secondCard = GetCardView(secondCardIndex);
        }
        
        public void PlaySound(SoundType soundType)
        {
            if (_audioSource == null || _gameSettings == null) return;
            
            AudioClip clip = soundType switch
            {
                SoundType.Flip => _gameSettings.FlipSound,
                SoundType.Match => _gameSettings.MatchSound,
                SoundType.Mismatch => _gameSettings.MismatchSound,
                SoundType.Complete => _gameSettings.CompletionSound,
                _ => null
            };
            
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
        
        public ICardView GetCardView(int index)
        {
            return index >= 0 && index < _cardViews.Count ? _cardViews[index] : null;
        }
        
        private void OnDestroy()
        {
            _cardClickSubject?.Dispose();
        }
    }
}