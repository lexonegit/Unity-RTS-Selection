using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physics based RTS-style selection script.
/// Repository: https://github.com/lexonegit/Unity-RTS-Selection
/// </summary>

public class RTSSelection : MonoBehaviour
{
    public enum SelectionModifier { Default, Additive, Subtractive };
    public enum DefaultModeStartBehavior { Keep, Clear };

    [Header("References")]
    public MeshCollider selectionCollider;

    [Header("UI")]
    public RectTransform selectionRect;

    [Header("Settings")]
    public int team = -1;
    public LayerMask raycastLayerMask;
    public float raycastMaxDistance = 500f;

    [Header("Multi select settings")]
    [Range(1, 64), Tooltip("The minimum width & height for the selection area (must be at least 1)")]
    public int minSelectionSize = 2;

    [Header("Single select settings")]
    [Range(0f, 5f), Tooltip("Radius for single selection (capsule cast) If 0 then a normal raycast is used instead")]
    public float singleSelectionRadius = 0f;

    [Tooltip("Keep the current selection or clear it when starting a new selection")]
    public DefaultModeStartBehavior defaultModeStartBehavior = DefaultModeStartBehavior.Keep;

    // Private
    private SelectionModifier selectionModifier = SelectionModifier.Default;
    private Mesh currentSelectionMesh;
    private Ray ray;
    private RaycastHit hit;
    private RaycastHit[] hits;
    private Vector2 p1, p2;
    public bool selecting = false;
    public bool multiSelecting = false;

    [HideInInspector] public List<ISelectable> detectedObjects = new List<ISelectable>();
    [HideInInspector] public List<ISelectable> toBeSelected = new List<ISelectable>();

    /// <summary>
    /// Starts the selection process (mouse down)
    /// </summary>
    public void BeginSelection(SelectionModifier mode)
    {
        // Avoid starting more than one selection process at a time
        if (selecting)
            return;

        selecting = true;
        selectionModifier = mode;

        if (defaultModeStartBehavior == DefaultModeStartBehavior.Clear && selectionModifier == SelectionModifier.Default)
            DeselectAll(true);

        StartCoroutine(UpdateMultiSelection(Input.mousePosition));
    }

    /// <summary>
    /// Confirms the selection (mouse up)
    /// </summary>
    public void ConfirmSelection()
    {
        if (multiSelecting)
            ConfirmMultiSelection();
        else
            ConfirmSingleSelection();
    }

    private IEnumerator UpdateMultiSelection(Vector3 initialMousePos)
    {
        // Start by detecting when multi selection starts (dragging)
        while (selecting)
        {
            if (initialMousePos == Input.mousePosition)
            {
                yield return null;
                continue;
            }

            // Mouse moved => it's a multi selection => move to the next step
            multiSelecting = true;
            break;
        }

        if (!multiSelecting) // Selection ended but it wasn't a multi selection
            yield break;

        // Multi selection started, show the selection rect UI
        selectionRect.gameObject.SetActive(true);

        while (selecting)
        {
            // Update the selection rectangle every frame until it is confirmed
            p1 = initialMousePos;
            p2 = Input.mousePosition;

            // Update UI dimensions
            float w = p2.x - p1.x;
            float h = p2.y - p1.y;
            selectionRect.anchoredPosition = p1 + new Vector2(w / 2, h / 2);
            selectionRect.sizeDelta = new Vector2
            (
                Mathf.Clamp(Mathf.Abs(w), minSelectionSize, Mathf.Infinity),
                Mathf.Clamp(Mathf.Abs(Mathf.Abs(h)), minSelectionSize, Mathf.Infinity)
            );

            yield return null; // Wait for next frame
        }

        // Selection concluded, hide the selection rect UI
        selectionRect.gameObject.SetActive(false);
    }

    private void ConfirmSingleSelection()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Raycast
        // If singleSelectionRadius is more than 0 then use a capsule cast, otherwise a normal raycast
        hits = singleSelectionRadius > 0
            ? Physics.CapsuleCastAll(ray.origin, ray.origin + ray.direction * raycastMaxDistance, singleSelectionRadius, ray.direction)
            : Physics.RaycastAll(ray.origin, ray.direction, raycastMaxDistance);

