using UnityEngine;
using redd096.PathFinding.AStar2D;
using redd096.Attributes;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Movement/Move Random With Path Finding")]
    public class MoveRandomWithPathFinding : ActionTask
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] MovementComponent movementComponent = default;
        [SerializeField] AgentAStar agentAStar = default;

        [Header("Area Movement")]
        [SerializeField] float radiusArea = 5;
        [SerializeField] float timeToWaitWhenReach = 1;
        [ColorGUI(AttributesUtility.EColor.Yellow)][SerializeField] string savePathInBlackboard = "Path";
        [SerializeField] bool useCustomSpeed = false;
        [EnableIf("useCustomSpeed")][SerializeField] float customSpeed = 5;

        [Header("Reach position or just move near")]
        [SerializeField] bool mustReachPosition = true;
        [DisableIf("mustReachPosition")][SerializeField] float approximately = 0.05f;

        [Header("Call OnCompleteTask when reach one destination")]
        [ColorGUI(AttributesUtility.EColor.Orange)][SerializeField] bool callEvent = false;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 drawAreaMovement = Color.magenta;
        [SerializeField] ShowDebugRedd096 drawPath = Color.cyan;
        [SerializeField] float delayRecalculatePath = 0.5f;

        float timerBeforeNextUpdatePath;
        Vector2 startPosition;
        float waitTimer;
        Path path;

        void OnDrawGizmos()
        {
            //draw area movement
            if (drawAreaMovement)
            {
                Gizmos.color = drawAreaMovement.ColorDebug;
                Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : (Vector2)transformTask.position, radiusArea);
                Gizmos.color = Color.white;
            }

            //draw path
            if (drawPath)
            {
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

            //save start position
            startPosition = transformTask.position;
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

            //if there is no path, find new one (at start or when reach destination)
            if (path == null || path.vectorPath == null || path.vectorPath.Count <= 0)
            {
                FindNewPath();
                return;
            }

            //wait when reached destination, before start moving again
            if (waitTimer > Time.time)
            {
                return;
            }

            //move to next node
            Move();

            //when reach node, remove from list. If reach end of path, set wait timer
            CheckReachNode();
            CheckReachEndPath();
        }

        #region private API

        void FindNewPath()
        {
            //delay between every update of the path (every few seconds, only if already calculated previous path)
            if (Time.time > timerBeforeNextUpdatePath && agentAStar && agentAStar.IsDone())
            {
                //reset timer
                timerBeforeNextUpdatePath = Time.time + delayRecalculatePath;

                //get random point in patrol area
                Vector3 randomPoint = startPosition + Random.insideUnitCircle * radiusArea;

                //get path
                agentAStar.FindPath(transformTask.position, randomPoint, OnPathComplete);
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

        void CheckReachEndPath()
        {
            //when reach destination, set wait timer
            if (path == null || path.vectorPath == null || path.vectorPath.Count <= 0)
            {
                waitTimer = Time.time + timeToWaitWhenReach;

                //call complete task event
                if (callEvent)
                    CompleteTask();
            }
        }

        #endregion
    }
}