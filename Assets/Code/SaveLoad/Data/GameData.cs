using System;
using System.Collections.Generic;
using Code;
using UnityEngine;

[Serializable]
public class GameData
{
   public float remainingTime;

   public GameData()
   {
      remainingTime = Constants.TotalTime;
   }
   public void Clear()
   {
      remainingTime = 0;
   }
}
