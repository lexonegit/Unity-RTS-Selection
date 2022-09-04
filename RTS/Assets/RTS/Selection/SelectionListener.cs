using UnityEngine;

namespace RTS.Selection
{
    [RequireComponent(typeof(SelectionReceiver))]
    public abstract class SelectionListener : MonoBehaviour
    {
        protected SelectionReceiver _receiver = default;

        protected void Awake()
        {
            _receiver = GetComponent<SelectionReceiver>();
        }

        protected virtual void OnEnable()
        {
            _receiver.OnSelect += OnSelect;
            _receiver.OnDeselect += OnDeselect;
            _receiver.OnHoverEnter += OnHoverEnter;
            _receiver.OnHoverExit += OnHoverExit;
        }
        
        protected virtual void OnDisable()
        {
            _receiver.OnSelect -= OnSelect;
            _receiver.OnDeselect -= OnDeselect;
            _receiver.OnHoverEnter -= OnHoverEnter;
            _receiver.OnHoverExit -= OnHoverExit;
        }

        protected abstract void OnSelect();
        protected abstract void OnDeselect();
        protected abstract void OnHoverEnter();
        protected abstract void OnHoverExit();
    }
}