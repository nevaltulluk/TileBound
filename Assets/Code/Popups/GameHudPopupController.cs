using Code;
using Code.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHudPopupController : MonoBehaviour
{
    private EventBus _eventBus;
    
    [Header("Animation Settings")]
    [SerializeField] public bool useAnimations = true;
    
    [SerializeField]private GameObject gameHud;
    [SerializeField]private GameObject externalGameplayUI;
    [SerializeField]private Image starPercentage;
    [SerializeField]private TextMeshProUGUI starText;
    [SerializeField]private TextMeshProUGUI starTextShadow;
    [SerializeField]private Button okButton;
    [SerializeField]private Button turnButton;
    [SerializeField]private Button freezeButton;
    [SerializeField]private Button starButton;
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
        freezeButton.onClick.AddListener(() => _eventBus.Fire(new Events.StartFreezeTimeBooster(10f)));
        starButton.onClick.AddListener(() => _eventBus.Fire(new Events.StartDoubleStarsBooster(10f)));
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
        UIAnimationHelper.SetActiveWithAnimation(gameHud, true, useAnimations, this);
        if (externalGameplayUI != null)
        {
            UIAnimationHelper.SetActiveWithAnimation(externalGameplayUI, true, useAnimations, this);
        }
    }

    private void OnGameStarted(Events.OnGameStarted obj)
    {
        UIAnimationHelper.SetActiveWithAnimation(gameHud, false, useAnimations, this);
        if (externalGameplayUI != null)
        {
            UIAnimationHelper.SetActiveWithAnimation(externalGameplayUI, false, useAnimations, this);
        }
    }
}
