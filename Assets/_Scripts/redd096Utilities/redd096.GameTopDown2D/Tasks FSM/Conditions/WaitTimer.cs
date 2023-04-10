using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Conditions/Wait Timer")]
    public class WaitTimer : ConditionTask
    {
        enum ETime { Time, RealTime }

        [Header("Wait Timer - Time.time or Time.realtime")]
        [SerializeField] VarOrBlackboard<float> timer = 1;
        [SerializeField] ETime timeToCheck = ETime.Time;

        float timeToEnd;

        protected override void OnEnterTask()
        {
            base.OnEnterTask();

            //set time to end
            timeToEnd = GetTime() + GetValue(timer);
        }

        public override bool OnCheckTask()
        {
            //return when timer is finished
            return GetTime() > timeToEnd;
        }

        float GetTime()
        {
            //use Time.time or Time.realtime
            switch (timeToCheck)
            {
                case ETime.Time:
                    return Time.time;
                case ETime.RealTime:
                    return Time.realtimeSinceStartup;
                default:
                    return Time.time;
            }
        }
    }
}