using UnityEngine;

namespace RTS.Control
{
    public class CameraRotate : MonoBehaviour
    {
        [SerializeField] private float _sensitivity = 200f;
        private static bool RightClick => Input.GetMouseButton(1);
        private static bool RightClickDown => Input.GetMouseButtonDown(1);
        private static bool RightClickUp => Input.GetMouseButtonUp(1);
        
        private void Update()
        {
            UpdateCursorVisibility();
            Rotate();
        }

        private void Rotate()
        {
            if (RightClick == false) return;
            var xRotation = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
            transform.Rotate(0, xRotation, 0, Space.World);
        }

        private static void UpdateCursorVisibility()
        {
            if (RightClick) Cursor.visible = false;
            else if (RightClickUp) Cursor.visible = true;
        }
    }
}