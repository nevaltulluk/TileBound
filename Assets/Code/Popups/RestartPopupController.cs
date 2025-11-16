using Code;
using Code.Popups;
using UnityEngine;
using UnityEngine.UI;

public class RestartPopupController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] public bool useAnimations = true;
    
    [SerializeField] private GameObject container;
    [SerializeField] private Button restartButton;
    private EventBus _eventBus;
    void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.TimeOver>(OnTimeOver);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
    }

    private void OnRestartButtonClicked()
    {
        UIAnimationHelper.SetActiveWithAnimation(container, false, useAnimations, this);
        _eventBus.Fire(new Events.RestartButtonClicked());
        _eventBus.Fire(new Events.StartGameInput());
    }
    

    private void OnTimeOver()
    {
        UIAnimationHelper.SetActiveWithAnimation(container, true, useAnimations, this);
        _eventBus.Fire(new Events.StopGameInput());
    }

    private void OnDestroy()
    {
        _eventBus.Unsubscribe<Events.TimeOver>(OnTimeOver);
    }
}
