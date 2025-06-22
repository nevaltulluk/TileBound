using UnityEngine;

public class MaterialScroller : MonoBehaviour
{
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.1f, 0.0f);
    private Renderer rend;
    private Vector2 offset = Vector2.zero;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;
        rend.material.SetTextureOffset("_MainTex", offset);
    }
}