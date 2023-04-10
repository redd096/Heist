using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Input/Interact By Input")]
    public class InteractByInput : ActionTask
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Necessary Components - default get in parent")]
        [SerializeField] InteractComponent interactComponent = default;
        [SerializeField] PlayerInput playerInput = default;

        [Header("Interact")]
        [SerializeField] string inputName = "Interact";

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //set references
            if (interactComponent == null) interactComponent = GetStateMachineComponent<InteractComponent>();
            if (playerInput == null) playerInput = GetStateMachineComponent<PlayerInput>();

            //show warnings if not found
            if (playerInput && playerInput.actions == null)
                Debug.LogWarning("Miss Actions on PlayerInput on " + StateMachine);
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            if (interactComponent == null || playerInput == null || playerInput.actions == null)
                return;

            //when press input, interact
            if (playerInput.actions.FindAction(inputName).WasPressedThisFrame())
                interactComponent.Interact();
        }
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
    }
}