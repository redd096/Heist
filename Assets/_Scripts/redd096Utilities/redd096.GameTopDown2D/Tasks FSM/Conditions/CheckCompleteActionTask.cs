using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Conditions/Check Complete Action Task")]
    public class CheckCompleteActionTask : ConditionTask
    {
        [Header("Action to complete")]
        [SerializeField] ActionTask actionToComplete = default;

        bool isActionComplete;

        private void OnEnable()
        {
            //register to event
            if (actionToComplete)
                actionToComplete.onCompleteTask += OnCompleteTask;
        }

        private void OnDisable()
        {
            //unregister from event
            if (actionToComplete)
                actionToComplete.onCompleteTask -= OnCompleteTask;
        }

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            //reset var
            isActionComplete = false;
        }

        public override bool OnCheckTask()
        {
            //return when action is complete
            return isActionComplete;
        }

        private void OnCompleteTask()
        {
            //set var
            isActionComplete = true;
        }
    }
}