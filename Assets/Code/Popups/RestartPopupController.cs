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
    [SerializeField] private Button continueButton;
    [SerializeField] private Button homeButton;
    private EventBus _eventBus;
    void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.TimeOver>(OnTimeOver);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        homeButton.onClick.AddListener(OnHomeButtonClicked);
    }

    private void OnRestartButtonClicked()
    {
        UIAnimationHelper.SetActiveWithAnimation(container, false, useAnimations, this);
        _eventBus.Fire(new Events.RestartButtonClicked());
        _eventBus.Fire(new Events.StartGameInput());
    }

    private void OnContinueButtonClicked()
    {
        UIAnimationHelper.SetActiveWithAnimation(container, false, useAnimations, this);
        _eventBus.Fire(new Events.ResetTimer());
        _eventBus.Fire(new Events.StartGameInput());
    }

    private void OnHomeButtonClicked()
    {
        UIAnimationHelper.SetActiveWithAnimation(container, false, useAnimations, this);
        _eventBus.Fire(new Events.OnGameFirstOpen());
        _eventBus.Fire(new Events.StopGameInput());
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
