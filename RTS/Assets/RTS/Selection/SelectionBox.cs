using RTS.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RTS.Selection
{
    public class SelectionBox : MonoBehaviour
    {
        private RectTransform _selectionRect = default;
        private Image _image = default;
        private Vector2 startPosition = default;
        private static Vector2 currentPosition => Input.mousePosition;

        private void Awake()
        {
            _selectionRect = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
            enabled = false;
        }

        private void OnEnable()
        {
            startPosition = Input.mousePosition;
        }

        private void OnDisable()
        {
            _image.enabled = false;
        }

        private void Update()
        {
            var w = currentPosition.x - startPosition.x;
            var h = currentPosition.y - startPosition.y;
            _selectionRect.anchoredPosition = startPosition + new Vector2(w / 2, h / 2);
            _selectionRect.sizeDelta = new Vector2
            (
                Mathf.Clamp(Mathf.Abs(w), MeshBuilder.MinimumBoxSelectSize, Mathf.Infinity),
                Mathf.Clamp(Mathf.Abs(Mathf.Abs(h)), MeshBuilder.MinimumBoxSelectSize, Mathf.Infinity)
            );
            _image.enabled = true;
        }
    }
}