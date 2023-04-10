using System.Collections;
using UnityEngine;

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/FPS Counter Simple")]
    public class FPSCounterSimple : MonoBehaviour
    {
        [Header("Use GUI or UI Text - default UI get in children")]
        [SerializeField] bool useOnGUI = true;
        [SerializeField] UnityEngine.UI.Text uiText = default;

        private static FPSCounterSimple instance;
        private int countFPS;

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
            }

            //get references
            if (uiText == null)
                uiText = GetComponentInChildren<UnityEngine.UI.Text>();

            //warning
            if (useOnGUI == false && uiText == null) Debug.LogWarning("Missing UIText on FPS Counter - " + gameObject);
        }

        private void OnEnable()
        {
            StartCoroutine(CalculateFPSCoroutine());
        }

        private void OnGUI()
        {
            //if use OnGUI, set it
            if (useOnGUI)
                GUI.Label(new Rect(30, 40, 100, 25), "FPS: " + countFPS);
        }

        private IEnumerator CalculateFPSCoroutine()
        {
            while (true)
            {
                //calculate fps
                countFPS = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);

                //if not use OnGUI, set UI Text
                if (useOnGUI == false && uiText)
                    uiText.text = "FPS: " + countFPS;

                //wait unscaled time
                float timer = Time.unscaledTime + 0.1f;
                while (Time.unscaledTime < timer)
                    yield return null;
            }
        }
    }
}