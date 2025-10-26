using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Popups
{
    public class LevelSuccessPopup : MonoBehaviour
    {
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
            hud.SetActive(false);
        }

        private void OnLevelSuccess(Events.LevelSuccess obj)
        {
            _eventBus.Fire(new Events.StopGameInput());
            hud.SetActive(true);
        }
        
        
    }
}