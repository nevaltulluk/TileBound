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
    [SerializeField]private TextMeshProUGUI starTextShadow;
    [SerializeField]private Button okButton;
    [SerializeField]private Button turnButton;
    void Start()
    {
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        var dataManager = MainContainer.instance.Resolve<DataManager>();
        _eventBus.Subscribe<Events.OnGameStarted>(OnGameStarted);
        _eventBus.Subscribe<Events.GameStartButtonClicked>(OnGameStartButtonClicked);
        _eventBus.Subscribe<Events.OnStarCountChanged>(OnStarCountChanged);
        okButton.onClick.AddListener(()=> _eventBus.Fire(new Events.OkButtonClicked()));
        turnButton.onClick.AddListener(()=> _eventBus.Fire(new Events.TurnButtonClicked()));
        LoadData(dataManager.GetData());
        
    }

    private void LoadData(GameData data)
    {
        starPercentage.fillAmount = data.currentStars / 10;
        starText.text = data.currentStars.ToString();
    }

    private void OnStarCountChanged(Events.OnStarCountChanged obj)
    {
        starText.text = obj.CurrentStars.ToString();
        starTextShadow.text = obj.CurrentStars.ToString();
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
