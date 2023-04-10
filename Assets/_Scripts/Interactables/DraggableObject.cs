using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    bool isPicked = false;
    Transform previousParent;

    public bool Pick(Transform character)
    {
        if (isPicked == false)
        {
            isPicked = true;
            previousParent = transform.parent;
            transform.parent = character;
            MoveToCharacter(character);
            return true;
        }

        return false;
    }

    public bool Drop()
    {
        if (isPicked)
        {
            isPicked = false;
            transform.parent = previousParent;
            return true;
        }

        return false;
    }

    void MoveToCharacter(Transform character)
    {
        RaycastHit characterCollisionPoint;
        RaycastHit draggableCollisionPoint;

        if (Physics.Raycast(character.position, character.forward, out draggableCollisionPoint))
        {
            if (Physics.Linecast(draggableCollisionPoint.point, character.position, out characterCollisionPoint))
            {
                Vector3 direction = (characterCollisionPoint.point - draggableCollisionPoint.point);
                transform.position += direction;
            }
        }
    }
}
