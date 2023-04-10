//https://www.youtube.com/watch?v=mZfyt03LDH4&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW&index=3

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace redd096.PathFinding.AStar3D
{
    /// <summary>
    /// Call this to calculate a path using pathfinding or to update the grid
    /// </summary>
    [AddComponentMenu("redd096/.PathFinding/AStar 3D/Path Finding A Star 3D")]
    public class PathFindingAStar : PathRequestManagerAStar
    {
        public static PathFindingAStar instance;

        [Header("Default use Find Object of Type")]
        public GridAStar Grid = default;

        //obstacles
        Coroutine updateObstaclePositionOnGridCoroutine;
        Queue<ObstacleAStar> obstaclesQueue = new Queue<ObstacleAStar>();

        void Awake()
        {
            instance = this;

            if (Grid == null)
                Grid = FindObjectOfType<GridAStar>();
        }

        #region public API

        /// <summary>
        /// Find path from one point to another
        /// </summary>
        /// <param name="pathRequest"></param>
        public void FindPath(PathRequest pathRequest)
        {
            //start processing path or add to queue
            RequestPath(pathRequest);
        }

        /// <summary>
        /// Remove request from queue. If request isn't in queue, or is already processing, return false
        /// </summary>
        /// <param name="pathRequest"></param>
        public bool CancelRequest(PathRequest pathRequest)
        {
            return RemoveRequestFromQueue(pathRequest);
        }

        /// <summary>
        /// Update obstacle position on the grid (used for pathfinding)
        /// </summary>
        /// <param name="obstacle"></param>
        public void UpdateObstaclePositionOnGrid(ObstacleAStar obstacle)
        {
            //add to queue
            if (obstaclesQueue.Contains(obstacle) == false)
                obstaclesQueue.Enqueue(obstacle);

            //start coroutine if not already running
            if (updateObstaclePositionOnGridCoroutine == null)
                updateObstaclePositionOnGridCoroutine = StartCoroutine(UpdateObstaclePositionOnGridCoroutine());
        }

        #endregion

        #region private API

        protected override IEnumerator FindPathCoroutine(PathRequest pathRequest)
        {
            /*
             * OPEN - the set of nodes to be evaluated
             * CLOSE - the set of nodes already evaluated
             * 
             * G cost - distance from start point
             * H cost - distance from end point
             * F cost - sum of G cost and H cost
             */

            //=================================================================

            /*
             * add the start node to OPEN
             * 
             * loop
             *  Current = node in OPEN with the lowest F cost
             *  remove Current from OPEN
             *  add Current to CLOSED
             *  
             *  if Current is the target node (path has been found)
             *   return path
             *  
             *  foreach Neighbour of the Current node
             *   if Neighbour is not walkable or Neighbour is in CLOSED
             *      skip to the next Neighbour
             *      
             *  if new path to Neighbour is shorter OR Neighbour is not in OPEN
             *      set F cost of Neighbour
             *      set parent of Neighbour to Current
             *      if Neighbour is not in OPEN
             *          add Neighbour to OPEN
             */

            //be sure the grid is created
            if (Grid.IsGridCreated() == false)
                Grid.BuildGrid();

            //get nodes from world position
            Node startNode = Grid.GetNodeFromWorldPosition(pathRequest.startPosition);
            Node targetNode = Grid.GetNodeFromWorldPosition(pathRequest.targetPosition);

            Heap<Node> openList = new Heap<Node>(Grid.MaxSize);     //nodes to be evaluated
            HashSet<Node> closedList = new HashSet<Node>();             //already evaluated

            //add the start node to OPEN
            openList.Add(startNode);

            Node currentNode = null;
            bool pathSuccess = false;
            while (openList.Count > 0)
            {
                #region before heap optimization
                /*
                //Current = node in OPEN with the lowest F cost
                currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    //if F cost is lower or is the same but H cost is lower
                    if (openList[i].fCost < currentNode.fCost || openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost)
                    {
                        currentNode = openList[i];
                    }
                }

                //remove Current from OPEN and add to CLOSED
                openList.Remove(currentNode);
                closedList.Add(currentNode);
                */
                #endregion

                //Current = node in OPEN with the lowest F cost
                //remove Current from OPEN and add to CLOSED
                currentNode = openList.RemoveFirst();                //all optimized with heap
                closedList.Add(currentNode);

                //path has been found, return it
                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                //foreach Neighbour of the Current node
                foreach (Node neighbour in currentNode.neighbours)
                {
                    //if Neighbour is not walkable or is in CLOSED, skip to next Neighbour
                    if (!neighbour.isWalkable || closedList.Contains(neighbour))
                        continue;

                    //if using agent and can't move on this node, skip to next Neighbour
                    if (pathRequest.agent && !pathRequest.agent.CanMoveOnThisNode(neighbour, Grid))
                        continue;

                    //get distance to Neighbour
                    int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;   //+ movement penalty

                    //if new path to Neighbour is shorter or Neighbour is not in OPEN
                    if (newCostToNeighbour < neighbour.gCost || !openList.Contains(neighbour))
                    {
                        //set F cost of Neighbour
                        neighbour.gCost = newCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);

                        //set parent of Neighbour to Current
                        neighbour.parentNode = currentNode;

                        //if Neighbour is not in OPEN, add it
                        if (!openList.Contains(neighbour))
                            openList.Add(neighbour);
                    }
                }
            }

            yield return null;

            //if found path, return it
            if (pathSuccess)
            {
                OnFinishProcessingPath(new Path(RetracePath(startNode, currentNode)));
            }
            //if no path, but can return nearest point
            else if (pathRequest.returnNearestPointToTarget)
            {
                //set start node because the start is not setted
                startNode.hCost = GetDistance(startNode, targetNode);

                //find the walkable node nearest to target point
                Node nearestNode = startNode;
                foreach (Node node in closedList)
                {
                    if (node.isWalkable && node.hCost < nearestNode.hCost)  //if walkable and nearest to target point
                    {
                        //only if not using an agent, or if agent can move on this node
                        if (pathRequest.agent == null || pathRequest.agent.CanMoveOnThisNode(node, Grid))
                            nearestNode = node;
                    }
                }

                //find path only if nearest node is not the start node
                if (startNode != nearestNode)
                {
                    pathSuccess = true;
                    yield return FindPathCoroutine(new PathRequest(startNode.worldPosition, nearestNode.worldPosition, pathRequest.func, pathRequest.agent, false));
                }
            }

            //if there is no path, return null
            if (pathSuccess == false)
            {
                OnFinishProcessingPath(null);
            }
        }

        /// <summary>
        /// Calculate distance between 2 nodes
        /// </summary>
        int GetDistance(Node nodeA, Node nodeB)
        {
            /* 
             * 
             * we use 10 from one point to another ( like 1.0 )
             * we use 14 in oblique ( like hypotenuse sqr(1.0 + 1.0) )
             * 
             * so in this example, if we have nodeA at (0,0) and nodeB at (5,2)
             * we move 2 times in oblique to reach y axis, then move 3 times along x axis
             * 14y + 10(x-y)
             * 14 *2 + 10 * (5 - 2)
             * 
             * y
             * 
             * 2  /- - - B
             * 1 / 
             * A 1 2 3 4 5  x
             * 
             */

            //get distance on X and Y
            int distX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            int distY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            //if distance on X is greater, move oblique (14) to reach Y axis, then move along X axis (10)
            if (distX > distY)
                return 14 * distY + 10 * (distX - distY);

            //else move oblique (14) to reach X axis, then move along Y axis (10)
            return 14 * distX + 10 * (distY - distX);
        }

        /// <summary>
        /// Retrace path from start to end
        /// </summary>
        List<Vector3> RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();

            //start from end waypoint
            Node currentNode = endNode;

            //while not reached start waypoint
            while (currentNode != startNode)
            {
                //add current waypoint and move to next one
                path.Add(currentNode);
                currentNode = currentNode.parentNode;
            }

            //simplify path (before reverse, to be sure to take last node in a direction)
            List<Vector3> vectorPath = SimplifyPath(path);

            //reverse list to get from start to end
            vectorPath.Reverse();

            return vectorPath;
        }

        /// <summary>
        /// Return path as a list of Vector, and pass only nodes when change direction
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        List<Vector3> SimplifyPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            //Vector2Int oldDirection = Vector2Int.zero;
            //
            ////calculate direction between every node in the path
            //Vector2Int newDirection;
            //for (int i = 1; i < path.Count; i++)
            //{
            //    //if direction is different, add this node position to the path
            //    newDirection = path[i - 1].gridPosition - path[i].gridPosition;
            //    if (newDirection != oldDirection)
            //    {
            //        waypoints.Add(path[i].worldPosition);
            //        oldDirection = newDirection;
            //    }
            //}

            for (int i = 0; i < path.Count; i++)
                waypoints.Add(path[i].worldPosition);

            return waypoints;
        }

        /// <summary>
        /// Update every position of every obstacle in queue
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateObstaclePositionOnGridCoroutine()
        {
            while (obstaclesQueue.Count > 0)
            {
                //get obstacle from queue and update its position
                ObstacleAStar obstacle = obstaclesQueue.Dequeue();
                if (obstacle)
                    obstacle.UpdatePositionOnGrid(Grid);

                yield return null;
            }

            updateObstaclePositionOnGridCoroutine = null;
        }

        #endregion
    }
}