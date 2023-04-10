using UnityEngine;
using redd096.Attributes;

namespace redd096.PathFinding.AStar2D
{
    /// <summary>
    /// Used to know size of the agent. When call PathFinding you can pass it as parameter
    /// </summary>
    [AddComponentMenu("redd096/.PathFinding/AStar 2D/Agent A Star 2D")]
    public class AgentAStar : MonoBehaviour
    {
        enum ETypeCollider { circle, box }

        [Header("Collider Agent - Only Box and Circle")]
        [SerializeField] bool useCustomCollider = false;
        [HideIf("useCustomCollider")][Tooltip("ONLY BOX AND CIRCLE")][SerializeField] Collider2D[] colliders = default;
        [ShowIf("useCustomCollider")][SerializeField] Vector2 offset = Vector2.zero;
        [ShowIf("useCustomCollider")][SerializeField] ETypeCollider typeCollider = ETypeCollider.box;
        [ShowIf("useCustomCollider")][EnableIf("typeCollider", ETypeCollider.box)][SerializeField] Vector2 sizeCollider = Vector2.one;
        [ShowIf("useCustomCollider")][EnableIf("typeCollider", ETypeCollider.circle)][SerializeField] float radiusCollider = 1;

        [HelpBox("Ignore self is Deprecated! Better not set self as not walkable node", HelpBoxAttribute.EMessageType.Error)]
        [Header("If this object is an obstacle, ignore self (default get from this gameObject)")]
        [SerializeField] ObstacleAStar obstacleAStar = default;

        [Header("DEBUG (only custom collider)")]
        [SerializeField] bool drawCustomCollider = false;
        [SerializeField] Color colorDebugCustomCollider = Color.cyan;

        //vars
        Node node;
        GridAStar grid;
        bool isWaitingPath;
        PathRequest lastPathRequest;
        CircleCollider2D[] circleColliders;  //for every collider, save which are circle colliders

        //nodes to calculate
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
            if (obstacleAStar == null) obstacleAStar = GetComponent<ObstacleAStar>();
            if (colliders == null || colliders.Length <= 0) colliders = GetComponentsInChildren<Collider2D>();

            //colliders can only be box or circle. Save which are circles
            circleColliders = new CircleCollider2D[colliders.Length];
            for (int i = 0; i < colliders.Length; i++)
                circleColliders[i] = colliders[i] as CircleCollider2D;
        }

        /// <summary>
        /// Agent can move on this node or hit some wall?
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public bool CanMoveOnThisNode(Node node, GridAStar grid)
        {
            if (node == null || grid == null)
                return false;

            //set vars
            this.node = node;
            this.grid = grid;

            if (useCustomCollider)
            {
                //box
                if (typeCollider == ETypeCollider.box)
                {
                    return CanMove_Box();
                }
                //circle
                else
                {
                    return CanMove_Circle();
                }
            }
            //else use colliders
            else
            {
                return CanMove_Colliders();
            }
        }

        #region private API

        bool CanMove_Box()
        {
            //calculate nodes
            //use node as center, because agent is calculated along the path (not transform.position)
            grid.GetNodesExtremesOfABox(node, node.worldPosition + offset, sizeCollider * 0.5f, out leftNode, out rightNode, out downNode, out upNode);

            //check every node
            for (int x = leftNode.gridPosition.x; x <= rightNode.gridPosition.x; x++)
            {
                for (int y = downNode.gridPosition.y; y <= upNode.gridPosition.y; y++)
                {
                    nodeToCheck = grid.GetNodeByCoordinates(x, y);

                    ////if agent can not move through OR there are obstacles, return false
                    //if (nodeToCheck.agentCanMoveThrough == false || ThereAreObstacles(nodeToCheck))
                    //if agent can not move through, return false
                    if (nodeToCheck.agentCanMoveThrough == false)
                        return false;
                }
            }

            return true;
        }

