using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Tasks FSM/Conditions/Check Distance 2D To Position")]
    public class CheckDistance2DToPosition : ConditionTask
    {
        enum ECompareMethod { EqualTo, DisequalTo, GreaterThan, LessThan, GreaterOrEqualTo, LessOrEqualTo }

        [Header("Check Distance")]
        [SerializeField] VarOrBlackboard<Vector2> positionToCheck = new VarOrBlackboard<Vector2>("Last Target Position");
        [SerializeField] ECompareMethod compare = ECompareMethod.LessThan;
        [SerializeField] VarOrBlackboard<float> distance = 0.1f;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 showLineToPosition = Color.cyan;
        [SerializeField] ShowDebugRedd096 showAreaDistance = Color.magenta;

        void OnDrawGizmos()
        {
            //draw line to position to check
            if (showLineToPosition)
            {
                Gizmos.color = showLineToPosition.ColorDebug;
                Gizmos.DrawLine(transformTask.position, GetValue(positionToCheck));
                Gizmos.color = Color.white;
            }
            //draw radius distance
            if (showAreaDistance)
            {
                Gizmos.color = showAreaDistance.ColorDebug;
                Gizmos.DrawWireSphere(transformTask.position, GetValue(distance));
                Gizmos.color = Color.white;
            }
        }

        public override bool OnCheckTask()
        {
            //else return compare
            return Compare(Vector2.Distance(GetValue(positionToCheck), transformTask.position), GetValue(distance), compare);
        }

        /// <summary>
        /// Compare between 2 values
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="cm"></param>
        /// <param name="floatingPoint"></param>
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