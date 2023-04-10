using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Exit Feedback")]
    public class ExitFeedbacks : FeedbackRedd096<ExitInteractable>
    {
        [Header("Animator - default get in children")]
        [SerializeField] Animator anim = default;
        [SerializeField] string boolParameter = "IsOpen";

        [Header("Command to show when open")]
        [SerializeField] GameObject objectToShow = default;

        protected override void OnEnable()
        {
            //get references
            if (anim == null) anim = GetComponentInChildren<Animator>();

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            owner.onOpen += OnSetState;
            owner.onClose += OnSetState;

            //set default value
            OnSetState();
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            owner.onOpen -= OnSetState;
            owner.onClose -= OnSetState;
        }

        void OnSetState()
        {
            //move to open or close animation
            if (anim)
            {
                anim.SetBool(boolParameter, owner.IsOpen);
            }

            //show object when open and hide when close
            if (objectToShow)
                objectToShow.SetActive(owner.IsOpen);
        }
    }
}