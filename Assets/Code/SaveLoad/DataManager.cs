using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class DataManager : MonoBehaviour , IService
{
    private GameData _gameData;
    private PlayerPrefsDataLoader _dataLoader = new PlayerPrefsDataLoader();
    private List<IPersistable> _persistables = new List<IPersistable>();

    private void Awake()
    {
        MainContainer.instance.Register(this);
        LoadGame();
    }

    public void NewGame()
    {
        _gameData = new GameData();
    }

    public void LoadGame()
    {
        _gameData = _dataLoader.LoadData();
        if (_gameData == null)
        {
            NewGame();
            Debug.Log("new game data crated");
        }
    }

    public void SaveGame()
    {
        _gameData.Clear();
        foreach (var persistable in _persistables)
        {
            persistable.SaveData(ref _gameData);
        }
        _dataLoader.SaveData(_gameData);
    }

    public bool HasSavedData()
    {
        return _dataLoader.HasData();
    }

    public void AddToPersistable(IPersistable persistable)
    {
        _persistables.Add(persistable);
    }

    public GameData GetData()
    {
        return _gameData;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    
}
