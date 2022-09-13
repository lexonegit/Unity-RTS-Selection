using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Simple RTS-style camera controller and input handler for demonstration purposes.
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
            // Don't begin selecting if clicking on UI
            // TODO: Exclude World space UI from this check
            if (IsPointerOverUIElement())
                return;

            // Different modes (Default, additive, subtractive)
            RTSSelection.SelectionModifier mode = RTSSelection.SelectionModifier.Default;

            if (Input.GetKey(KeyCode.LeftShift))
                mode = RTSSelection.SelectionModifier.Additive;
            else
            if (Input.GetKey(KeyCode.LeftControl))
                mode = RTSSelection.SelectionModifier.Subtractive;

            selection.BeginSelection(mode);
        }

        // All selection confirms on mouse up
        if (Input.GetMouseButtonUp(0) && selection.selecting)
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

    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        for (int index = 0; index < raycastResults.Count; index++)
        {
            RaycastResult curRaysastResult = raycastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }
}
