using System.Collections;
using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Weapons/Weapon Range")]
    public class WeaponRange : WeaponBASE
    {
        public enum EFireMode { SemiAutomatic, Automatic, Burst }

        [Header("Range Weapon")]
        [Tooltip("Keep pressed or click?")] public EFireMode FireMode = EFireMode.SemiAutomatic;
        [Tooltip("Delay between shots")] public float RateOfFire = 0.2f;
        [Tooltip("Push back when shoot")] public float Recoil = 1;
        [Tooltip("Rotate random the shot when instantiated")] public float NoiseAccuracy = 10;

        [Header("Burst Fire")]
        [EnableIf("FireMode", EFireMode.Burst)][Tooltip("Number of shots to shoot at every click")][Min(1)] public int NumberOfShots = 3;
        [EnableIf("FireMode", EFireMode.Burst)][Tooltip("Delay between shots")] public float DelayBetweenShots = 0.1f;

        [Header("Barrel")]
        [Tooltip("Spawn bullets")] public Transform[] Barrels = default;
        [Tooltip("When more than one barrel, shoot every bullet simultaneously or from one random?")] public bool BarrelSimultaneously = true;

        [Header("Bullet")]
        public Bullet BulletPrefab = default;
        public float Damage = 10;
        public float BulletSpeed = 10;

        [Header("Ammo - NONE = always full")]
        [Dropdown("GetAllAmmoTypes")] public string AmmoType = "NONE";
        [Tooltip("When pick this weapon for the first time, pick also ammo")] public int AmmoOnPick = 12;
        [ShowAssetPreview] public Sprite AmmoSprite = default;

        [Header("DEBUG")]
        [SerializeField] bool drawDebug = false;

        //private
        float timeForNextShoot;
        Coroutine automaticShootCoroutine;
        Coroutine burstShootCoroutine;

        //bullets
        Pooling<Bullet> bulletsPooling = new Pooling<Bullet>();
        Transform _bulletsParent;
        Transform BulletsParent { get { if (_bulletsParent == null) _bulletsParent = new GameObject(name + "'s Bullets Parent").transform; return _bulletsParent; } }

        //events
        public System.Action<Transform> onInstantiateBullet { get; set; }       //called for every bullet instantiated
        public System.Action onShoot { get; set; }                              //called one time for shoot, also if instantiate more bullets
        public System.Action onLastShotBurst { get; set; }                      //called when shoot last shot of a burst (for FireMode == Burst), also if there is only one ammo
        public System.Action onFailShoot { get; set; }                          //called when fail to shoot, for example when there are no ammo
        public System.Action onPressAttack { get; set; }                        //called when press attack, also if not shoot or shoot more times
        public System.Action onReleaseAttack { get; set; }                      //called when release attack

        #region unity editor
#if UNITY_EDITOR

        string[] GetAllAmmoTypes()
        {
            //get guid to every prefab in project
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:prefab");
            System.Collections.Generic.List<string> values = new System.Collections.Generic.List<string>();

            //default is no type of ammo
            values.Add("NONE");

            //return array with loaded assets
            Ammo ammo;
            for (int i = 0; i < guids.Length; i++)
            {
                //cast to be sure is an Ammo prefab
                ammo = UnityEditor.AssetDatabase.LoadAssetAtPath<Ammo>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]));
                if (ammo) values.Add(ammo.AmmoType);
            }

            return values.ToArray();
        }

