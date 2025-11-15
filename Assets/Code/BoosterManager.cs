using System.Collections;
using Code;
using UnityEngine;

public class BoosterManager : MonoBehaviour
{
    private EventBus _eventBus;
    private Coroutine _freezeTimeCoroutine;
    private Coroutine _doubleStarsCoroutine;

    private void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.StartFreezeTimeBooster>(OnStartFreezeTimeBooster);
        _eventBus.Subscribe<Events.StartDoubleStarsBooster>(OnStartDoubleStarsBooster);
    }

    private void OnStartFreezeTimeBooster(Events.StartFreezeTimeBooster boosterEvent)
    {
        if (_freezeTimeCoroutine != null)
        {
            StopCoroutine(_freezeTimeCoroutine);
            _eventBus.Fire(new Events.EndFreezeTimeBooster());
        }
        
        _freezeTimeCoroutine = StartCoroutine(FreezeTimeBoosterCoroutine(boosterEvent.Duration));
    }

    private void OnStartDoubleStarsBooster(Events.StartDoubleStarsBooster boosterEvent)
    {
        if (_doubleStarsCoroutine != null)
        {
            StopCoroutine(_doubleStarsCoroutine);
            _eventBus.Fire(new Events.EndDoubleStarsBooster());
        }
        
        _doubleStarsCoroutine = StartCoroutine(DoubleStarsBoosterCoroutine(boosterEvent.Duration));
    }

    private IEnumerator FreezeTimeBoosterCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        _eventBus.Fire(new Events.EndFreezeTimeBooster());
        _freezeTimeCoroutine = null;
    }

    private IEnumerator DoubleStarsBoosterCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        _eventBus.Fire(new Events.EndDoubleStarsBooster());
        _doubleStarsCoroutine = null;
    }

    private void OnDestroy()
    {
        if (_eventBus != null)
        {
            _eventBus.Unsubscribe<Events.StartFreezeTimeBooster>(OnStartFreezeTimeBooster);
            _eventBus.Unsubscribe<Events.StartDoubleStarsBooster>(OnStartDoubleStarsBooster);
        }
    }
}

