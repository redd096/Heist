using UnityEngine;
using UnityEngine.UI;
using TMPro;
using redd096.Attributes;


namespace redd096.GameTopDown2D
{
    /// <summary>
    /// Create a prefab with this script and instantiate where you need
    /// </summary>
    [AddComponentMenu("redd096/.GameTopDown2D/Generic/Popup Text")]
    public class PopupText : MonoBehaviour
    {
        [Header("Popup - default get from children")]
        [SerializeField] bool useTextMeshPro = false;
        [HideIf("useTextMeshPro")][SerializeField] Text textUI = default;
        [ShowIf("useTextMeshPro")][SerializeField] TextMeshPro textMesh = default;
        [SerializeField] string[] possibleTexts = default;

        [Header("Movement")]
        [SerializeField] Vector2 minMovement = Vector2.up * 2f;
        [SerializeField] Vector2 maxMovement = Vector2.up * 2f;

        [Header("Time before start fade out")]
        [Tooltip("Float without fade")][SerializeField] float timeBeforeFadeMin = 0.1f;
        [Tooltip("Float without fade")][SerializeField] float timeBeforeFadeMax = 0.1f;

        [Header("Duration fade out")]
        [Tooltip("After floatTime, continue to float but start to fade")][SerializeField] float fadeDurationMin = 1.5f;
        [Tooltip("After floatTime, continue to float but start to fade")][SerializeField] float fadeDurationMax = 1.5f;

        [Header("Coding Vars")]
        [Tooltip("Deactivate instead of Destroy")][SerializeField] bool usePooling = true;
        [Tooltip("Start movement at OnEnable (if just put prefab in scene)")][SerializeField] bool autoInitializeOnEnable = false;

        Color startColor;
        Color textColor;

        //randomized vars
        float timeBeforeFade;
        float fadeDuration;
        Vector2 movement;

        void Awake()
        {
            //get refs
            if (textUI == null) textUI = GetComponentInChildren<Text>();
            if (textMesh == null) textMesh = GetComponentInChildren<TextMeshPro>();

            //save start vars
            if (useTextMeshPro && textMesh)
            {
                startColor = textMesh.color;
            }
            else if (useTextMeshPro == false && textUI)
            {
                startColor = textUI.color;
            }
        }

        void OnEnable()
        {
            //auto initialize if necessary
            if (autoInitializeOnEnable)
                Init(GetTextPosition());
        }

        void Update()
        {
            if (CheckTextNotNull() == false)
                return;

            //move text and reduce float time
            MoveText(movement * Time.deltaTime);
            timeBeforeFade -= Time.deltaTime;

            //if finished float time
            if (timeBeforeFade <= 0)
            {
                //start fade out
                textColor.a -= Time.deltaTime / fadeDuration;

                //set text color
                SetTextColor(textColor);

                //destroy when alpha at 0
                if (textColor.a <= 0f)
                {
                    if (usePooling)
                        gameObject.SetActive(false);
                    else
                        Destroy(gameObject);
                }
            }
        }

        #region private API

        bool CheckTextNotNull()
        {
            //return if used text is not null
            return (useTextMeshPro && textMesh) || (useTextMeshPro == false && textUI);
        }

        void SetText(string text)
        {
            if (CheckTextNotNull() == false)
                return;

            if (useTextMeshPro) textMesh.SetText(text);
            else textUI.text = text;
        }

        void SetTextColor(Color color)
        {
            if (CheckTextNotNull() == false)
                return;

            if (useTextMeshPro) textMesh.color = color;
            else textUI.color = color;
        }

        void SetTextPosition(Vector2 position)
        {
            if (CheckTextNotNull() == false)
                return;

            if (useTextMeshPro) textMesh.transform.position = position;
            else textUI.transform.position = position;
        }

        void MoveText(Vector2 movement)
        {
            if (CheckTextNotNull() == false)
                return;

            if (useTextMeshPro) textMesh.transform.position += (Vector3)movement;
            else textUI.transform.position += (Vector3)movement;
        }

        Vector2 GetTextPosition()
        {
            if (CheckTextNotNull() == false)
                return Vector2.zero;

            if (useTextMeshPro) return textMesh.transform.position;
            else return textUI.transform.position;
        }

        #endregion

        #region public API

        /// <summary>
        /// Initialize to set one random text from the array in the prefab
        /// </summary>
        public void Init(Vector2 textPosition)
        {
            Init(textPosition, possibleTexts);
        }

        /// <summary>
        /// Initialize to set text
        /// </summary>
        /// <param name="textToUse"></param>
        public void Init(Vector2 textPosition, string textToUse)
        {
            Init(textPosition, new string[1] { textToUse });
        }

        /// <summary>
        /// Initialize to set one random text from array
        /// </summary>
        /// <param name="textsToUse"></param>
        public void Init(Vector2 textPosition, params string[] textsToUse)
        {
            if (CheckTextNotNull() == false)
            {
                Debug.LogWarning("Miss Text or TextMeshPro on " + name);
                return;
            }

            if (textsToUse == null || textsToUse.Length <= 0)
            {
                Debug.LogWarning("Miss texts to show on " + name);
                return;
            }

            //set text
            SetText(textsToUse[Random.Range(0, textsToUse.Length)]);

            //set random vars
            movement = new Vector2(Random.Range(minMovement.x, maxMovement.x), Random.Range(minMovement.y, maxMovement.y));
            timeBeforeFade = Random.Range(timeBeforeFadeMin, timeBeforeFadeMax);
            fadeDuration = Random.Range(fadeDurationMin, fadeDurationMax);

            //and reset (necessary for pooling)
            SetTextPosition(textPosition);
            textColor = startColor;
            SetTextColor(textColor);
        }

        #endregion
    }
}