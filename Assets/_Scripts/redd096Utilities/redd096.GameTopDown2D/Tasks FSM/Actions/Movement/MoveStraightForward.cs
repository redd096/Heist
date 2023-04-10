using UnityEngine;
using redd096.Attributes;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Movement/Move Straight Forward")]
    public class MoveStraightForward : ActionTask
    {
        enum EMoveDirection { AimDirection, LocalRight, LocalUp, GlobalUp, GlobalRight, GlobalDown, GlobalLeft }

        [Header("Necessary Components - default get in parent")]
        [SerializeField] MovementComponent movementComponent = default;
        [SerializeField] AimComponent aimComponent = default;

        [Header("Movement - use aim direction, based on rotation or global")]
        [SerializeField] EMoveDirection moveDirection = EMoveDirection.AimDirection;
        [SerializeField] bool useCustomSpeed = false;
        [EnableIf("useCustomSpeed")][SerializeField] float customSpeed = 5;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 drawLineForward = Color.red;

        void OnDrawGizmos()
        {
            //draw line forward
            if (drawLineForward)
            {
                Gizmos.color = drawLineForward.ColorDebug;
                Gizmos.DrawLine(transformTask.position, (Vector2)transformTask.position + GetDirection() * 2);
                Gizmos.color = Color.white;
            }
        }

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //get references
            if (movementComponent == null) movementComponent = GetStateMachineComponent<MovementComponent>();
            if (aimComponent == null) aimComponent = GetStateMachineComponent<AimComponent>();
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            //move forward
            if (movementComponent)
            {
                if (useCustomSpeed == false)
                    movementComponent.MoveInDirection(GetDirection());
                else
                    movementComponent.MoveInDirection(GetDirection(), customSpeed);
            }
        }

        private Vector2 GetDirection()
        {
            switch (moveDirection)
            {
                case EMoveDirection.AimDirection:
                    if (aimComponent) return aimComponent.AimDirectionInput;
                    else return Vector2.right;
                case EMoveDirection.LocalRight:
                    return transformTask.right;
                case EMoveDirection.LocalUp:
                    return transformTask.up;
                case EMoveDirection.GlobalUp:
                    return Vector2.up;
                case EMoveDirection.GlobalRight:
                    return Vector2.right;
                case EMoveDirection.GlobalDown:
                    return Vector2.down;
                case EMoveDirection.GlobalLeft:
                    return Vector2.left;
                default:
                    return Vector2.right;
            }
        }
    }
}