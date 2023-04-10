using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/Dynamic Resolution URP")]
    public class DynamicResolutionURP : MonoBehaviour
    {
        [HelpBox("In Player Settings, in Scripting Define Symbols, add UNITY_URP to every platform")]
        [Header("Resolution Scale")]
        [SerializeField] float maxResolutionScale = 1.0f;
        [SerializeField] float minResolutionScale = 0.85f;

        [Header("Step to reduce or increase")]
        [SerializeField] float scaleStep = 0.1f;

        [Header("FPS")]
        [SerializeField] FPSCounter fpsCounter = default;
        [SerializeField] int minimumFPS = 30;
        [SerializeField] int maximumFPS = 45;

        [Header("Show on Screen? Only in editor, in dev builds, or also normal builds?")]
        [SerializeField] bool showInEditor = true;
        [SerializeField] bool showInBuildDevelopment = true;
        [SerializeField] bool showInBuild = false;

        [Header("Use GUI or UI Text")]
        [SerializeField] bool useOnGUI = false;
        [SerializeField] UnityEngine.UI.Text uiText = default;

        [Header("DEBUG - J and K to reduce or increase")]
        [SerializeField] bool useButtonsToReduceAndIncrease = false;

        private static DynamicResolutionURP instance;

        //current scale
        float currentScale = 1.0f;
#if UNITY_URP
        UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urp;
        float renderScale { get => urp.renderScale; set { urp.renderScale = value; } }
#else
        float renderScale = 1.0f;
#endif

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

            //get references
#if UNITY_URP
            urp = (UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
#endif

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
            //DEBUG - press J or K to reduce or increase resolution
            if (useButtonsToReduceAndIncrease)
            {
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current[Key.J].wasPressedThisFrame)
                    currentScale -= scaleStep;
                if (Keyboard.current[Key.K].wasPressedThisFrame)
                    currentScale += scaleStep;
#else
                if (Input.GetKeyDown(KeyCode.J))
                    currentScale -= scaleStep;
                else if (Input.GetKeyDown(KeyCode.K))
                    currentScale += scaleStep;
#endif
            }
            //NORMAL - if can't keep fps, reduce resolution. Else try to increase it
            else
            {
                if (fpsCounter.CurrentFPS < minimumFPS)
                    currentScale -= scaleStep;
                else if (fpsCounter.CurrentFPS > maximumFPS)
                    currentScale += scaleStep;
            }

            //clamp it
            currentScale = Mathf.Clamp(currentScale, minResolutionScale, maxResolutionScale);

            //if current scale is different, change resolution
            if (Mathf.Approximately(currentScale, renderScale) == false)
                renderScale = currentScale;                                                         //effective resize in game

            //if not use OnGUI, set UI Text
            if (useOnGUI == false && uiText)
            {
                uiText.text = DebugText();
            }
        }

        private void OnGUI()
        {
            if (useOnGUI && CanShow())
            {
                GUI.Label(new Rect(30, 40, Screen.width / 2 - 60, Screen.height), DebugText());
            }
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

        private void OnOperationModeChanged(bool isConsole)
        {
            //if switch is in the dock, increase minimum, else reduce
            minResolutionScale = isConsole ? 0.85f : 0.7f;

            //be sure to stay in new minimum
            currentScale = Mathf.Clamp(currentScale, minResolutionScale, maxResolutionScale);
            //in the next Update the resolution will be changed if necessary
        }

        string DebugText()
        {
            int rezWidth = (int)(renderScale * Screen.currentResolution.width);
            int rezHeight = (int)(renderScale * Screen.currentResolution.height);
            return string.Format("Scale: {0:F2}x{1:F2}\nResolution: {2}x{3}\nScaleFactor: {4:F2}x{5:F2}\nFPS: {6}",
                currentScale,
                currentScale,
                rezWidth,
                rezHeight,
                renderScale,
                renderScale,
                fpsCounter.CurrentFPS);
        }

        #endregion
    }
}