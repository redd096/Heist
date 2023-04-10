using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;

namespace redd096.PathFinding.FlowField2D
{
    /// <summary>
    /// Used to create dynamic NotWalkable nodes on the grid. Or set penalty to them
    /// </summary>
    [AddComponentMenu("redd096/.PathFinding/FlowField 2D/Obstacle FlowField 2D")]
    public class ObstacleFlowField : MonoBehaviour
    {
        enum ETypeCollider { sphere, box }

        [Header("Collider Obstacle")]
        [SerializeField] bool useCustomCollider = false;
        [DisableIf("useCustomCollider")][SerializeField] Collider2D[] colliders = default;
        [ShowIf("useCustomCollider")][SerializeField] Vector2 offset = Vector2.zero;
        [ShowIf("useCustomCollider")][SerializeField] ETypeCollider typeCollider = ETypeCollider.box;
        [ShowIf("useCustomCollider")][EnableIf("typeCollider", ETypeCollider.box)][SerializeField] Vector2 sizeCollider = Vector2.one;
        [ShowIf("useCustomCollider")][EnableIf("typeCollider", ETypeCollider.sphere)][SerializeField] float radiusCollider = 1;

        [Header("Type Obstacle (set unwalkable or add penalty)")]
        [SerializeField] bool setUnwalkable = true;
        [SerializeField] bool addPenalty = false;
        [EnableIf("addPenalty")][SerializeField] int penalty = 1;

        [Header("DEBUG (only custom collider)")]
        [SerializeField] bool drawCustomCollider = false;
        [SerializeField] Color colorDebugCustomCollider = Color.cyan;

        public bool IsUnwalkable => setUnwalkable;
        public int AddPenalty => setUnwalkable == false && addPenalty ? penalty : 0;

        //vars
        GridFlowField grid;
        List<Node> nodesPosition = new List<Node>();    //nodes with this obstacle
        Vector2 previousPosition;

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
                //draw sphere
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

        void OnEnable()
        {
            //set obstacle on nodes
            UpdatePosition();
        }

        void Update()
        {
            //if moved
            if ((Vector2)transform.position != previousPosition)
            {
                //set obstacle on nodes
                UpdatePosition();
            }
        }

        void OnDisable()
        {
            //remove obstacle from grid
            RemoveFromPreviousNodes();
        }

        [Button("Find Children Colliders")]
        public void SetColliders_Editor()
        {
            //if not use custom collider, automatically get reference to unity colliders
            if (useCustomCollider == false && (colliders == null || colliders.Length <= 0))
                colliders = GetComponentsInChildren<Collider2D>();
        }

        #region public API

        /// <summary>
        /// Calculate new position on the grid and update nodes
        /// </summary>
        /// <param name="grid"></param>
        public void UpdatePositionOnGrid(GridFlowField grid)
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

        #endregion

        #region private API

        void UpdatePosition()
        {
            //set obstacle position
            if (PathFindingFlowField.instance)
            {
                previousPosition = transform.position;
                PathFindingFlowField.instance.UpdateObstaclePositionOnGrid(this);
            }
        }

        void SetNewNodes()
        {
            //only if active in scene
            if (gameObject.activeInHierarchy == false || grid == null)
                return;

            //set nodes using box or sphere
            if (useCustomCollider)
            {
                if (typeCollider == ETypeCollider.box)
                    SetNodesUsingBox();
                else
                    SetNodesUsingSphere();
            }
            //or using colliders
            else
            {
                SetNodesUsingColliders();
            }
        }

        void SetNodesUsingBox()
        {
            //calculate nodes
            centerNode = grid.GetNodeFromWorldPosition((Vector2)transform.position + offset);
            grid.GetNodesExtremesOfABox(centerNode, (Vector2)transform.position + offset, sizeCollider * 0.5f, out leftNode, out rightNode, out downNode, out upNode);

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

        void SetNodesUsingSphere()
        {
            //calculate nodes
            centerNode = grid.GetNodeFromWorldPosition((Vector2)transform.position + offset);
            grid.GetNodesExtremesOfABox(centerNode, (Vector2)transform.position + offset, Vector2.one * radiusCollider, out leftNode, out rightNode, out downNode, out upNode);

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
                centerNode = grid.GetNodeFromWorldPosition(col.bounds.center);
                grid.GetNodesExtremesOfABox(centerNode, col.bounds.center, (Vector2)col.bounds.extents, out leftNode, out rightNode, out downNode, out upNode);

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