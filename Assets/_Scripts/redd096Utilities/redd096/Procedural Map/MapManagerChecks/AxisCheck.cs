using UnityEngine;

namespace redd096
{
    enum Axis
    {
        X, Y, Z
    }

    enum CheckOperator
    {
        greater, greaterEqual, lower, lowerEqual, equal, notEqual
    }

    [CreateAssetMenu(menuName = "redd096/Procedural Map Manager Check/Axis Check")]
    public class AxisCheck : ProceduralMapManagerCheck
    {
        [SerializeField] Axis axis = Axis.X;
        [SerializeField] CheckOperator checkToDo = CheckOperator.greaterEqual;
        [SerializeField] float threshold = 0;

        public override bool IsValid(Room roomToPlace, ProceduralMapManager mapManager)
        {
            //for every axis, check
            switch (axis)
            {
                case Axis.X:
                    return CheckToDo(roomToPlace.transform.position.x);
                case Axis.Y:
                    return CheckToDo(roomToPlace.transform.position.y);
                case Axis.Z:
                    return CheckToDo(roomToPlace.transform.position.z);
                default:
                    return false;
            }
        }

        bool CheckToDo(float roomAxis)
        {
            //check operator
            switch (checkToDo)
            {
                case CheckOperator.greater:
                    return roomAxis > threshold;
                case CheckOperator.greaterEqual:
                    return roomAxis >= threshold;
                case CheckOperator.lower:
                    return roomAxis < threshold;
                case CheckOperator.lowerEqual:
                    return roomAxis <= threshold;
                case CheckOperator.equal:
                    return roomAxis == threshold;
                case CheckOperator.notEqual:
                    return roomAxis != threshold;
                default:
                    return false;
            }
        }
    }
}