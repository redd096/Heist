using System.Collections.Generic;
using UnityEngine;

namespace redd096.PathFinding.FlowField3D
{
    /// <summary>
    /// Used to update position of these waypoints for the grid
    /// </summary>
    [AddComponentMenu("redd096/.PathFinding/FlowField 2D/Grid Waypoints/Waypoints Position Updater 3D")]
    public class WaypointsPositionUpdater : MonoBehaviour
    {
        [Header("Default get components in children")]
        [SerializeField] WaypointFlowField[] waypoints = default;

        Dictionary<WaypointFlowField, Vector3> positions = new Dictionary<WaypointFlowField, Vector3>();

        private void Start()
        {
            //get references
            if (waypoints == null || waypoints.Length <= 0)
                waypoints = GetComponentsInChildren<WaypointFlowField>();

            //save previous positions
            foreach (WaypointFlowField waypoint in waypoints)
            {
                if (waypoint && positions.ContainsKey(waypoint) == false)
                    positions.Add(waypoint, waypoint.transform.position);
            }
        }

        void Update()
        {
            foreach (WaypointFlowField waypoint in positions.Keys)
            {
                //if moved
                if (waypoint && waypoint.transform.position != positions[waypoint])
                {
                    //update node position on grid
                    UpdatePosition(waypoint);
                }
            }
        }

        void UpdatePosition(WaypointFlowField waypoint)
        {
            //if there is a grid
            if (PathFindingFlowField.instance && PathFindingFlowField.instance.Grid != null && PathFindingFlowField.instance.Grid.IsGridCreated())
            {
                //and it's waypoint grid
                if (PathFindingFlowField.instance.Grid is GridFlowFieldWaypoints grid)
                {
                    //update position on node
                    positions[waypoint] = waypoint.transform.position;
                    Node node = grid.GetNodeFromWaypoint(waypoint);
                    if (node != null)
                        node.worldPosition = positions[waypoint];
                }
            }
        }
    }
}