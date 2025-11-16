using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Popups
{
    public class LevelSuccessPopup : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] public bool useAnimations = true;
        
        [SerializeField] private Button _button;
        [SerializeField] private GameObject hud;
        
        private EventBus _eventBus;

        private void Start()
        {
            _eventBus = MainContainer.instance.Resolve<EventBus>();
            _eventBus.Subscribe<Events.LevelSuccess>(OnLevelSuccess);
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            _eventBus.Fire(new Events.RequestNextLevel());
            _eventBus.Fire(new Events.StartGameInput());
            UIAnimationHelper.SetActiveWithAnimation(hud, false, useAnimations, this);
        }

        private void OnLevelSuccess(Events.LevelSuccess obj)
        {
            _eventBus.Fire(new Events.StopGameInput());
            UIAnimationHelper.SetActiveWithAnimation(hud, true, useAnimations, this);
        }
        
        
    }
}