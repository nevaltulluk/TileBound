using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class DarkModeImage : MonoBehaviour
    {
        [SerializeField] private Sprite originalSprite;
        [SerializeField] private Sprite darkSprite;
        
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
            if (image == null)
            {
                Debug.LogWarning($"DarkModeImage on {gameObject.name} requires an Image component.");
                return;
            }
            
            // Store original sprite if not already set (e.g., if component was added at runtime)
            if (originalSprite == null && image.sprite != null)
            {
                originalSprite = image.sprite;
            }
        }

        public void SetDarkMode(bool isDark)
        {
            if (image == null) return;
            
            if (isDark && darkSprite != null)
            {
                image.sprite = darkSprite;
            }
            else if (!isDark && originalSprite != null)
            {
                image.sprite = originalSprite;
            }
        }

        // Public setter for Editor tool to assign sprites
        public void SetSprites(Sprite original, Sprite dark)
        {
            originalSprite = original;
            darkSprite = dark;
        }
    }
}

