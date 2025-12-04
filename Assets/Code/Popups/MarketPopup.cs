using Code.Popups;
using UnityEngine;
using UnityEngine.UI;

public class MarketPopup : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] public bool useAnimations = true;
    
    [SerializeField] private GameObject container;
    [SerializeField] private Button closeButton;

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    public void Open()
    {
        UIAnimationHelper.SetActiveWithAnimation(container, true, useAnimations, this);
    }

    public void Close()
    {
        UIAnimationHelper.SetActiveWithAnimation(container, false, useAnimations, this);
    }

    private void OnCloseButtonClicked()
    {
        Close();
    }
}


