using RTS.Core;
using UnityEngine;
using UnityEngine.Animations;

namespace RTS.Control
{
    public class CameraPan : MonoBehaviour
    {
        [SerializeField] private PositionConstraint _positionConstraint = default;
        [SerializeField] private Transform _anchor = default;
        [SerializeField] private float _sensitivity = 1;
        private Vector3 _mousePosition;

        private static IInputProvider Provider => GameData.Instance.Input.Provider;
        private LayerMask Ground => GameData.Instance.Settings.GroundLayer;
        private static Camera Camera => Provider.Camera;
        private static bool PanCamera => Provider.PanCamera;
        private static bool PanCameraDown => Provider.PanCameraDown;
        private static bool PanCameraUp => Provider.PanCameraUp;

        private void Awake()
        {
            var source = new ConstraintSource
            {
                sourceTransform = _anchor,
                weight = 1f
            };

            if (_positionConstraint.sourceCount == 0)
                _positionConstraint.AddSource(source);
            else
                _positionConstraint.SetSource(0, source);
        }

        private void Update()
        {
            UpdateCursorVisibility();
            PanAnchor();
        }

        private void PanAnchor()
        {
            var mousePos = Input.mousePosition;
            mousePos = Camera.ScreenToViewportPoint(Input.mousePosition);

            if (PanCamera && PanCameraDown == false)
            {
                var movement = _mousePosition - mousePos;
                movement = Quaternion.Euler(0, transform.localEulerAngles.y, 0) * new Vector3(movement.x, 0, movement.y);
                _anchor.Translate(movement * _sensitivity, Space.World);
            }

            _mousePosition = mousePos;
        }
        
        private static void UpdateCursorVisibility()
        {
            if (PanCamera) Cursor.visible = false;
            else if (PanCameraUp) Cursor.visible = true;
        }
    }
}