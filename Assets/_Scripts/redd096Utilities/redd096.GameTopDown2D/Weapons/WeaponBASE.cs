using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Weapons/Weapon BASE")]
    public abstract class WeaponBASE : MonoBehaviour, IInteractable
    {
        [Header("Weapon BASE")]
        public string WeaponName = "Weapon Name";
        public int WeaponPrice = 10;
        [ShowAssetPreview] public Sprite WeaponSprite = default;

        [Header("DEBUG")]
        [SerializeField] bool destroyWeaponOnDrop = false;
        [ReadOnly] public Character Owner;
        [ReadOnly] public bool IsEquipped = false;
        [HideInInspector] public WeaponBASE WeaponPrefab = default;

        //events
        public System.Action onPickWeapon { get; set; }
        public System.Action onDropWeapon { get; set; }
        public System.Action onEquipWeapon { get; set; }
        public System.Action onUnequipWeapon { get; set; }

        #region public API

        /// <summary>
        /// Set owner
        /// </summary>
        /// <param name="owner"></param>
        public virtual void PickWeapon(Character owner)
        {
            Owner = owner;

            //call event
            onPickWeapon?.Invoke();
        }

        /// <summary>
        /// Remove owner
        /// </summary>
        public virtual void DropWeapon()
        {
            Owner = null;

            //be sure to reset attack vars
            ReleaseAttack();

            //call event
            onDropWeapon?.Invoke();

            //destroy weapon, if setted
            if (destroyWeaponOnDrop)
                Destroy(gameObject);
        }

        /// <summary>
        /// Call when equip weapon
        /// </summary>
        public virtual void EquipWeapon()
        {
            IsEquipped = true;
            onEquipWeapon?.Invoke();
        }

        /// <summary>
        /// Call when unequip weapon
        /// </summary>
        public virtual void UnequipWeapon()
        {
            IsEquipped = false;
            onUnequipWeapon?.Invoke();
        }

        /// <summary>
        /// Interact to pick this weapon
        /// </summary>
        /// <param name="whoInteract"></param>
        public void Interact(InteractComponent whoInteract)
        {
            //if has weapon component, call pick weapon
            WeaponComponent weaponComponent = whoInteract.GetComponent<WeaponComponent>();
            if (weaponComponent)
                weaponComponent.PickWeapon(this);
        }

        #endregion

        #region abstracts

        public abstract void PressAttack();
        public abstract void ReleaseAttack();

        #endregion
    }
}