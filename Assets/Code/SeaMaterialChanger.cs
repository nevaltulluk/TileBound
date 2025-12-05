using Code;
using UnityEngine;

public class SeaMaterialChanger : MonoBehaviour
{
    [SerializeField] private Material lightMaterial;
    [SerializeField] private Material darkMaterial;
    [SerializeField] private Renderer targetRenderer;

    private void Awake()
    {
        // Auto-find renderer if not assigned
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogWarning($"SeaMaterialChanger on {gameObject.name} requires a Renderer component.");
            }
        }
    }

    public void SetDarkMode(bool isDark)
    {
        if (targetRenderer == null) return;

        if (isDark && darkMaterial != null)
        {
            targetRenderer.material = darkMaterial;
        }
        else if (!isDark && lightMaterial != null)
        {
            targetRenderer.material = lightMaterial;
        }
    }
}





