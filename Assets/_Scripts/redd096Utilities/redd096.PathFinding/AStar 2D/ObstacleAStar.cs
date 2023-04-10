using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;

namespace redd096.PathFinding.AStar2D
{
    /// <summary>
    /// Used to create dynamic NotWalkable nodes on the grid. Or set penalty to them
    /// </summary>
    [AddComponentMenu("redd096/.PathFinding/AStar 2D/Obstacle A Star 2D")]
    public class ObstacleAStar : MonoBehaviour
    {
        enum ETypeCollider { circle, box }

        [Header("Collider Obstacle")]
        [SerializeField] bool useCustomCollider = true;
        [DisableIf("useCustomCollider")][SerializeField] Collider2D[] colliders = default;
        [ShowIf("useCustomCollider")][SerializeField] Vector2 offset = Vector2.zero;
        [ShowIf("useCustomCollider")][SerializeField] ETypeCollider typeCollider = ETypeCollider.box;
        [ShowIf("useCustomCollider")][EnableIf("typeCollider", ETypeCollider.circle)][SerializeField] float radiusCollider = 1;
        [ShowIf("useCustomCollider")][EnableIf("typeCollider", ETypeCollider.box)][SerializeField] Vector2 sizeCollider = Vector2.one;

        [Header("Type Obstacle (set unwalkable or add penalty)")]
        [SerializeField] bool setUnwalkable = false;
        [SerializeField] bool addPenalty = true;
        [EnableIf("addPenalty")][SerializeField] int penalty = 1;

        [Header("DEBUG (only custom collider)")]
        [SerializeField] bool drawCustomCollider = false;
        [SerializeField] Color colorDebugCustomCollider = Color.cyan;

        public bool IsUnwalkable => setUnwalkable;
        public int AddPenalty => setUnwalkable == false && addPenalty ? penalty : 0;

        //vars
        GridAStar grid;
        List<Node> nodesPosition = new List<Node>();    //nodes with this obstacle

        //nodes to calculate
        Node centerNode;
        Node leftNode;
        Node rightNode;
        Node upNode;
        Node downNode;
        Node nodeToCheck;

        void OnDrawGizmos()
        {
            if (drawCustomCollider && useCustomCollider)
            {
                Gizmos.color = colorDebugCustomCollider;

                //draw box
                if (typeCollider == ETypeCollider.box)
                {
                    Gizmos.DrawWireCube((Vector2)transform.position + offset, sizeCollider);
                }
                //draw circle
                else
                {
                    Gizmos.DrawWireSphere((Vector2)transform.position + offset, radiusCollider);
                }

                Gizmos.color = Color.white;
            }
        }

        void Awake()
        {
            //get references
            if (colliders == null || colliders.Length <= 0)
                colliders = GetComponentsInChildren<Collider2D>();
        }

        void Update()
        {
            //update obstacle position
            if (PathFindingAStar.instance)
                PathFindingAStar.instance.UpdateObstaclePositionOnGrid(this);
        }

        void OnDisable()
        {
            //remove obstacle from grid
            RemoveFromPreviousNodes();
        }

        #region public API

        /// <summary>
        /// Calculate new position on the grid and update nodes
        /// </summary>
        /// <param name="grid"></param>
        public void UpdatePositionOnGrid(GridAStar grid)
        {
            if (grid == null)
                return;

            //set vars
            this.grid = grid;

            //update nodes
            RemoveFromPreviousNodes();
            SetNewNodes();
        }

        /// <summary>
        /// Remove from current nodes
        /// </summary>
        public void RemoveFromPreviousNodes()
        {
            //remove this from previous nodes
            foreach (Node node in nodesPosition)
            {
                if (node != null)
                    node.RemoveObstacle(this);
            }

            //clear list
            nodesPosition.Clear();
        }

        /// <summary>
        /// Calculate new position on the grid and add to new nodes (is better use UpdatePositionOnGrid to set grid and remove from previous nodes)
        /// </summary>
        public void SetNewNodes()
        {
            //only if active in scene
            if (gameObject.activeInHierarchy == false || grid == null)
                return;

            //set nodes using box or circle
            if (useCustomCollider)
            {
                if (typeCollider == ETypeCollider.box)
                    SetNodesUsingBox();
                else
                    SetNodesUsingCircle();
            }
            //or using colliders
            else
            {
                SetNodesUsingColliders();
            }
        }

