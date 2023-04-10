using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using Input = redd096.InputRedd096;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

namespace redd096
{
    [AddComponentMenu("redd096/Singletons/UI Inputs Utility")]
    public class UIInputsUtility : Singleton<UIInputsUtility>
    {
        [Header("Delay when release input")]
        [SerializeField] bool useDelayOnRelease = false;
        [Tooltip("Delay to not calculate mouse released immediatly")][SerializeField] float delayReleaseInput = 0.2f;

        Coroutine delayReleaseCoroutine;
        bool isInputUp;

        void Update()
        {
            //when press input down, stop coroutine if running
            if (GetInputDown())
            {
                if (delayReleaseCoroutine != null) StopCoroutine(delayReleaseCoroutine);
                isInputUp = false;
            }

            //when press input up, start delay
            if (GetInputUpWithoutDelay())
            {
                delayReleaseCoroutine = StartCoroutine(DelayReleaseCoroutine());
            }
        }

        IEnumerator DelayReleaseCoroutine()
        {
            //calculate input up after delay
            yield return new WaitForSeconds(delayReleaseInput);
            isInputUp = true;

            //reset after one frame
            yield return null;
            isInputUp = false;
        }

        #region raycast

        List<RaycastResult> GetHitsAtPosition()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
                return new List<RaycastResult>();

            //set pointer event at input position
            PointerEventData pointerEvent = new PointerEventData(eventSystem);
            pointerEvent.position = GetScreenInputPosition();

            //then raycast and get hits
            List<RaycastResult> hits = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerEvent, hits);
            //graphicRaycaster.Raycast(pointerEvent, hits);

            return hits;
        }

        public T GetElementAtPosition<T>() where T : Component
        {
            //find element inside hits
            T element;
            foreach (RaycastResult hit in GetHitsAtPosition())
            {
                element = hit.gameObject.GetComponentInParent<T>();
                if (element != null)
                    return element;
            }

            return null;
        }

        public bool HasHitElementAtPosition<T>(T element) where T : Component
        {
            //check in every hit, if one is the element
            foreach (RaycastResult hit in GetHitsAtPosition())
            {
                if (hit.gameObject.GetComponentInParent<T>() == element)
                    return true;
            }

            return false;
        }

        public bool HasThisComponentAtPosition<T>() where T : Component
        {
            //check in every hit, if one has this component
            foreach (RaycastResult hit in GetHitsAtPosition())
            {
                if (hit.gameObject.GetComponentInParent<T>() != null)
                    return true;
            }

            return false;
        }

        public GameObject GetFirstHit()
        {
            List<RaycastResult> hits = GetHitsAtPosition();

            //if hit something, get first hit
            if (hits != null && hits.Count > 0)
                return hits[0].gameObject;

            return null;
        }

        public T GetElementFirstHit<T>() where T : Component
        {
            List<RaycastResult> hits = GetHitsAtPosition();

            //if hit something, get first hit
            if (hits != null && hits.Count > 0)
                return hits[0].gameObject.GetComponent<T>();

            return null;
        }

        public bool HasThisComponentFirstHit<T>() where T : Component
        {
            List<RaycastResult> hits = GetHitsAtPosition();

            //if hit something, get first hit
            if (hits != null && hits.Count > 0)
            {
                return hits[0].gameObject.GetComponentInParent<T>() != null;
            }

            return false;
        }

        #endregion

        #region inputs API

        public bool GetInputDown()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        public Vector2 GetWorldInputPosition(Camera cam)
        {
            return cam.ScreenToWorldPoint(GetScreenInputPosition());
        }

        public bool GetInputUp()
        {
            //return with delay, or just InputUp
            return useDelayOnRelease ? isInputUp : GetInputUpWithoutDelay();
        }

        bool GetInputUpWithoutDelay()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Input.touchCount > 0 && ( Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled );
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

        public Vector2 GetScreenInputPosition()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Input.GetTouch(0).position;
#else
            return Input.mousePosition;
#endif
        }

        #endregion
    }
}