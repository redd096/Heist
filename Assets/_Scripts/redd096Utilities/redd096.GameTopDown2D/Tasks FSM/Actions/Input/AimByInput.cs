using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Input/Aim By Input")]
    public class AimByInput : ActionTask
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Necessary Components - default get in parent")]
        [SerializeField] AimComponent aimComponent = default;
        [SerializeField] PlayerInput playerInput = default;

        [Header("For Mouse - default cam is MainCamera")]
        [SerializeField] Camera cam = default;
        [SerializeField] string mouseSchemeName = "KeyboardAndMouse";

        [Header("Aim")]
        [SerializeField] string inputName = "Aim";
        [SerializeField] bool resetWhenReleaseAnalogInput = false;

        Vector2 inputValue;
        Vector2 lastAnalogSavedValue = Vector2.right;

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //set references
            if (aimComponent == null) aimComponent = GetStateMachineComponent<AimComponent>();
            if (playerInput == null) playerInput = GetStateMachineComponent<PlayerInput>();
            if (cam == null) cam = Camera.main;

            //show warnings if not found
            if (playerInput && playerInput.actions == null)
                Debug.LogWarning("Miss Actions on PlayerInput on " + StateMachine);
            if (cam == null)
                Debug.LogWarning("Miss Camera for " + StateMachine);
        }

        public override void OnUpdateTask()
        {
            base.OnUpdateTask();

            if (aimComponent == null || playerInput == null || playerInput.actions == null)
                return;

            //get input
            inputValue = playerInput.actions.FindAction(inputName).ReadValue<Vector2>();

            //set direction using mouse position
            if (playerInput.currentControlScheme == mouseSchemeName)
            {
                //be sure to have camera setted
                if (cam)
                {
                    aimComponent.AimAt(cam.ScreenToWorldPoint(inputValue));
                }
            }
            //or using analog
            else
            {
                //check if moving analog or reset input when released
                if (inputValue != Vector2.zero || resetWhenReleaseAnalogInput)
                {
                    aimComponent.AimAt((Vector2)transformTask.position + inputValue);
                    lastAnalogSavedValue = inputValue;  //save input
                }
                //else show last saved input
                else
                {
                    aimComponent.AimAt((Vector2)transformTask.position + lastAnalogSavedValue);
                }
            }
        }
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
    }
}