        bool CanMove_Circle()
        {
            //calculate nodes
            //use node as center, because agent is calculated along the path (not transform.position)
            grid.GetNodesExtremesOfABox(node, node.worldPosition + offset, Vector2.one * radiusCollider, out leftNode, out rightNode, out downNode, out upNode);

            //check every node
            for (int x = leftNode.gridPosition.x; x <= rightNode.gridPosition.x; x++)
            {
                for (int y = downNode.gridPosition.y; y <= upNode.gridPosition.y; y++)
                {
                    nodeToCheck = grid.GetNodeByCoordinates(x, y);

                    //if inside radius
                    if (Vector2.Distance(node.worldPosition, nodeToCheck.worldPosition) <= radiusCollider)
                    {
                        ////if agent can not move through OR there are obstacles, return false
                        //if (nodeToCheck.agentCanMoveThrough == false || ThereAreObstacles(nodeToCheck))
                        //if agent can not move through, return false
                        if (nodeToCheck.agentCanMoveThrough == false)
                            return false;
                    }
                }
            }

            return true;
        }

        bool CanMove_Colliders()
        {
            //foreach collider
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == null)
                    continue;

                //calculate nodes
                //use node as center, because agent is calculated along the path (not transform.position)
                grid.GetNodesExtremesOfABox(node, node.worldPosition + colliders[i].offset * colliders[i].transform.lossyScale, colliders[i].bounds.extents, out leftNode, out rightNode, out downNode, out upNode);

                //check every node
                for (int x = leftNode.gridPosition.x; x <= rightNode.gridPosition.x; x++)
                {
                    for (int y = downNode.gridPosition.y; y <= upNode.gridPosition.y; y++)
                    {
                        nodeToCheck = grid.GetNodeByCoordinates(x, y);

                        //if inside collider
                        //only box or circle, can't use same check of ObstacleAStar cause agent is calculated along the path (not transform.position)
                        if (circleColliders[i] == null || Vector2.Distance(node.worldPosition, nodeToCheck.worldPosition) <= circleColliders[i].radius)
                        {
                            ////if agent can not move through OR there are obstacles, return false
                            //if (nodeToCheck.agentCanMoveThrough == false || ThereAreObstacles(nodeToCheck))
                            //if agent can not move through, return false
                            if (nodeToCheck.agentCanMoveThrough == false)
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        bool ThereAreObstacles(Node nodeToCheck)
        {
            //if there are obstacles
            if (nodeToCheck.GetObstaclesOnThisNode().Count > 0)
            {
                //if there is self obstacle, return false
                if (obstacleAStar)
                {
                    if (nodeToCheck.GetObstaclesOnThisNode().Contains(obstacleAStar))
                        return false;
                }

                //if there are others obstacles, return true
                return true;
            }

            //if there aren't obstacles, return false
            return false;
        }

        #endregion

        #region public API

        /// <summary>
        /// Calculate path, then call function passing the path as parameter. If called before receive path, will stop previous request. Call IsDone() to check when has finished
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="targetPosition"></param>
        /// <param name="func">function to call when finish processing path. Will pass the path as parameter</param>
        public void FindPath(Vector2 startPosition, Vector2 targetPosition, System.Action<Path> func)
        {
            //call find path on Path Finding
            if (PathFindingAStar.instance)
            {
                //if still waiting previous path, stop that request
                if (isWaitingPath)
                {
                    PathFindingAStar.instance.CancelRequest(lastPathRequest);
                }

                isWaitingPath = true;                                                                   //set is waiting path
                lastPathRequest = new PathRequest(startPosition, targetPosition, func, this);           //save last path request
                PathFindingAStar.instance.FindPath(lastPathRequest);
            }
        }

        /// <summary>
        /// Remove last path request from queue. If already processing, do nothing
        /// </summary>
        public void CancelLastPathRequest()
        {
            //stop request
            if (PathFindingAStar.instance)
            {
                if (PathFindingAStar.instance.CancelRequest(lastPathRequest))
                    isWaitingPath = false;                                                              //if succeeded, set is not waiting path
            }
        }

        /// <summary>
        /// Called from pathfinding, when finish processing path
        /// </summary>
        public void OnFinishProcessingPath(PathRequest pathRequest)
        {
            //if finish processing last request (if finish another request but not last, can't set isWaiting at false)
            if (pathRequest == lastPathRequest)
            {
                //set has finished to wait path
                isWaitingPath = false;
            }
        }

        /// <summary>
        /// Is not waiting path (already received or not requested)
        /// </summary>
        /// <returns></returns>
        public bool IsDone()
        {
            return !isWaitingPath;
        }

        #endregion
    }
}