using UnityEngine;

namespace Code
{
    public class LevelManager : IService
    {
        private EventBus _eventBus;

        public LevelManager()
        {
            _eventBus = MainContainer.instance.Resolve<EventBus>();
            _eventBus.Subscribe<Events.OnStarCountChanged>(OnStarCountChanged);
        }

        private void OnStarCountChanged(Events.OnStarCountChanged e)
        {
            if (e.CurrentStars >= Constants.RequiredStarCount)
            {
                _eventBus.Fire(new Events.LevelSuccess());
            }
        }
    }
}