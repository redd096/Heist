using System.Collections.Generic;
using UnityEngine;

namespace redd096.PathFinding.FlowField3D
{
    public abstract class GridBASE : MonoBehaviour
    {
        //grid
        protected Node[,] grid;

        //public properties
        public virtual Vector3 GridWorldPosition => transform.position;

        #region create grid

        protected abstract void SetGrid();
        protected abstract void CreateGrid();
        protected abstract void SetNeighbours();

        #endregion

        #region flow field

        protected void ResetFlowFieldGrid()
        {
            //reset every node in the grid (not neighbours or penalty, just reset best cost and direction used for FlowField Pathfinding)
            foreach (Node node in grid)
            {
                //set default values
                node.bestCost = short.MaxValue;
                node.bestDirection = Vector2Int.zero;
            }
        }

        protected void SetBestCosts(TargetRequest[] targetRequests, System.Func<Node, bool> checkCanMoveOnNode = null)
        {
            foreach (TargetRequest targetRequest in targetRequests)
            {
                Node targetNode = GetNodeFromWorldPosition(targetRequest.savedPosition);

                //set target node at 0 or lower
                targetNode.bestCost = (short)-targetRequest.weight;

                //start from target node
                Queue<Node> cellsToCheck = new Queue<Node>();
                cellsToCheck.Enqueue(targetNode);

                while (cellsToCheck.Count > 0)
                {
                    //get every neighbour in cardinal directions
                    Node currentNode = cellsToCheck.Dequeue();
                    foreach (Node currentNeighbour in currentNode.neighboursCardinalDirections)
                    {
                        //if not walkable, ignore
                        if (currentNeighbour.IsWalkable == false) { continue; }

                        //if using agent and can't move on this node, skip to next Neighbour
                        if (checkCanMoveOnNode != null && checkCanMoveOnNode(currentNeighbour) == false)
                            continue;

                        //else, calculate best cost
                        if (currentNeighbour.movementPenalty + currentNode.bestCost < currentNeighbour.bestCost)
                        {
                            currentNeighbour.bestCost = (short)(currentNeighbour.movementPenalty + currentNode.bestCost);
                            cellsToCheck.Enqueue(currentNeighbour);
                        }
                    }
                }
            }
        }

        protected void SetBestDirections(bool canMoveDiagonal)
        {
            //foreach node in the grid
            foreach (Node currentNode in grid)
            {
                //calculate best direction from this node to neighbours
                int bestCost = currentNode.bestCost;
                foreach (Node neighbour in canMoveDiagonal ? currentNode.neighbours : currentNode.neighboursCardinalDirections)
                {
                    //if this best cost is lower then found one, this is the best node to move to
                    if (neighbour.bestCost < bestCost)
                    {
                        //save best cost and set direction
                        bestCost = neighbour.bestCost;
                        currentNode.bestDirection = neighbour.gridPosition - currentNode.gridPosition;
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
        /// Get node at grid position
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <returns></returns>
        public Node GetNodeByCoordinates(int x, int y)
        {
            return grid[x, y];
        }

        /// <summary>
        /// Get node from world position (or nearest one)
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public abstract Node GetNodeFromWorldPosition(Vector3 worldPosition);

        /// <summary>
        /// Set best direction for every node in the grid, to target node
        /// </summary>
        /// <param name="targetRequests"></param>
        /// <param name="canMoveDiagonal">can move diagonal or only horizontal and vertical?</param>
        public abstract void SetFlowField(TargetRequest[] targetRequests, bool canMoveDiagonal);

        #endregion
    }
}