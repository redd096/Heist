using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace redd096.PathFinding.AStar3D
{
    public abstract class PathRequestManagerAStar : MonoBehaviour
    {
        List<PathRequest> pathRequestQueue = new List<PathRequest>();
        PathRequest currentPathRequest;
        bool isProcessingPath;

        protected bool RemoveRequestFromQueue(PathRequest pathRequest)
        {
            //remove request from queue
            if (pathRequestQueue.Contains(pathRequest))
            {
                pathRequestQueue.Remove(pathRequest);
                return true;
            }

            return false;
        }

        protected void RequestPath(PathRequest pathRequest)
        {
            //add path request to queue
            pathRequestQueue.Add(pathRequest);

            //if is the first request, process it
            TryProcessNext();
        }

        protected void OnFinishProcessingPath(Path path)
        {
            //if called from agent, call is finished to process path
            if (currentPathRequest.agent)
                currentPathRequest.agent.OnFinishProcessingPath(currentPathRequest);

            //call function passing the path as parameter
            currentPathRequest.func?.Invoke(path);
            isProcessingPath = false;

            //if there is, start next request in the queue
            TryProcessNext();
        }

        void TryProcessNext()
        {
            //get next request from queue and start find path
            if (isProcessingPath == false && pathRequestQueue.Count > 0)
            {
                currentPathRequest = pathRequestQueue[0];
                pathRequestQueue.RemoveAt(0);
                isProcessingPath = true;
                StartCoroutine(FindPathCoroutine(currentPathRequest));
            }
        }

        protected abstract IEnumerator FindPathCoroutine(PathRequest pathRequest);
    }

    #region classes

    public class PathRequest
    {
        public Vector3 startPosition;
        public Vector3 targetPosition;
        public System.Action<Path> func;
        public AgentAStar agent;
        public bool returnNearestPointToTarget;

        /// <summary>
        /// Struct used to request a path to PathFinding
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="targetPosition"></param>
        /// <param name="func">function to call when finish processing path. Will pass the path as parameter</param>
        /// <param name="agent"></param>
        /// <param name="returnNearestPointToTarget">if no path to target position, return path to nearest point</param>
        public PathRequest(Vector3 startPosition, Vector3 targetPosition, System.Action<Path> func, AgentAStar agent = null, bool returnNearestPointToTarget = true)
        {
            this.startPosition = startPosition;
            this.targetPosition = targetPosition;
            this.func = func;
            this.agent = agent;
            this.returnNearestPointToTarget = returnNearestPointToTarget;
        }
    }

    public class Path
    {
        public List<Vector3> vectorPath;

        /// <summary>
        /// Struct used to pass found path
        /// </summary>
        /// <param name="vectorPath"></param>
        public Path(List<Vector3> vectorPath)
        {
            this.vectorPath = vectorPath;
        }
    }

    #endregion
}