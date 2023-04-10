using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/Close on Deselect")]
    public class CloseOnDeselect : MonoBehaviour
    {
        [Header("Object to check")]
        [SerializeField] GameObject objectToClose = default;

        [Header("Check selection - enable prevent so if selection isn't checked object, but we clicked it, do not close")]
        [Tooltip("Close when currentSelectedGameObject isn't the checked object")][SerializeField] bool checkEventSystemSelection = true;
        [Tooltip("If checked object is an Image it isn't a Selectable, so if click it then selection will be anyway Null. Enable this to prevent to close")][SerializeField] bool preventSelectionCheckingClick = true;

        [Header("Check where click")]
        [Tooltip("Instead of check EventSystem selection, could check only where user click")][SerializeField] bool checkWhereClick = false;

        [Header("Objects")]
        [Tooltip("If click object setted above or its childs, keep it active?")][SerializeField] bool addObjectToTheList = true;
        [Tooltip("If click one of these objects or childs of them, keep object active")][SerializeField] GameObject[] objectsCanBeSelectedWithoutClose = default;

        //check where click
        //this is used, cause if click objectToClose, but its an image instead of button, textfield or other
        //the image isn't a Selectable, so EventSystem set selectedGameObject to null and close it (if setted in inspector to close when not selected)
        GameObject lastSelectedGameObject;
        Camera cam;
        Vector3 mousePosition;

        void Update()
        {
            //if it's active, check to close it
            if (objectToClose && objectToClose.activeInHierarchy)
            {
                //if some object in it is selected or cliked, don't close
                if (addObjectToTheList)
                {
                    if (CheckObject(objectToClose))
                        return;
                }

                //if some of these objects or their child is selected or cliked, don't close
                if (objectsCanBeSelectedWithoutClose != null)
                {
                    foreach (GameObject go in objectsCanBeSelectedWithoutClose)
                    {
                        if (CheckObject(go))
                            return;
                    }
                }

                //else close it
                objectToClose.SetActive(false);
            }
        }

        bool CheckObject(GameObject go)
        {
            if (checkEventSystemSelection)
            {
                //check only if changed selected gameObject
                if (EventSystem.current.currentSelectedGameObject == lastSelectedGameObject)
                    return true;

                lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;

                //check selection
                if (CheckOneTransformIsSelected(go.GetComponentsInChildren<Transform>()))
                    return true;

                if (preventSelectionCheckingClick)
                    if (CheckClickedInsideOneRectTransform(go.GetComponentsInChildren<RectTransform>()))
                        return true;
            }

            //check click
            if (checkWhereClick)
                if (CheckClickToKeepActive(go.GetComponentsInChildren<RectTransform>()))
                    return true;

            return false;
        }

        #region checks to close

        bool CheckOneTransformIsSelected(Transform[] transforms)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                //if event system has this selected
                if (lastSelectedGameObject == transforms[i].gameObject)
                    return true;
            }
            return false;
        }

        bool CheckClickedInsideOneRectTransform(RectTransform[] rectTransforms)
        {
#if ENABLE_INPUT_SYSTEM
            //only if clicked this frame
            if (Mouse.current.leftButton.wasPressedThisFrame == false)
                return false;

            if (cam == null) cam = Camera.main;
            mousePosition = Mouse.current.position.ReadValue();
#else
            //only if clicked this frame
            if (Input.GetMouseButtonDown(0) == false)
                return false;

            if (cam == null) cam = Camera.main;
            mousePosition = Input.mousePosition;
#endif

            for (int i = 0; i < rectTransforms.Length; i++)
            {
                //if mouse position is inside rect transform
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[i], mousePosition, cam))
                    return true;
            }
            return false;
        }

        bool CheckClickToKeepActive(RectTransform[] rectTransforms)
        {
#if ENABLE_INPUT_SYSTEM
            //if not click, keep active
            if (Mouse.current.leftButton.wasPressedThisFrame == false)
                return true;

            if (cam == null) cam = Camera.main;
            mousePosition = Mouse.current.position.ReadValue();
#else
            //if not click, keep active
            if (Input.GetMouseButtonDown(0) == false)
                return true;

            if (cam == null) cam = Camera.main;
            mousePosition = Input.mousePosition;
#endif

            for (int i = 0; i < rectTransforms.Length; i++)
            {
                //if mouse position is inside rect transform
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[i], mousePosition, cam))
                    return true;
            }
            return false;
        }

        #endregion
    }
}