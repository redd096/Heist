using UnityEngine;
using redd096;

public class TriggerZoneFeedbacks : FeedbackRedd096<TriggerZone>
{
    [Header("Instantiate feedback on relative draggable, or at center of trigger zone?")]
    [SerializeField] bool instantiateOnDraggable = false;

    [Header("Feedbacks")]
    [SerializeField] FeedbackStructRedd096 onEnterDraggable = default;
    [SerializeField] FeedbackStructRedd096 onExitDraggable = default;

    protected override void AddEvents()
    {
        base.AddEvents();

        owner.onEnterDraggable += OnEnterDraggable;
        owner.onExitDraggable += OnExitDraggable;
    }

    protected override void RemoveEvents()
    {
        base.RemoveEvents();

        owner.onEnterDraggable -= OnEnterDraggable;
        owner.onExitDraggable -= OnExitDraggable;
    }

    void OnEnterDraggable(DraggableObject draggable)
    {
        InstantiateFeedback(onEnterDraggable, instantiateOnDraggable ? draggable.transform : transform);
    }

    void OnExitDraggable(DraggableObject draggable)
    {
        InstantiateFeedback(onExitDraggable, instantiateOnDraggable ? draggable.transform : transform);
    }
}
