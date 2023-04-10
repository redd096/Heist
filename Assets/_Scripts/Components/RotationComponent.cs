using redd096;
using UnityEngine;
using redd096.Attributes;

public class RotationComponent : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] float rotationSpeed = 10;

    [Header("Necessary Components (by default get from this gameObject)")]
    [SerializeField] Rigidbody rb = default;

    [Header("DEBUG")]
    [ReadOnly] public Vector3 AimDirectionInput = Vector3.right;            //when aim, set it with only direction (used to know where this object is aiming)
    [ReadOnly] public Vector3 AimPositionNotNormalized = Vector3.right;     //when aim, set it without normalize (used to set crosshair on screen - to know mouse position or analog inclination)
    [SerializeField] ShowDebugRedd096 showPositionNotNormalized = Color.red;
    [SerializeField] ShowDebugRedd096 showDirectionInput = Color.cyan;

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
            Gizmos.DrawWireSphere(transform.position + AimDirectionInput, 1);
        }
        Gizmos.color = Color.white;
    }

    private void FixedUpdate()
    {
        //start only if there are all necessary components
        if (CheckComponents() == false)
            return;

        Rotate();
    }

    void Rotate()
    {
        //float angle = Vector3.SignedAngle(transform.forward, AimDirectionInput, Vector3.up);
        //rb.AddTorque(Vector3.up * Mathf.Clamp01(angle) * rotationSpeed);

        Quaternion lookRotation = Quaternion.LookRotation(AimDirectionInput, Vector3.up);
        rb.MoveRotation(Quaternion.Lerp(rb.rotation, lookRotation, Time.fixedDeltaTime * rotationSpeed));
        rb.MoveRotation(Quaternion.Euler(0, rb.rotation.eulerAngles.y, 0));
    }

    #region private API

    bool CheckComponents()
    {
        //check if have components
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        //if movement mode is rigidbody, be sure to have a rigidbody
        if (rb == null)
        {
            Debug.LogWarning("Miss Rigidbody on " + name);
            return false;
        }

        return true;
    }

    #endregion

    #region public API

    /// <summary>
    /// Set aim in direction
    /// </summary>
    /// <param name="aimDirection"></param>
    public void AimInDirection(Vector3 aimDirection)
    {
        //set direction aim
        AimPositionNotNormalized = transform.position + aimDirection;
        AimDirectionInput = aimDirection.normalized;
    }

    /// <summary>
    /// Set aim at position
    /// </summary>
    /// <param name="aimPosition"></param>
    public void AimAt(Vector3 aimPosition)
    {
        //set direction aim
        AimPositionNotNormalized = aimPosition;
        AimDirectionInput = (aimPosition - transform.position).normalized;
    }

    #endregion
}
