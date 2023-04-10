using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Components/Aim Component")]
    public class AimComponent : MonoBehaviour
    {
        [Header("DEBUG")]
        [ReadOnly] public bool IsLookingRight = true;                           //check if looking right
        [ReadOnly] public Vector2 AimDirectionInput = Vector2.right;            //when aim, set it with only direction (used to know where this object is aiming)
        [ReadOnly] public Vector2 AimPositionNotNormalized = Vector2.right;     //when aim, set it without normalize (used to set crosshair on screen - to know mouse position or analog inclination)
        [SerializeField] ShowDebugRedd096 showPositionNotNormalized = Color.red;
        [SerializeField] ShowDebugRedd096 showDirectionInput = Color.cyan;

        //events
        public System.Action<bool> onChangeAimDirection { get; set; }

        void OnDrawGizmos()
        {
            //draw sphere to see where is aiming
            if (showPositionNotNormalized)
            {
                Gizmos.color = showPositionNotNormalized.ColorDebug;
                Gizmos.DrawWireSphere(AimPositionNotNormalized, 1);
            }
            if (showDirectionInput)
            {
                Gizmos.color = showDirectionInput.ColorDebug;
                Gizmos.DrawWireSphere((Vector2)transform.position + AimDirectionInput, 1);
            }
            Gizmos.color = Color.white;
        }

        bool CheckIsLookingRight()
        {
            //check if change direction
            if (IsLookingRight && AimDirectionInput.x < 0)
                return false;
            else if (IsLookingRight == false && AimDirectionInput.x > 0)
                return true;

            //else return previous direction
            return IsLookingRight;
        }

        #region public API

        /// <summary>
        /// Set aim in direction
        /// </summary>
        /// <param name="aimDirection"></param>
        public void AimInDirection(Vector2 aimDirection)
        {
            //set direction aim
            AimPositionNotNormalized = (Vector2)transform.position + aimDirection;
            AimDirectionInput = aimDirection.normalized;

            //set if change aim direction
            if (IsLookingRight != CheckIsLookingRight())
            {
                IsLookingRight = CheckIsLookingRight();

                //call event
                onChangeAimDirection?.Invoke(IsLookingRight);
            }
        }

        /// <summary>
        /// Set aim at position
        /// </summary>
        /// <param name="aimPosition"></param>
        public void AimAt(Vector2 aimPosition)
        {
            //set direction aim
            AimPositionNotNormalized = aimPosition;
            AimDirectionInput = (aimPosition - (Vector2)transform.position).normalized;

            //set if change aim direction
            if (IsLookingRight != CheckIsLookingRight())
            {
                IsLookingRight = CheckIsLookingRight();

                //call event
                onChangeAimDirection?.Invoke(IsLookingRight);
            }
        }

        #endregion
    }
}