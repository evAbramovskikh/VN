using UnityEngine;
using UnityEngine.UIElements;

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
        void ShowMatchEffect();
        VisualElement CreateVisualElement();
    }
    
    /// <summary>
    /// View карты с продвинутыми анимациями
    /// </summary>
    public class CardView : MonoBehaviour, ICardView
    {
        [SerializeField] private Sprite _backSprite;
        [SerializeField] private float _flipDuration = 0.3f;
        
        private VisualElement _cardElement;
        private VisualElement _cardContainer;
        private VisualElement _frontFace;
        private VisualElement _backFace;
        private VisualElement _glowEffect;
        private bool _isFlipped;
        
        public VisualElement CreateVisualElement()
        {
            _cardElement = new VisualElement();
            _cardElement.AddToClassList("card");
            
            _cardContainer = new VisualElement();
            _cardContainer.AddToClassList("card-container");
            
            _backFace = new VisualElement();
            _backFace.AddToClassList("card-face");
            _backFace.AddToClassList("card-back");
            if (_backSprite != null)
            {
                _backFace.style.backgroundImage = new StyleBackground(_backSprite);
            }
            
            _frontFace = new VisualElement();
            _frontFace.AddToClassList("card-face");
            _frontFace.AddToClassList("card-front");
            _frontFace.style.rotate = new Rotate(Angle.Degrees(180));
            
            _glowEffect = new VisualElement();
            _glowEffect.AddToClassList("glow-effect");
            
            _cardContainer.Add(_backFace);
            _cardContainer.Add(_frontFace);
            _cardElement.Add(_cardContainer);
            _cardElement.Add(_glowEffect);
            
            return _cardElement;
        }
        
        public void SetFlipped(bool flipped)
        {
            if (_isFlipped == flipped) return;
            _isFlipped = flipped;
            
            if (flipped)
            {
                _cardContainer.AddToClassList("flipped");
                
                // Добавляем эффект свечения при переворачивании
                _glowEffect.style.opacity = 1;
                _glowEffect.schedule.Execute(() =>
                {
                    _glowEffect.style.opacity = 0;
                }).StartingIn(300);
            }
            else
            {
                _cardContainer.RemoveFromClassList("flipped");
            }
        }
        
        public void SetMatched(bool matched)
        {
            if (matched)
            {
                _cardElement.AddToClassList("matched");
                
                // Анимация исчезновения с масштабированием
                _cardElement.schedule.Execute(() =>
                {
                    _cardElement.style.scale = new Scale(new Vector3(1.2f, 1.2f, 1));
                }).StartingIn(100);
                
                _cardElement.schedule.Execute(() =>
                {
                    _cardElement.style.scale = new Scale(new Vector3(0.8f, 0.8f, 1));
                    _cardElement.style.opacity = 0.3f;
                }).StartingIn(300);
            }
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
            if (sprite != null)
            {
                _frontFace.style.backgroundImage = new StyleBackground(sprite);
            }
        }
        
        public void ShowMatchEffect()
        {
            // Создаем эффект звезд
            var starsContainer = new VisualElement();
            starsContainer.AddToClassList("stars-container");
            
            for (int i = 0; i < 4; i++)
            {
                var star = new VisualElement();
                star.AddToClassList("star");
                star.style.rotate = new Rotate(Angle.Degrees(i * 90));
                starsContainer.Add(star);
            }
            
            _cardElement.Add(starsContainer);
            
            // Удаляем эффект через секунду
            starsContainer.schedule.Execute(() =>
            {
                starsContainer.RemoveFromHierarchy();
            }).StartingIn(1000);
        }
    }
}