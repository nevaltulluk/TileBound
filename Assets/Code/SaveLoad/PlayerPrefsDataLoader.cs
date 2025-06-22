using UnityEngine;

public class PlayerPrefsDataLoader
{
    public void SaveData(GameData data)
    {
        var serializedData = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(nameof(GameData),serializedData);
    }

    public GameData LoadData()
    {
        var data = PlayerPrefs.GetString(nameof(GameData));
        var loadedData = JsonUtility.FromJson<GameData>(data);
        return loadedData;
    }

    public bool HasData()
    {
        return PlayerPrefs.HasKey(nameof(GameData));
    }
}
