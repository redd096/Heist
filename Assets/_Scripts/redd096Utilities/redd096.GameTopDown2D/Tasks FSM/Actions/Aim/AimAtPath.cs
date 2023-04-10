using UnityEngine;
using redd096.Attributes;
using redd096.PathFinding.AStar2D;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Aim/Aim At Path")]
    public class AimAtPath : ActionTask
    {
        [Header("Necessary Components - default get in parent")]
        [SerializeField] AimComponent aimComponent = default;

        [Header("Rotate immediatly or use a rotation speed")]
        [SerializeField] VarOrBlackboard<Path> path = new VarOrBlackboard<Path>("Path");
        [SerializeField] bool rotateUsingSpeed = false;
        [EnableIf("rotateUsingSpeed")][SerializeField] float rotationSpeed = 50;

        [Header("Call OnCompleteTask when look at next node")]
        [ColorGUI(AttributesUtility.EColor.Orange)][SerializeField] bool callEvent = false;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 drawLineToCurrentDirection = Color.cyan;
        [SerializeField] ShowDebugRedd096 drawLineToNextNode = Color.red;
        [SerializeField] ShowDebugRedd096 drawPath = Color.red;

        //events
        public System.Action onStartAimAtPosition { get; set; }
        public System.Action onEndAimAtPosition { get; set; }

        void OnDrawGizmos()
        {
            //draw line to aim direction or right
            if (drawLineToCurrentDirection)
            {
                Gizmos.color = drawLineToCurrentDirection.ColorDebug;
                Gizmos.DrawLine(transformTask.position, Application.isPlaying && aimComponent ? (Vector2)transformTask.position + aimComponent.AimDirectionInput * 2 : (Vector2)transformTask.position + Vector2.right * 2);
            }
            //draw line to position to next node
            if (drawLineToNextNode && GetValue(path) != null)
            {
                Gizmos.color = drawLineToNextNode.ColorDebug;
                Gizmos.DrawLine(transformTask.position, GetValue(path).nextNode);
            }
            //draw path
            if (drawPath)
            {
                if (GetValue(path) != null && GetValue(path).vectorPath != null && GetValue(path).vectorPath.Count > 0)
                {
                    Gizmos.color = drawPath.ColorDebug;
                    for (int i = 0; i < GetValue(path).vectorPath.Count; i++)
                    {
                        if (i + 1 < GetValue(path).vectorPath.Count)
                            Gizmos.DrawLine(GetValue(path).vectorPath[i], GetValue(path).vectorPath[i + 1]);
                    }
                    Gizmos.color = Color.white;
                }
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

            //aim at path
            if (aimComponent && GetValue(path) != null)
            {
                //immediatly
                if (rotateUsingSpeed == false)
                {
                    aimComponent.AimAt(GetValue(path).nextNode);
                    if (callEvent) CompleteTask();  //call event
                }
                //or with rotation speed
                else
                {
                    //calculate direction to target
                    Vector2 directionToReach = (GetValue(path).nextNode - (Vector2)transformTask.position).normalized;  //direction to next node
                    float angle = Vector2.SignedAngle(aimComponent.AimDirectionInput, directionToReach);                //rotation angle

                    //rotate only if not already looking at position
                    if (Mathf.Abs(angle) > Mathf.Epsilon)
                    {
                        //calculate rotation, but if exceed, clamp it
                        float rotationAngle = rotationSpeed * Time.deltaTime > Mathf.Abs(angle) ? angle : rotationSpeed * Time.deltaTime * Mathf.Sign(angle);
                        Vector2 newAimPosition = Quaternion.AngleAxis(rotationAngle, Vector3.forward) * aimComponent.AimDirectionInput;

                        //set new aim position
                        aimComponent.AimInDirection(newAimPosition);
                    }
                    //when reach position, call event
                    else if (callEvent)
                    {
                        CompleteTask();
                    }
                }
            }
        }
    }
}