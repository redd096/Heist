using UnityEngine;
using redd096;

public class WannabeGateFeedbacks : FeedbackRedd096<WannabeGate>
{
    [SerializeField] FeedbackStructRedd096 onOpenGate = default;
    [SerializeField] FeedbackStructRedd096 onCloseGate = default;

    protected override void AddEvents()
    {
        base.AddEvents();

        owner.onOpenGate += OnOpenGate;
        owner.onCloseGate += OnCloseGate;
    }

    protected override void RemoveEvents()
    {
        base.RemoveEvents();

        owner.onOpenGate -= OnOpenGate;
        owner.onCloseGate -= OnCloseGate;
    }

    void OnOpenGate(GameObject gate)
    {
        InstantiateFeedback(onOpenGate, gate.transform);
    }

    void OnCloseGate(GameObject gate)
    {
        InstantiateFeedback(onCloseGate, gate.transform);
    }
}
