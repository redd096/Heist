using System.Collections;
using UnityEngine;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Pickables/Pick Up BASE")]
    public abstract class PickUpBASE<T> : PickUpBASE where T : Component
    {
        enum EUpdateMode { Never, FixedUpdate, Coroutine }

        [Header("Check TriggerStay in addition to TriggerEnter")]
        [SerializeField] EUpdateMode triggerStayUpdateMode = EUpdateMode.Coroutine;
        [Tooltip("Delay between updates using Coroutine method")][SerializeField] float timeCoroutine = 0.2f;

        [Header("Destroy when instantiated - 0 = no destroy")]
        [SerializeField] float timeBeforeDestroy = 0;

        protected Character whoHit;
        protected T whoHitComponent;
        bool alreadyUsed;

        void OnEnable()
        {
            //reset vars
            whoHit = null;
            whoHitComponent = null;
            alreadyUsed = false;

            //if there is, start auto destruction timer
            if (timeBeforeDestroy > 0)
                StartCoroutine(AutoDestruction());

            //if update mode is Coroutine
            if (triggerStayUpdateMode == EUpdateMode.Coroutine)
                StartCoroutine(UpdateCoroutine());
        }

        void FixedUpdate()
        {
            if (triggerStayUpdateMode == EUpdateMode.FixedUpdate)
                CheckTriggerStay();
        }

        IEnumerator UpdateCoroutine()
        {
            while (triggerStayUpdateMode == EUpdateMode.Coroutine)
            {
                CheckTriggerStay();
                yield return new WaitForSeconds(timeCoroutine);
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (alreadyUsed)
                return;

            //if hitted by player
            Character ch = collision.transform.GetComponentInParent<Character>();
            if (ch && ch.CharacterType == Character.ECharacterType.Player)
            {
                whoHit = ch;
                whoHitComponent = whoHit.GetSavedComponent<T>();

                //pick up
                if (CanPickUp())
                {
                    PickUp();
                    OnPick();
                }
                //or fail pick up
                else
                {
                    OnFailPick();
                }
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            //remove who hit on trigger exit
            if (whoHit && collision.transform.GetComponentInParent<Character>() == whoHit)
            {
                whoHit = null;
                whoHitComponent = null;
            }
        }

        #region private API

        void CheckTriggerStay()
        {
            if (alreadyUsed)
                return;

            //if trigger enter and can't pick up, check again. Maybe player stay compenetrate and loose health, so now can pick up
            if (whoHit && CanPickUp())
            {
                PickUp();
                OnPick();
            }
        }

        IEnumerator AutoDestruction()
        {
            //wait, then destroy
            yield return new WaitForSeconds(timeBeforeDestroy);
            alreadyUsed = true;
            Destroy(gameObject);
        }

        #endregion

        #region protected API

        public abstract override void PickUp();

        protected abstract bool CanPickUp();

        protected virtual void OnPick()
        {
            //call event
            onPick?.Invoke(this);

            //destroy this gameObject
            alreadyUsed = true;
            Destroy(gameObject);
        }

        protected virtual void OnFailPick()
        {
            //call event
            onFailPick?.Invoke(this);
        }

        #endregion
    }

    public abstract class PickUpBASE : MonoBehaviour, IPickable
    {
        //events
        public System.Action<PickUpBASE> onPick { get; set; }
        public System.Action<PickUpBASE> onFailPick { get; set; }

        public abstract void PickUp();
    }
}