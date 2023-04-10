using UnityEngine;
using redd096.PathFinding.AStar2D;
using redd096.Attributes;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Movement/Move To Transform With Path Finding")]
    public class MoveToTransformWithPathFinding : ActionTask
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] MovementComponent movementComponent = default;
        [SerializeField] AgentAStar agentAStar = default;

        [Header("Chase")]
        [SerializeField] VarOrBlackboard<Transform> target = new VarOrBlackboard<Transform>("Target");
        [ColorGUI(AttributesUtility.EColor.Yellow)][SerializeField] string savePathInBlackboard = "Path";
        [SerializeField] bool useCustomSpeed = false;
        [EnableIf("useCustomSpeed")][SerializeField] float customSpeed = 5;

        [Header("Reach position or just move near")]
        [SerializeField] bool mustReachPosition = true;
        [DisableIf("mustReachPosition")][SerializeField] float approximately = 0.05f;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 drawPath = Color.cyan;
        [SerializeField] float delayRecalculatePath = 0.5f;

        float timerBeforeNextUpdatePath;
        Path path;

        void OnDrawGizmos()
        {
            if (drawPath)
            {
                //draw path
                if (path != null && path.vectorPath != null && path.vectorPath.Count > 0)
                {
                    Gizmos.color = drawPath.ColorDebug;
                    for (int i = 0; i < path.vectorPath.Count; i++)
                    {
                        if (i + 1 < path.vectorPath.Count)
                            Gizmos.DrawLine(path.vectorPath[i], path.vectorPath[i + 1]);
                    }
                    Gizmos.color = Color.white;
                }
            }
        }

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //get references
            if (movementComponent == null) movementComponent = GetStateMachineComponent<MovementComponent>();
            if (agentAStar == null) agentAStar = GetStateMachineComponent<AgentAStar>();
        }

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            //stop previous path request
            if (agentAStar && agentAStar.IsDone() == false)
                agentAStar.CancelLastPathRequest();

            //remove previous path
            if (path != null)
                path = null;
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            if (GetValue(target) == null)
                return;

            //update path to target
            UpdatePath();

            //if there is path, move to next node
            if (path != null && path.vectorPath != null && path.vectorPath.Count > 0)
            {
                ////if on a walkable node, save it
                //Node2D currentNode = pathFinding.Grid.NodeFromWorldPosition(transformTask.position);
                //if (currentNode.isWalkable)
                //    lastWalkableNode = currentNode;

                //move to next node
                Move();

                //when reach node, remove from list
                CheckReachNode();
            }
            ////if there is no path, move straight to target (only if last walkable node is setted)
            //else if(lastWalkableNode != null)
            //{
            //    //if target is in a not walkable node, but neighbour of our last walkable node, move straight to it
            //    Node2D targetNode = pathFinding.Grid.NodeFromWorldPosition(target.position);
            //    if (pathFinding.Grid.GetNeighbours(lastWalkableNode).Contains(targetNode))
            //    {
            //        MoveAndAim(target.position);
            //    }
            //    //else move back to last walkable node
            //    else
            //    {
            //        MoveAndAim(lastWalkableNode.worldPosition);
            //    }
            //}
        }

        #region private API

        void UpdatePath()
        {
            //delay between every update of the path (every few seconds, only if already calculated previous path)
            if (Time.time > timerBeforeNextUpdatePath && agentAStar && agentAStar.IsDone())
            {
                //reset timer
                timerBeforeNextUpdatePath = Time.time + delayRecalculatePath;

                //get path
                agentAStar.FindPath(transformTask.position, GetValue(target).position, OnPathComplete);
            }
        }

        void OnPathComplete(Path path)
        {
            //set path
            this.path = path;

            //and save on blackboard (used for example to aim at next node)
            StateMachine.SetBlackboardElement(savePathInBlackboard, path);
        }

        void Move()
        {
            if (movementComponent)
            {
                //calculate direction
                Vector2 direction = (path.nextNode - (Vector2)transformTask.position).normalized;

                //move to next node in path
                if (useCustomSpeed == false)
                    movementComponent.MoveTo(path.nextNode);
                else
                    movementComponent.MoveTo(path.nextNode, customSpeed);

                //if different direction, then we have sorpassed position to reach
                //(set movementComponent position instead of transformTask, in case StateMachine is child and not root)
                if ((path.nextNode - movementComponent.CalculateNextPosition()).normalized != direction)
                    movementComponent.transform.position = path.nextNode;
            }
        }

        void CheckReachNode()
        {
            //if reach node, remove from list
            if ((mustReachPosition && (Vector2)transformTask.position == path.nextNode) ||                              //reach position
                mustReachPosition == false && Vector2.Distance(transformTask.position, path.nextNode) < approximately)  //or approximately
            {
                path.vectorPath.RemoveAt(0);
            }
        }

        #endregion
    }
}