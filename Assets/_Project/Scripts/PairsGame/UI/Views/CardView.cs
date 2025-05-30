using System;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

namespace PairsGame.UI.Views
{
    /// <summary>
    /// Интерфейс представления карты
    /// </summary>
    public interface ICardView
    {
        void SetFlipped(bool flipped);
        void SetMatched(bool matched);
        void SetInteractable(bool interactable);
        void SetCardImage(Sprite sprite);
        VisualElement CreateVisualElement();
    }
    
    /// <summary>
    /// View карты с простыми анимациями
    /// </summary>
    public class CardView : MonoBehaviour, ICardView
    {
        [SerializeField] private float _flipDuration = 0.3f;
        
        private VisualElement _cardElement;
        private VisualElement _cardImage;
        private Sprite _backSprite;
        private Sprite _frontSprite;
        private bool _isFlipped;
        private bool _isMatched;
        
        // Инжектируем спрайт рубашки через настройки
        public void Initialize(Sprite backSprite)
        {
            _backSprite = backSprite;
        }
        
        public VisualElement CreateVisualElement()
        {
            _cardElement = new VisualElement();
            _cardElement.AddToClassList("card");
            
            _cardImage = new VisualElement();
            _cardImage.AddToClassList("card-face");
            _cardImage.style.position = Position.Absolute;
            
            // Изначально показываем рубашку
            if (_backSprite != null)
            {
                _cardImage.style.backgroundImage = new StyleBackground(_backSprite);
            }
            
            _cardElement.Add(_cardImage);
            
            return _cardElement;
        }
        
        public void SetFlipped(bool flipped)
        {
            if (_isFlipped == flipped) return;
            _isFlipped = flipped;
            
            AnimateFlip(flipped).Forget();
        }
        
        private async UniTaskVoid AnimateFlip(bool flipped)
        {
            // Анимация сжатия
            _cardElement.AddToClassList("card-flipping");
            
            // Ждем половину анимации
            await UniTask.Delay(TimeSpan.FromSeconds(_flipDuration / 2));
            
            // Меняем спрайт
            if (flipped && _frontSprite != null)
            {
                _cardImage.style.backgroundImage = new StyleBackground(_frontSprite);
            }
            else if (!flipped && _backSprite != null)
            {
                _cardImage.style.backgroundImage = new StyleBackground(_backSprite);
            }
            
            // Ждем окончания анимации
            await UniTask.Delay(TimeSpan.FromSeconds(_flipDuration / 2));
            
            _cardElement.RemoveFromClassList("card-flipping");
        }
        
        public void SetMatched(bool matched)
        {
            if (_isMatched == matched) return;
            _isMatched = matched;
            
            if (matched)
            {
                _cardElement.AddToClassList("matched");
                
                // Анимация успешного совпадения
                ShowMatchEffect();
            }
            else
            {
                _cardElement.RemoveFromClassList("matched");
            }
        }
        
        private void ShowMatchEffect()
        {
            // Эффект пульсации
            _cardElement.AddToClassList("pulse");
            
            _cardElement.schedule.Execute(() =>
            {
                _cardElement.RemoveFromClassList("pulse");
            }).StartingIn(300);
        }
        
        public void SetInteractable(bool interactable)
        {
            _cardElement.pickingMode = interactable ? PickingMode.Position : PickingMode.Ignore;
            
            if (interactable)
            {
                _cardElement.RemoveFromClassList("disabled");
            }
            else
            {
                _cardElement.AddToClassList("disabled");
            }
        }
        
        public void SetCardImage(Sprite sprite)
        {
            _frontSprite = sprite;
        }
    }
}