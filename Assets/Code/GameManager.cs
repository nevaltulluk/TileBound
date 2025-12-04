using Code;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour, IPersistable
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] private Color _startingColor;
    [SerializeField] private Color _endingColor;
    [SerializeField] private Renderer _seaRenderer;
    
    private float _remainingTime;
    private bool _isGameOver;
    private EventBus _eventBus;
    private float _currentStars;
    private bool _isFreezeTimeActive;
    private bool _isDoubleStarsActive;

    private void Start()
    {
        var dataManager = MainContainer.instance.Resolve<DataManager>();
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.RestartButtonClicked>(OnGameRestart);
        _eventBus.Subscribe<Events.ResetTimer>(OnResetTimer);
        _eventBus.Subscribe<Events.GameStartButtonClicked>(OnGameStarted);
        _eventBus.Subscribe<Events.SpawnStar>(OnSpawnStar);
        _eventBus.Subscribe<Events.OnLevelStarted>(OnLevelStarted);
        _eventBus.Subscribe<Events.StartFreezeTimeBooster>(OnStartFreezeTimeBooster);
        _eventBus.Subscribe<Events.EndFreezeTimeBooster>(OnEndFreezeTimeBooster);
        _eventBus.Subscribe<Events.StartDoubleStarsBooster>(OnStartDoubleStarsBooster);
        _eventBus.Subscribe<Events.EndDoubleStarsBooster>(OnEndDoubleStarsBooster);
        _eventBus.Subscribe<Events.LevelSuccess>(OnLevelSuccess);
        _eventBus.Subscribe<Events.TimeOver>(OnTimeOver);
        dataManager.AddToPersistable(this);
        LoadData(dataManager.GetData());
        SetBgColor();
        _isGameOver = true;
        _eventBus.Fire(new Events.OnGameFirstOpen());
    }

    private void OnLevelStarted(Events.OnLevelStarted obj)
    {
        _currentStars = 0;
        _eventBus.Fire(new Events.OnStarCountChanged(_currentStars));
    }

    private void OnSpawnStar(Events.SpawnStar obj)
    {
        float starIncrement = _isDoubleStarsActive ? 2f : 1f;
        _currentStars += starIncrement;
        _eventBus.Fire(new Events.OnStarCountChanged(_currentStars));
    }
    
    private void OnStartFreezeTimeBooster(Events.StartFreezeTimeBooster obj)
    {
        _isFreezeTimeActive = true;
    }
    
    private void OnEndFreezeTimeBooster(Events.EndFreezeTimeBooster obj)
    {
        _isFreezeTimeActive = false;
    }
    
    private void OnStartDoubleStarsBooster(Events.StartDoubleStarsBooster obj)
    {
        _isDoubleStarsActive = true;
    }
    
    private void OnEndDoubleStarsBooster(Events.EndDoubleStarsBooster obj)
    {
        _isDoubleStarsActive = false;
    }

    private void OnGameStarted(Events.GameStartButtonClicked obj)
    {
        _isGameOver = false;
    }

    private void OnGameRestart()
    {
        _remainingTime = Constants.TotalTime;
        _isGameOver = false;
        SetSeaBaseColor(_startingColor);
        _currentStars = 0;
        _isFreezeTimeActive = false;
        _isDoubleStarsActive = false;
        _eventBus.Fire(new Events.OnStarCountChanged( _currentStars));
    }

    private void OnLevelSuccess(Events.LevelSuccess obj)
    {
        _isGameOver = true;
        ResetTimer();
    }

    private void OnTimeOver(Events.TimeOver obj)
    {
        ResetTimer();
    }

    private void OnResetTimer(Events.ResetTimer obj)
    {
        ResetTimer();
        _isGameOver = false;
    }

    private void ResetTimer()
    {
        _remainingTime = Constants.TotalTime;
        SetSeaBaseColor(_startingColor);
    }

    private void Update()
    {
        if (_isGameOver)
        {
            return;
        }
        int minutes = Mathf.FloorToInt(_remainingTime / 60f);
        int seconds = Mathf.FloorToInt(_remainingTime % 60f);

        string timeFormatted = $"{minutes}:{seconds:00}";
        timerText.text = timeFormatted;
        
        // Only decrease time if freeze time booster is not active
        if (!_isFreezeTimeActive)
        {
            _remainingTime -= Time.deltaTime;
        }
        SetBgColor();
        if (_remainingTime < 0)
        {
            SetSeaBaseColor(_endingColor);
            Debug.Log("Game Over");
            _isGameOver = true;
            _eventBus.Fire(new Events.TimeOver());
        }
        
    }

    private void SetBgColor()
    {
        if (_remainingTime < Constants.ChangeShiftTimer)
        {
            float t = 1f - (_remainingTime / Constants.ChangeShiftTimer);
            Color targetColor = Color.Lerp(_startingColor, _endingColor, t);
            SetSeaBaseColor(targetColor);
        }
    }

    private void SetSeaBaseColor(Color color)
    {
        if (_seaRenderer != null && _seaRenderer.material != null)
        {
            // Set the base color property (_Color) to make it darker/lighter
            _seaRenderer.material.SetColor("_Color", color);
        }
    }

    public void SaveData(ref GameData gameData)
    {
        gameData.remainingTime = _remainingTime;
        gameData.currentStars = _currentStars;
    }

    public void LoadData(GameData gameData)
    {
        _remainingTime = gameData.remainingTime;
        _currentStars = gameData.currentStars;
        if (_remainingTime == 0)
        {
            _remainingTime = Constants.TotalTime;
        }
    }

    
}
