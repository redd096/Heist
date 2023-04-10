using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Conditions/Check Input Press")]
    public class CheckInputPress : ConditionTask
    {
#if ENABLE_INPUT_SYSTEM
        enum EPressType { Pressed, Down }

        [Header("Necessary Components - default get in parent")]
        [SerializeField] PlayerInput playerInput = default;

        [Header("Button")]
        [SerializeField] EPressType pressType = EPressType.Down;
        [SerializeField] VarOrBlackboard<string> buttonName = "Pause";

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //set references
            if (playerInput == null) playerInput = GetStateMachineComponent<PlayerInput>();

            //show warning if not found
            if (playerInput && playerInput.actions == null)
                Debug.LogWarning("Miss Actions on PlayerInput on " + StateMachine);
        }

        public override bool OnCheckTask()
        {
            if (playerInput == null)
                return false;

            //check if pressed or pressed down
            if (pressType == EPressType.Pressed)
                return playerInput.actions.FindAction(GetValue(buttonName)).phase == InputActionPhase.Started;
            else if (pressType == EPressType.Down)
                return playerInput.actions.FindAction(GetValue(buttonName)).triggered;

            return false;
        }
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
    }
}