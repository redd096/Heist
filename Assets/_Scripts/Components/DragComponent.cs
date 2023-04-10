using redd096.Attributes;
using redd096;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragComponent : MonoBehaviour
{
    enum EUpdateMode { Update, FixedUpdate, Coroutine }

    [Header("Find Draggables")]
    [Tooltip("Find Draggables on Update or FixedUpdate?")][SerializeField] EUpdateMode updateMode = EUpdateMode.Coroutine;
    [Tooltip("Delay between updates using Coroutine method")][EnableIf("updateMode", EUpdateMode.Coroutine)][SerializeField] float timeCoroutine = 0.1f;
    [Tooltip("Distance to check for draggable")][SerializeField] float distancePickDraggable = 1;
    [Tooltip("Snap object to character, but keep a little distance")][SerializeField] float distanceObjectWhenPicked = 0.1f;
    [Tooltip("Ignore draggables with this layer")][SerializeField] LayerMask layersToIgnore = default;

    [Header("DEBUG")]
    [SerializeField] ShowDebugRedd096 showAreaInteractable = Color.cyan;

    //events
    public System.Action<DraggableObject> onFoundDraggable { get; set; }
    public System.Action<DraggableObject> onLostDraggable { get; set; }

    public DraggableObject Dragged => dragged;
    public DraggableObject PossibleToPick => possibleToPickDraggable;
    public DraggableObject previousPossibleToPick => previousPossibleToPickDraggable;
    public float DistanceObjectWhenPicked => distanceObjectWhenPicked;

    //interactables
    DraggableObject dragged;
    DraggableObject possibleToPickDraggable;
    DraggableObject previousPossibleToPickDraggable;

    void OnDrawGizmos()
    {
        //draw area interactable
        if (showAreaInteractable)
        {
            Gizmos.color = showAreaInteractable.ColorDebug;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * distancePickDraggable);
            Gizmos.color = Color.white;
        }
    }

    void OnEnable()
    {
        //start coroutine
        if (updateMode == EUpdateMode.Coroutine)
            StartCoroutine(UpdateCoroutine());
    }

    void Update()
    {
        //do only if update mode is Update
        if (updateMode == EUpdateMode.Update)
            FindInteractables();
    }

    void FixedUpdate()
    {
        //do only if update mode is FixedUpdate
        if (updateMode == EUpdateMode.FixedUpdate)
            FindInteractables();
    }

    IEnumerator UpdateCoroutine()
    {
        //do only if update mode is Coroutine
        while (updateMode == EUpdateMode.Coroutine)
        {
            FindInteractables();
            yield return new WaitForSeconds(timeCoroutine);
        }
    }

    #region public API

    public void FindInteractables()
    {
        //find draggable in distance
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, distancePickDraggable, ~layersToIgnore);
        possibleToPickDraggable = hit.transform == null ? null : hit.transform.GetComponentInParent<DraggableObject>();

        if (previousPossibleToPickDraggable != possibleToPickDraggable)
        {
            //call events
            if (previousPossibleToPickDraggable != null)
                onLostDraggable?.Invoke(previousPossibleToPickDraggable);

            if (possibleToPickDraggable != null)
                onFoundDraggable?.Invoke(possibleToPickDraggable);

            //and save previous nearest interactable
            previousPossibleToPickDraggable = possibleToPickDraggable;
        }
    }

    public void Interact()
    {
        //try drop
        if (dragged)
        {
            if (dragged.Drop())
            {
                dragged = null;
            }
        }
        //or try pick
        else if (possibleToPickDraggable != null)
        {
            if (possibleToPickDraggable.Pick(this))
            {
                dragged = possibleToPickDraggable;

                onLostDraggable?.Invoke(previousPossibleToPickDraggable);
                onLostDraggable?.Invoke(possibleToPickDraggable);
                previousPossibleToPickDraggable = null;
                possibleToPickDraggable = null;
            }
        }
    }

    #endregion
}
