using UnityEngine;

namespace RTS.Selection
{
    public interface ISelectable
    {
        GameObject gameObject { get; }
        bool Selected { get; }
        bool Hovering { get; }
        void Select();
        void Deselect();
        void HoverEnter();
        void HoverExit();
    }
}