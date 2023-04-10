using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Components/Weapon Component")]
    public class WeaponComponent : MonoBehaviour
    {
        public enum EWeaponOnDeath { None, OnlyEquippedOne, EveryWeapon }

        [Header("Instantiate default weapons")]
        [SerializeField] WeaponBASE[] weaponsPrefabs = default;

        [Header("Number of Weapons")]
        [Min(1)][SerializeField] int maxWeapons = 2;

        [Header("Drop or Destroy Weapon On Death (necessary HealthComponent - default get from this gameObject)")]
        [SerializeField] EWeaponOnDeath dropWeaponOnDeath = EWeaponOnDeath.None;
        [SerializeField] EWeaponOnDeath destroyWeaponOnDeath = EWeaponOnDeath.EveryWeapon;
        [SerializeField] HealthComponent healthComponent = default;

        [Header("DEBUG")]
        [ReadOnly] public WeaponBASE[] CurrentWeapons = default;    //it will be always the same size of Max Weapons
        [ReadOnly] public int IndexEquippedWeapon = 0;              //it will be always the correct index, or zero

        //the equipped weapon
        public WeaponBASE CurrentWeapon => CurrentWeapons != null && IndexEquippedWeapon < CurrentWeapons.Length ? CurrentWeapons[IndexEquippedWeapon] : null;

        //events
        public System.Action onPickWeapon { get; set; }         //called at every pick
        public System.Action onDropWeapon { get; set; }         //called at every drop
        public System.Action onSwitchWeapon { get; set; }       //called when call Switch Weapon
        public System.Action onChangeWeapon { get; set; }       //called at every pick and every drop. Also when switch weapon

        protected Character owner;
        Transform _currentWeaponsParent;
        Transform CurrentWeaponsParent { get { if (_currentWeaponsParent == null) _currentWeaponsParent = new GameObject(name + "'s Weapons").transform; return _currentWeaponsParent; } }
        public Transform WeaponsParent => CurrentWeaponsParent;

        protected virtual void Awake()
        {
            //set vars
            CurrentWeapons = new WeaponBASE[maxWeapons];

            //get references
            owner = GetComponent<Character>();

            //instantiate default weapons, only if this is not a Player or weapons are not saved in saves manager
            //if (owner == null || owner.CharacterType != Character.ECharacterType.Player || SavesManager.CanLoadDefaultWeapons())
                SetDefaultWeapons();
        }

        protected virtual void OnEnable()
        {
            //get references
            if (healthComponent == null)
                healthComponent = GetComponent<HealthComponent>();

            //add events
            if (healthComponent)
            {
                healthComponent.onDie += OnDie;
            }
        }

        protected virtual void OnDisable()
        {
            //remove events
            if (healthComponent)
            {
                healthComponent.onDie -= OnDie;
            }
        }

        #region private API

        void SetDefaultWeapons()
        {
            //instantiate and equip default weapons
            if (weaponsPrefabs != null && weaponsPrefabs.Length > 0)
            {
                for (int i = 0; i < CurrentWeapons.Length; i++)
                {
                    if (i < weaponsPrefabs.Length)
                    {
                        PickWeaponPrefab(weaponsPrefabs[i]);
                    }
                    else
                        break;
                }
            }
        }

        void OnDie(HealthComponent whoDied, Character whoHit)
        {
            //clone weapons to destroy also if dropped
            WeaponBASE[] tempWeapons = CurrentWeapons.Clone() as WeaponBASE[];

            //drop equipped weapon on death
            if (dropWeaponOnDeath == EWeaponOnDeath.OnlyEquippedOne)
            {
                DropWeapon(IndexEquippedWeapon);
            }
            //or drop every weapon
            else if (dropWeaponOnDeath == EWeaponOnDeath.EveryWeapon)
            {
                if (CurrentWeapons != null)
                    for (int i = 0; i < CurrentWeapons.Length; i++)
                        DropWeapon(i);
            }

            //destroy equipped weapon on death
            if (destroyWeaponOnDeath == EWeaponOnDeath.OnlyEquippedOne)
            {
                if (tempWeapons != null && IndexEquippedWeapon < tempWeapons.Length && tempWeapons[IndexEquippedWeapon])
                    Destroy(tempWeapons[IndexEquippedWeapon]);
            }
            //or destroy every weapon
            else if (destroyWeaponOnDeath == EWeaponOnDeath.EveryWeapon)
            {
                if (tempWeapons != null)
                    for (int i = 0; i < tempWeapons.Length; i++)
                        if (tempWeapons[i])
                            Destroy(tempWeapons[i]);
            }
        }

        bool UpdateIndexEquippedWeapon()
        {
            //if there are not weapons, set index to 0
            if (CurrentWeapons == null || CurrentWeapons.Length <= 0)
            {
                //return changed weapon
                IndexEquippedWeapon = 0;
                return true;
            }

            //if current weapon is not null, keep it
            if (IndexEquippedWeapon < CurrentWeapons.Length && CurrentWeapons[IndexEquippedWeapon])
                return false;

            //else move to previous weapons - be sure to start from array length (if index is greater). Start from length instead of length -1 because for cycle start substracting 1
            if (IndexEquippedWeapon > CurrentWeapons.Length)
                IndexEquippedWeapon = CurrentWeapons.Length;

            //be sure to cycle every weapon in array
            for (int i = 0; i < CurrentWeapons.Length; i++)
            {
                IndexEquippedWeapon--;

                //if reach array limit, restart
                if (IndexEquippedWeapon < 0)
                    IndexEquippedWeapon = CurrentWeapons.Length - 1;

                //if found weapon not null
                if (IndexEquippedWeapon < CurrentWeapons.Length)
                {
                    if (CurrentWeapons[IndexEquippedWeapon])
                    {
                        //active it
                        CurrentWeapons[IndexEquippedWeapon].transform.position = transform.position;
                        CurrentWeapons[IndexEquippedWeapon].gameObject.SetActive(true);
                        CurrentWeapons[IndexEquippedWeapon].EquipWeapon();

                        //return is changing weapon
                        return true;
                    }
                }
            }

            //if not found weapon, set at 0
            IndexEquippedWeapon = 0;
            return true;
        }

        #endregion

        #region public API

        public void PickWeaponPrefab(WeaponBASE prefab, bool selectIndex = false, int index = 0)
        {
            if (prefab)
            {
                //instantiate prefab and save prefab in weapon instance
                WeaponBASE instantiatedWeapon = Instantiate(prefab);
                instantiatedWeapon.WeaponPrefab = prefab;

                //pick
                if (selectIndex)
                    PickWeapon(instantiatedWeapon, index);
                else
                    PickWeapon(instantiatedWeapon);
            }
        }

        /// <summary>
        /// Pick Weapon and set at index
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="index"></param>
        public virtual void PickWeapon(WeaponBASE weapon, int index)
        {
            if (CurrentWeapons == null || CurrentWeapons.Length <= 0)
                return;

            //if there is already a weapon equipped, drop it
            if (CurrentWeapons[index] != null)
                DropWeapon(index, false);

            //pick weapon      
            CurrentWeapons[index] = weapon;

            //set owner and parent
            if (weapon)
            {
                weapon.PickWeapon(owner);
                weapon.transform.SetParent(CurrentWeaponsParent);
                foreach (Collider2D col in weapon.GetComponentsInChildren<Collider2D>()) col.enabled = false;   //deactive colliders (necessary to not pick again when press interact)

                //if not equipped, deactive, else call is equipped on weapon
                if (index != IndexEquippedWeapon) weapon.gameObject.SetActive(false);
                else weapon.EquipWeapon();
            }

            //set index equipped weapon
            UpdateIndexEquippedWeapon();

            //call events
            onPickWeapon?.Invoke();
            onChangeWeapon?.Invoke();
        }

        /// <summary>
        /// Pick Weapon (add in an empty slot or replace equipped one with it)
        /// </summary>
        /// <param name="weapon"></param>
        public void PickWeapon(WeaponBASE weapon)
        {
            //find empty slot (or equipped one)
            int index = IndexEquippedWeapon;
            for (int i = 0; i < CurrentWeapons.Length; i++)
            {
                if (CurrentWeapons[i] == null)
                {
                    index = i;
                    break;
                }
            }

            PickWeapon(weapon, index);
        }

        /// <summary>
        /// Drop Weapon at index
        /// </summary>
        public virtual void DropWeapon(int index, bool updateIndexEquippedWeapon = true)
        {
            if (CurrentWeapons == null || index >= CurrentWeapons.Length)
                return;

            //remove owner and parent
            if (CurrentWeapons[index])
            {
                CurrentWeapons[index].DropWeapon();
                CurrentWeapons[index].transform.SetParent(null);
                foreach (Collider2D col in CurrentWeapons[index].GetComponentsInChildren<Collider2D>()) col.enabled = true;   //reactive colliders

                //if not equipped, reactive, else call that now is not equipped on weapon
                if (index != IndexEquippedWeapon) CurrentWeapons[index].gameObject.SetActive(true);
                else CurrentWeapons[index].UnequipWeapon();
            }

            //drop weapon
            CurrentWeapons[index] = null;

            //set index equipped weapon
            if (updateIndexEquippedWeapon)
                UpdateIndexEquippedWeapon();

            //call events
            onDropWeapon?.Invoke();
            onChangeWeapon?.Invoke();
        }

        /// <summary>
        /// Drop equipped weapon
        /// </summary>
        public void DropWeapon()
        {
            DropWeapon(IndexEquippedWeapon);
        }

        /// <summary>
        /// Set max number of weapons, and update array
        /// </summary>
        /// <param name="maxWeapons">Min number is 1</param>
        /// <param name="updateIndexWeapon"></param>
        public void SetMaxWeapons(int maxWeapons, bool updateIndexWeapon = true)
        {
            //min number is 1
            if (maxWeapons < 1)
                return;

            //set max weapons
            this.maxWeapons = maxWeapons;

            //copy weapons
            WeaponBASE[] weapons = new WeaponBASE[maxWeapons];
            if (CurrentWeapons != null)
            {
                for (int i = 0; i < weapons.Length; i++)
                {
                    if (i < CurrentWeapons.Length)
                        weapons[i] = CurrentWeapons[i];
                    else
                        break;
                }
            }
            CurrentWeapons = weapons;

            //set index equipped weapon
            if (updateIndexWeapon && UpdateIndexEquippedWeapon())
            {
                //if changed weapon, call event
                onChangeWeapon?.Invoke();
            }
        }

        /// <summary>
        /// Switch Weapon
        /// </summary>
        /// <param name="nextWeapon">move to next in array or previous?</param>
        public void SwitchWeapon(bool nextWeapon = true)
        {
            if (CurrentWeapons == null || CurrentWeapons.Length <= 0)
                return;

            //move to next or previous weapons
            int newIndex = IndexEquippedWeapon;
            for (int i = 0; i < CurrentWeapons.Length - 1; i++)     //weapons length -1, because one is current weapon
            {
                newIndex += (nextWeapon ? 1 : -1);

                //if reach array limit, restart
                if (newIndex >= CurrentWeapons.Length)
                    newIndex = 0;
                else if (newIndex < 0)
                    newIndex = CurrentWeapons.Length - 1;

                //if found weapon not null, set it
                if (SwitchWeaponTo(newIndex))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Switch to weapon at index
        /// </summary>
        /// <param name="index"></param>
        public bool SwitchWeaponTo(int index)
        {
            if (CurrentWeapons == null || CurrentWeapons.Length <= 0)
                return false;

            //if found weapon not null, set it
            if (index < CurrentWeapons.Length && CurrentWeapons[index])
            {
                //deactive previous weapon
                if (IndexEquippedWeapon < CurrentWeapons.Length && CurrentWeapons[IndexEquippedWeapon])
                {
                    CurrentWeapons[IndexEquippedWeapon].gameObject.SetActive(false);
                    if (CurrentWeapons[IndexEquippedWeapon].IsEquipped) CurrentWeapons[IndexEquippedWeapon].UnequipWeapon();
                }

                //and active new one
                CurrentWeapons[index].transform.position = transform.position;
                CurrentWeapons[index].gameObject.SetActive(true);
                CurrentWeapons[index].EquipWeapon();

                IndexEquippedWeapon = index;

                //call events
                onSwitchWeapon?.Invoke();
                onChangeWeapon?.Invoke();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Return if every slot in array contain a weapon
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            //return false if there a slot empty
            foreach (WeaponBASE weapon in CurrentWeapons)
            {
                if (weapon == null)
                    return false;
            }

            //else return true
            return true;
        }

        #endregion
    }
}