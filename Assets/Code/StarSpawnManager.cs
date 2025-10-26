using System.Collections;
using Code;
using UnityEngine;

public class StarSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private float moveDistance = 1.5f;  // how far up it moves
    [SerializeField] private float duration = 0.4f;      // total animation time
    [SerializeField] private AnimationCurve jumpCurve = 
        AnimationCurve.EaseInOut(0, 0, 1, 1);    

    private EventBus _eventBus;

    void Start()
    {
        _eventBus = MainContainer.instance.Resolve<EventBus>();
        _eventBus.Subscribe<Events.SpawnStar>(OnSpawnStar);
        
        if (jumpCurve.length <= 1)
        {
            jumpCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 3f, 3f),
                new Keyframe(0.6f, 1.2f, 0f, 0f),
                new Keyframe(1f, 1f)           
            );
        }
    }

    private void OnSpawnStar(Events.SpawnStar obj)
    {
        var star = Instantiate(starPrefab, obj.Position + Vector3.down, Quaternion.identity, transform);
        StartCoroutine(MoveUpAndDestroy(star));
    }

    private IEnumerator MoveUpAndDestroy(GameObject star)
    {
        Vector3 startPos = star.transform.position;
        Vector3 endPos = startPos + Vector3.up * moveDistance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            float curveValue = jumpCurve.Evaluate(t);
            star.transform.position = Vector3.LerpUnclamped(startPos, endPos, curveValue);

            yield return null;
        }

        Destroy(star);
    }
}