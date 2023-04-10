using UnityEngine;

namespace redd096.GameTopDown2D
{
    /// <summary>
    /// Use this script to place weapons in scene. So this will set also WeaponPrefab
    /// </summary>
    [AddComponentMenu("redd096/.GameTopDown2D/Weapons/Spawn Weapon")]
    public class SpawnWeapon : MonoBehaviour
    {
        [SerializeField] WeaponBASE weaponPrefab = default;

        //editor used to show weapon sprite when place the spawn
        [HideInInspector][SerializeField] SpriteRenderer spriteInEditor = default;

        WeaponBASE instantiatedWeapon;

        void OnValidate()
        {
            //only in editor
            if (Application.isPlaying == false)
            {
                //add sprite renderer
                if (spriteInEditor == null)
                    spriteInEditor = gameObject.AddComponent<SpriteRenderer>();

                //set sprite weapon
                spriteInEditor.sprite = weaponPrefab ? weaponPrefab.WeaponSprite : null;
            }
        }

        void Awake()
        {
            //remove sprite renderer in game
            if (spriteInEditor)
                Destroy(spriteInEditor);

            if (weaponPrefab)
            {
                //instantiate weapon and save prefab
                instantiatedWeapon = Instantiate(weaponPrefab, transform.position, transform.rotation);
                instantiatedWeapon.WeaponPrefab = weaponPrefab;

                //add event
                instantiatedWeapon.onPickWeapon += OnPickWeapon;
            }
        }

        private void OnDestroy()
        {
            //remove event
            if (instantiatedWeapon)
                instantiatedWeapon.onPickWeapon -= OnPickWeapon;
        }

        void OnPickWeapon()
        {
            //destroy this object on pick
            Destroy(gameObject);
        }
    }
}