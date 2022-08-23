using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physics based RTS-style selection script.
/// </summary>

public class RTSSelection : MonoBehaviour
{
    [Header("References")]
    public MeshCollider selectionCollider;

    [Header("UI")]
    public RectTransform selectionRect;

    [Header("Settings")]
    public LayerMask raycastLayerMask;
    public float minSelectionArea = 0.001f;
    public float maxDistance = 500f;

    // Private
    private List<ISelectable> selectedObjects = new List<ISelectable>();
    private Ray ray;
    private RaycastHit hit;
    private Vector2 p1, p2;
    private bool selecting = false;

    /// <summary>
    /// Begins the selection process. 
    /// </summary>
    public void BeginSelection()
    {
        selecting = true;
        selectionRect.gameObject.SetActive(true);

        StartCoroutine(UpdateSelection(Input.mousePosition));
    }

    private IEnumerator UpdateSelection(Vector2 initialMousePos)
    {
        // Update the selection rectangle every frame until it is confirmed
        while (selecting)
        {
            p1 = initialMousePos;
            p2 = Input.mousePosition;

            // Update UI
            float w = p2.x - p1.x;
            float h = p2.y - p1.y;
            selectionRect.anchoredPosition = p1 + new Vector2(w / 2, h / 2);
            selectionRect.sizeDelta = new Vector2(Mathf.Abs(w), Mathf.Abs(h));

            yield return null; // Wait for next frame
        }
    }

    /// <summary>
    /// Confirms the current selection and selects anything that is in the selection area.
    /// </summary>
    public void ConfirmSelection()
    {
        selecting = false;
        selectionRect.gameObject.SetActive(false);
        DeselectAll(); // Clear previous selection

        // Update selection
        Vector3[] vertices = new Vector3[8];
        Vector2[] corners = CreateCorners(p1, p2);

        // If corners is null then it means that the selection area is too small
        if (corners == null)
            return;

        int index = 0;
        for (int i = 0; i < corners.Length; ++i)
        {
            ray = Camera.main.ScreenPointToRay(corners[i]);

            // Raycast towards the corner
            bool raycast = Physics.Raycast(ray, out hit, maxDistance, raycastLayerMask);

            // If nothing was hit, use the max distance point
            Vector3 hitPoint = raycast ? hit.point : ray.GetPoint(maxDistance);

            vertices[index] = hitPoint; // Vertices at the end of the ray
            vertices[index + 4] = ray.origin - hitPoint; // Vertices closest to the camera

            // If raycast hit something => green line, otherwise yellow line
            Debug.DrawLine(Camera.main.ScreenToWorldPoint(corners[i]), hitPoint, raycast ? Color.green : Color.yellow, 3f);

            index++;
        }

        // Create the selection collider
        Mesh mesh = CreateSelectionMesh(vertices);

        StartCoroutine(SetSelectionColliderMesh(mesh));
    }

    private IEnumerator SetSelectionColliderMesh(Mesh mesh)
    {
        // Set the collider mesh
        selectionCollider.sharedMesh = mesh;

        // Wait 1 physics update (otherwise the collision trigger won't fire)
        yield return new WaitForFixedUpdate();

        // Remove the collider mesh, it's no longer needed
        selectionCollider.sharedMesh = null;
        Destroy(mesh);
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

        // If the selection is too small, return null,
        // Unity will throw errors otherwise.
        if (width < minSelectionArea || height < minSelectionArea)
            return null;

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

    private void OnTriggerEnter(Collider col)
    {
        ISelectable selectable = col.GetComponent<ISelectable>();

        if (selectable == null)
            return;

        Select(selectable);
    }

    private void Select(ISelectable selectable)
    {
        selectable.Select();

        selectedObjects.Add(selectable);
    }

    private void Deselect(ISelectable selectable)
    {
        selectable.Deselect();

        selectedObjects.Remove(selectable);
    }

    private void DeselectAll()
    {
        for (int i = 0; i < selectedObjects.Count; ++i)
            selectedObjects[i].Deselect();

        selectedObjects.Clear();
    }
}
