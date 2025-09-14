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

   public GameData()
   {
      remainingTime = Constants.TotalTime;
      totalStars = 0;
      currentStars = 0;
      currentCoin = 0;
   }
   public void Clear()
   {
      remainingTime = 0;
      totalStars = 0;
      currentStars = 0;
      currentCoin = 0;
   }
}
