using UnityEngine;

public class ThrowComponent : MonoBehaviour
{
    [Header("Throw")]
    [SerializeField] float pushForce = 10;
    [SerializeField] float pushHeight = 5;

    [Header("Necessary Components (by default get from this gameObject)")]
    [SerializeField] DragComponent dragComponent = default;
    [SerializeField] AimComponent aimComponent = default;

    public void Throw()
    {
        //start only if there are all necessary components
        if (CheckComponents() == false)
            return;

        if (dragComponent.Dragged)
        {
            DraggableObject draggedObject = dragComponent.Dragged;
            dragComponent.Drop();

            Vector3 direction = new Vector3(aimComponent.AimDirectionInput.x, 0, aimComponent.AimDirectionInput.z).normalized;
            draggedObject.GetComponent<Rigidbody>().AddForce(direction * pushForce + Vector3.up * pushHeight, ForceMode.VelocityChange);
        }
    }
    bool CheckComponents()
    {
        //check if have components
        if (dragComponent == null)
            dragComponent = GetComponent<DragComponent>();

        if (dragComponent == null)
        {
            Debug.LogWarning("Miss DragComponent on " + name);
            return false;
        }

        if (aimComponent == null)
            aimComponent = GetComponent<AimComponent>();

        if (aimComponent == null)
        {
            Debug.LogWarning("Miss AimComponent on " + name);
            return false;
        }

        return true;
    }
}