        #endregion

        #region private API

        void SetNodesUsingBox()
        {
            //calculate nodes
            //use an offset to check if node is inside also if collider not reach center of the node (add grid.NodeRadius in the half size)
            centerNode = grid.GetNodeFromWorldPosition((Vector2)transform.position + offset);
            grid.GetNodesExtremesOfABox(centerNode, (Vector2)transform.position + offset, (sizeCollider * 0.5f) + (Vector2.one * grid.NodeRadius), out leftNode, out rightNode, out downNode, out upNode);

            //check every node
            for (int x = leftNode.gridPosition.x; x <= rightNode.gridPosition.x; x++)
            {
                for (int y = downNode.gridPosition.y; y <= upNode.gridPosition.y; y++)
                {
                    nodeToCheck = grid.GetNodeByCoordinates(x, y);

                    //set it
                    if (nodeToCheck != null)
                        nodeToCheck.AddObstacle(this);

                    //and add to the list
                    if (nodesPosition.Contains(nodeToCheck) == false)
                        nodesPosition.Add(nodeToCheck);
                }
            }
        }

        void SetNodesUsingCircle()
        {
            //calculate nodes
            //use an offset to check if node is inside also if collider not reach center of the node (add grid.NodeRadius in the half size)
            centerNode = grid.GetNodeFromWorldPosition((Vector2)transform.position + offset);
            grid.GetNodesExtremesOfABox(centerNode, (Vector2)transform.position + offset, (Vector2.one * radiusCollider) + (Vector2.one * grid.NodeRadius), out leftNode, out rightNode, out downNode, out upNode);

            //check every node
            for (int x = leftNode.gridPosition.x; x <= rightNode.gridPosition.x; x++)
            {
                for (int y = downNode.gridPosition.y; y <= upNode.gridPosition.y; y++)
                {
                    nodeToCheck = grid.GetNodeByCoordinates(x, y);

                    //if inside radius (+ node radius offset)
                    if (Vector2.Distance(centerNode.worldPosition, nodeToCheck.worldPosition) <= radiusCollider + grid.NodeRadius)
                    {
                        //set it
                        if (nodeToCheck != null)
                            nodeToCheck.AddObstacle(this);

                        //and add to the list
                        if (nodesPosition.Contains(nodeToCheck) == false)
                            nodesPosition.Add(nodeToCheck);
                    }
                }
            }
        }

        void SetNodesUsingColliders()
        {
            //foreach collider
            foreach (Collider2D col in colliders)
            {
                if (col == null)
                    continue;

                //calculate nodes
                //use an offset to check if node is inside also if collider not reach center of the node (add grid.NodeRadius in the half size)
                centerNode = grid.GetNodeFromWorldPosition(col.bounds.center);
                grid.GetNodesExtremesOfABox(centerNode, col.bounds.center, (Vector2)col.bounds.extents + (Vector2.one * grid.NodeRadius), out leftNode, out rightNode, out downNode, out upNode);

                //check every node
                for (int x = leftNode.gridPosition.x; x <= rightNode.gridPosition.x; x++)
                {
                    for (int y = downNode.gridPosition.y; y <= upNode.gridPosition.y; y++)
                    {
                        nodeToCheck = grid.GetNodeByCoordinates(x, y);

                        //if node is inside collider (+ node radius offset)
                        if (Vector2.Distance(col.ClosestPoint(nodeToCheck.worldPosition), nodeToCheck.worldPosition) < Mathf.Epsilon + grid.NodeRadius)
                        {
                            //set it
                            if (nodeToCheck != null)
                                nodeToCheck.AddObstacle(this);

                            //and add to the list
                            if (nodesPosition.Contains(nodeToCheck) == false)
                                nodesPosition.Add(nodeToCheck);
                        }
                    }
                }
            }
        }

        #endregion
    }
}