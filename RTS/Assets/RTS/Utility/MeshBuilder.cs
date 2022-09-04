using UnityEngine;

namespace RTS.Utility
{
    public static class MeshBuilder
    {
        public const int MinimumBoxSelectSize = 2;

        private static readonly int[] CubeTriangles = {
            0, 1, 2, 2, 1, 3, 4, 6, 0, 0, 6, 2, 6, 7, 2, 2, 7, 3,
            7, 5, 3, 3, 5, 1, 5, 0, 1, 1, 4, 0, 4, 5, 6, 6, 5, 7
        };
        
        public static Mesh BoxSelect(Vector2 start, Vector2 end)
        {
            return new Mesh
            {
                name = "SelectionMesh",
                vertices = MeshVerts(),
                triangles = CubeTriangles
            };
            
            Vector3[] MeshVerts()
            {
                var verts = BuildVerts(start, end);
                var meshVerts = new Vector3[8];

                // Create vertices for the selection mesh
                for (var i = 0; i < 4; ++i)
                {
                    meshVerts[i] = verts[i];
                    meshVerts[i + 4] = verts[i] + verts[i + 4];
                }

                return meshVerts;
            }

            Vector3[] BuildVerts(Vector2 start, Vector2 end)
            {
                // Update selection
                var vertices = new Vector3[8];
                var corners = CreateCorners(start, end);

                for (var i = 0; i < corners.Length; ++i)
                {
                    var ray = Camera.main.ScreenPointToRay(corners[i]);
                    var raycast = Physics.Raycast(ray, out var hit, Camera.main.farClipPlane);
                    var hitPoint = raycast ? hit.point : ray.GetPoint(Camera.main.farClipPlane);
                    vertices[i] = hitPoint; // Vertices at the end of the ray
                    vertices[i + 4] = ray.origin - hitPoint; // Vertices closest to the camera
                    Debug.DrawLine(Camera.main.ScreenToWorldPoint(corners[i]), hitPoint, raycast ? Color.green : Color.yellow, 3f);
                }

                return vertices;
            }
            
            Vector2[] CreateCorners(Vector2 start, Vector2 end)
            {
                // Min and Max are used to get the 2 corners, regardless of drag direction.
                var bottomLeft = Vector3.Min(start, end);
                var topRight = Vector3.Max(start, end);

                Vector2[] corners = {
                    new(bottomLeft.x, topRight.y), // Top-left
                    new(topRight.x, topRight.y), // Top-right
                    new(bottomLeft.x, bottomLeft.y), // Bottom-left
                    new(topRight.x, bottomLeft.y) // Bottom-right
                };

                var width = (corners[0] - corners[1]).magnitude;
                var height = (corners[0] - corners[2]).magnitude;

                // Debug.Log("True selection dimensions: " + width + " x " + height);

                // Selection size verifying
                // The mesh generation will fail if the selection size is too small (<1 ish)
                // That's why the following procedure is needed.

                // If width is less than the minimum, adjust it to be the same as the minimum
                if (width < MinimumBoxSelectSize)
                {
                    var diff = MinimumBoxSelectSize - width;

                    corners[0].x -= diff * 0.5f;
                    corners[1].x += diff * 0.5f;
                    corners[2].x -= diff * 0.5f;
                    corners[3].x += diff * 0.5f;
                }

                // If height is less than the minimum, adjust it to be the same as the minimum
                if (height < MinimumBoxSelectSize)
                {
                    var diff = MinimumBoxSelectSize - height;
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
        }
    }
}