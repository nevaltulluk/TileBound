using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Events = Code.Events;
using Code.Popups;

public class HomePagePopupController : MonoBehaviour
{
    private EventBus _eventBus;
    
    [Header("Animation Settings")]
    [SerializeField] public bool useAnimations = true;
    
    [SerializeField] private Button _startGameButton;
    [SerializeField] private GameObject hud;
    [SerializeField] private TextMeshProUGUI starText;
    [SerializeField] private TextMeshProUGUI starShadow;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI goldShadow;
    
    void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.OnStarCountChanged>(OnStarCountChanged);
        _eventBus.Subscribe<Events.OnGameFirstOpen>(OnGameFirstOpen);
        _startGameButton.onClick.AddListener(OnStartGameClicked);
    }

    private void OnGameFirstOpen(Events.OnGameFirstOpen obj)
    {
        UIAnimationHelper.SetActiveWithAnimation(hud, true, useAnimations, this);
        _eventBus.Fire(new Events.StopGameInput());
    }

    private void OnStarCountChanged(Events.OnStarCountChanged obj)
    {
        starText.text = obj.CurrentStars.ToString();
        starShadow.text = obj.CurrentStars.ToString();
    }

    private void OnStartGameClicked()
    {
        _eventBus.Fire(new Events.GameStartButtonClicked());
        _eventBus.Fire(new Events.StartGameInput());
        UIAnimationHelper.SetActiveWithAnimation(hud, false, useAnimations, this);
    }
}
