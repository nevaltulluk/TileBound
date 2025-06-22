using Code;
using UnityEngine;
using UnityEngine.UI;

public class RestartPopupController : MonoBehaviour
{
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
        container.SetActive(false);
        _eventBus.Fire(new Events.RestartButtonClicked());
        _eventBus.Fire(new Events.StartGameInput());
    }
    

    private void OnTimeOver()
    {
        container.SetActive(true);
        _eventBus.Fire(new Events.StopGameInput());
    }

    private void OnDestroy()
    {
        _eventBus.Unsubscribe<Events.TimeOver>(OnTimeOver);
    }
}
