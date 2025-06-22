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
    private EventBus _eventBus;

    private void Start()
    {
        var dataManager = MainContainer.instance.Resolve<DataManager>();
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.RestartButtonClicked>(OnGameRestart);
        dataManager.AddToPersistable(this);
        LoadData(dataManager.GetData());
    }

    private void OnGameRestart()
    {
        _remainingTime = Constants.TotalTime;
        _seaRenderer.material.color = _startingColor;
    }

    private void Update()
    {
        if (_remainingTime == 0) 
            return;
        if (_remainingTime < 0){_remainingTime = 0; return;}
        
        int minutes = Mathf.FloorToInt(_remainingTime / 60f);
        int seconds = Mathf.FloorToInt(_remainingTime % 60f);

        string timeFormatted = $"{minutes}:{seconds:00}";
        timerText.text = timeFormatted;
        _remainingTime -= Time.deltaTime;
        if (_remainingTime < Constants.ChangeShiftTimer)
        {
            float t = 1f - (_remainingTime / Constants.ChangeShiftTimer);
            _seaRenderer.material.color = Color.Lerp(_startingColor, _endingColor, t);
        }
        if (_remainingTime < 0)
        {
            _seaRenderer.material.color = _endingColor;
            Debug.Log("Game Over");
            _eventBus.Fire(new Events.TimeOver());
        }
        
    }

    public void SaveData(ref GameData gameData)
    {
        gameData.remainingTime = _remainingTime;
    }

    public void LoadData(GameData gameData)
    {
        _remainingTime = gameData.remainingTime;
    }

    
}
