using RTS.Selection;
using UnityEngine;
using UnityEngine.AI;

namespace RTS.Core
{
    public class GameCursor
    {
        public static bool CursorVisible => Cursor.visible;
        public static Vector3 NormalizedPosition => new(Mathf.Clamp01(Input.mousePosition.x / Screen.width), Mathf.Clamp01(Input.mousePosition.y / Screen.height), 0);
        private static Ray CursorRay => GameData.Instance.Input.Provider.Camera.ViewportPointToRay(NormalizedPosition);
        private static Camera Camera => GameData.Instance.Input.Provider.Camera;
        private static LayerMask GroundLayer => GameData.Instance.Settings.GroundLayer;
        private static LayerMask UnitLayer => GameData.Instance.Settings.UnitLayer;
    
        public static bool TryGetSelectable(out ISelectable selectable)
        {
            selectable = null;
            var rayHit = Physics.Raycast(CursorRay, out var hitInfo, Camera.farClipPlane, UnitLayer, QueryTriggerInteraction.Collide);
            if (rayHit && hitInfo.rigidbody) 
                selectable = hitInfo.rigidbody.GetComponent<ISelectable>();

            return rayHit && selectable != null;
        }
    
        public static bool GetNavMeshPoint(out Vector3 location, int navMesh = NavMesh.AllAreas)
        {
            location = Vector3.zero;
            if (GetGroundPoint(out var groundHitPoint) is false) 
                return false;
            if (NavMesh.SamplePosition(groundHitPoint, out var navMeshHitInfo, 100, navMesh) is false) 
                return false;
            location = navMeshHitInfo.position;
            return true;
        }
    
        private static bool GetGroundPoint(out Vector3 location)
        {
            location = Vector3.zero;
            var rayHit = Physics.Raycast(CursorRay, out var hitInfo, Camera.farClipPlane, GroundLayer, QueryTriggerInteraction.Ignore);
            if (rayHit) location = hitInfo.point;
            return rayHit;
        }
    }
}