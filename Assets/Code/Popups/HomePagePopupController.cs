using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Events = Code.Events;

public class HomePagePopupController : MonoBehaviour
{
    private EventBus _eventBus;
    
    [SerializeField] private Button _startGameButton;
    [SerializeField] private GameObject hud;
    [SerializeField] private TextMeshProUGUI starText;
    [SerializeField] private TextMeshProUGUI starShadow;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI goldShadow;
    
    void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.OnGameStarted>(OnGameStarted);
        _eventBus.Subscribe<Events.OnStarCountChanged>(OnStarCountChanged);
        _startGameButton.onClick.AddListener(OnStartGameClicked);
    }
    
    private void OnStarCountChanged(Events.OnStarCountChanged obj)
    {
        starText.text = obj.CurrentStars.ToString();
        starShadow.text = obj.CurrentStars.ToString();
    }

    private void OnStartGameClicked()
    {
        _eventBus.Fire(new Events.GameStartButtonClicked());
        hud.SetActive(false);
    }

    private void OnGameStarted(Events.OnGameStarted obj)
    {
        hud.SetActive(true);
    }

    
}
