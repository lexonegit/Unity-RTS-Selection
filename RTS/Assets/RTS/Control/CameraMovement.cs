using UnityEngine;

namespace RTS.Control
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _rotateSensitivity = 200f;
        private static bool RightClickHeld => Input.GetMouseButton(1);
        private static Vector3 InputDirection => new(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        
        private void Update()
        {
            Movement();
            Rotate();
        }

        private void Rotate()
        {
            if (RightClickHeld == false) return;
            var xRotation = Input.GetAxis("Mouse X") * _rotateSensitivity * Time.deltaTime;
            transform.Rotate(0, xRotation, 0, Space.World);
        }

        private void Movement()
        {
            var movementDirection = Quaternion.Euler(0, transform.localEulerAngles.y, 0) * InputDirection;
            transform.Translate(movementDirection * _moveSpeed * Time.deltaTime, Space.World);
        }
    }
}