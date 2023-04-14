using redd096.Attributes;
using redd096;
using System.Collections;
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
    [SerializeField] float radiusRaycast = 0.2f;

    [Header("Necessary Components (by default get from this gameObject)")]
    [SerializeField] AimComponent aimComponent = default;

    [Header("DEBUG")]
    [SerializeField] ShowDebugRedd096 showAreaInteractable = Color.cyan;

    //events
    public System.Action<DraggableObject> onFoundDraggable { get; set; }
    public System.Action<DraggableObject> onLostDraggable { get; set; }
    public System.Action onPick { get; set; }
    public System.Action onDrop { get; set; }

    public DraggableObject Dragged => dragged;
    public DraggableObject PossibleToPick => possibleToPickDraggable;
    public RaycastHit PossibleToPickRaycastHit => possibleToPickHit;
    public DraggableObject previousPossibleToPick => previousPossibleToPickDraggable;
    public float DistanceObjectWhenPicked => distanceObjectWhenPicked;

    //interactables
    DraggableObject dragged;
    DraggableObject possibleToPickDraggable;
    RaycastHit possibleToPickHit;
    DraggableObject previousPossibleToPickDraggable;

    void OnDrawGizmos()
    {
        //draw area interactable
        if (showAreaInteractable)
        {
            Gizmos.color = showAreaInteractable.ColorDebug;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * distancePickDraggable);
            Gizmos.DrawWireSphere(transform.position + transform.forward * distancePickDraggable, radiusRaycast);
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

    #region private API

    bool CheckComponents()
    {
        //check if have components
        if (aimComponent == null)
            aimComponent = GetComponentInParent<AimComponent>();

        //if movement mode is rigidbody, be sure to have a rigidbody
        if (aimComponent == null)
        {
            Debug.LogWarning($"Miss AimComponent on {name}");
            return false;
        }

        return true;
    }

    void SnapDraggableToCharacter()
    {
        RaycastHit characterCollisionPoint;

        if (Physics.Linecast(possibleToPickHit.point, transform.position, out characterCollisionPoint, ~layersToIgnore, QueryTriggerInteraction.Ignore))
        {
            Vector3 direction = characterCollisionPoint.point - possibleToPickHit.point;
            dragged.transform.position += direction - (direction.normalized * distanceObjectWhenPicked);
        }
    }

    #endregion

    #region public API

    public void FindInteractables()
    {
        //start only if there are all necessary components
        if (CheckComponents() == false)
            return;

        //find draggable in distance
        Physics.SphereCast(transform.position, radiusRaycast, aimComponent.AimDirectionInput, out possibleToPickHit, distancePickDraggable, ~layersToIgnore, QueryTriggerInteraction.Ignore);
        possibleToPickDraggable = possibleToPickHit.transform == null ? null : possibleToPickHit.transform.GetComponentInParent<DraggableObject>();

        if (previousPossibleToPickDraggable != possibleToPickDraggable)
        {
            //call events
            if (previousPossibleToPickDraggable != null)
            {
                previousPossibleToPickDraggable.Highlight(false);
                onLostDraggable?.Invoke(previousPossibleToPickDraggable);
            }

            if (possibleToPickDraggable != null)
            {
                possibleToPickDraggable.Highlight(true, GetComponent<PlayerPawn>().CurrentController.GetComponent<User>());
                onFoundDraggable?.Invoke(possibleToPickDraggable);
            }

            //and save previous nearest interactable
            previousPossibleToPickDraggable = possibleToPickDraggable;
        }
    }

    public void Interact()
    {
        //try drop
        if (Drop())
            return;

        //or try pick
        else if (possibleToPickDraggable != null)
        {
            if (possibleToPickDraggable.Pick(this))
            {
                possibleToPickDraggable.Highlight(false);
                dragged = possibleToPickDraggable;
                SnapDraggableToCharacter();

                onLostDraggable?.Invoke(previousPossibleToPickDraggable);
                onLostDraggable?.Invoke(possibleToPickDraggable);
                previousPossibleToPickDraggable = null;
                possibleToPickDraggable = null;

                onPick?.Invoke();
            }
        }
    }

    public bool Drop()
    {
        if (dragged)
        {
            if (dragged.Drop())
            {
                dragged = null;

                onDrop?.Invoke();
                return true;
            }
        }

        return false;
    }

    #endregion
}
