using System;
using UnityEngine;
using UnityEngine.Animations;

namespace RTS.Control
{
    public class CameraDirectionalMovement : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 10f;
        private static Vector3 InputDirection => new(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        
        private void Update()
        {
            Movement();
        }
        
        private void Movement()
        {
            var movementDirection = Quaternion.Euler(0, transform.localEulerAngles.y, 0) * InputDirection;
            transform.Translate(movementDirection * _moveSpeed * Time.deltaTime, Space.World);
        }
    }
}