#endif
        #endregion

        public override void PressAttack()
        {
            //check rate of fire (and is not doing a burst shoot)
            if (Time.time > timeForNextShoot && burstShootCoroutine == null)
            {
                //shoot
                if (Shoot())
                {
                    //if burst mode and first shoot worked, start burst coroutine
                    if (FireMode == EFireMode.Burst)
                    {
                        if (burstShootCoroutine == null)
                            burstShootCoroutine = StartCoroutine(BurstShootCoroutine());
                    }
                }
            }

            //start coroutine if automatic
            if (FireMode == EFireMode.Automatic)
            {
                if (automaticShootCoroutine != null)
                    StopCoroutine(automaticShootCoroutine);

                automaticShootCoroutine = StartCoroutine(AutomaticShootCoroutine());
            }

            //call event
            onPressAttack?.Invoke();
        }

        public override void ReleaseAttack()
        {
            //stop coroutine if running (automatic shoot)
            if (automaticShootCoroutine != null)
                StopCoroutine(automaticShootCoroutine);

            //call event
            onReleaseAttack?.Invoke();
        }

        protected virtual void OnDisable()
        {
            //be sure to reset burst shoot when disable weapon
            burstShootCoroutine = null;
        }

        #region private API

        /// <summary>
        /// Create bullet from every barrel or one random barrel
        /// </summary>
        bool Shoot()
        {
            //if this weapon need ammos
            if (AmmoType != "NONE" && Owner && Owner.GetSavedComponent<AdvancedWeaponComponent>())
            {
                //if there are not enough ammos, fail shoot
                if (Owner.GetSavedComponent<AdvancedWeaponComponent>().GetCurrentAmmo(AmmoType) <= 0)
                {
                    //stop coroutine if automatic - when take ammo release and repress to shoot
                    if (automaticShootCoroutine != null)
                        StopCoroutine(automaticShootCoroutine);

                    onFailShoot?.Invoke();
                    return false;
                }
                //else remove ammos
                else
                {
                    Owner.GetSavedComponent<AdvancedWeaponComponent>().AddAmmo(AmmoType, -1);
                }
            }

            //update rate of fire
            timeForNextShoot = Time.time + RateOfFire;

            //shoot every bullet
            if (BarrelSimultaneously)
            {
                foreach (Transform barrel in Barrels)
                {
                    InstantiateBullet(barrel);
                }
            }
            //or shoot one bullet from random barrel
            else
            {
                InstantiateBullet(Barrels[Random.Range(0, Barrels.Length)]);
            }

            //pushback owner
            if (Owner && Owner.GetSavedComponent<MovementComponent>() && Owner.GetSavedComponent<AimComponent>())
                Owner.GetSavedComponent<MovementComponent>().PushInDirection(-Owner.GetSavedComponent<AimComponent>().AimDirectionInput, Recoil);

            //call event
            onShoot?.Invoke();
            return true;
        }

        /// <summary>
        /// Instantiate bullet and set it
        /// </summary>
        /// <param name="barrel"></param>
        void InstantiateBullet(Transform barrel)
        {
            if (BulletPrefab == null)
                return;

            //create random noise in accuracy
            float randomNoiseAccuracy = Random.Range(-NoiseAccuracy, NoiseAccuracy);
            Vector2 direction = Quaternion.AngleAxis(randomNoiseAccuracy, Vector3.forward) * barrel.right;                                  //direction with noise

            //draw debug
            if (drawDebug)
                Debug.DrawLine(barrel.position, (Vector2)barrel.position + direction, Color.red, 1);

            //instantiate bullet
            Bullet bullet = bulletsPooling.Instantiate(BulletPrefab, BulletsParent);
            bullet.transform.position = barrel.position;
            bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90, Vector3.forward) * direction);    //rotate direction to left, to use right as forward

            //and set it
            bullet.Init(this, Owner, direction);

            //call event
            onInstantiateBullet?.Invoke(barrel);
        }

        /// <summary>
        /// Continue shooting
        /// </summary>
        /// <returns></returns>
        IEnumerator AutomaticShootCoroutine()
        {
            while (true)
            {
                //stop if lose owner
                if (Owner == null)
                    break;

                //check rate of fire
                if (Time.time > timeForNextShoot)
                {
                    //shoot
                    Shoot();
                }

                yield return null;
            }
        }

        /// <summary>
        /// Shot every bullet in the burst
        /// </summary>
        /// <returns></returns>
        IEnumerator BurstShootCoroutine()
        {
            //number of shots for this burst, minus one because is already shooted
            int shotsToShoot = NumberOfShots - 1;
            float timeNextShotInTheBurst = Time.time + DelayBetweenShots;   //don't use rate of fire, but delay between shots of this burst (set here because first one is already shooted)

            while (shotsToShoot > 0)
            {
                //stop if lose owner
                if (Owner == null)
                    break;

                //check time between shots
                if (Time.time > timeNextShotInTheBurst)
                {
                    //shoot
                    if (Shoot())
                    {
                        timeNextShotInTheBurst = Time.time + DelayBetweenShots;     //set delay
                        shotsToShoot--;                                             //decrease counter
                    }
                    //break if can't shoot (eg. when no ammo)
                    else
                    {
                        break;
                    }
                }

                yield return null;
            }

            //when shoot last shot of a burst
            onLastShotBurst?.Invoke();

            burstShootCoroutine = null;
        }

        #endregion
    }
}