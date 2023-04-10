using UnityEngine;

namespace redd096
{
    [CreateAssetMenu(fileName = "ScaleEffect", menuName = "redd096/UI Control/ScrollSnapEffect/Scale Effect")]
    public class ScaleEffect : BaseScrollSnapEffect
    {
        public Vector2 selectedItemScale = Vector2.one * 1.2f;
        public Vector2 unselectedItemScale = Vector2.one * 0.7f;

        public override void OnItemUpdated(RectTransform transform, float displacement)
        {
            Scale(transform, displacement);
        }

        private void Scale(RectTransform transform, float displacement)
        {
            var ratio = GetEffectRatioAbs(displacement);
            var diff = selectedItemScale - unselectedItemScale;
            transform.localScale = new Vector3(unselectedItemScale.x, unselectedItemScale.y, 1) + (Vector3)diff * ratio;
        }
    }
}