using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Conditions/Check Transform")]
    public class CheckTransform : ConditionTask
    {
        enum ECompareMethod { EqualTo, DisequalTo }

        [Header("Value A")]
        [SerializeField] VarOrBlackboard<Transform> valueToCompareA = new VarOrBlackboard<Transform>("Target");

        [Header("Compare Method")]
        [SerializeField] ECompareMethod compare = ECompareMethod.DisequalTo;

        [Header("Value B")]
        [SerializeField] VarOrBlackboard<Transform> valueToCompareB = default;

        public override bool OnCheckTask()
        {
            //compare
            return Compare(GetValue(valueToCompareA), GetValue(valueToCompareB), compare);
        }

        /// <summary>
        /// Compare between 2 values
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="cm"></param>
        /// <returns></returns>
        bool Compare(Transform a, Transform b, ECompareMethod cm)
        {
            switch (cm)
            {
                case ECompareMethod.EqualTo:
                    return a == b;
                case ECompareMethod.DisequalTo:
                    return a != b;
                default:
                    return a == b;
            }
        }
    }
}