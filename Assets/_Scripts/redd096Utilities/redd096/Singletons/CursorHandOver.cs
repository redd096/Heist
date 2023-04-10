using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using Input = redd096.InputRedd096;
#endif

//use null as defaultIcon and Vector2.zero as defaultOffset.

//For handIcon
//get cursor aero_link.cur from C:\Windows\Cursors
//convert to png https://convertio.co/it/cur-png/
//will receive a zip with png of different sizes. Find 32x32 (normally is the smaller) and use it.
//Set handOffset to new Vector2(7, 0).

//CursorMode use Auto. The alternative is ForceSoftware but is laggy

//in import, set texture to Cursor.
//enable alphaIsTransparency, Read/Write
//disable mip maps
//set format to RGBA 32

namespace redd096
{
    [AddComponentMenu("redd096/Singletons/Utility Singleton")]
    public class CursorHandOver : Singleton<CursorHandOver>
    {
        [Header("If null, use default cursor")]
        [SerializeField] Texture2D defaultIcon = default;
        [SerializeField] Vector2 defaultOffset = Vector2.zero;
        [SerializeField] CursorMode defaultMode = CursorMode.Auto;

        [Header("Original is aero_link in C:/Windows/Cursors")]
        [SerializeField] Texture2D handIcon = default;
        [SerializeField] Vector2 handOffset = new Vector2(7, 0);
        [SerializeField] CursorMode handMode = CursorMode.Auto;

        private bool isHand;

        private void Update()
        {
            //if pointer is over UI object
            if (EventSystem.current.IsPointerOverGameObject())
            {
                //check if hit a Selectable
                if (HasThisComponentFirstHit(typeof(Selectable), typeof(IPointerClickHandler), typeof(IPointerDownHandler)))
                {
                    if (isHand == false)
                    {
                        isHand = true;
                        Cursor.SetCursor(handIcon, handOffset, handMode);
                    }
                    return;
                }
            }

            //else use default cursor
            if (isHand)
            {
                isHand = false;
                Cursor.SetCursor(defaultIcon, defaultOffset, defaultMode);
            }
        }

        #region raycast UI

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

            return hits;
        }

        bool HasThisComponentFirstHit(params System.Type[] types)
        {
            List<RaycastResult> hits = GetHitsAtPosition();

            //if hit something, get first hit
            if (hits != null && hits.Count > 0)
            {
                //check every passed type
                foreach (System.Type type in types)
                {
                    if (hits[0].gameObject.GetComponentInParent(type))
                        return true;
                }
            }

            return false;
        }

        Vector2 GetScreenInputPosition()
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