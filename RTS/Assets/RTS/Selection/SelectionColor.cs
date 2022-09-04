using System;
using UnityEngine;

namespace RTS.Selection
{
    public class SelectionColor : SelectionListener
    {
        [Serializable]
        private class ColorConfig : ISerializationCallbackReceiver
        {
            [field:SerializeField] public Color SelectionColor { get; private set; } = Color.red;
            [field:SerializeField] public Color HoverColor { get; private set; } = Color.yellow;
            
            private static Color _selectionColor = default;
            private static Color _hoverColor = default;
            
            public void OnBeforeSerialize()
            {
                SelectionColor = _selectionColor;
                HoverColor = _hoverColor;
            }

            public void OnAfterDeserialize()
            {
                _selectionColor = SelectionColor;
                _hoverColor = HoverColor;
            }
        }
        
        [SerializeField] private ColorConfig _colorConfig = default;
        private MeshRenderer _meshRenderer = default;
        private Material _defaultMaterial = default;
        private Material _selectionMaterial = default;
        private Material _hoverMaterial = default;
        
        private void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _defaultMaterial = _meshRenderer.sharedMaterial;
            _selectionMaterial = new Material(_meshRenderer.material) { color = _colorConfig.SelectionColor };
            _hoverMaterial = new Material(_meshRenderer.material) { color = _colorConfig.HoverColor };
        }
        
        protected override void OnSelect() => _meshRenderer.material = _selectionMaterial;
        protected override void OnDeselect() => _meshRenderer.material = _receiver.Hovering ? _hoverMaterial : _defaultMaterial;
        protected override void OnHoverEnter() => _meshRenderer.material = _receiver.Selected ? _selectionMaterial : _hoverMaterial;
        protected override void OnHoverExit() => _meshRenderer.material = _receiver.Selected ? _selectionMaterial : _defaultMaterial;
    }
}
