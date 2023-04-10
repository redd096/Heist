using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using redd096.Attributes;

//create a gameObject and attach this script.
//Then create two childs:
//1. attach SpriteMask to the first (will be the showed sprite)
//2. attach SpriteRenderer to the second. Set a sprite, black color, and mask interaction to "Visible Outside Mask"
//NB set scale of SpriteRenderer to the size of the screen. You can disable sprite renderer to not have black screen in editor (component, not object)

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/Scene Changer Animation")]
    public class SceneChangerAnimation : MonoBehaviour
    {
        [Header("Get Random Sprite - default refs get from children")]
        [SerializeField] SpriteMask mask = default;
        [SerializeField] SpriteRenderer blackScreen = default;
        [SerializeField] Sprite[] sprites = default;

        [Header("Animation")]
        [SerializeField] bool fadeInOnStart = true;
        [EnableIf("fadeInOnStart")][SerializeField] float waitBeforeFadeOnStart = 0.3f;
        [SerializeField] float animDuration = 2f;
        [Tooltip("Use realtime or deltaTime?")][SerializeField] bool useRealtimeInsteadOfDeltatime = true;

        [Header("Fade Size")]
        [SerializeField] bool useBlackScreenSize = true;
        [EnableIf("useBlackScreenSize")][SerializeField] float offsetBlackScreen = 20;
        [DisableIf("useBlackScreenSize")][SerializeField] float fixedSize = 2000;

        //screen size (width or height + offset), else use fixed size
        Vector2 sizeFadeOut => Vector2.zero;
        Vector2 sizeFadeIn => Vector2.one * (useBlackScreenSize ? (blackScreen ? Mathf.Max(blackScreen.transform.localScale.x, blackScreen.transform.localScale.y) : 0) + offsetBlackScreen : fixedSize);

        EventSystem eventSystem;
        Coroutine fadeCoroutine;

        //events
        public System.Action onFadeInComplete { get; set; }

        void Start()
        {
            //get references
            if (mask == null) mask = GetComponentInChildren<SpriteMask>();
            if (blackScreen == null) blackScreen = GetComponentInChildren<SpriteRenderer>();

            //fade in when enter in scene
            if (fadeInOnStart)
            {
                //be sure objects are active
                if (mask) mask.enabled = true;
                if (blackScreen) blackScreen.enabled = true;

                //fade in after few seconds
                Invoke("FadeIn", waitBeforeFadeOnStart);
            }
        }

        #region public API

        /// <summary>
        /// Do fade in
        /// </summary>
        public void FadeIn()
        {
            StartFade(true, OnFadeInComplete);
        }

        /// <summary>
        /// Fade out, then move to next scene in build settings
        /// </summary>
        public void FadeOutNextScene()
        {
            StartFade(false, () => SceneLoader.instance.LoadNextScene());
        }

        /// <summary>
        /// Fade out, then load passed scene
        /// </summary>
        /// <param name="scene"></param>
        public void FadeOutLoadScene(string scene)
        {
            StartFade(false, () => SceneLoader.instance.LoadScene(scene));
        }

        /// <summary>
        /// Fade out, then reload this scene
        /// </summary>
        public void FadeOutReloadScene()
        {
            StartFade(false, () => SceneLoader.instance.ReloadScene());
        }

        /// <summary>
        /// Fade out, then exit from game
        /// </summary>
        public void ExitGame()
        {
            StartFade(false, () => SceneLoader.instance.ExitGame());
        }

        #endregion

        #region private API

        void StartFade(bool fadeIn, System.Action func)
        {
            //disable event system until finish fade
            eventSystem = EventSystem.current;
            if (eventSystem)
                eventSystem.enabled = false;

            //select random sprite
            if (mask && sprites != null && sprites.Length > 0)
                mask.sprite = sprites[Random.Range(0, sprites.Length)];

            //be sure objects are active
            if (mask) mask.enabled = true;
            if (blackScreen) blackScreen.enabled = true;

            //start fade coroutine
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeCoroutine(fadeIn, func));
        }

        void OnFadeInComplete()
        {
            //re-enable event system
            if (eventSystem)
            {
                eventSystem.enabled = true;

                //set singleton if not setted, because unity remove it when disabled
                if (EventSystem.current == null)
                    EventSystem.current = eventSystem;

            }

            //deactivate images, to be sure the screen is clear
            if (mask) mask.enabled = false;
            if (blackScreen) blackScreen.enabled = false;

            //call event
            onFadeInComplete?.Invoke();
        }

        IEnumerator FadeCoroutine(bool fadeIn, System.Action func)
        {
            //set default vars
            Vector2 startScale = mask ? (Vector2)mask.transform.localScale : Vector2.one;

            float delta = 0;
            while (delta < 1)
            {
                yield return null;

                //fade (with realtime or delta)
                delta += (useRealtimeInsteadOfDeltatime ? Time.unscaledDeltaTime : Time.deltaTime) / animDuration;
                if (mask) mask.transform.localScale = Vector2.Lerp(startScale, fadeIn ? sizeFadeIn : sizeFadeOut, delta);
            }

            //call function on complete
            func?.Invoke();
        }

        #endregion
    }
}