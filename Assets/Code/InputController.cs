using System;
using UnityEngine;

namespace Code
{
    public class InputController : MonoBehaviour
    {
        private Vector3 _lastMousePosition;
        public float cameraDragSpeedx = 0.1f;
        public float cameraDragSpeedy = 0.1f;
        public Transform cameraTransform; 
        private Vector3 _clickStartPosition;
        private float _clickThreshold = 0.2f; // Maximum movement threshold for a click
        private float _maxClickDuration = 0.3f; // Maximum time duration for a click
        private float _clickStartTime;
        private HexManager _hexManager;

        private void Start()
        {
            _hexManager = MainContainer.instance.Resolve<HexManager>();
        }

        private void Update()
        {
            HandleMouseInput();
            HandleCameraMovement();
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _clickStartPosition = Input.mousePosition;
                _clickStartTime = Time.time;
            }

            if (Input.GetMouseButtonUp(0))
            {
                Vector3 clickEndPosition = Input.mousePosition;
                float clickDuration = Time.time - _clickStartTime;

                if (Vector3.Distance(_clickStartPosition, clickEndPosition) < _clickThreshold && clickDuration <= _maxClickDuration)
                {
                    HandleClick();
                }
            }

            if (_hexManager != null && _hexManager.LastPlacedHex != null)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    _hexManager.LastPlacedHex.transform.Rotate(0, 60, 0); // Rotate clockwise
                }

                if (Input.GetMouseButtonDown(2))
                {
                    _hexManager.LastPlacedHex.transform.Rotate(0, -60, 0); // Rotate counterclockwise
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
                _lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                Vector3 movement = new Vector3(delta.y * cameraDragSpeedx, 0, -delta.x * cameraDragSpeedy);
                cameraTransform.localPosition += movement;
                _lastMousePosition = Input.mousePosition;
            }
        }
    }
}