using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Components/Enemies/Shield")]
    public class Shield : MonoBehaviour
    {
        [Header("Necessary Components - default get from this gameObject")]
        [SerializeField] AimComponent aimComponent = default;

        [Header("Shield Area")]
        [SerializeField] Vector2 offset = Vector2.right;
        [SerializeField] Vector2 size = Vector2.one;

        [Header("Reduce Damage - return only a percentage")]
        [Range(0f, 1f)][SerializeField] float percentageDamage = 0.25f;

        [Header("DEBUG")]
        [SerializeField] ShowDebugRedd096 drawShield = Color.yellow;

        //events
        public System.Action<Vector2> onHitShield { get; set; }

        //shield position vars
        Vector2 center;
        Vector2 upLeft;
        Vector2 upRight;
        Vector2 downRight;
        Vector2 downLeft;

        void OnDrawGizmos()
        {
            if (drawShield)
            {
                Gizmos.color = drawShield.ColorDebug;

                //draw attack area
                int direction = aimComponent && aimComponent.IsLookingRight ? 1 : -1;
                Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(offset.x * direction, offset.y), size);

                Gizmos.color = Color.white;
            }
        }

        void Start()
        {
            //get references
            if (aimComponent == null)
                aimComponent = GetComponent<AimComponent>();

            if (aimComponent == null)
                Debug.LogWarning("Miss AimComponent necessary for Shield in " + name);
        }

        public bool CheckHitShield(Vector2 hitPoint)
        {
            //do nothing if not enabled
            if (enabled == false)
                return false;

            //update position of the shield box
            UpdateBox();

            //if inside shield, return true
            if (IsInsideShieldArea(hitPoint))
            {
                onHitShield?.Invoke(hitPoint);
                return true;
            }

            //else return if intersect line with shield box (because for example an explosion will send origin point, so we need to calculate line from origin to hit if intersect)
            if (IntersectWithShield(hitPoint))
            {
                onHitShield?.Invoke(hitPoint);
                return true;
            }

            return false;
        }

        public float PercentageDamage()
        {
            return percentageDamage;
        }

        #region private API

        void UpdateBox()
        {
            //update box values (without component, is right)
            int direction = aimComponent ? (aimComponent.IsLookingRight ? 1 : -1) : 1;
            center = (Vector2)transform.position + new Vector2(offset.x * direction, offset.y);
            upLeft = center + new Vector2(-size.x, size.y) * 0.5f;
            upRight = center + size * 0.5f;
            downRight = center + new Vector2(size.x, -size.y) * 0.5f;
            downLeft = center - size * 0.5f;
        }

        bool IsInsideShieldArea(Vector2 point)
        {
            //check if inside box
            if (point.x > downLeft.x && point.x < upRight.x         //horizontal
                && point.y > downLeft.y && point.y < upRight.y)     //vertical
            {
                return true;
            }

            return false;
        }

        bool IntersectWithShield(Vector2 point)
        {
            //check every side of the shield if intersect with line (from point to this gameObject)
            return LineIntersectLine(point, transform.position, upLeft, upRight) || LineIntersectLine(point, transform.position, downLeft, downRight)   //check up and down
                || LineIntersectLine(point, transform.position, upLeft, downLeft) || LineIntersectLine(point, transform.position, upRight, downRight);  //check left and right
        }

        /// <summary>
        /// Check if there is an intersection between two lines
        /// </summary>
        /// <param name="StartLineA"></param>
        /// <param name="EndLineA"></param>
        /// <param name="StartLineB"></param>
        /// <param name="EndLineB"></param>
        /// <returns></returns>
        bool LineIntersectLine(Vector2 StartLineA, Vector2 EndLineA, Vector2 StartLineB, Vector2 EndLineB)
        {
            //function return >0 if right and <0 if left, so if Start and End are in two different position respect the line, will be one negative number and one positive.
            //if multiply negative * positive, will give negative (<0). So in the first line we check B intersect A, in the second line we check A intersect B.
            //why we need to check both? Because can be that B is from left to right, but is over or under A, so there is not a really intersect
            return CheckPointPosition(StartLineA, EndLineA, StartLineB) * CheckPointPosition(StartLineA, EndLineA, EndLineB) < 0
                && CheckPointPosition(StartLineB, EndLineB, StartLineA) * CheckPointPosition(StartLineB, EndLineB, EndLineA) < 0;
        }

        /// <summary>
        /// return < 0 if Point is on the left side of the Line, return > 0 if Point is on the right side of the Line
        /// </summary>
        /// <param name="StartLine"></param>
        /// <param name="EndLine"></param>
        /// <param name="Point"></param>
        /// <returns></returns>
        float CheckPointPosition(Vector2 StartLine, Vector2 EndLine, Vector2 Point)
        {
            //return (EndLine - StartLine) * (Point - StartLine);
            return (EndLine.x - StartLine.x) * (Point.y - StartLine.y) - (EndLine.y - StartLine.y) * (Point.x - StartLine.x);
        }

        bool UnrealLineIntersect(Vector3 SegmentStartA, Vector3 SegmentEndA, Vector3 SegmentStartB, Vector3 SegmentEndB, ref Vector3 out_IntersectionPoint)
        {
            Vector3 VectorA = SegmentEndA - SegmentStartA;
            Vector3 VectorB = SegmentEndB - SegmentStartB;

            float S = (-VectorA.y * (SegmentStartA.x - SegmentStartB.x) + VectorA.x * (SegmentStartA.y - SegmentStartB.y)) / (-VectorB.x * VectorA.y + VectorA.x * VectorB.y);
            float T = (VectorB.x * (SegmentStartA.y - SegmentStartB.y) - VectorB.y * (SegmentStartA.x - SegmentStartB.x)) / (-VectorB.x * VectorA.y + VectorA.x * VectorB.y);

            bool bIntersects = (S >= 0 && S <= 1 && T >= 0 && T <= 1);

            if (bIntersects)
            {
                out_IntersectionPoint.x = SegmentStartA.x + (T * VectorA.x);
                out_IntersectionPoint.y = SegmentStartA.y + (T * VectorA.y);
                out_IntersectionPoint.z = SegmentStartA.z + (T * VectorA.z);
            }

            return bIntersects;
        }

        #endregion
    }
}