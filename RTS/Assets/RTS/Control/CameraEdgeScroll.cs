using System;
using System.Numerics;
using RTS.Core;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace RTS.Control
{
    public class CameraEdgeScroll : MonoBehaviour
    {
        [SerializeField, Range(0.01f, .25f)] private float _edgeThickness = 0.15f;
        [SerializeField] private float _sensitivity = 1;
        [SerializeField] private RectTransform _edgeScrollBounds = default;
        [SerializeField] private bool _requireCursorLockToScroll = true;
        private Vector3 InputDirection => new(MoveDirection(GameCursor.NormalizedPosition.x), 0, MoveDirection(GameCursor.NormalizedPosition.y));
        
        private void OnValidate()
        {
            if(_edgeScrollBounds) _edgeScrollBounds.transform.localScale = Vector3.one * (1 - _edgeThickness * 2);
        }
        
        private float MoveDirection(float value)
        {
            if (value < _edgeThickness)
                return -Mathf.InverseLerp(_edgeThickness, 0, value);
            return value > 1 - _edgeThickness ? Mathf.InverseLerp(1 - _edgeThickness, 1, value) : default;
        }

        private void Update()
        {
            if (_requireCursorLockToScroll && Cursor.lockState == CursorLockMode.None) return;
            var movementDirection = Quaternion.Euler(0, transform.localEulerAngles.y, 0) * InputDirection;
            transform.Translate(movementDirection * _sensitivity * Time.deltaTime, Space.World);
        }
    }
}