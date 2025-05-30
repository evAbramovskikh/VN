using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UniRx;
using Cysharp.Threading.Tasks;
using PairsGame.Domain.Models;
using PairsGame.Application.Presenters;

namespace PairsGame.UI.Views
{
    /// <summary>
    /// Интерфейс представления игры с расширенными возможностями
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
        void ShowMatchAnimation(Card first, Card second);
        void PlaySound(SoundType soundType);
        ICardView GetCardView(int index);
    }
    
    /// <summary>
    /// View игры с продвинутыми анимациями UI Toolkit
    /// </summary>
    public sealed class PairsGameView : MonoBehaviour, IPairsGameView
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private StyleSheet _styleSheet;
        [SerializeField] private CardView _cardPrefab;
        
        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _flipSound;
        [SerializeField] private AudioClip _matchSound;
        [SerializeField] private AudioClip _mismatchSound;
        [SerializeField] private AudioClip _completeSound;
        
        private VisualElement _root;
        private VisualElement _gameBoard;
        private Label _movesLabel;
        private Label _matchesLabel;
        private VisualElement _loadingOverlay;
        private VisualElement _completedOverlay;
        
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
                var cardElement = cardView.CreateVisualElement();
                
                int index = i; // Захват переменной для замыкания
                cardElement.RegisterCallback<ClickEvent>(_ => OnCardClick(index));
                
                // Добавляем индекс для анимаций
                cardElement.AddToClassList($"card-{i}");
                
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
            // Анимация изменения счетчика
            _movesLabel.AddToClassList("pulse");
            _movesLabel.text = $"Ходов: {moves}";
            
            _movesLabel.schedule.Execute(() => 
            {
                _movesLabel.RemoveFromClassList("pulse");
            }).StartingIn(300);
        }
        
        public void SetMatchesCount(int matches, int total)
        {
            _matchesLabel.text = $"Найдено пар: {matches}/{total}";
            
            // Анимация прогресса
            var progress = (float)matches / total;
            _root.style.SetValue(new StyleFloat("--progress", progress));
        }
        
        public void SetInteractionEnabled(bool enabled)
        {
            _gameBoard.pickingMode = enabled ? PickingMode.Position : PickingMode.Ignore;
        }
        
        public void ShowGameCompleted(int moves)
        {
            _completedOverlay.style.display = DisplayStyle.Flex;
            
            var completedLabel = _completedOverlay.Q<Label>("completed-moves");
            if (completedLabel != null)
            {
                completedLabel.text = $"Завершено за {moves} ходов!";
            }
            
            // Сложная анимация появления
            _completedOverlay.style.opacity = 0;
            _completedOverlay.style.scale = new Scale(new Vector3(0.8f, 0.8f, 1));
            
            _completedOverlay.schedule.Execute(() =>
            {
                _completedOverlay.AddToClassList("show-completed");
                _completedOverlay.style.opacity = 1;
                _completedOverlay.style.scale = new Scale(Vector3.one);
            }).StartingIn(100);
            
            // Анимация конфетти
            ShowConfettiAnimation();
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
                    cardElement.style.opacity = 0;
                    cardElement.style.translate = new Translate(0, 20, 0);
                    
                    cardElement.schedule.Execute(() =>
                    {
                        cardElement.AddToClassList("card-appear");
                        cardElement.style.opacity = 1;
                        cardElement.style.translate = new Translate(0, 0, 0);
                    }).StartingIn(delay);
                }
            }
        }
        
        public void ShowMatchAnimation(Card first, Card second)
        {
            // Анимация успешного совпадения
            var firstView = GetCardView(first.Position);
            var secondView = GetCardView(second.Position);
            
            if (firstView != null && secondView != null)
            {
                // Добавляем визуальные эффекты
                firstView.ShowMatchEffect();
                secondView.ShowMatchEffect();
            }
        }
        
        public void PlaySound(SoundType soundType)
        {
            if (_audioSource == null) return;
            
            AudioClip clip = soundType switch
            {
                SoundType.Flip => _flipSound,
                SoundType.Match => _matchSound,
                SoundType.Mismatch => _mismatchSound,
                SoundType.Complete => _completeSound,
                _ => null
            };
            
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
        
        private void ShowConfettiAnimation()
        {
            // Создаем частицы конфетти
            var confettiContainer = new VisualElement();
            confettiContainer.AddToClassList("confetti-container");
            _root.Add(confettiContainer);
            
            for (int i = 0; i < 50; i++)
            {
                var confetti = new VisualElement();
                confetti.AddToClassList("confetti");
                confetti.style.left = UnityEngine.Random.Range(0, 100) + "%";
                confetti.style.backgroundColor = GetRandomColor();
                
                confettiContainer.Add(confetti);
            }
            
            // Удаляем конфетти через 3 секунды
            confettiContainer.schedule.Execute(() =>
            {
                confettiContainer.RemoveFromHierarchy();
            }).StartingIn(3000);
        }
        
        private Color GetRandomColor()
        {
            var colors = new[] 
            { 
                new Color(0.9f, 0.3f, 0.3f),
                new Color(0.3f, 0.9f, 0.3f),
                new Color(0.3f, 0.3f, 0.9f),
                new Color(0.9f, 0.9f, 0.3f),
                new Color(0.9f, 0.3f, 0.9f)
            };
            return colors[UnityEngine.Random.Range(0, colors.Length)];
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