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
            selection.BeginSelection();
        }

        if (Input.GetMouseButtonUp(0))
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
            // Rotate around y axis
            transform.Rotate(0, Input.GetAxis("Mouse X") * rotateSensitivity * Time.deltaTime, 0, Space.World);
        }
    }
}
