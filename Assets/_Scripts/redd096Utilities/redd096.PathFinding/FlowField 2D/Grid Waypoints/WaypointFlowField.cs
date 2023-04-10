using UnityEngine;
using redd096.Attributes;
using System.Collections.Generic;

namespace redd096.PathFinding.FlowField2D
{
    [AddComponentMenu("redd096/.PathFinding/FlowField 2D/Grid Waypoints/Waypoint FlowField 2D")]
    public class WaypointFlowField : MonoBehaviour
    {
        [HelpBox("If this waypoint can move, consider add WaypointsPositionUpdater")]
        [Header("Reachable waypoints from this")]
#pragma warning disable CS0414  //disable warning variables declared but not used
        [GridSelectable("reachableWaypoints")][SerializeField] bool gridSelectableEditor = default;
#pragma warning restore CS0414
        [HideInInspector] public Vector2Int[] reachableWaypoints = new Vector2Int[8] { new Vector2Int(-1, -1), Vector2Int.down, new Vector2Int(1, -1), Vector2Int.left, Vector2Int.right, new Vector2Int(-1, 1), Vector2Int.up, new Vector2Int(1, 1) };

        [Header("Waypoint")]
        public bool isWalkable = true;
        public int movementPenalty = 1;

        [Header("DEBUG")]
        [ReadOnly] public Vector2Int gridPosition;

        public Vector2 worldPosition => transform.position;

        /// <summary>
        /// Add or remove reachable waypoint
        /// </summary>
        /// <param name="direction">direction from this waypoint (using gridPosition)</param>
        /// <param name="isReachable"></param>
        public void SetReachableWaypoint(Vector2Int direction, bool isReachable)
        {
            //add or remove direction from the list
            List<Vector2Int> listReachableWaypoints = new List<Vector2Int>(reachableWaypoints);
            if (isReachable)
            {
                if (listReachableWaypoints.Contains(direction) == false)
                    listReachableWaypoints.Add(direction);
            }
            else
            {
                if (listReachableWaypoints.Contains(direction))
                    listReachableWaypoints.Remove(direction);
            }
            reachableWaypoints = listReachableWaypoints.ToArray();
        }
    }
}