using System;
using Code;
using UnityEngine;

public class HexTrigger : MonoBehaviour
{
    public TileType tileType;
    public bool isSpawnStar;
    private EventBus _eventBus;

    private void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("TileTrigger")) return;
        if (!isSpawnStar) return;
        if (other.GetComponent<HexTrigger>().tileType == tileType &&  _eventBus != null)
        {
            _eventBus.Fire(new Events.SpawnStar(other.gameObject.transform.position));
            Debug.Log("Spawn Star --- " + tileType);
        }
        
    }
}
