using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physics based RTS-style selection script.
/// </summary>

public class RTSSelection : MonoBehaviour
{
    public enum SelectionMode { Default, Add, Subtract };

    [Header("References")]
    public MeshCollider selectionCollider;

    [Header("UI")]
    public RectTransform selectionRect;

    [Header("Settings")]
    public LayerMask raycastLayerMask;
    public float raycastMaxDistance = 500f;

    [Header("Multi select settings")]
    [Range(1, 64), Tooltip("The minimum width & height for the selection area (must be at least 1)")]
    public int minSelectionSize = 2;

    [Header("Single select settings")]
    [Range(0f, 5f), Tooltip("Radius for single selection (capsule cast) If 0 then a normal raycast is used instead")]
    public float singleSelectionRadius = 0f;

    [HideInInspector] public List<ISelectable> selectedObjects = new List<ISelectable>();

    // Private
    private SelectionMode selectionMode = SelectionMode.Default;
    private Mesh currentSelectionMesh;
    private Ray ray;
    private RaycastHit hit;
    private RaycastHit[] hits;
    private Vector2 p1, p2;
    private bool selecting = false;
    private bool multiSelect = false;


    /// <summary>
    /// Starts the selection process (mouse down)
    /// </summary>
    public void BeginSelection(SelectionMode mode)
    {
        selecting = true;
        multiSelect = false;
        selectionMode = mode;

        StartCoroutine(UpdateMultiSelection(Input.mousePosition));
    }

    /// <summary>
    /// Confirms the selection (mouse up)
    /// </summary>
    public void ConfirmSelection()
    {
        selecting = false;
        Cleanup(); // Destroy previous selection mesh (if it exists)

        // Clear previous selection, but only if using default selection mode
        if (selectionMode == SelectionMode.Default)
            ClearSelection();

        if (multiSelect)
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

            // Mouse moved = it's a multi selection => move to the next step
            multiSelect = true;
            break;
        }

        if (!multiSelect) // Selection ended but it wasn't a multi selection
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

        // Pick the first valid selectable hit from the raycast
        for (int i = 0; i < hits.Length; ++i)
        {
            ISelectable selectable = hits[i].collider.GetComponentInParent<ISelectable>();

            if (selectable == null)
                continue;

            HandleSelectable(selectable);
            break;
        }
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

        // Create the selection collider
        currentSelectionMesh = CreateSelectionMesh(vertices);

        StartCoroutine(SetSelectionColliderMesh());
    }

    private IEnumerator SetSelectionColliderMesh()
    {
        // Set the collider mesh
        selectionCollider.sharedMesh = currentSelectionMesh;

        // Wait 1 physics update (otherwise the collision trigger won't fire)
        yield return new WaitForFixedUpdate();

        // Cleanup, if it's still needed (another selection process could have already interrupted this one)
        if (selectionCollider.sharedMesh != null)
            Cleanup();
    }

    private void OnTriggerEnter(Collider col)
    {
        ISelectable selectable = col.GetComponentInParent<ISelectable>();

        if (selectable == null)
            return;

        HandleSelectable(selectable);
    }

    private void Cleanup()
    {
        // Remove the collider mesh, it's no longer needed
        selectionCollider.sharedMesh = null;
        Destroy(currentSelectionMesh);
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
        // That's why the following procedure is needed.

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

        // width = (corners[0] - corners[1]).magnitude;
        // height = (corners[0] - corners[2]).magnitude;
        // Debug.Log("Adjusted selection dimensions: " + width + " x " + height);

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

    private void HandleSelectable(ISelectable selectable)
    {
        switch (selectionMode)
        {
            case SelectionMode.Default:
                Select(selectable);
                break;
            case SelectionMode.Add:
                Select(selectable);
                break;
            case SelectionMode.Subtract:
                Deselect(selectable);
                break;
        }
    }

    private void Select(ISelectable selectable)
    {
        // Check if this was already selected
        // Your target objects might have more than 1 collider attached, so this is needed
        if (selectedObjects.Contains(selectable))
            return;

        selectable.Select();

        selectedObjects.Add(selectable);
    }

    private void Deselect(ISelectable selectable)
    {
        selectable.Deselect();

        selectedObjects.Remove(selectable);
    }

    private void ClearSelection()
    {
        for (int i = 0; i < selectedObjects.Count; ++i)
            selectedObjects[i].Deselect();

        selectedObjects.Clear();
    }
}