using UnityEngine;
using UnityEngine.UI;

namespace redd096
{
    [AddComponentMenu("redd096/UI Control/Scroll Rect With Slider")]
    public class ScrollRectWithSlider : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] ScrollRect scrollRect = default;
        [SerializeField] Slider verticalSlider = default;
        [SerializeField] Slider horizontalSlider = default;

        [Header("Hide when not necessary")]
        [SerializeField] bool hideWhenNotNecessary = true;
        [SerializeField] RectTransform viewport = default;
        [SerializeField] RectTransform content = default;

        void OnEnable()
        {
            if (scrollRect == null)
                Debug.LogWarning("Miss ScrollRect for ScrollRectWithSlider on " + name);
            if (verticalSlider == null && horizontalSlider == null)
                Debug.LogWarning("Miss Slider for ScrollRectWithSlider on " + name);

            //set events
            if (scrollRect) scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
            if (verticalSlider) verticalSlider.onValueChanged.AddListener(OnVerticalSliderChanged);
            if (horizontalSlider) horizontalSlider.onValueChanged.AddListener(OnHorizontalSliderChanged);

            //set default positions
            OnScrollRectChanged(scrollRect ? scrollRect.normalizedPosition : Vector2.zero);
        }

        void OnDisable()
        {
            //remove events
            if (scrollRect) scrollRect.onValueChanged.RemoveListener(OnScrollRectChanged);
            if (verticalSlider) verticalSlider.onValueChanged.RemoveListener(OnVerticalSliderChanged);
            if (horizontalSlider) horizontalSlider.onValueChanged.RemoveListener(OnHorizontalSliderChanged);
        }

        void FixedUpdate()
        {
            //check size
            if (hideWhenNotNecessary && viewport && content)
            {
                //hide if not necessary
                if (content.rect.height <= viewport.rect.height)
                {
                    if (verticalSlider) verticalSlider.gameObject.SetActive(false);
                    if (horizontalSlider) horizontalSlider.gameObject.SetActive(false);
                }
                //else show
                else
                {
                    if (verticalSlider) verticalSlider.gameObject.SetActive(true);
                    if (horizontalSlider) horizontalSlider.gameObject.SetActive(true);
                }
            }
        }

        void OnScrollRectChanged(Vector2 value)
        {
            //if(verticalSlider && horizontalSlider)
            //    Debug.Log($"scroll rect change: vertical from {verticalSlider.value} to {1 - scrollRect.verticalNormalizedPosition}, horizontal from {horizontalSlider.value} to {1 - scrollRect.horizontalNormalizedPosition}");
            //else if(verticalSlider)
            //    Debug.Log($"scroll rect change: vertical from {verticalSlider.value} to {1 - scrollRect.verticalNormalizedPosition}");
            //else if(horizontalSlider)
            //    Debug.Log($"scroll rect change: horizontal from {horizontalSlider.value} to {1 - scrollRect.horizontalNormalizedPosition}");

            //scrollRect go from 1 to 0, slider from 0 to 1
            if (verticalSlider) verticalSlider?.SetValueWithoutNotify(1 - scrollRect.verticalNormalizedPosition);
            if (horizontalSlider) horizontalSlider.SetValueWithoutNotify(1 - scrollRect.horizontalNormalizedPosition);
        }

        void OnVerticalSliderChanged(float value)
        {
            //Debug.Log($"vertical slider change: from {scrollRect.verticalNormalizedPosition} to {1 - verticalSlider.value}");

            //scrollRect go from 1 to 0, slider from 0 to 1
            if (scrollRect) scrollRect.verticalNormalizedPosition = 1 - verticalSlider.value;
        }

        void OnHorizontalSliderChanged(float value)
        {
            //Debug.Log($"horizontal slider change: from {scrollRect.horizontalNormalizedPosition} to {1 - horizontalSlider.value}");

            //scrollRect go from 1 to 0, slider from 0 to 1
            if (scrollRect) scrollRect.horizontalNormalizedPosition = 1 - horizontalSlider.value;
        }
    }
}