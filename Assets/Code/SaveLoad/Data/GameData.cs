using System;
using System.Collections.Generic;
using Code;
using UnityEngine;

[Serializable]
public class GameData
{
    public float remainingTime;
    public float currentStars;
    public float currentCoin;
    public List<HexData> placedHexes = new List<HexData>();
    public int currentLevel;

    public GameData()
    {
        remainingTime = Constants.TotalTime;
        currentStars = 0;
        currentCoin = 0;
        placedHexes = new List<HexData>();
        currentLevel = 0;
    }
    public void Clear()
    {
        remainingTime = 0;
        currentStars = 0;
        currentCoin = 0;
        currentLevel = 0;
        placedHexes.Clear();
    }
}

public class HexInfo : MonoBehaviour
{
    public Vector2Int HexCoordinates { get; set; }
    public Code.EventType EventType { get; set; }
    
    public int PrefabIndex { get; set; }
}