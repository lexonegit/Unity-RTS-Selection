using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSUnit : MonoBehaviour, ISelectable
{
    public void Select()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public void Deselect()
    {
        GetComponent<MeshRenderer>().material.color = Color.white;
    }
}
