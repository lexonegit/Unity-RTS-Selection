using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RTS.Core;
using RTS.Utility;
using UnityEngine;

namespace RTS.Selection
{
    public class Selector : MonoBehaviour, IDisposable
    {
        [SerializeField] public MeshCollider _selectionCollider = default;
        [SerializeField] private SelectionBox _selectionBox = default;
        [SerializeField] private float _clickRadius = 0f;
        
        private Vector2 _startPosition = default;
        private static Vector2 _endPosition => Input.mousePosition;
        private bool _dragSelection => Vector2.Distance(_startPosition, _endPosition) > 1;
        private static IInputProvider InputProvider => GameData.Instance.Input.Provider;

        private ISelectable _hoverSelection = default;
        private readonly List<ISelectable> Selection = new();
        private Mesh _currentSelectionMesh = default;

        private void OnDestroy() => Dispose();
        private void Update() => ProcessInput();

        private void ProcessInput()
        {
            if (GameCursor.CursorVisible)
                ProcessHover(ref _hoverSelection);
            
            if (InputProvider.UnitSelectionDown)
                StartSelection();
            else if (InputProvider.UnitSelectionUp)
                EndSelection();
        }
        
        private void StartSelection()
        {
            _startPosition = Input.mousePosition;
            _selectionBox.enabled = true;
            Cursor.visible = false;
        }

        private void EndSelection()
        {
            _selectionBox.enabled = false;
            Cursor.visible = true;
            if(InputProvider.AdditiveModifier is false && InputProvider.SubtractiveModifier is false)
                ClearSelection();
            
            if (_dragSelection)
                StartCoroutine(BoxSelect());
            else
                ClickSelect().ForEach(ProcessSelectable);
        }

        private List<ISelectable> ClickSelect() => ClickCast().Select(hit => hit.rigidbody.GetComponent<ISelectable>()).ToList();

        private IEnumerable<RaycastHit> ClickCast()
        {
            var cam = GameData.Instance.Input.Provider.Camera;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            var distance = cam.farClipPlane;
            var start = ray.origin;
            var end = ray.origin + ray.direction * distance;
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.yellow, 3f);
            return _clickRadius > 0 ? Physics.CapsuleCastAll(start, end, _clickRadius, ray.direction, Mathf.Infinity, GameData.Instance.Settings.UnitLayer).Where(hit => hit.rigidbody != null) 
                : Physics.RaycastAll(start, end, Mathf.Infinity, GameData.Instance.Settings.UnitLayer).Where(hit => hit.rigidbody != null);
        }

        private IEnumerator BoxSelect()
        {
            _selectionCollider.sharedMesh = MeshBuilder.BoxSelect(_startPosition, _endPosition);
            yield return new WaitForFixedUpdate();
            Dispose();
        }

        private void OnTriggerEnter(Collider col)
        {
            // if (col.gameObject.layer != GameData.Instance.Settings.UnitLayer) return;
            if (col.attachedRigidbody == null) return;
            if (col.attachedRigidbody.TryGetComponent<ISelectable>(out var selectable))
                ProcessSelectable(selectable);
        }

        private void ProcessSelectable(ISelectable selectable)
        {
            if (selectable is null) return;
        
            if(InputProvider.SubtractiveModifier)
                Deselect();
            else
                Select();

            void Select()
            {
                if (Selection.Contains(selectable)) return;
                selectable.Select();
                Selection.Add(selectable);
            }
        
            void Deselect()
            {
                if (Selection.Contains(selectable) is false) return;
                selectable.Deselect();
                Selection.Remove(selectable);
            }
        }
        
        private static void ProcessHover(ref ISelectable current)
        {
            if (GameCursor.TryGetSelectable(out var newSelection))
            {
                if (current == newSelection) return;
                current?.HoverExit();
                newSelection.HoverEnter();
                current = newSelection;
            }
            else
            {
                current?.HoverExit();
                current = null;
            }
        }
    
        private void ClearSelection()
        {
            Selection.ForEach(selectable => selectable.Deselect());
            Selection.Clear();
        }

        public void Dispose()
        {
            if(_selectionCollider.sharedMesh is null) return;
            _selectionCollider.sharedMesh = null;
            Destroy(_currentSelectionMesh);
        }
    }
}