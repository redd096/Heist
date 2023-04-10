using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using redd096.StateMachine.StateMachineRedd096;

public class ThrowByInput : ActionTask
{
#if ENABLE_INPUT_SYSTEM
    [Header("Necessary Components - default get in parent")]
    [SerializeField] ThrowComponent throwComponent = default;
    [SerializeField] PlayerInput playerInput = default;

    [Header("Throw")]
    [SerializeField] string inputName = "Throw";

    protected override void OnInitTask()
    {
        base.OnInitTask();

        //set references
        if (throwComponent == null) throwComponent = GetStateMachineComponent<ThrowComponent>();
        if (playerInput == null) playerInput = GetStateMachineComponent<PlayerInput>();

        //show warnings if not found
        if (playerInput && playerInput.actions == null)
            Debug.LogWarning("Miss Actions on PlayerInput on " + StateMachine);
    }

    public override void OnUpdateTask()
    {
        base.OnUpdateTask();

        if (throwComponent == null || playerInput == null || playerInput.actions == null)
            return;

        //when press input
        if (playerInput.actions.FindAction(inputName).WasPressedThisFrame())
            throwComponent.Throw();
    }
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
}
