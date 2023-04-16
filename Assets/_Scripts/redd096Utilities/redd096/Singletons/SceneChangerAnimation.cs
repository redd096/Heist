using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using redd096.Attributes;
using UnityEngine.UI;

//Create a material with unity shader UI/Default and set:
//- Stencil Comparison      6
//- Stencil ID              1
//- Stencil Operation       0
//- Stencil Write Mask      0
//- Stencil Read Mask       1
//- Color Mask              15
//- Use Alpha Clip          false

//create a canvas and attach this script.
//Set SortOrder to a big number to be sure is on top of everything

//Create an Image inside the canvas (this will be the showed sprite)
//- Set the sprite we want to show
//- Attach Mask
//- Set Show Mask Graphic to false

//Create another Image, child of the first one
//- Set Color to black
//- Set Maskable to false
//- Set as Material the one created before

//NB you can disable the images object to not shown in editor scene.
//Not the canvas if the script is on it, or it will not call Start.

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/Scene Changer Animation")]
    public class SceneChangerAnimation : Singleton<SceneChangerAnimation>
    {
        [Header("Necessary components - default get from children")]
        [SerializeField] Canvas canvas = default;
        [SerializeField] Image mask = default;
        [SerializeField] Image blackScreen = default;

        [Header("Get random sprite - if empty, keep the one on the mask image")]
        [SerializeField] Sprite[] randomSprites = default;

        [Header("Fade In on Awake")]
        [SerializeField] bool fadeInOnAwake = true;
        [EnableIf("fadeInOnAwake")][SerializeField] float waitBeforeFadeOnAwake = 0.3f;

        [Header("Animation")]
        [SerializeField] float animDuration = 1f;
        [Tooltip("Use realtime or deltaTime?")][SerializeField] bool useRealtimeInsteadOfDeltatime = true;

        [Header("Fade use ScreenSize, need offset?")]
        [SerializeField] Vector2 maskOffset = Vector2.zero;
        [SerializeField] Vector2 blackScreenOffset = Vector2.zero;

        private EventSystem _eventSystem;

        Vector2 maskSizeFade => canvasRect.sizeDelta + maskOffset;
        Vector2 blackScreenSizeFade => canvasRect.sizeDelta + blackScreenOffset;

        EventSystem eventSystem;
        Coroutine fadeCoroutine;
        RectTransform canvasRect;
        RectTransform maskRect;
        RectTransform blackScreenRect;

        //events
        public System.Action onFadeInComplete { get; set; }

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();

            //get references
            if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
            if (mask == null) mask = GetComponentInChildren<Image>(true);
            if (blackScreen == null) blackScreen = mask.transform.GetChild(0).GetComponentInChildren<Image>(true);

            if (canvas == null && mask == null || blackScreen == null)
            {
                Debug.LogWarning($"Missing references on SceneChangerAnimation", gameObject);
                return;
            }

            //get rect transform references
            if (canvasRect == null) canvasRect = canvas.GetComponent<RectTransform>();
            if (maskRect == null) maskRect = mask.GetComponent<RectTransform>();
            if (blackScreenRect == null) blackScreenRect = blackScreen.GetComponent<RectTransform>();
        }

        protected override void SetDefaults()
        {
            base.SetDefaults();

            //fade in when enter in scene
            if (fadeInOnAwake)
            {
                //set size
                maskRect.sizeDelta = Vector2.zero;
                blackScreenRect.sizeDelta = blackScreenSizeFade;

                //be sure objects are active
                mask.gameObject.SetActive(true);
                blackScreen.gameObject.SetActive(true);
                canvas.gameObject.SetActive(true);

                //fade in after few seconds
                Invoke(nameof(StartFadeIn), waitBeforeFadeOnAwake);
            }
            //else, deactivate images, to be sure the screen is clear
            else
            {
                canvas.gameObject.SetActive(false);
            }
        }

        void StartFadeIn()
        {
            FadeIn();
        }

        #region public API

        /// <summary>
        /// Do fade in
        /// </summary>
        public static void FadeIn()
        {
            instance.StartFade(true, instance.OnFadeInComplete);
        }

        /// <summary>
        /// Do fade out
        /// </summary>
        public static void FadeOut()
        {
            instance.StartFade(false, null);
        }

        /// <summary>
        /// Fade out, then move to next scene in build settings
        /// </summary>
        public static void FadeOutNextScene()
        {
            instance.StartFade(false, () => SceneLoader.instance.LoadNextScene());
        }

        /// <summary>
        /// Fade out, then load passed scene
        /// </summary>
        /// <param name="scene"></param>
        public static void FadeOutLoadScene(string scene)
        {
            instance.StartFade(false, () => SceneLoader.instance.LoadScene(scene));
        }

        /// <summary>
        /// Fade out, then reload this scene
        /// </summary>
        public static void FadeOutReloadScene()
        {
            instance.StartFade(false, () => SceneLoader.instance.ReloadScene());
        }

        /// <summary>
        /// Fade out, then exit from game
        /// </summary>
        public static void FadeOutExitGame()
        {
            instance.StartFade(false, () => SceneLoader.instance.ExitGame());
        }

        #endregion

        #region private API

        void StartFade(bool fadeIn, System.Action func)
        {
            if (canvas == null || mask == null || blackScreen == null)
            {
                Debug.LogWarning($"Missing references on SceneChangerAnimation", gameObject);
                return;
            }

            //disable event system until finish fade
            eventSystem = EventSystem.current;
            if (eventSystem == null)
                eventSystem = _eventSystem;

            if (eventSystem)
            {
                _eventSystem = eventSystem;
                eventSystem.enabled = false;
            }

            //select random sprite
            if (randomSprites != null && randomSprites.Length > 0)
                mask.sprite = randomSprites[Random.Range(0, randomSprites.Length)];

            //set size
            maskRect.sizeDelta = fadeIn ? Vector2.zero : maskSizeFade;
            blackScreenRect.sizeDelta = blackScreenSizeFade;

            //be sure objects are active
            mask.gameObject.SetActive(true);
            blackScreen.gameObject.SetActive(true);
            canvas.gameObject.SetActive(true);

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
            canvas.gameObject.SetActive(false);

            //call event
            onFadeInComplete?.Invoke();
        }

        IEnumerator FadeCoroutine(bool fadeIn, System.Action func)
        {
            //set default vars
            Vector2 startScale = maskRect.sizeDelta;

            float delta = 0;
            while (delta < 1)
            {
                yield return null;

                //fade (with realtime or delta)
                delta += (useRealtimeInsteadOfDeltatime ? Time.unscaledDeltaTime : Time.deltaTime) / animDuration;
                if (mask) maskRect.sizeDelta = Vector2.Lerp(startScale, fadeIn ? maskSizeFade : Vector2.zero, delta);
            }

            //call function on complete
            func?.Invoke();
        }

        #endregion
    }
}