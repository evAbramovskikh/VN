using UnityEngine;

namespace PairsGame.Infrastructure
{
    /// <summary>
    /// Настройки игры
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "PairsGame/Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Визуальные настройки")]
        public Sprite CardBackSprite;
        public Sprite[] CardSprites;
        
        [Header("Игровые настройки")]
        public float FlipAnimationDuration = 0.3f;
        public float MatchCheckDelay = 1f;
        public float CompletionDelay = 2f;
        
        [Header("Звуки")]
        public AudioClip FlipSound;
        public AudioClip MatchSound;
        public AudioClip MismatchSound;
        public AudioClip CompletionSound;
        
        public Sprite GetCardSprite(int pairId)
        {
            if (CardSprites == null || CardSprites.Length == 0) return null;
            return CardSprites[pairId % CardSprites.Length];
        }
    }
}