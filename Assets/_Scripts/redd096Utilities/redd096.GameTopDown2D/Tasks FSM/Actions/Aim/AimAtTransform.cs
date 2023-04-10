using UnityEngine;
using redd096.Attributes;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Aim/Aim At Transform")]
    public class AimAtTransform : ActionTask
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] AimComponent aimComponent = default;

        [Header("Aim")]
        [SerializeField] VarOrBlackboard<Transform> target = new VarOrBlackboard<Transform>("Target");
        [Tooltip("Rotate immediatly or use a rotation speed")][SerializeField] bool rotateUsingSpeed = false;
        [EnableIf("rotateUsingSpeed")][SerializeField] float rotationSpeed = 50;

        [Header("Call OnCompleteTask when look at target")]
        [ColorGUI(AttributesUtility.EColor.Orange)][SerializeField] bool callEvent = false;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 drawLineToCurrentDirection = Color.cyan;
        [SerializeField] ShowDebugRedd096 drawLineToTarget = Color.red;

        //events
        public System.Action onStartAimAtTarget { get; set; }
        public System.Action onEndAimAtTarget { get; set; }

        void OnDrawGizmos()
        {
            //draw line to aim direction or right
            if (drawLineToCurrentDirection)
            {
                Gizmos.color = drawLineToCurrentDirection.ColorDebug;
                Gizmos.DrawLine(transformTask.position, Application.isPlaying && aimComponent ? (Vector2)transformTask.position + aimComponent.AimDirectionInput * 2 : (Vector2)transformTask.position + Vector2.right * 2);
            }
            //draw line to target
            if (drawLineToTarget)
            {
                Gizmos.color = drawLineToTarget.ColorDebug;
                if (GetValue(target)) Gizmos.DrawLine(transformTask.position, GetValue(target).position);
            }
            Gizmos.color = Color.white;
        }

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //get references
            if (aimComponent == null) aimComponent = GetStateMachineComponent<AimComponent>();
        }

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            //call event
            onStartAimAtTarget?.Invoke();
        }

        protected override void OnExitTask()
        {
            base.OnExitTask();

            //call event
            onEndAimAtTarget?.Invoke();
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            //aim at target
            if (aimComponent && GetValue(target))
            {
                //immediatly
                if (rotateUsingSpeed == false)
                {
                    aimComponent.AimAt(GetValue(target).position);
                    if (callEvent) CompleteTask();  //call event
                }
                //or with rotation speed
                else
                {
                    //calculate direction to target
                    Vector2 directionToReach = (GetValue(target).position - transformTask.position).normalized;     //direction to target
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
                    //when reach target, call event
                    else if (callEvent)
                    {
                        CompleteTask();
                    }
                }
            }
        }
    }
}