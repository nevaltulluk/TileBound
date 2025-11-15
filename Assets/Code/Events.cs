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
        public struct OnGameFirstOpen{}
        public struct GameStartButtonClicked{}
        
        public struct OkButtonClicked{}
        public struct TurnButtonClicked{}

        public struct OnStarCountChanged
        {
            public readonly float CurrentStars;

            public OnStarCountChanged( float currentStars)
            {
                this.CurrentStars = currentStars;
            }
        }
        
        public struct LevelSuccess {}
        public struct LevelFail {}
        public struct RequestNextLevel {}
        
        public struct OnLevelStarted
        {
            public int Level;

            public OnLevelStarted(int level)
            {
                this.Level = level;
            }
        }
    }
}