using UnityEngine;
using redd096.Attributes;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Conditions/Check Float")]
    public class CheckFloat : ConditionTask
    {
        enum ECompareMethod { EqualTo, DisequalTo, GreaterThan, LessThan, GreaterOrEqualTo, LessOrEqualTo }
        enum EValueToCompare { Value, Time, RealTime, TimeScale }

        [HelpBox("Use normal value or from blackboard, else use Time.time, Time.realtime, Time.timeScale")]
        [Header("Value A")]
        [SerializeField] EValueToCompare valueToCompareA = EValueToCompare.Value;
        [SerializeField] VarOrBlackboard<float> valueA = 10;

        [Header("Compare Method")]
        [SerializeField] ECompareMethod compare = ECompareMethod.GreaterThan;

        [Header("Value B")]
        [SerializeField] EValueToCompare valueToCompareB = EValueToCompare.Value;
        [SerializeField] VarOrBlackboard<float> valueB = 10;

        public override bool OnCheckTask()
        {
            //compare
            return Compare(Value(valueToCompareA, valueA), Value(valueToCompareB, valueB), compare);
        }

        /// <summary>
        /// Get correct value
        /// </summary>
        /// <param name="valueToCompare"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        float Value(EValueToCompare valueToCompare, VarOrBlackboard<float> value)
        {
            switch (valueToCompare)
            {
                case EValueToCompare.Value:
                    return GetValue(value);
                case EValueToCompare.Time:
                    return Time.time;
                case EValueToCompare.RealTime:
                    return Time.realtimeSinceStartup;
                case EValueToCompare.TimeScale:
                    return Time.timeScale;
                default:
                    return GetValue(value);
            }
        }

        /// <summary>
        /// Compare between 2 values
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="cm"></param>
        /// <returns></returns>
        bool Compare(float a, float b, ECompareMethod cm)
        {
            switch (cm)
            {
                case ECompareMethod.EqualTo:
                    return Mathf.Approximately(a, b);
                case ECompareMethod.DisequalTo:
                    return Mathf.Approximately(a, b) == false;
                case ECompareMethod.GreaterThan:
                    return a > b;
                case ECompareMethod.LessThan:
                    return a < b;
                case ECompareMethod.GreaterOrEqualTo:
                    return a >= b;
                case ECompareMethod.LessOrEqualTo:
                    return a <= b;
                default:
                    return Mathf.Approximately(a, b);
            }
        }
    }
}