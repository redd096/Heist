using UnityEngine;
using redd096;

public class DraggableObjectFeedbacks : FeedbackRedd096<DraggableObject>
{
    [SerializeField] FeedbackStructRedd096 onPick = default;
    [SerializeField] FeedbackStructRedd096 onDrop = default;
    [SerializeField] FeedbackStructRedd096 onThrow = default;

    protected override void AddEvents()
    {
        base.AddEvents();

        owner.onPick += OnPick;
        owner.onDrop += OnDrop;
        owner.onThrow += OnThrow;
    }

    protected override void RemoveEvents()
    {
        base.RemoveEvents();

        owner.onPick -= OnPick;
        owner.onDrop -= OnDrop;
        owner.onThrow -= OnThrow;
    }

    private void OnPick()
    {
        InstantiateFeedback(onPick);
    }

    private void OnDrop()
    {
        InstantiateFeedback(onDrop);
    }

    private void OnThrow()
    {
        InstantiateFeedback(onThrow);
    }
}
