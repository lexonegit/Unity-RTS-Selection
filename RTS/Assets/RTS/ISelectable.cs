using UnityEngine;

public interface ISelectable
{
    int Team { get; }
    GameObject gameObject { get; }
    void Select();
    void Deselect();
}