using RTS.Core;
using UnityEngine;

namespace RTS.Control
{
    public class CameraZoom : MonoBehaviour
    {
        [SerializeField] private Transform _anchor = default;
        [SerializeField] private float _sensitivity = 4f;
        [SerializeField, Range(0f, 1f)] private float _zoomProgress = .7f;

        [SerializeField] private float fovMax = 90;
        [SerializeField] private float fovMin = 70;
        [SerializeField] private float rotMin = 25;
        [SerializeField] private float rotMax = 70;
        [SerializeField] private float distanceMin = 5;
        [SerializeField] private float distanceMax = 10;
        [SerializeField] private float heightMin = 3;
        [SerializeField] private float heightMax = 5;
            
        [SerializeField] private AnimationCurve FovCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve RotCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve distanceCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private float RotX(float interpolation) => Mathf.Lerp(rotMin, rotMax, RotCurve.Evaluate(interpolation)); 
        private float Distance(float interpolation) => -Mathf.Lerp(distanceMin, distanceMax, distanceCurve.Evaluate(interpolation));
        private float FieldOfView(float interpolation) => Mathf.Lerp(fovMin, fovMax, FovCurve.Evaluate(interpolation));
        private float Height(float interpolation) => Mathf.Lerp(heightMin, heightMax, heightCurve.Evaluate(interpolation));
        
        private static Camera Camera => GameData.Instance.Input.Provider.Camera;
        
        private void Update()
        {
            var mouseWheel = _sensitivity * Time.deltaTime * -Input.mouseScrollDelta;
            _zoomProgress = Mathf.Clamp01(_zoomProgress + mouseWheel.y);
            Apply();
        }
        
        [ContextMenu("Apply")]
        private void Apply()
        {
            var rotation = _anchor.eulerAngles;
            rotation.x = RotX(_zoomProgress);
            _anchor.eulerAngles = rotation;

            var position = Camera.transform.localPosition;
            position.z = Distance(_zoomProgress);
            position.y = Height(_zoomProgress);
            Camera.transform.localPosition = position;

            Camera.fieldOfView = FieldOfView(_zoomProgress);
        }
    }
}