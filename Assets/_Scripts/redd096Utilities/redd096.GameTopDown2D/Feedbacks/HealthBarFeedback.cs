using System.Collections;
using UnityEngine;
using redd096.Attributes;
using UnityEngine.UI;

namespace redd096.GameTopDown2D
{
    /// <summary>
    /// If not setted automatically, you need to put predictionImage above healthImage (to render behind)
    /// </summary>
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Health Bar Feedback")]
    public class HealthBarFeedback : FeedbackRedd096<HealthComponent>
    {
        [Header("Health Bar (Image with fill amount)")]
        [SerializeField] Image healthImage = default;

        [Header("Predict")]
        [SerializeField] bool enablePredict = true;
        [EnableIf("enablePredict")] [SerializeField] Image predictionImage = default;
        [EnableIf("enablePredict")] [Tooltip("For how much time prediction will remain still")] [SerializeField] float durationPrediction = 0.5f;
        [EnableIf("enablePredict")] [Tooltip("Duration animation from start value to end value")] [SerializeField] float timeToReachValue = 0.5f;
        [EnableIf("enablePredict")] [SerializeField] Color colorOnIncrease = Color.green;
        [EnableIf("enablePredict")] [SerializeField] Color colorOnDecrease = Color.red;

        Coroutine updateHealthBarCoroutine;

        protected override void OnEnable()
        {
            //get references
            if (healthImage == null) healthImage = GetComponentInParent<Image>();

            //set prediction above health (to render it behind)
            if (healthImage && predictionImage)
                predictionImage.transform.SetSiblingIndex(healthImage.transform.GetSiblingIndex() - 1);

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            owner.onChangeHealth += UpdateLife;
            UpdateLife(); //set default value
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            owner.onChangeHealth -= UpdateLife;
        }

        void UpdateLife()
        {
            //do nothing if health bar is not setted
            if (healthImage == null)
                return;

            //if predict is not enabled or there isn't prediction image, just set health value
            if (enablePredict == false || predictionImage == null)
            {
                healthImage.fillAmount = owner.CurrentHealth / owner.MaxHealth;
            }
            //else start coroutine to update health using prediction too
            else
            {
                if (updateHealthBarCoroutine != null)
                    StopCoroutine(updateHealthBarCoroutine);

                updateHealthBarCoroutine = StartCoroutine(UpdateHealthBarCoroutine());
            }
        }

        IEnumerator UpdateHealthBarCoroutine()
        {
            //check if increase or decrease
            float valueToReach = owner.CurrentHealth / owner.MaxHealth;
            bool increase = valueToReach >= predictionImage.fillAmount;

            //set prediction color (increase or decrease)
            predictionImage.color = increase ? colorOnIncrease : colorOnDecrease;

            //if increase, move prediction first, else move healthBar
            if (increase)
                predictionImage.fillAmount = valueToReach;
            else
                healthImage.fillAmount = valueToReach;

            //wait duration
            yield return new WaitForSeconds(durationPrediction);

            //then start animation
            float startValue = increase ? healthImage.fillAmount : predictionImage.fillAmount;
            float delta = 0;
            while (delta < 1)
            {
                delta += Time.deltaTime / timeToReachValue;

                if (increase)
                    healthImage.fillAmount = Mathf.Lerp(startValue, valueToReach, delta);
                else
                    predictionImage.fillAmount = Mathf.Lerp(startValue, valueToReach, delta);

                yield return null;
            }
        }
    }
}