using UnityEngine;
using redd096.Attributes;
using redd096.GameTopDown2D;
using UnityEditorInternal;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using redd096.StateMachine.StateMachineRedd096;

public class InteractByInput : ActionTask
{
#if ENABLE_INPUT_SYSTEM
    [Header("Necessary Components - default get in parent")]
    [SerializeField] DragComponent interactComponent = default;
    [SerializeField] PlayerInput playerInput = default;

    [Header("Interact")]
    [SerializeField] string inputName = "Interact";

    protected override void OnInitTask()
    {
        base.OnInitTask();

        //set references
        if (interactComponent == null) interactComponent = GetStateMachineComponent<DragComponent>();
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
