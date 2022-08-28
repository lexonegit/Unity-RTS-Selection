using UnityEngine;

public interface ISelectable
{
    GameObject gameObject { get; }
    void Select();
    void Deselect();
}