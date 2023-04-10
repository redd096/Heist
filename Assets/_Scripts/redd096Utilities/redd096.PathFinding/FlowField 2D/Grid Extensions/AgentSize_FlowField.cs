using redd096.Attributes;
using UnityEngine;

namespace redd096.PathFinding.FlowField2D
{
    /// <summary>
    /// Used to know size of the agent. In Flow Field it is generic for every possible agent
    /// </summary>
    [System.Serializable]
    public class AgentSize_FlowField
    {
        enum ETypeCollider { sphere, box }

        [Header("Collider Agent - Only Box And Sphere")]
        [SerializeField] bool useCustomCollider = false;
        [ShowIf("useCustomCollider")][SerializeField] ETypeCollider typeCollider = ETypeCollider.box;
        [ShowIf("useCustomCollider")][EnableIf("typeCollider", ETypeCollider.box)][SerializeField] Vector2 sizeCollider = Vector2.one;
        [ShowIf("useCustomCollider")][EnableIf("typeCollider", ETypeCollider.sphere)][SerializeField] float radiusCollider = 1;

        [Header("DEBUG (only custom collider)")]
        [SerializeField] bool drawCustomCollider = false;
        [SerializeField] Color colorDebugCustomCollider = Color.cyan;

        //vars
        Node node;
        GridFlowField grid;

        //nodes to calculate
        Node leftNode;
        Node rightNode;
        Node upNode;
        Node downNode;
        Node nodeToCheck;

        public void OnDrawGizmos(Vector2 position)
        {
            if (drawCustomCollider && useCustomCollider)
            {
                Gizmos.color = colorDebugCustomCollider;

                //draw box
                if (typeCollider == ETypeCollider.box)
                {
                    Gizmos.DrawWireCube(position, sizeCollider);
                }
                //draw sphere
                else
                {
                    Gizmos.DrawWireSphere(position, radiusCollider);
                }

                Gizmos.color = Color.white;
            }
        }

        /// <summary>
        /// Agent can move on this node or hit some wall?
        /// </summary>
        /// <param name="node"></param>
        /// <param name="grid"></param>
        /// <returns></returns>
        public bool CanMoveOnThisNode(Node node, GridFlowField grid)
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
                    return CanMove_Sphere();
                }
            }

            //else can always move
            return true;
        }

        #region private API

        bool CanMove_Box()
        {
            //calculate nodes
            //use node as center, because agent is calculated along the path (not transform.position)
            grid.GetNodesExtremesOfABox(node, node.worldPosition, sizeCollider * 0.5f, out leftNode, out rightNode, out downNode, out upNode);

            //check every node
            for (int x = leftNode.gridPosition.x; x <= rightNode.gridPosition.x; x++)
            {
                for (int y = downNode.gridPosition.y; y <= upNode.gridPosition.y; y++)
                {
                    nodeToCheck = grid.GetNodeByCoordinates(x, y);

                    ////if agent can not move through OR there are obstacles, return false
                    //if (nodeToCheck.agentCanMoveThrough == false || ThereAreObstacles(nodeToCheck))
                    //if agent can not move through, return false
                    if (nodeToCheck.AgentCanOverlap == false)
                        return false;
                }
            }

            return true;
        }

        bool CanMove_Sphere()
        {
            //calculate nodes
            //use node as center, because agent is calculated along the path (not transform.position)
            grid.GetNodesExtremesOfABox(node, node.worldPosition, Vector2.one * radiusCollider, out leftNode, out rightNode, out downNode, out upNode);

            //check every node
            for (int x = leftNode.gridPosition.x; x <= rightNode.gridPosition.x; x++)
            {
                for (int y = downNode.gridPosition.y; y <= upNode.gridPosition.y; y++)
                {
                    nodeToCheck = grid.GetNodeByCoordinates(x, y);

                    //if inside radius
                    if (Vector2.Distance(node.worldPosition, nodeToCheck.worldPosition) <= radiusCollider + grid.NodeRadius)
                    {
                        ////if agent can not move through OR there are obstacles, return false
                        //if (nodeToCheck.agentCanMoveThrough == false || ThereAreObstacles(nodeToCheck))
                        //if agent can not move through, return false
                        if (nodeToCheck.AgentCanOverlap == false)
                            return false;
                    }
                }
            }

            return true;
        }

        #endregion
    }
}