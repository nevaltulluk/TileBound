using UnityEngine;

namespace Code
{
    public class InputController : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    HexPlaceholder placeholder = hit.collider.GetComponent<HexPlaceholder>();
                    if (placeholder != null)
                    {
                        placeholder.OnClick();
                    }
                }
            }

            if (HexManager.Instance != null && HexManager.Instance.lastPlacedHex != null)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    HexManager.Instance.lastPlacedHex.transform.Rotate(0, 60, 0); // Rotate clockwise
                }

                if (Input.GetMouseButtonDown(2))
                {
                    HexManager.Instance.lastPlacedHex.transform.Rotate(0, -60, 0); // Rotate counterclockwise
                }
            }
        }
    }
}