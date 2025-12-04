using UnityEngine;
using UnityEngine.UI;
using Code.Popups;

public class CardUpgradeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private GameObject _gameObjectToOpen;
    [SerializeField] private GameObject _gameObjectToClose;
    
    [Header("Animation Settings")]
    [SerializeField] private bool useAnimations = true;
    
    void Start()
    {
        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }
    }
    
    private void OnUpgradeButtonClicked()
    {
        if (_gameObjectToClose != null)
        {
            UIAnimationHelper.SetActiveWithAnimation(_gameObjectToClose, false, useAnimations, this);
        }
        
        if (_gameObjectToOpen != null)
        {
            UIAnimationHelper.SetActiveWithAnimation(_gameObjectToOpen, true, useAnimations, this);
        }
    }
    
    private void OnDestroy()
    {
        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
        }
    }
}


