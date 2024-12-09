using UnityEngine;

namespace Code
{
    public class InputController : MonoBehaviour
    {
        private Vector3 lastMousePosition;
        public float cameraDragSpeedx = 0.1f;
        public float cameraDragSpeedy = 0.1f;
        public Transform cameraTransform; 
        private Vector3 clickStartPosition;
        private float clickThreshold = 0.2f; // Maximum movement threshold for a click
        private float maxClickDuration = 0.3f; // Maximum time duration for a click
        private float clickStartTime;
        
        private void Update()
        {
            HandleMouseInput();
            HandleCameraMovement();
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                clickStartPosition = Input.mousePosition;
                clickStartTime = Time.time;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector3 clickEndPosition = Input.mousePosition;
                float clickDuration = Time.time - clickStartTime;

                if (Vector3.Distance(clickStartPosition, clickEndPosition) < clickThreshold && clickDuration <= maxClickDuration)
                {
                    HandleClick();
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

        private static void HandleClick()
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

        private void HandleCameraMovement()
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                Vector3 movement = new Vector3(delta.y * cameraDragSpeedx, 0, -delta.x * cameraDragSpeedy);
                cameraTransform.localPosition += movement;
                lastMousePosition = Input.mousePosition;
            }
        }
    }
}