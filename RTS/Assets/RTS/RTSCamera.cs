using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple camera controller and input handler for demonstration purposes.
/// </summary>

public class RTSCamera : MonoBehaviour
{
    public RTSSelection selection;

    public float moveSpeed = 10f;
    public float rotateSensitivity = 200f;

    private void Update()
    {
        // Mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Different modes (Default, add, subtract)
            RTSSelection.SelectionMode mode = RTSSelection.SelectionMode.Default;

            if (Input.GetKey(KeyCode.LeftShift))
                mode = RTSSelection.SelectionMode.Add;
            else
            if (Input.GetKey(KeyCode.LeftControl))
                mode = RTSSelection.SelectionMode.Subtract;

            selection.BeginSelection(mode);
        }

        if (Input.GetMouseButtonUp(0)) // All selection confirms on mouse up
        {
            selection.ConfirmSelection();
        }

        // Movement
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 dir = Quaternion.Euler(0, transform.localEulerAngles.y, 0) * new Vector3(movement.x, 0, movement.y);
        transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);

        // Rotation
        if (Input.GetMouseButton(1))
        {
            float xRotation = Input.GetAxis("Mouse X");

            // Rotate around y axis
            transform.Rotate(0, xRotation * rotateSensitivity * Time.deltaTime, 0, Space.World);
        }

    }
}
