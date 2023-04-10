using UnityEngine;

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/FPS Counter")]
    public class FPSCounter : MonoBehaviour
    {
        [Header("Show on Screen? Only in editor, in dev builds, or also normal builds?")]
        [SerializeField] bool showInEditor = true;
        [SerializeField] bool showInBuildDevelopment = true;
        [SerializeField] bool showInBuild = false;

        [Header("Use GUI or UI Text")]
        [SerializeField] bool useOnGUI = false;
        [SerializeField] UnityEngine.UI.Text uiText = default;

        [Header("DEBUG - Set FPS Limit")]
        [SerializeField] bool setFpsLimit = false;
        [SerializeField][Min(-1)] int limitFps = -1;

        private static FPSCounter instance;
        private int countFPS;

        private int lastFrameIndex;
        private float[] frameDeltaTimeArray;

        public int CurrentFPS => countFPS;

        private void Awake()
        {
            //check singleton
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            //create array
            frameDeltaTimeArray = new float[50];

            OnValidateFunc();

            //show warning if necessary
            if (CanShow() && useOnGUI == false && uiText == null)
                Debug.LogWarning($"Missing UIText on {this}", gameObject);
        }

        private void OnValidate()
        {
            OnValidateFunc();
        }

        private void Update()
        {
            //add current delta time to array
            frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
            lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length; //always +1, when reach last index restart from 0

            //calculate FPS
            countFPS = Mathf.RoundToInt(CalculateFPS());

            //if not use OnGUI, set UI Text
            if (useOnGUI == false && uiText)
                uiText.text = "FPS: " + countFPS;
        }

        private void OnGUI()
        {
            //if use OnGUI, set it
            if (useOnGUI && CanShow())
                GUI.Label(new Rect(30, 40, 100, 25), "FPS: " + countFPS);
        }

        #region private API

        private void OnValidateFunc()
        {
            //when change in inspector, be sure to update also canvas
            if (CanShow() == false || useOnGUI)
            {
                if (uiText != null)
                    uiText.gameObject.SetActive(false);
            }
            else if (CanShow() && useOnGUI == false)
            {
                if (uiText != null)
                    uiText.gameObject.SetActive(true);
            }

            //set fps limit if necessary
            if (setFpsLimit)
                Application.targetFrameRate = limitFps;
        }

        private bool CanShow()
        {
            //check if it's in editor
            if (Application.isEditor)
                return showInEditor;

            //or is dev build
            if (Debug.isDebugBuild)
                return showInBuildDevelopment;

            //or a normal build
            return showInBuild;
        }

        private float CalculateFPS()
        {
            //calculate average between every saved delta time
            float total = 0f;
            foreach (float deltaTime in frameDeltaTimeArray)
            {
                total += deltaTime;
            }

            return frameDeltaTimeArray.Length / total;
        }

#endregion
    }
}