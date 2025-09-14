using UnityEngine;

namespace Code
{
    public class Events
    {
        public struct SpawnStar
        {
            public Vector3 Position;
            
            public SpawnStar(Vector3 position)
            {
                this.Position = position;
            }
        }

        public struct TimeOver
        {
            
        }
        
        public struct RestartButtonClicked{}
        public struct StopGameInput{}
        public struct StartGameInput{}
        public struct OnGameStarted{}
        public struct GameStartButtonClicked{}

        public struct OnStarCountChanged
        {
            public readonly float TotalStars;
            public readonly float CurrentStars;

            public OnStarCountChanged(float totalStars, float currentStars)
            {
                this.TotalStars = totalStars;
                this.CurrentStars = currentStars;
            }
        }
    }
}