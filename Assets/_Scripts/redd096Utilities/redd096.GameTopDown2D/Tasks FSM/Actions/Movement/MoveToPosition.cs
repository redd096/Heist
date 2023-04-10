using UnityEngine;
using redd096.Attributes;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Movement/Move To Position")]
    public class MoveToPosition : ActionTask
    {
        [Header("Necessary Components - default get in parent and child")]
        [SerializeField] MovementComponent movementComponent = default;

        [Header("Movement")]
        [SerializeField] VarOrBlackboard<Vector2> positionToReach = new VarOrBlackboard<Vector2>("Last Target Position");
        [SerializeField] bool useCustomSpeed = false;
        [EnableIf("useCustomSpeed")][SerializeField] float customSpeed = 5;

        [Header("Reach position or just move near")]
        [SerializeField] bool mustReachPosition = true;
        [DisableIf("mustReachPosition")][SerializeField] float approximately = 0.05f;

        [Header("Call OnCompleteTask when reach position")]
        [ColorGUI(AttributesUtility.EColor.Orange)][SerializeField] bool callEvent = false;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 showPositionToReach = Color.cyan;

        bool reachedPosition;

        void OnDrawGizmos()
        {
            //draw line to reach position
            if (showPositionToReach)
            {
                Gizmos.color = showPositionToReach.ColorDebug;
                Gizmos.DrawLine(transformTask.position, GetValue(positionToReach));
                Gizmos.color = Color.white;
            }
        }

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //get references
            if (movementComponent == null) movementComponent = GetStateMachineComponent<MovementComponent>();
        }

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            //reset vars
            reachedPosition = false;
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            //if reached position, do nothing
            if (reachedPosition)
            {
                return;
            }

            //move to position
            Move();

            //set when reach position
            if ((mustReachPosition && (Vector2)transformTask.position == GetValue(positionToReach)) ||                              //reach position
                mustReachPosition == false && Vector2.Distance(transformTask.position, GetValue(positionToReach)) < approximately)  //or approximately
            {
                reachedPosition = true;

                //call complete task event
                if (callEvent)
                    CompleteTask();
            }
        }

        void Move()
        {
            if (movementComponent)
            {
                //calculate direction
                Vector2 direction = (GetValue(positionToReach) - (Vector2)transformTask.position).normalized;

                //move
                if (useCustomSpeed == false)
                    movementComponent.MoveTo(GetValue(positionToReach));
                else
                    movementComponent.MoveTo(GetValue(positionToReach), customSpeed);

                //if different direction, then we have sorpassed position to reach
                //(set movementComponent position instead of transformTask, in case StateMachine is child and not root)
                if ((GetValue(positionToReach) - movementComponent.CalculateNextPosition()).normalized != direction)
                    movementComponent.transform.position = GetValue(positionToReach);
            }
        }
    }
}