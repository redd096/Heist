using UnityEngine;
using redd096.Attributes;

namespace redd096
{
    [AddComponentMenu("redd096/MonoBehaviours/Fluctuate")]
    public class Fluctuate : MonoBehaviour
    {
        [Header("Use animation curve or sin")]
        [SerializeField] bool useAnimationCurve = true;
        [EnableIf("useAnimationCurve")][SerializeField] AnimationCurve animationCurve = default;
        [DisableIf("useAnimationCurve")][SerializeField] float height = 0.5f;
        [DisableIf("useAnimationCurve")][SerializeField] float speed = 1.5f;

        Vector3 startPosition;
        float previousTime;

        void Update()
        {
            //startPosition is (current position - previous "fluctuation"). We update it in case someone move this object
            if (useAnimationCurve)
                startPosition = transform.position - Vector3.up * animationCurve.Evaluate(previousTime);
            else
                startPosition = transform.position - Vector3.up * Mathf.Sin(previousTime * speed) * height;

            //new position is (start position + current "fluctuation"). Time.time instead of previousTime
            if (useAnimationCurve)
                transform.position = startPosition + Vector3.up * animationCurve.Evaluate(Time.time);
            else
                transform.position = startPosition + Vector3.up * Mathf.Sin(Time.time * speed) * height;

            //save previous time to recalculate startPosition
            previousTime = Time.time;
        }
    }
}