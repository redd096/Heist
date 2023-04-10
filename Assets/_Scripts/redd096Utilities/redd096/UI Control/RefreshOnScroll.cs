using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace redd096
{
    /// <summary>
    /// use events to know when start animation and when finish to update list
    /// TODO Create a CustomScrollRect, where we can override BeginDrag and EndDrag for example to call update only on EndDrag (release touch)
    /// </summary>
    [AddComponentMenu("redd096/UI Control/Refresh On Scroll")]
    public class RefreshOnScroll : MonoBehaviour
    {
        public ScrollRect scrollview;

        //refresh icon
        public Transform iconToShow = default;
        public float animationDistance = 20;
        public float animationMovementDuration = 0.5f;
        public float animationRotationSpeed = 360;
        public float animationRotationDuration = 1;

        public System.Action onStartRefreshAnimation { get; set; }
        public System.Action onEndRefreshAnimation { get; set; }

        Vector2 iconStartLocalPosition;
        Vector2 contentStartLocalPosition;
        bool isRefreshing;

        private void OnEnable()
        {
            //save start position
            contentStartLocalPosition = scrollview.content.localPosition;
            iconStartLocalPosition = iconToShow.localPosition;

            scrollview.onValueChanged.AddListener(OnScrollViewValueChanged);
        }

        private void OnDisable()
        {
            scrollview.onValueChanged.RemoveListener(OnScrollViewValueChanged);
        }

        private void OnScrollViewValueChanged(Vector2 position)
        {
            if (isRefreshing)
                return;

            //check when move down content
            float distance = contentStartLocalPosition.y - scrollview.content.localPosition.y;

            //move also icon
            iconToShow.localPosition = new Vector2(iconToShow.localPosition.x, iconStartLocalPosition.y - distance);// / iconToShow.lossyScale.y);


            //when icon is visible, start animation
            if (iconToShow.localPosition.y < contentStartLocalPosition.y)
            {
                isRefreshing = true;
                StartCoroutine(IconAnimationCoroutine());
            }
        }

        IEnumerator IconAnimationCoroutine()
        {
            //call event
            onStartRefreshAnimation?.Invoke();
            yield return null;

            //stop drag
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.button = PointerEventData.InputButton.Left;
            scrollview.OnEndDrag(pointerEventData);

            //move down icon
            float delta = 0;
            Vector2 tempStartPosition = iconToShow.localPosition;
            while (delta < 1)
            {
                delta += Time.deltaTime / animationMovementDuration;
                iconToShow.localPosition = Vector2.Lerp(tempStartPosition, new Vector2(tempStartPosition.x, iconStartLocalPosition.y - animationDistance / iconToShow.lossyScale.y), delta);
                yield return null;
            }

            //rotation animation
            delta = 0;
            Quaternion iconStartRotation = iconToShow.rotation;
            while (delta < 1)
            {
                delta += Time.deltaTime / animationRotationDuration;
                iconToShow.Rotate(Vector3.back, animationRotationSpeed * Time.deltaTime);
                yield return null;
            }

            //reset icon
            iconToShow.localPosition = iconStartLocalPosition;
            iconToShow.rotation = iconStartRotation;

            //and call event
            onEndRefreshAnimation?.Invoke();

            isRefreshing = false;
        }
    }
}