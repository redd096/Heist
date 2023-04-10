using redd096.Attributes;
using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Actions/Generic/Set Movement Speed")]
    public class SetMovementSpeed : ActionTask
    {
        enum EMode { SetOnInit, SetOnEnter, SetOnExit, SetOnEnterResetOnExit, SetOnEnterPutAtZeroOnExit }

        [Header("Necessary Components - default get in parent")]
        [SerializeField] MovementComponent movementComponent = default;

        [Header("Set Input Speed")]
        [SerializeField] float speedToSet = 5;
        [SerializeField] EMode mode = EMode.SetOnEnterResetOnExit;

        [Header("Call OnCompleteTask when finish")]
        [ColorGUI(AttributesUtility.EColor.Orange)][SerializeField] bool callEvent = false;

        float previousSpeed;

        protected override void OnInitTask()
        {
            base.OnInitTask();

            //get references
            if (movementComponent == null) movementComponent = GetStateMachineComponent<MovementComponent>();

            if (movementComponent && mode == EMode.SetOnInit)
            {
                //set speed
                movementComponent.InputSpeed = speedToSet;

                //call event if finish
                if (callEvent)
                    CompleteTask();
            }
        }

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            if (movementComponent)
            {
                //save previous speed
                previousSpeed = movementComponent.InputSpeed;

                //set speed
                if (mode != EMode.SetOnExit)
                    movementComponent.InputSpeed = speedToSet;

                //call event if finish
                if (callEvent && mode == EMode.SetOnEnter)
                    CompleteTask();
            }
        }

        protected override void OnExitTask()
        {
            base.OnExitTask();

            if (movementComponent)
            {
                //set speed
                if (mode == EMode.SetOnExit)
                    movementComponent.InputSpeed = speedToSet;
                else if (mode == EMode.SetOnEnterPutAtZeroOnExit)
                    movementComponent.InputSpeed = 0;
                else if (mode == EMode.SetOnEnterResetOnExit)
                    movementComponent.InputSpeed = previousSpeed;

                //call event if finish
                if (callEvent)
                    CompleteTask();
            }
        }
    }
}