        Debug.DrawRay(ray.origin, ray.direction * raycastMaxDistance, Color.magenta, 3f);

        // Iterate hits
        for (int i = 0; i < hits.Length; ++i)
        {
            ISelectable selectable = hits[i].collider.GetComponentInParent<ISelectable>();

            if (selectable == null)
                continue;

            StartCoroutine(ProcessRaycastHit(selectable));
            return; // We are only interested in the first hit, so we can stop here
        }

        // If we get here then no valid selectable was hit
        if (selectionModifier == SelectionModifier.Default)
            DeselectAll(true);

        Cleanup();
    }

    private void ConfirmMultiSelection()
    {
        // Update selection
        Vector3[] vertices = new Vector3[8];
        Vector2[] corners = CreateCorners(p1, p2);

        int index = 0;
        for (int i = 0; i < corners.Length; ++i)
        {
            ray = Camera.main.ScreenPointToRay(corners[i]);

            // Raycast towards the corner
            bool raycast = Physics.Raycast(ray, out hit, raycastMaxDistance, raycastLayerMask);

            // If nothing was hit, use the max distance point
            Vector3 hitPoint = raycast ? hit.point : ray.GetPoint(raycastMaxDistance);

            vertices[index] = hitPoint; // Vertices at the end of the ray
            vertices[index + 4] = ray.origin - hitPoint; // Vertices closest to the camera

            // If raycast hit something => green line, otherwise yellow line
            Debug.DrawLine(Camera.main.ScreenToWorldPoint(corners[i]), hitPoint, raycast ? Color.green : Color.yellow, 3f);

            index++;
        }

        // Create the selection collider mesh
        currentSelectionMesh = CreateSelectionMesh(vertices);

        // Set the collider mesh
        selectionCollider.sharedMesh = currentSelectionMesh;

        StartCoroutine(ProcessTriggerHits());
    }

    private IEnumerator ProcessRaycastHit(ISelectable selectable)
    {
        // Clear previous selection
        detectedObjects.Clear();

        // Technically raycasts could be processed instantly on this frame,
        // but for the sake of consistency we use the same logic as in multi selection,
        // which is to register the selection on the next frame.
        // You can comment out this line if you want to process raycasts instantly
        yield return new WaitForFixedUpdate();

        // Add the raycast hit
        TryAddToDetected(selectable);

        FinalizeSelection();
    }

    private IEnumerator ProcessTriggerHits()
    {
        // Clear previous selection
        detectedObjects.Clear();

        // Wait 1 physics update (otherwise the collision trigger won't fire)
        yield return new WaitForFixedUpdate();

        FinalizeSelection();
    }

    private void FinalizeSelection()
    {
        // Procedure explanation
        // 1. Deselect previously selected objects
        // 2. Process what objects should be kept/added/removed => add to toBeSelected
        // 3. Select all the objects in toBeSelected

        // detectedObjects contains all the objects that were hit by the selection in THIS "frame" (single or multi)
        // toBeSelected contains all the objects that are going to stay OR become selected now

        // 1. Deselect previously selected objects
        DeselectAll();

        // 2. Process what objects should be kept/added/removed => add to toBeSelected
        if (selectionModifier == SelectionModifier.Default)
        {
            // Default selection mode => override everything
            toBeSelected = new List<ISelectable>(detectedObjects);
        }
        else
        {
            // Additive and subtractive selection mode => add/remove from the current selection
            for (int i = 0; i < detectedObjects.Count; ++i)
                ProcessSelectable(detectedObjects[i]);
        }

        // 3. Select all the objects in toBeSelected
        for (int i = 0; i < toBeSelected.Count; ++i)
            toBeSelected[i].Select();


        // Selection complete! => do some cleaning up
        Cleanup();
    }

    private void ProcessSelectable(ISelectable selectable)
    {
        switch (selectionModifier)
        {
            // Add if it's not already added
            case SelectionModifier.Additive:
                TryAddToSelected(selectable);
                break;

            // Remove if it has been added
            case SelectionModifier.Subtractive:
                TryRemoveFromSelected(selectable);
                break;
        }
    }

    private void Cleanup()
    {
        multiSelecting = false;
        selecting = false;
        detectedObjects.Clear();

        // Remove the selection mesh and collider (no longer needed)
        if (currentSelectionMesh != null)
        {
            selectionCollider.sharedMesh = null;
            Destroy(currentSelectionMesh);
        }
    }

    private void DeselectAll(bool clear = false)
    {
        for (int i = 0; i < toBeSelected.Count; ++i)
            toBeSelected[i].Deselect();

        if (clear)
            toBeSelected.Clear();
    }

    private void TryAddToDetected(ISelectable selectable)
    {
        if (selectable == null)
            return;

        if (detectedObjects.Contains(selectable))
            return;

        // Only the same "team" objects can be selected
        if (selectable.Team != team)
            return;

        detectedObjects.Add(selectable);
    }

    private void TryAddToSelected(ISelectable selectable)
    {
        if (!toBeSelected.Contains(selectable))
            toBeSelected.Add(selectable);
    }

    private void TryRemoveFromSelected(ISelectable selectable)
    {
        if (toBeSelected.Contains(selectable))
            toBeSelected.Remove(selectable);
    }

    private Vector2[] CreateCorners(Vector2 p1, Vector2 p2)
    {
        // Min and Max are used to get the 2 corners, regardless of drag direction.
        var bottomLeft = Vector3.Min(p1, p2);
        var topRight = Vector3.Max(p1, p2);

        Vector2[] corners = new Vector2[]
        {
            new Vector2(bottomLeft.x, topRight.y), // Top-left
            new Vector2(topRight.x, topRight.y), // Top-right
            new Vector2(bottomLeft.x, bottomLeft.y), // Bottom-left
            new Vector2(topRight.x, bottomLeft.y) // Bottom-right
        };

        float width = (corners[0] - corners[1]).magnitude;
        float height = (corners[0] - corners[2]).magnitude;

        // Debug.Log("True selection dimensions: " + width + " x " + height);

        // Selection size verifying
        // The mesh generation will fail if the selection size is too small (<1 ish)
        // That's why the following procedure is needed. Is there a better way of doing this...?

        // If width is less than the minimum, adjust it to be the same as the minimum
        if (width < minSelectionSize)
        {
            float diff = minSelectionSize - width;

            corners[0].x -= diff * 0.5f;
            corners[1].x += diff * 0.5f;
            corners[2].x -= diff * 0.5f;
            corners[3].x += diff * 0.5f;
        }

        // If height is less than the minimum, adjust it to be the same as the minimum
        if (height < minSelectionSize)
        {
            float diff = minSelectionSize - height;

            corners[0].y += diff * 0.5f;
            corners[1].y += diff * 0.5f;
            corners[2].y -= diff * 0.5f;
            corners[3].y -= diff * 0.5f;
        }

        // Debug.Log("Adjusted selection dimensions: " + (corners[0] - corners[1]).magnitude; + " x " + (corners[0] - corners[2]).magnitude);

        return corners;
    }

    private static int[] cubeTriangles = {
        0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3,
        7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7
    };
    private Mesh CreateSelectionMesh(Vector3[] verts)
    {
        Vector3[] meshVerts = new Vector3[8];

        // Create vertices for the selection mesh
        for (int i = 0; i < 4; ++i)
        {
            meshVerts[i] = verts[i];
            meshVerts[i + 4] = verts[i] + verts[i + 4];
        }

        Mesh mesh = new Mesh();
        mesh.name = "SelectionMesh";
        mesh.vertices = meshVerts;
        mesh.triangles = cubeTriangles;

        return mesh;
    }

    //
    // UNITY EVENTS
    //

    private void OnTriggerEnter(Collider col)
    {
        ISelectable selectable = col.GetComponentInParent<ISelectable>();
        TryAddToDetected(selectable);
    }
}