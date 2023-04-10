using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Get Hit Feedback")]
    public class GetHitFeedback : FeedbackRedd096<HealthComponent>
    {
        [Header("On Get Damage")]
        [SerializeField] FeedbackStructRedd096 feedbackOnGetDamage = default;
        [SerializeField] CameraShakeStruct cameraShakeOnGetDamage = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnGetDamage = default;

        [Header("On Get Damage - rotate from hit point to this transform, for example for blood spurts")]
        [SerializeField] FeedbackStructRedd096 feedbackRotatedOnGetDamage = default;

        [Header("Blink On Get Damage - Default get component in children")]
        [SerializeField] bool blinkOnGetDamage = true;
        [EnableIf("blinkOnGetDamage")][SerializeField] SpriteRenderer[] spritesToUse = default;
        [EnableIf("blinkOnGetDamage")][SerializeField] Material blinkMaterial = default;
        [EnableIf("blinkOnGetDamage")][SerializeField] float blinkDuration = 0.2f;

        [Header("Stop Time On Get Damage")]
        [SerializeField] bool stopTimeOnGetDamage = false;
        [EnableIf("stopTimeOnGetDamage")][SerializeField] float timeScaleToSet = 0.0f;
        [EnableIf("stopTimeOnGetDamage")][SerializeField] float timeBeforeResetTime = 0.1f;

        [Header("Popup On Get Damage")]
        [SerializeField] PopupText popupPrefab = default;
        Pooling<PopupText> poolPopup = new Pooling<PopupText>();

        [Header("On Get Health")]
        [SerializeField] FeedbackStructRedd096 feedbackOnGetHealth = default;
        [SerializeField] CameraShakeStruct cameraShakeOnGetHealth = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnGetHealth = default;

        [Header("On Die (instantiate every element in array)")]
        [SerializeField] FeedbackStructRedd096 feedbackOnDie = default;
        [SerializeField] CameraShakeStruct cameraShakeOnDie = default;
        [SerializeField] GamepadVibrationStruct gamepadVibrationOnDie = default;

        Camera cam;
        Dictionary<SpriteRenderer, Material> savedMaterials = new Dictionary<SpriteRenderer, Material>();

        Coroutine blinkCoroutine;
        Coroutine stopTimeCoroutine;

        void Awake()
        {
            //save materials
            if (spritesToUse == null || spritesToUse.Length <= 0) spritesToUse = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sprite in spritesToUse)
                savedMaterials.Add(sprite, sprite.material);
        }

        protected override void OnEnable()
        {
            //get references
            if (spritesToUse == null || spritesToUse.Length <= 0) spritesToUse = GetComponentsInChildren<SpriteRenderer>();
            if (cam == null) cam = Camera.main;

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            owner.onGetDamage += OnGetDamage;
            owner.onDie += OnDie;
            owner.onGetHealth += OnGetHealth;
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            owner.onGetDamage -= OnGetDamage;
            owner.onDie -= OnDie;
            owner.onGetHealth -= OnGetHealth;
        }

        #region private API

        void OnGetDamage(Character whoHit, Vector2 hitPoint)
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnGetDamage);

            //camera shake and gamepad vibration
            cameraShakeOnGetDamage.TryShake();
            gamepadVibrationOnGetDamage.TryVibration();

            //=====================================================================================

            //rotation
            Vector2 direction = ((Vector2)transform.position - hitPoint).normalized;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90, Vector3.forward) * direction);   //Forward keep to Z axis. Up use X instead of Y (AngleAxis 90) and rotate to direction

            //when rotate to opposite direction (from default), rotate 180 updown
            if (direction.x < 0)
            {
                rotation *= Quaternion.AngleAxis(180, Vector3.right);
            }

            //instantiate rotated vfx and sfx
            feedbackRotatedOnGetDamage.InstantiateFeedback(transform.position, rotation);

            //=====================================================================================

            //blink
            if (blinkCoroutine == null && blinkOnGetDamage)
                blinkCoroutine = StartCoroutine(BlinkCoroutine());

            //stop time on get damage
            if (stopTimeOnGetDamage)
            {
                if (stopTimeCoroutine != null)
                    StopCoroutine(stopTimeCoroutine);

                stopTimeCoroutine = StartCoroutine(StopTimeCoroutine());
            }

            //popup on get damage
            if (popupPrefab && cam)
            {
                PopupText popup = poolPopup.Instantiate(popupPrefab);
                popup.GetComponentInChildren<Canvas>().worldCamera = cam;
                popup.Init(transform.position);//cam.WorldToScreenPoint(transform.position);
            }
        }

        IEnumerator BlinkCoroutine()
        {
            //set blink material, wait, then back to saved material
            foreach (SpriteRenderer sprite in savedMaterials.Keys)
                sprite.material = blinkMaterial;

            yield return new WaitForSeconds(blinkDuration);

            foreach (SpriteRenderer sprite in savedMaterials.Keys)
                sprite.material = savedMaterials[sprite];

            blinkCoroutine = null;
        }

        IEnumerator StopTimeCoroutine()
        {
            //set time scale
            Time.timeScale = timeScaleToSet;

            //wait
            float time = Time.realtimeSinceStartup + timeBeforeResetTime;
            while (time > Time.realtimeSinceStartup)
            {
                yield return null;
            }

            //reset time
            Time.timeScale = 1;
        }

        void OnGetHealth()
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnGetHealth);

            //camera shake and gamepad vibration
            cameraShakeOnGetHealth.TryShake();
            gamepadVibrationOnGetHealth.TryVibration();
        }

        void OnDie(HealthComponent whoDied, Character whoHit)
        {
            //instantiate vfx and sfx
            InstantiateFeedback(feedbackOnDie);

            //camera shake and gamepad vibration
            cameraShakeOnDie.TryShake();
            gamepadVibrationOnDie.TryVibration();
        }

        #endregion
    }
}