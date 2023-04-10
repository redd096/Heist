using UnityEngine;
using redd096.Attributes;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/Dynamic Resolution Camera")]
    public class DynamicResolutionCamera : MonoBehaviour
    {
        [HelpBox("Enable Allow Dynamic Resolution to every cameras in scenes, and in Player Settings enable Frame Timing Stats")]
        [Header("Resolution Scale")]
        [SerializeField] float maxResolutionScale = 1.0f;
        [SerializeField] float minResolutionScale = 0.85f;

        [Header("Step to reduce or increase")]
        [SerializeField] float scaleStep = 0.1f;

        [Header("Show on Screen? Only in editor, in dev builds, or also normal builds?")]
        [SerializeField] bool showInEditor = true;
        [SerializeField] bool showInBuildDevelopment = true;
        [SerializeField] bool showInBuild = false;

        [Header("Use GUI or UI Text")]
        [SerializeField] bool useOnGUI = false;
        [SerializeField] UnityEngine.UI.Text uiText = default;

        [Header("DEBUG - J and K to reduce or increase")]
        [SerializeField] bool useButtonsToReduceAndIncrease = false;

        private static DynamicResolutionCamera instance;

        //current scale
        float currentScale = 1.0f;
        float previousScale = 1.0f;

        //number of frames with correct fps, before try increase resolution
        int framesWithCorrectFps = 0;

        // Variables for dynamic resolution algorithm that persist across frames
        const uint NumFrameTimings = 2;                     //frames to check
        FrameTiming[] frameTimings = new FrameTiming[3];    //received frame timings
        uint frameCount = 0;                                //saved frames
        double gpuFrameTime;
        double cpuFrameTime;

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

            OnValidateFunc();

            //show warning if necessary
            if (CanShow() && useOnGUI == false && uiText == null)
                Debug.LogWarning($"Missing UIText on {this}", gameObject);
        }

        private void OnValidate()
        {
            OnValidateFunc();
        }

        void Update()
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
                if (IsCorrectFPS() == false)
                {
                    framesWithCorrectFps = 0;
                    currentScale -= scaleStep;
                }
                else
                {
                    framesWithCorrectFps++;
                    if (framesWithCorrectFps > 60)  //keep correct frames for few time
                        currentScale += scaleStep;
                }
            }

            //clamp it
            currentScale = Mathf.Clamp(currentScale, minResolutionScale, maxResolutionScale);

            //if current scale is different, change resolution
            if (Mathf.Approximately(currentScale, previousScale) == false)
            {
                previousScale = currentScale;
                ScalableBufferManager.ResizeBuffers(currentScale, currentScale);                //effective resize in game
            }

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

        private string DebugText()
        {
            int rezWidth = (int)Mathf.Ceil(ScalableBufferManager.widthScaleFactor * Screen.currentResolution.width);
            int rezHeight = (int)Mathf.Ceil(ScalableBufferManager.heightScaleFactor * Screen.currentResolution.height);
            return string.Format("Scale: {0:F2}x{1:F2}\nResolution: {2}x{3}\nScaleFactor: {4:F2}x{5:F2}\nGPU: {6:F2} CPU: {7:F2}",
                currentScale,
                currentScale,
                rezWidth,
                rezHeight,
                ScalableBufferManager.widthScaleFactor,
                ScalableBufferManager.heightScaleFactor,
                gpuFrameTime,
                cpuFrameTime);
        }

        private bool IsCorrectFPS()
        {
            //wait few frames
            ++frameCount;
            if (frameCount <= NumFrameTimings)
            {
                return true;
            }

            //then get frame timings and checks if their are the correct number
            FrameTimingManager.CaptureFrameTimings();
            FrameTimingManager.GetLatestTimings(NumFrameTimings, frameTimings);

            //if not, some frame is skipped
            if (frameTimings.Length < NumFrameTimings)
            {
                return false;
            }

            //save gpu and cpu frame time
            gpuFrameTime = (double)frameTimings[0].gpuFrameTime;
            cpuFrameTime = (double)frameTimings[0].cpuFrameTime;
            return true;
        }

        #endregion
    }
}