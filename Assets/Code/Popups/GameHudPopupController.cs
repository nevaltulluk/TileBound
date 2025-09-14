using Code;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHudPopupController : MonoBehaviour
{
    private EventBus _eventBus;
    [SerializeField]private GameObject gameHud;
    [SerializeField]private Image starPercentage;
    [SerializeField]private TextMeshProUGUI starText;
    void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.OnGameStarted>(OnGameStarted);
        _eventBus.Subscribe<Events.GameStartButtonClicked>(OnGameStartButtonClicked);
        _eventBus.Subscribe<Events.OnStarCountChanged>(OnStarCountChanged);
    }

    private void OnStarCountChanged(Events.OnStarCountChanged obj)
    {
        starText.text = obj.TotalStars.ToString();
        starPercentage.fillAmount = obj.CurrentStars / 10;
    }

    private void OnGameStartButtonClicked(Events.GameStartButtonClicked obj)
    {
        
        gameHud.SetActive(true);
    }

    private void OnGameStarted(Events.OnGameStarted obj)
    {
        gameHud.SetActive(false);
    }
}
