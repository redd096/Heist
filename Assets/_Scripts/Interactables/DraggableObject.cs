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
}
