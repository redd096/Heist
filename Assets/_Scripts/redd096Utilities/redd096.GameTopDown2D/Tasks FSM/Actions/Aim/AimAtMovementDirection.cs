using UnityEngine;
using redd096.Attributes;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Aim/Aim At Movement Direction")]
    public class AimAtMovementDirection : ActionTask
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] AimComponent aimComponent = default;
        [SerializeField] MovementComponent movementComponent = default;

        [Header("Rotate immediatly or use a rotation speed")]
        [SerializeField] bool rotateUsingSpeed = false;
        [EnableIf("rotateUsingSpeed")][SerializeField] float rotationSpeed = 50;

        [Header("Call OnCompleteTask when look at direction")]
        [ColorGUI(AttributesUtility.EColor.Orange)][SerializeField] bool callEvent = false;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 drawLineToCurrentDirection = Color.cyan;
        [SerializeField] ShowDebugRedd096 drawLineToMovementDirection = Color.red;

        //events
        public System.Action onStartAimAtPosition { get; set; }
        public System.Action onEndAimAtPosition { get; set; }

        void OnDrawGizmos()
        {
            //draw line to current direction (or vector2.right)
            if (drawLineToCurrentDirection)
            {
                Gizmos.color = drawLineToCurrentDirection.ColorDebug;
                Gizmos.DrawLine(transformTask.position, Application.isPlaying && aimComponent ? (Vector2)transformTask.position + aimComponent.AimDirectionInput * 2 : (Vector2)transformTask.position + Vector2.right * 2);
            }
            //draw line to movement direction (or vector2.right)
            if (drawLineToMovementDirection)
            {
                Gizmos.color = drawLineToMovementDirection.ColorDebug;
                Gizmos.DrawLine(transformTask.position, movementComponent ? (Vector2)transformTask.position + movementComponent.CurrentVelocity.normalized : (Vector2)transformTask.position + Vector2.right * 2);
            }
            Gizmos.color = Color.white;
        }

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //get references
            if (aimComponent == null) aimComponent = GetStateMachineComponent<AimComponent>();
            if (movementComponent == null) movementComponent = GetComponent<MovementComponent>();
        }

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            //call event
            onStartAimAtPosition?.Invoke();
        }

        protected override void OnExitTask()
        {
            base.OnExitTask();

            //call event
            onEndAimAtPosition?.Invoke();
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            //aim in direction
            if (aimComponent && movementComponent)
            {
                //immediatly
                if (rotateUsingSpeed == false)
                {
                    aimComponent.AimInDirection(movementComponent.CurrentVelocity.normalized);
                    if (callEvent) CompleteTask();  //call event
                }
                //or with rotation speed
                else
                {
                    //calculate direction to target
                    Vector2 positionToAim = (Vector2)transformTask.position + movementComponent.CurrentVelocity;
                    Vector2 directionToReach = (positionToAim - (Vector2)transformTask.position).normalized;        //direction to movement
                    float angle = Vector2.SignedAngle(aimComponent.AimDirectionInput, directionToReach);            //rotation angle

                    //rotate only if not already looking at target
                    if (Mathf.Abs(angle) > Mathf.Epsilon)
                    {
                        //calculate rotation, but if exceed, clamp it
                        float rotationAngle = rotationSpeed * Time.deltaTime > Mathf.Abs(angle) ? angle : rotationSpeed * Time.deltaTime * Mathf.Sign(angle);
                        Vector2 newAimPosition = Quaternion.AngleAxis(rotationAngle, Vector3.forward) * aimComponent.AimDirectionInput;

                        //set new aim position
                        aimComponent.AimInDirection(newAimPosition);
                    }
                    //when reach direction, call event
                    else if (callEvent)
                    {
                        CompleteTask();
                    }
                }
            }
        }
    }
}