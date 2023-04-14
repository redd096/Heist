using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using redd096.Attributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace redd096.PathFinding.FlowField3D
{
    /// <summary>
    /// Grid used for pathfinding
    /// </summary>
    [AddComponentMenu("redd096/.PathFinding/FlowField 3D/Grid Waypoints/Grid FlowField Waypoints 3D")]
    public class GridFlowFieldWaypoints : GridBASE
    {
        [Header("Use every waypoint in scene or just array?")]
        [SerializeField] bool findWaypointsInScene = false;
        [SerializeField] WaypointFlowField[] waypoints = default;

        [Header("Grid")]
        [SerializeField] bool updateOnAwake = true;

        [Header("DEBUG - generate automatically grid")]
        [SerializeField] Vector2 gridWorldSize = Vector2.one;
        [SerializeField] WaypointFlowField prefab = default;
        [SerializeField] Vector2 spaceBetweenWaypoints = Vector2.one;

        [Header("Gizmos - cyan GridArea - red unwalkable - magenta obstacle - green walkable")]
        [SerializeField] bool drawGridArea = false;
        [SerializeField] bool drawUnwalkableNodes = false;
        [SerializeField] bool drawWalkableNodes = false;
        [SerializeField] bool drawCosts = false;
        [SerializeField] bool drawDirections = false;
        [SerializeField] float alphaNodes = 1f;
        [SerializeField] float nodeDiameter = 1f;
        //instantiate prefab from down left to up right
        [Button] void GenerateAutomaticallyGrid()
        { Vector3 worldBottomLeft = GridWorldPosition + (Vector3.left * gridWorldSize.x * 0.5f) + (Vector3.back * gridWorldSize.y * 0.5f);
            Vector2 halfSpace = spaceBetweenWaypoints * 0.5f;
            for (int x = 0; x < Mathf.RoundToInt(gridWorldSize.x / spaceBetweenWaypoints.x); x++)
            { for (int y = 0; y < Mathf.RoundToInt(gridWorldSize.y / spaceBetweenWaypoints.y); y++)
                { Vector3 worldPosition = worldBottomLeft + Vector3.right * (x * spaceBetweenWaypoints.x + halfSpace.x) + Vector3.forward * (y * spaceBetweenWaypoints.y + halfSpace.y);
                    WaypointFlowField waypoint = Instantiate(prefab, transform); waypoint.transform.position = worldPosition; } } }
        //find waypoints in scene and set in array
        [Button] void FindWaypointsInScene() => waypoints = FindObjectsOfType<WaypointFlowField>();
        //build grid
#if UNITY_EDITOR
        [Button] void UpdateNodes() { BuildGrid(); SceneView.RepaintAll(); Undo.RegisterFullObjectHierarchyUndo(gameObject, "Update Nodes"); }
#endif

        //grid
        Dictionary<Vector2Int, WaypointFlowField> gridWaypoints = new Dictionary<Vector2Int, WaypointFlowField>();

        void Awake()
        {
            //create grid
            if (updateOnAwake && IsGridCreated() == false)
                BuildGrid();
        }

        void OnDrawGizmos()
        {
            if (drawGridArea)
            {
                //draw area
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(GridWorldPosition, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
            }

            //draw every node in grid
            if (drawWalkableNodes || drawUnwalkableNodes || drawCosts || drawDirections)
            {
                if (grid != null)
                {
                    foreach (Node node in grid)
                    {
                        //set color if walkable or not (red = not walkable, green = walkable, magenta = walkable but with obstacles)
                        //Gizmos.color = new Color(1, 1, 1, alphaNodes) * (node.isWalkable ? (drawObstacles && node.GetObstaclesOnThisNode().Count > 0 ? Color.magenta : Color.green) : Color.red);
                        //Gizmos.DrawSphere(node.worldPosition, overlapRadius);

                        //draw if unwalkable
                        if (drawUnwalkableNodes && (node.IsWalkable == false))
                        {
                            Gizmos.color = new Color(1, 1, 1, alphaNodes) * Color.red;
                            Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                        }
                        //draw if walkable
                        else if (drawWalkableNodes && node.IsWalkable)
                        {
                            Gizmos.color = new Color(1, 1, 1, alphaNodes) * Color.green;
                            Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                        }

                        Gizmos.color = Color.white;

#if UNITY_EDITOR
                        //draw cost
                        if (drawCosts)
                            Handles.Label(node.worldPosition, node.bestCost.ToString());

                        //draw arrow direction
                        if (drawDirections)
                        {
                            float overlapRadius = (nodeDiameter * 0.5f) - 0.05f;
                            Gizmos.DrawWireCube(node.worldPosition + new Vector3(node.bestDirection.x, 0, node.bestDirection.y) * overlapRadius, Vector3.one * 0.05f);
                            if (node.bestDirection != Vector2Int.zero)
                                Gizmos.DrawLine(node.worldPosition, node.worldPosition + new Vector3(node.bestDirection.x, 0, node.bestDirection.y) * overlapRadius);
                            //Handles.ArrowHandleCap(0, node.worldPosition, Quaternion.LookRotation(new Vector3(node.bestDirection.x, 0, node.bestDirection.y)), overlapRadius, EventType.Repaint);
                        }
#endif
                    }
                }
            }
        }

        #region create grid

        protected override void SetGrid()
        {
            //find waypoints in scene
            if (findWaypointsInScene)
                waypoints = FindObjectsOfType<WaypointFlowField>();
        }

        protected override void CreateGrid()
        {
            //order on y then x
            WaypointFlowField[] waypointsByOrder = waypoints.OrderBy(waypoint => Mathf.RoundToInt(waypoint.worldPosition.z))
                .ThenBy(waypoint => Mathf.RoundToInt(waypoint.worldPosition.x)).ToArray();

            //reset grid
            grid = null;
            gridWaypoints.Clear();

            //be sure there is something before start
            if (waypointsByOrder == null || waypointsByOrder.Length <= 0)
                return;

            //create waypoints grid
            int currentZ = Mathf.RoundToInt(waypointsByOrder[0].worldPosition.z);
            int coordinatesX = 0;
            int coordinatesY = 0;
            int maxX = 0;
            int maxY = 0;
            for (int i = 0; i < waypointsByOrder.Length; i++)
            {
                WaypointFlowField currentWaypoint = waypointsByOrder[i];

                //if go to next row, reset x and increase y
                if (Mathf.RoundToInt(currentWaypoint.worldPosition.z) > currentZ)
                {
                    coordinatesX = 0;
                    coordinatesY++;
                    currentZ = Mathf.RoundToInt(currentWaypoint.worldPosition.z);
                }

                //add to grid and increase x
                currentWaypoint.gridPosition = new Vector2Int(coordinatesX, coordinatesY);
                gridWaypoints.Add(currentWaypoint.gridPosition, currentWaypoint);
                coordinatesX++;
                if (coordinatesX > maxX)
                    maxX = coordinatesX;
            }
            maxY = coordinatesY + 1;

            //create grid
            grid = new Node[maxX, maxY];
            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    //set new node in grid
                    WaypointFlowField waypoint = gridWaypoints[new Vector2Int(x, y)];
                    grid[x, y] = new Node(waypoint.isWalkable, waypoint.isWalkable, waypoint.worldPosition, x, y, waypoint.movementPenalty);
                }
            }
        }

        protected override void SetNeighbours()
        {
            //set neighbours for every node
            Vector2Int checkCoordinates = Vector2Int.zero;
            foreach (Node node in grid)
            {
                node.neighboursCardinalDirections.Clear();
                node.neighbours.Clear();

                foreach (Vector2Int coordinates in GetWaypointFromNode(node).reachableWaypoints)
                {
                    //this is the node we are using as parameter
                    if (coordinates.x == 0 && coordinates.y == 0)
                        continue;

                    //find grid position
                    checkCoordinates.x = node.gridPosition.x + coordinates.x;
                    checkCoordinates.y = node.gridPosition.y + coordinates.y;

                    //if that position is inside the grid, add to neighbours
                    if (gridWaypoints.ContainsKey(checkCoordinates))
                    {
                        node.neighbours.Add(grid[checkCoordinates.x, checkCoordinates.y]);

                        //set another list with neighbours only in cardinal directions (up, down, left, right)
                        if (coordinates.x == 0 || coordinates.y == 0)
                            node.neighboursCardinalDirections.Add(grid[checkCoordinates.x, checkCoordinates.y]);
                    }
                }
            }
        }

        #endregion

        #region public API

        /// <summary>
        /// Set best direction for every node in the grid, to target node
        /// </summary>
        /// <param name="targetRequests"></param>
        /// <param name="canMoveDiagonal">can move diagonal or only horizontal and vertical?</param>
        public override void SetFlowField(TargetRequest[] targetRequests, bool canMoveDiagonal)
        {
            ResetFlowFieldGrid();
            SetBestCosts(targetRequests);
            SetBestDirections(canMoveDiagonal);
        }

        /// <summary>
        /// Get node from world position (or nearest one)
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public override Node GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            Vector2Int nearestCoordinates = default;
            float distance = -1;

            //foreach waypoint in the dictionary
            foreach (Vector2Int coordinates in gridWaypoints.Keys)
            {
                //check distance to find nearest
                float newDistance = Vector3.Distance(grid[coordinates.x, coordinates.y].worldPosition, worldPosition);
                if (distance < 0 || newDistance < distance)
                {
                    distance = newDistance;
                    nearestCoordinates = coordinates;
                }
            }

            //return node
            return grid[nearestCoordinates.x, nearestCoordinates.y];
        }

        public WaypointFlowField GetWaypointFromNode(Node node)
        {
            return gridWaypoints[node.gridPosition];
        }

        public Node GetNodeFromWaypoint(WaypointFlowField waypoint)
        {
            return grid[waypoint.gridPosition.x, waypoint.gridPosition.y];
        }

        public void SetReachableNode(Node startNode, Vector2Int direction, bool isReachable)
        {
            //add to list of reachable nodes
            GetWaypointFromNode(startNode).SetReachableWaypoint(direction, isReachable);

            //and update neighbours no node
            Node nodeToReach = GetNodeByCoordinates(startNode.gridPosition.x + direction.x, startNode.gridPosition.y + direction.y);
            if (isReachable)
            {
                if (startNode.neighbours.Contains(nodeToReach) == false)
                    startNode.neighbours.Add(nodeToReach);
                if ((direction.x == 0 || direction.y == 0) && startNode.neighboursCardinalDirections.Contains(nodeToReach) == false)
                    startNode.neighboursCardinalDirections.Add(nodeToReach);
            }
            else
            {
                if (startNode.neighbours.Contains(nodeToReach))
                    startNode.neighbours.Remove(nodeToReach);
                if (startNode.neighboursCardinalDirections.Contains(nodeToReach))
                    startNode.neighboursCardinalDirections.Remove(nodeToReach);
            }
        }

        #endregion
    }
}