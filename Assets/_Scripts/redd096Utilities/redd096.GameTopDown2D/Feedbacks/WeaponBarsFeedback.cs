using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Feedbacks/Weapon Bars Feedback")]
    public class WeaponBarsFeedback : FeedbackRedd096<WeaponComponent>
    {
        [Header("Bars (already in scene/prefab)")]
        [SerializeField] Image rateOfFireImage = default;
        [SerializeField] bool showRateOfFireAlsoOnAutomatic = false;

        Coroutine rateOfFireCoroutine;
        WeaponRange weaponRange;

        protected override void OnEnable()
        {
            //by default disable bars
            if (rateOfFireImage) rateOfFireImage.gameObject.SetActive(false);

            base.OnEnable();
        }

        protected override void AddEvents()
        {
            base.AddEvents();

            //add events
            owner.onChangeWeapon += OnChangeWeapon;

            //add events if has weapon
            OnChangeWeapon();
        }

        protected override void RemoveEvents()
        {
            base.RemoveEvents();

            //remove events
            owner.onChangeWeapon -= OnChangeWeapon;

            //remove events if has weapon
            DropWeapon();
        }

        #region private API

        void OnChangeWeapon()
        {
            //add events if has weapon
            if (owner && owner.CurrentWeapon && owner.CurrentWeapon is WeaponRange)
            {
                //be sure to remove old weapon
                DropWeapon();

                //save ref
                weaponRange = owner.CurrentWeapon as WeaponRange;

                weaponRange.onShoot += OnShoot;
                weaponRange.onLastShotBurst += OnLastShotBurst;
            }
        }

        void DropWeapon()
        {
            //remove events if has weapon
            if (weaponRange)
            {
                weaponRange.onShoot -= OnShoot;
                weaponRange.onLastShotBurst -= OnLastShotBurst;
            }

            //be sure to stop coroutines
            if (rateOfFireCoroutine != null) StopCoroutine(rateOfFireCoroutine);

            //be sure to disable bars
            if (rateOfFireImage) rateOfFireImage.gameObject.SetActive(false);
        }

        void OnShoot()
        {
            //only if fire mode is not Burst
            if (weaponRange && weaponRange.FireMode != WeaponRange.EFireMode.Burst)
            {
                //start rate of fire coroutine
                if (rateOfFireCoroutine != null)
                    StopCoroutine(rateOfFireCoroutine);

                rateOfFireCoroutine = StartCoroutine(RateOfFireCoroutine());
            }
        }

        void OnLastShotBurst()
        {
            //only in Burst mode
            if (weaponRange && weaponRange.FireMode == WeaponRange.EFireMode.Burst)
            {
                //start rate of fire coroutine
                if (rateOfFireCoroutine != null)
                    StopCoroutine(rateOfFireCoroutine);

                rateOfFireCoroutine = StartCoroutine(RateOfFireCoroutine());
            }
        }

        IEnumerator RateOfFireCoroutine()
        {
            if (weaponRange && rateOfFireImage)
            {
                //only if show also on automatic, or if is not automatic
                if (showRateOfFireAlsoOnAutomatic || weaponRange.FireMode != WeaponRange.EFireMode.Automatic)
                {
                    //enable bar
                    rateOfFireImage.fillAmount = 1;
                    rateOfFireImage.gameObject.SetActive(true);

                    //update bar
                    float delta = 0;
                    while (delta < 1)
                    {
                        delta += Time.deltaTime / weaponRange.RateOfFire;
                        rateOfFireImage.fillAmount = delta;

                        yield return null;
                    }

                    //disable bar
                    rateOfFireImage.gameObject.SetActive(false);
                }
            }
        }

        #endregion
    }
}