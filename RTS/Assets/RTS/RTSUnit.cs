using UnityEngine;

public class RTSUnit : MonoBehaviour, ISelectable
{
    public int team = -1;
    public int Team
    {
        get { return team; }
    }

    public void Select()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public void Deselect()
    {
        GetComponent<MeshRenderer>().material.color = Color.white;
    }

    // This is extra
    private void Start()
    {
        GetComponent<MeshRenderer>().material.color = Color.white;
    }
}
