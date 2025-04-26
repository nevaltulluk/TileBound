using Code;
using UnityEngine;

public class StarSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject starPrefab;
    
    private EventBus _eventBus;
     
    void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.SpawnStar>(OnSpawnStar);
    }

    private void OnSpawnStar(Events.SpawnStar obj)
    {
        Instantiate(starPrefab, obj.Position + Vector3.up, Quaternion.identity, transform);
    }
}
