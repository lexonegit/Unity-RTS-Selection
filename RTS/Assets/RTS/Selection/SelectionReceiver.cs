using System;
using UnityEngine;

namespace RTS.Selection
{
    public class SelectionReceiver : MonoBehaviour, ISelectable
    {
        public bool Selected { get; private set; }
        public bool Hovering { get; private set; }
        public Action OnSelect = delegate { };
        public Action OnDeselect = delegate { };
        public Action OnHoverEnter = delegate { };
        public Action OnHoverExit = delegate { };
        
        public void Select()
        {
            if (Selected) return;
            Selected = true;
            OnSelect();
        }

        public void Deselect()
        {
            if (Selected == false) return;
            Selected = false;
            OnDeselect();
        }

        public void HoverEnter()
        {
            Hovering = true;
            OnHoverEnter();
        }

        public void HoverExit()
        {
            Hovering = false;
            OnHoverExit();
        }
    }
}