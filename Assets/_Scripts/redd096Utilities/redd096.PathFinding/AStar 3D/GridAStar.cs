//miss smooth penalty https://www.youtube.com/watch?v=Tb-rM3wGwv4&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW&index=7

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace redd096.PathFinding.AStar3D
{
    /// <summary>
    /// Grid used for pathfinding
    /// </summary>
    [AddComponentMenu("redd096/.PathFinding/AStar 3D/Grid A Star 3D")]
    public class GridAStar : MonoBehaviour
    {
        #region structs

        [System.Serializable]
        public struct TerrainType
        {
            public LayerMask TerrainLayer;
            public int TerrainPenalty;
        }

        #endregion

        #region variables

        [Header("Layer Mask Unwalkable")]
        [SerializeField] protected LayerMask unwalkableMask = default;
        [Tooltip("If not inside these regions, penalty is 0")][SerializeField] protected TerrainType[] penaltyRegions = default;

        [Header("Grid")]
        [SerializeField] protected bool updateOnAwake = true;
        [SerializeField] protected Vector2 gridWorldSize = Vector2.one;
        [SerializeField] [Min(0.1f)] protected float nodeDiameter = 1;

        [Header("Gizmos - cyan Area - green/red walkable node - magenta obstacle")]
        [SerializeField] protected bool drawGridArea = false;
        [SerializeField] protected bool drawObstacles = false;
        [SerializeField] protected float alphaNodes = 0.3f;

        //grid
        Node[,] grid;

        float nodeRadius;
        float overlapRadius;                        //node radius - 0.05f to not hit adjacent colliders
        Vector2Int gridSize;                        //rows and columns (number of nodes)
        LayerMask penaltyRegionsMask;               //layerMask with every penalty region

        //public properties
        public int MaxSize => gridSize.x * gridSize.y;
        public virtual Vector3 GridWorldPosition => transform.position;
        public Vector2 GridWorldSize => gridWorldSize;
        public float NodeRadius => nodeRadius;

        #endregion

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

                //draw every node in grid
                if (grid != null)
                {
                    foreach (Node node in grid)
                    {
                        //set color if walkable or not (red = not walkable, green = walkable, magenta = walkable but with obstacles)
                        Gizmos.color = new Color(1, 1, 1, alphaNodes) * (node.isWalkable ? (drawObstacles && node.GetObstaclesOnThisNode().Count > 0 ? Color.magenta : Color.green) : Color.red);
                        //Gizmos.DrawSphere(node.worldPosition, overlapRadius);
                        Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                    }
                }

                Gizmos.color = Color.white;
            }
        }

        #region create grid

        protected virtual void SetGrid()
        {
            //set radius for every node
            nodeRadius = nodeDiameter * 0.5f;
            overlapRadius = nodeRadius - 0.05f;

            //set grid size
            gridSize.x = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSize.y = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

            //add every walkable regions to a single layer mask, to use when raycast to calculate penalty
            penaltyRegionsMask.value = default;
            foreach (TerrainType terrain in penaltyRegions)
            {
                penaltyRegionsMask.value = penaltyRegionsMask | terrain.TerrainLayer.value;
            }
        }

        void CreateGrid()
        {
            //reset grid and find bottom left world position
            grid = new Node[gridSize.x, gridSize.y];
            Vector3 worldBottomLeft = GridWorldPosition + (Vector3.left * gridWorldSize.x / 2) + (Vector3.back * gridWorldSize.y / 2);

            //create grid
            Vector3 worldPosition;
            bool isWalkable;
            bool agentCanMoveThrough;
            int movementPenalty;
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    //find world position and if walkable
                    worldPosition = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                    isWalkable = IsWalkable(worldPosition, out agentCanMoveThrough);

                    //if walkable, calculate movement penalty
                    movementPenalty = 0;
                    if (isWalkable)
                    {
                        CalculateMovementPenalty(worldPosition, out movementPenalty);
                    }

                    //set new node in grid
                    grid[x, y] = new Node(isWalkable, agentCanMoveThrough, worldPosition, x, y, movementPenalty);
                }
            }
        }

        protected virtual bool IsWalkable(Vector3 worldPosition, out bool agentCanMoveThrough)
        {
            //overlap sphere (agent can move through only on walkable nodes)
            agentCanMoveThrough = gameObject.scene.GetPhysicsScene().OverlapSphere(worldPosition, overlapRadius, new Collider[1], unwalkableMask, QueryTriggerInteraction.UseGlobal) <= 0;
            return agentCanMoveThrough;
        }

        void CalculateMovementPenalty(Vector3 worldPosition, out int movementPenalty)
        {
            //raycast to check terrain
            RaycastHit hit;
            movementPenalty = 0;
            if (Physics.Raycast(worldPosition + Vector3.up, Vector3.down, out hit, 1.1f, penaltyRegionsMask))
            {
                int hittedLayer = hit.collider.gameObject.layer;

                //find terrain inside walkable regions
                foreach (TerrainType region in penaltyRegions)
                {
                    if (region.TerrainLayer == (region.TerrainLayer | (1 << hittedLayer)))  //if this region contains layer
                    {
                        //set this terrain movement penalty
                        movementPenalty = region.TerrainPenalty;
                        break;
                    }
                }
            }
        }

        void SetNeighbours()
        {
            //set neighbours for every node
            int checkX;
            int checkY;
            foreach (Node node in grid)
            {
                node.neighbours.Clear();

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        //this is the node we are using as parameter
                        if (x == 0 && y == 0)
                            continue;

                        //find grid position
                        checkX = node.gridPosition.x + x;
                        checkY = node.gridPosition.y + y;

                        //if that position is inside the grid, add to neighbours
                        if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                        {
                            node.neighbours.Add(grid[checkX, checkY]);
                        }
                    }
                }
            }
        }

        #endregion

        #region public API

        /// <summary>
        /// Recreate grid (set which node is walkable and which not)
        /// </summary>
        public void BuildGrid()
        {
            SetGrid();
            CreateGrid();
            SetNeighbours();
        }

        /// <summary>
        /// Is grid created or is null?
        /// </summary>
        /// <returns></returns>
        public bool IsGridCreated()
        {
            //return if the grid was being created
            return grid != null;
        }

        /// <summary>
        /// Is world position inside the grid?
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public bool IsInsideGrid(Vector3 worldPosition)
        {
            //outside left or right
            if (worldPosition.x < GridWorldPosition.x - (gridWorldSize.x * 0.5f) || worldPosition.x > GridWorldPosition.x + (gridWorldSize.x * 0.5f))
                return false;

            //outside back or forward
            if (worldPosition.z < GridWorldPosition.z - (gridWorldSize.y * 0.5f) || worldPosition.z > GridWorldPosition.z + (gridWorldSize.y * 0.5f))
                return false;

            //else is inside
            return true;
        }

        /// <summary>
        /// Get node from world position
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public Node GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            //be sure to get right result also if grid doesn't start at [0,0]
            worldPosition -= GridWorldPosition;

            //find percent
            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;     //use Z position
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            //get coordinates from it
            int x = Mathf.RoundToInt((gridSize.x - 1) * percentX);
            int y = Mathf.RoundToInt((gridSize.y - 1) * percentY);

            //return node
            return grid[x, y];
        }

        /// <summary>
        /// Get node at grid position
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <returns></returns>
        public Node GetNodeByCoordinates(int x, int y)
        {
            return grid[x, y];
        }

        /// <summary>
        /// From a start node, calculate node at the extremes of a box
        /// </summary>
        /// <param name="startNode">start node</param>
        /// <param name="center">center of the box</param>
        /// <param name="halfSize">half size of the box</param>
        /// <param name="leftNode"></param>
        /// <param name="rightNode"></param>
        /// <param name="backNode"></param>
        /// <param name="forwardNode"></param>
        public void GetNodesExtremesOfABox(Node startNode, Vector3 center, Vector3 halfSize, out Node leftNode, out Node rightNode, out Node backNode, out Node forwardNode)
        {
            //set left node
            leftNode = startNode;
            for (int x = startNode.gridPosition.x - 1; x >= 0; x--)
            {
                if (grid[x, startNode.gridPosition.y].worldPosition.x + nodeRadius >= (center - halfSize).x)
                    leftNode = grid[x, startNode.gridPosition.y];
                else
                    break;
            }
            //set right node
            rightNode = startNode;
            for (int x = startNode.gridPosition.x + 1; x < gridSize.x; x++)
            {
                if (grid[x, startNode.gridPosition.y].worldPosition.x - nodeRadius <= (center + halfSize).x)
                    rightNode = grid[x, startNode.gridPosition.y];
                else
                    break;
            }
            //set up node
            forwardNode = startNode;
            for (int y = startNode.gridPosition.y + 1; y < gridSize.y; y++)
            {
                if (grid[startNode.gridPosition.x, y].worldPosition.z - nodeRadius <= (center + halfSize).z)
                    forwardNode = grid[startNode.gridPosition.x, y];
                else
                    break;
            }
            //set down node
            backNode = startNode;
            for (int y = startNode.gridPosition.y - 1; y >= 0; y--)
            {
                if (grid[startNode.gridPosition.x, y].worldPosition.z + nodeRadius >= (center - halfSize).z)
                    backNode = grid[startNode.gridPosition.x, y];
                else
                    break;
            }
        }

        #endregion
    }

    #region unity editor

#if UNITY_EDITOR

    [CustomEditor(typeof(GridAStar), true)]
    public class GridAStar3DEditor : Editor
    {
        private GridAStar gridAStar;

        private void OnEnable()
        {
            gridAStar = target as GridAStar;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(10);

            if (GUILayout.Button("Update Nodes"))
            {
                //update nodes
                gridAStar.BuildGrid();

                //update position of every obstacle
                foreach (ObstacleAStar obstacle in FindObjectsOfType<ObstacleAStar>())
                    if (obstacle)
                        obstacle.UpdatePositionOnGrid(gridAStar);

                //repaint scene and set undo
                SceneView.RepaintAll();
                Undo.RegisterFullObjectHierarchyUndo(target, "Update Nodes");
            }
        }
    }

#endif

    #endregion
}