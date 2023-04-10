using System.Collections;
using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/Replace GameObject Mouse Or Gamepad")]
    public class ReplaceGameObjectMouseOrGamepad : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        [Header("Change object if use mouse or gamepad")]
        [SerializeField] string mouseSchemeName = "KeyboardAndMouse";
        [SerializeField] GameObject[] objectsMouse = default;
        [SerializeField] GameObject[] objectsGamepad = default;

        PlayerInput playerInput;
        bool usingMouse;

        void OnEnable()
        {
            //get references
            if (playerInput == null)
                playerInput = FindObjectOfType<PlayerInput>();

            //set gameObject
            ReplaceGameObjects();

            //start update coroutine
            StartCoroutine(UpdateCoroutine());
        }

        IEnumerator UpdateCoroutine()
        {
            yield return new WaitForSeconds(0.2f);

            //if change device, replace gameObjects
            if (playerInput && IsUsingMouse() != usingMouse)
            {
                ReplaceGameObjects();
            }
        }

        bool IsUsingMouse()
        {
            //return if current control scheme is mouse scheme
            return playerInput ? playerInput.currentControlScheme == mouseSchemeName : true;
        }

        void ReplaceGameObjects()
        {
            //set if using mouse or gamepad
            usingMouse = IsUsingMouse();

            //active or deactive objects
            foreach (GameObject go in objectsMouse)
                if (go)
                    go.SetActive(usingMouse);

            foreach (GameObject go in objectsGamepad)
                if (go)
                    go.SetActive(!usingMouse);
        }
#else
        [HelpBox("This works only with new unity input system", HelpBoxAttribute.EMessageType.Error)]
        public string Error = "It works only with new unity input system";
#endif
    }
}