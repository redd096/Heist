using System.Collections.Generic;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public List<DraggableObject> ObjectsInside => currentObjectsInside;

    List<DraggableObject> currentObjectsInside = new List<DraggableObject>();

    public System.Action<DraggableObject> onEnterDraggable;
    public System.Action<DraggableObject> onExitDraggable;

    private void OnTriggerEnter(Collider other)
    {
        //add to list
        DraggableObject draggable = other.GetComponentInParent<DraggableObject>();
        if (draggable && currentObjectsInside.Contains(draggable) == false)
        {
            currentObjectsInside.Add(draggable);

            onEnterDraggable?.Invoke(draggable);

            //check win
            GameManager.levelManager.CheckWin();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //remove from list
        DraggableObject draggable = other.GetComponentInParent<DraggableObject>();
        if (draggable && currentObjectsInside.Contains(draggable))
        {
            currentObjectsInside.Remove(draggable);

            onExitDraggable?.Invoke(draggable);
        }
    }
}
