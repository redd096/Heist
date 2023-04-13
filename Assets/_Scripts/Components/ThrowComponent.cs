using UnityEngine;

public class ThrowComponent : MonoBehaviour
{
    [Header("Throw")]
    [SerializeField] float pushForce = 10;
    [SerializeField] float pushHeight = 5;
    [Tooltip("Throw in movement direction instead of aim direction")][SerializeField] bool throwAtMovementDirection = true;

    [Header("Necessary Components (by default get from this gameObject)")]
    [SerializeField] DragComponent dragComponent = default;
    [SerializeField] MovementComponent movementComponent = default;
    [SerializeField] AimComponent aimComponent = default;

    [Header("Throw at precise axis degrees (0 is forward)")]
    [SerializeField] bool snapToAxis = true;
    [SerializeField] float snappedAngle = 90;

    public void Throw()
    {
        //start only if there are all necessary components
        if (CheckComponents() == false)
            return;

        if (dragComponent.Dragged)
        {
            DraggableObject draggedObject = dragComponent.Dragged;
            dragComponent.Drop();

            //movement or aim direction
            Vector3 directionInput = throwAtMovementDirection ? movementComponent.MoveDirectionInput : aimComponent.AimDirectionInput;
            if (directionInput == Vector3.zero) directionInput = transform.forward;             //if no aim or move direction, throw forward
            Vector3 direction = new Vector3(directionInput.x, 0, directionInput.z).normalized;  //be sure to ignore Y
            if (snapToAxis) direction = GetSnappedDirection(direction);                         //if necessary, snap direction
            draggedObject.GetComponent<Rigidbody>().AddForce(direction * pushForce + Vector3.up * pushHeight, ForceMode.VelocityChange);
        }
    }

    Vector3 GetSnappedDirection(Vector3 direction)
    {
        //calculate current angle
        float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        angle += 360;   //for negative values

        //find snapped angle
        float foundAngle = angle % snappedAngle;
        foundAngle = angle - foundAngle;

        //get axis with this angle
        Vector3 foundDirection = Quaternion.AngleAxis(foundAngle, Vector3.up) * transform.forward;
        //Debug.Log($"angle {angle} - direction {direction} - found angle {foundAngle} - found direction {foundDirection}");
        return foundDirection;
    }

    bool CheckComponents()
    {
        //check if have components
        if (dragComponent == null) dragComponent = GetComponent<DragComponent>();
        if (dragComponent == null)
        {
            Debug.LogWarning("Miss DragComponent on " + name);
            return false;
        }

        //check movement component
        if (throwAtMovementDirection)
        {
            if (movementComponent == null) movementComponent = GetComponent<MovementComponent>();
            if (movementComponent == null)
            {
                Debug.LogWarning("Miss MovementComponent on " + name);
                return false;
            }
        }
        //or aim component
        else
        {
            if (aimComponent == null) aimComponent = GetComponent<AimComponent>();
            if (aimComponent == null)
            {
                Debug.LogWarning("Miss AimComponent on " + name);
                return false;
            }
        }

        return true;
    }
}
