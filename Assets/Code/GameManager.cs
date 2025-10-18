using System;
using System.Collections;
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

    private void Start()
    {
        var dataManager = MainContainer.instance.Resolve<DataManager>();
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.RestartButtonClicked>(OnGameRestart);
        _eventBus.Subscribe<Events.GameStartButtonClicked>(OnGameStarted);
        _eventBus.Subscribe<Events.SpawnStar>(OnSpawnStar);
        dataManager.AddToPersistable(this);
        LoadData(dataManager.GetData());
        SetBgColor();
        _isGameOver = true;
    }

    private void OnSpawnStar(Events.SpawnStar obj)
    {
        _currentStars++;
        _eventBus.Fire(new Events.OnStarCountChanged(_currentStars));
    }

    private void OnGameStarted(Events.GameStartButtonClicked obj)
    {
        _isGameOver = false;
    }

    private void OnGameRestart()
    {
        _remainingTime = Constants.TotalTime;
        _isGameOver = false;
        _seaRenderer.material.color = _startingColor;
        _currentStars = 0;
        _eventBus.Fire(new Events.OnStarCountChanged( _currentStars));
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
        _remainingTime -= Time.deltaTime;
        SetBgColor();
        if (_remainingTime < 0)
        {
            _seaRenderer.material.color = _endingColor;
            Debug.Log("Game Over");
            _eventBus.Fire(new Events.TimeOver());
            _isGameOver = true;
        }
        
    }

    private void SetBgColor()
    {
        if (_remainingTime < Constants.ChangeShiftTimer)
        {
            float t = 1f - (_remainingTime / Constants.ChangeShiftTimer);
            _seaRenderer.material.color = Color.Lerp(_startingColor, _endingColor, t);
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
