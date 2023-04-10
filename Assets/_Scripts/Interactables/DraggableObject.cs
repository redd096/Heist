using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    bool isPicked = false;
    Transform previousParent;

    public bool Pick(DragComponent character)
    {
        if (isPicked == false)
        {
            isPicked = true;
            previousParent = transform.parent;
            transform.parent = character.transform;
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

    void MoveToCharacter(DragComponent character)
    {
        RaycastHit characterCollisionPoint;
        RaycastHit draggableCollisionPoint;

        if (Physics.Raycast(character.transform.position, character.transform.forward, out draggableCollisionPoint))
        {
            if (Physics.Linecast(draggableCollisionPoint.point, character.transform.position, out characterCollisionPoint))
            {
                Vector3 direction = characterCollisionPoint.point - draggableCollisionPoint.point;
                transform.position += direction - (direction.normalized * character.DistanceObjectWhenPicked);
            }
        }
    }
}
