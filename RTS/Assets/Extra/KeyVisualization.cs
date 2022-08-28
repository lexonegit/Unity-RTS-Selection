using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyVisualization : MonoBehaviour
{
    public TMP_Text infoText;
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            infoText.text = "HOLDING SHIFT";
        else
        if (Input.GetKey(KeyCode.LeftControl))
            infoText.text = "HOLDING CTRL";
        else
            infoText.text = "";
    }
}
