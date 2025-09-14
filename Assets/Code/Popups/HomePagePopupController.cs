using UnityEditor.TestTools.CodeCoverage;
using UnityEngine;
using UnityEngine.UI;
using Events = Code.Events;

public class HomePagePopupController : MonoBehaviour
{
    private EventBus _eventBus;
    
    [SerializeField] private Button _startGameButton;
    
    void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.OnGameStarted>(OnGameStarted);
        
        _startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    private void OnStartGameClicked()
    {
        _eventBus.Fire(new Events.GameStartButtonClicked());
        gameObject.SetActive(false);
    }

    private void OnGameStarted(Events.OnGameStarted obj)
    {
        gameObject.SetActive(true);
    }

    
}
