using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/UI Manager")]
    public class UIManager : MonoBehaviour
    {
        [Header("Debug Mode")]
        public bool ShowDebugLogs = false;

        [Header("Menu")]
        [Min(0)][SerializeField] float delayInputWhenOpenMenu = 0.3f;
        [SerializeField] GameObject pauseMenu = default;
        [SerializeField] GameObject endMenu = default;

        //delay input when open menu
        EventSystem eventSystem;
        Coroutine delayInputCoroutine;

        protected virtual void Start()
        {
            //by default, deactive menus
            PauseMenu(false);
            EndMenu(false);
        }

        #region open menu with input delay

        public void OpenMenu(GameObject menu, bool active)
        {
            if (menu == null)
            {
                if (ShowDebugLogs) Debug.Log("Missing menu: " + menu);
                return;
            }

            //when active menu, deactive event system for a little time
            if (active && delayInputWhenOpenMenu > Mathf.Epsilon)
            {
                //remember event system
                if (EventSystem.current)
                {
                    eventSystem = EventSystem.current;
                    eventSystem.enabled = false;
                }

                //restart coroutine
                if (delayInputCoroutine != null) StopCoroutine(delayInputCoroutine);
                delayInputCoroutine = StartCoroutine(DelayInputCoroutine());
            }

            //active or deactive menu
            menu.SetActive(active);
        }

        IEnumerator DelayInputCoroutine()
        {
            //wait (real time, so if Time.timeScale is 0 it works anyway)
            float timeDelay = Time.realtimeSinceStartup + delayInputWhenOpenMenu;
            while (Time.realtimeSinceStartup < timeDelay)
                yield return null;

            //then re-enable event system
            if (eventSystem)
            {
                eventSystem.enabled = true;

                //try set singleton if not setted, because unity remove it when disabled
                if (EventSystem.current == null)
                    EventSystem.current = eventSystem;
            }
        }

        #endregion

        #region menu

        public void PauseMenu(bool active)
        {
            //active or deactive pause menu
            OpenMenu(pauseMenu, active);
        }

        public void EndMenu(bool active)
        {
            if (endMenu == null)
            {
                if (ShowDebugLogs) Debug.Log("Missing end menu: " + endMenu);
                return;
            }

            //be sure to remove pause menu when active end menu
            if (active)
                PauseMenu(false);

            //active or deactive end menu
            OpenMenu(endMenu, active);
        }

        #endregion
    }
}