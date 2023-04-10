using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Input/Move By Input")]
    public class MoveByInput : ActionTask
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Necessary Components - default get in parent")]
        [SerializeField] MovementComponent movementComponent = default;
        [SerializeField] PlayerInput playerInput = default;

        [Header("Movement")]
        [SerializeField] string inputName = "Move";

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //set references
            if (movementComponent == null) movementComponent = GetStateMachineComponent<MovementComponent>();
            if (playerInput == null) playerInput = GetStateMachineComponent<PlayerInput>();

            //show warnings if not found
            if (playerInput && playerInput.actions == null)
                Debug.LogWarning("Miss Actions on PlayerInput on " + StateMachine);
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            if (movementComponent == null || playerInput == null || playerInput.actions == null)
                return;

            //move in direction
            movementComponent.MoveInDirection(playerInput.actions.FindAction(inputName).ReadValue<Vector2>());
        }
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
    }
}