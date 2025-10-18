using System;
using System.Collections.Generic;
using Code;
using UnityEngine;

[Serializable]
public class GameData
{
    public float remainingTime;
    public float totalStars;
    public float currentStars;
    public float currentCoin;
    public List<HexData> placedHexes = new List<HexData>();

    public GameData()
    {
        remainingTime = Constants.TotalTime;
        totalStars = 0;
        currentStars = 0;
        currentCoin = 0;
        placedHexes = new List<HexData>();
    }
    public void Clear()
    {
        remainingTime = 0;
        totalStars = 0;
        currentStars = 0;
        currentCoin = 0;
        placedHexes.Clear();
    }
}

public class HexInfo : MonoBehaviour
{
    public Vector2Int HexCoordinates { get; set; }
    public Code.EventType EventType { get; set; }
    
    public int PrefabIndex { get; set; }
}