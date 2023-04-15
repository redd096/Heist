using UnityEngine;
using redd096;

public class LevelManagerFeedbacks : FeedbackRedd096<LevelManager>
{
    [SerializeField] FeedbackStructRedd096 onWin = default;
    [SerializeField] FeedbackStructRedd096 onFinishTimer = default;

    protected override void AddEvents()
    {
        base.AddEvents();

        owner.onWin += OnWin;
        owner.onFinishTimer += OnFinishTimer;
    }

    protected override void RemoveEvents()
    {
        base.RemoveEvents();

        owner.onWin -= OnWin;
        owner.onFinishTimer -= OnFinishTimer;
    }

    void OnWin()
    {
        InstantiateFeedback(onWin);
    }

    void OnFinishTimer()
    {
        InstantiateFeedback(onFinishTimer);
    }
}
