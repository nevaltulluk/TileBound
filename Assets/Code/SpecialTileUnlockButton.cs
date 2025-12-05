using UnityEngine;
using UnityEngine.UI;
using Code;

namespace Code
{
    /// <summary>
    /// MonoBehaviour component that can be assigned to buttons to unlock special tiles.
    /// Assign this component to a button and set the SpecialTile field in the inspector.
    /// </summary>
    public class SpecialTileUnlockButton : MonoBehaviour
    {
        [SerializeField] private SpecialTiles specialTile;
        [SerializeField] private Button button;
        
        private EventBus _eventBus;
        
        private void Awake()
        {
            // If button is not assigned, try to get it from this GameObject
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            
            // Get EventBus from MainContainer
            _eventBus = MainContainer.instance.Resolve<EventBus>();
        }
        
        private void Start()
        {
            // Subscribe button click to fire unlock event
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
            else
            {
                Debug.LogWarning($"SpecialTileUnlockButton on {gameObject.name}: No Button component found!");
            }
        }
        
        private void OnButtonClicked()
        {
            _eventBus.Fire(new Events.SpecialTileUnlocked(specialTile));
            Debug.Log($"Unlock button clicked for: {specialTile}");
        }
        
        private void OnDestroy()
        {
            // Clean up listener
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}




