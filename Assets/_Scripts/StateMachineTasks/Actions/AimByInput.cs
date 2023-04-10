using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using redd096.StateMachine.StateMachineRedd096;

public class AimByInput : ActionTask
{
#if ENABLE_INPUT_SYSTEM
    [Header("Necessary Components - default get in parent")]
    [SerializeField] AimComponent aimComponent = default;
    [SerializeField] PlayerInput playerInput = default;

    [Header("Aim")]
    [SerializeField] string inputName = "Move";
    [SerializeField] bool resetWhenReleaseAnalogInput = false;

    Vector2 inputValue;
    Vector2 lastAnalogSavedValue = Vector2.right;

    protected override void OnInitTask()
    {
        base.OnInitTask();

        //set references
        if (aimComponent == null) aimComponent = GetStateMachineComponent<AimComponent>();
        if (playerInput == null) playerInput = GetStateMachineComponent<PlayerInput>();

        lastAnalogSavedValue = new Vector2(transformTask.forward.x, transformTask.forward.z);

        //show warnings if not found
        if (playerInput && playerInput.actions == null)
            Debug.LogWarning("Miss Actions on PlayerInput on " + StateMachine);
    }

    public override void OnUpdateTask()
    {
        base.OnUpdateTask();

        if (aimComponent == null || playerInput == null || playerInput.actions == null)
            return;

        //get input
        inputValue = playerInput.actions.FindAction(inputName).ReadValue<Vector2>();

        //check if moving analog or reset input when released
        if (inputValue != Vector2.zero || resetWhenReleaseAnalogInput)
        {
            aimComponent.AimInDirection(new Vector3(inputValue.x, 0, inputValue.y));
            lastAnalogSavedValue = inputValue;  //save input
        }
        //else show last saved input
        else
        {
            aimComponent.AimInDirection(new Vector3(lastAnalogSavedValue.x, 0, lastAnalogSavedValue.y));
        }
    }
#else
    [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
    public string Error = "It works only with new unity input system";
#endif
}