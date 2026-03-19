using UnityEngine;
using UnityEngine.InputSystem;

namespace WorldConquest.Map
{
    /// <summary>
    /// Top-down camera with pan (right mouse drag) and scroll zoom.
    /// Uses the new Unity Input System.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 200f;

        private Vector2 lastMousePosition;
        private bool isDragging;

        void Update()
        {
            HandlePan();
            HandleZoom();
        }

        private void HandlePan()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
            {
                lastMousePosition = mouse.position.ReadValue();
                isDragging = true;
            }

            if (mouse.rightButton.wasReleasedThisFrame || mouse.middleButton.wasReleasedThisFrame)
                isDragging = false;

            if (isDragging)
            {
                Vector2 currentPos = mouse.position.ReadValue();
                Vector2 delta = currentPos - lastMousePosition;
                lastMousePosition = currentPos;

                float speed = panSpeed * Time.deltaTime;
                transform.Translate(-delta.x * speed, 0f, -delta.y * speed, Space.World);
            }
        }

        private void HandleZoom()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.001f) return;

            Camera cam = Camera.main;
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            }
            else
            {
                Vector3 pos = cam.transform.position;
                pos.y = Mathf.Clamp(pos.y - scroll * zoomSpeed, minZoom, maxZoom);
                cam.transform.position = pos;
            }
        }
    }
}
