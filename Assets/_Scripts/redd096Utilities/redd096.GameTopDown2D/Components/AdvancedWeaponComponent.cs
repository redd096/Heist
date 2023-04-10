using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Components/Advanced Weapon Component")]
    public class AdvancedWeaponComponent : WeaponComponent
    {
        #region struct
        [System.Serializable]
        public class AmmoStruct
        {
            [Dropdown("GetAllAmmoTypes")] public string AmmoType;
            public int Quantity;

            public AmmoStruct(string ammoType, int quantity)
            {
                AmmoType = ammoType;
                Quantity = quantity;
            }

#if UNITY_EDITOR

            string[] GetAllAmmoTypes()
            {
                //get guid to every ammo in project
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:prefab");
                List<string> values = new List<string>();

                //return array with loaded assets
                Ammo ammo;
                for (int i = 0; i < guids.Length; i++)
                {
                    ammo = UnityEditor.AssetDatabase.LoadAssetAtPath<Ammo>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]));
                    if (ammo) values.Add(ammo.AmmoType);
                }

                return values.Count > 0 ? values.ToArray() : new string[1] { "NO AMMO IN PROJECT" };
            }
#endif
        }

        [System.Serializable]
        public class DebugAmmoStruct
        {
            public string AmmoType;
            public int Quantity;

            public DebugAmmoStruct(string ammoType, int quantity)
            {
                AmmoType = ammoType;
                Quantity = quantity;
            }
        }
        #endregion

        [Header("Ammo Limits and Default Ammos")]
        [SerializeField] AmmoStruct[] defaultLimitAmmos = default;
        [SerializeField] List<AmmoStruct> defaultAmmos = new List<AmmoStruct>();

        [Header("DEBUG")]
        [ReadOnly][SerializeField] List<DebugAmmoStruct> CurrentLimitAmmosDebug = new List<DebugAmmoStruct>();
        [ReadOnly][SerializeField] List<DebugAmmoStruct> CurrentAmmosDebug = new List<DebugAmmoStruct>();

        //events
        public System.Action onAddAmmo { get; set; }
        public System.Action onSetLimitAmmo { get; set; }

        //ammos - type, number
        Dictionary<string, int> currentLimitAmmos = new Dictionary<string, int>();
        Dictionary<string, int> currentAmmos = new Dictionary<string, int>();

        /// <summary>
        /// Used to read and set immediatly. In game is better to use public functions like GetCurrentAmmo or AddAmmo
        /// </summary>
        public Dictionary<string, int> CurrentAmmos_NotSafe { get { return currentAmmos; } set { currentAmmos = value; } }

        protected override void Awake()
        {
            base.Awake();

            //set default ammo limits
            SetDefaultLimitAmmosFromEditorToDictionary();

            //set ammo from inspector, only if this is not a Player or ammos are not saved in saves manager
            //if (owner == null || owner.CharacterType != Character.ECharacterType.Player || SavesManager.CanLoadDefaultAmmos())
                SetDefaultAmmoFromEditorToDictionary();
        }

        public override void PickWeapon(WeaponBASE weapon, int index)
        {
            base.PickWeapon(weapon, index);

            //if pick range weapon, add ammo
            if (weapon is WeaponRange rangeWeapon)
            {
                AddAmmo(rangeWeapon.AmmoType, rangeWeapon.AmmoOnPick);
                rangeWeapon.AmmoOnPick = 0;         //set to 0, so if drop and pick again weapon, do not pick ammo anymore
            }
        }

        #region private API

        void SetDefaultLimitAmmosFromEditorToDictionary()
        {
            if (Application.isPlaying)
            {
                //set default ammo limits
                foreach (AmmoStruct ammo in defaultLimitAmmos)
                {
                    SetLimitAmmo(ammo.AmmoType, ammo.Quantity);
                }
            }
        }

        void SetDefaultAmmoFromEditorToDictionary()
        {
            //set default ammo
            foreach (AmmoStruct ammo in defaultAmmos)
            {
                AddAmmo(ammo.AmmoType, ammo.Quantity);
            }
        }

        void SetLimitAmmosDebug(string ammoType)
        {
            if (currentLimitAmmos.ContainsKey(ammoType) == false)
                return;

            //find in array
            for (int i = 0; i < CurrentLimitAmmosDebug.Count; i++)
            {
                //update value
                if (CurrentLimitAmmosDebug[i].AmmoType == ammoType)
                {
                    CurrentLimitAmmosDebug[i] = new DebugAmmoStruct(ammoType, currentLimitAmmos[ammoType]);
                    return;
                }
            }

            //if not found, create it
            CurrentLimitAmmosDebug.Add(new DebugAmmoStruct(ammoType, currentLimitAmmos[ammoType]));
        }

        void SetAmmosDebug(string ammoType)
        {
            if (currentAmmos.ContainsKey(ammoType) == false)
                return;

            //find in array
            for (int i = 0; i < CurrentAmmosDebug.Count; i++)
            {
                //update value
                if (CurrentAmmosDebug[i].AmmoType == ammoType)
                {
                    CurrentAmmosDebug[i] = new DebugAmmoStruct(ammoType, currentAmmos[ammoType]);
                    return;
                }
            }

            //if not found, create it
            CurrentAmmosDebug.Add(new DebugAmmoStruct(ammoType, currentAmmos[ammoType]));
        }

        #endregion

        #region public API

        /// <summary>
        /// Add ammo of type (also negative quantity)
        /// </summary>
        /// <param name="ammoType"></param>
        /// <param name="quantity"></param>
        public void AddAmmo(string ammoType, int quantity)
        {
            //create if not contains this ammo
            if (currentAmmos.ContainsKey(ammoType) == false)
                currentAmmos.Add(ammoType, 0);

            //add quantity
            currentAmmos[ammoType] += quantity;

            //if reach limit, set it. Or if lower than 0
            if (currentLimitAmmos.ContainsKey(ammoType) && currentAmmos[ammoType] >= currentLimitAmmos[ammoType])
                currentAmmos[ammoType] = currentLimitAmmos[ammoType];
            else if (currentAmmos[ammoType] < 0)
                currentAmmos[ammoType] = 0;

            //call event
            onAddAmmo?.Invoke();

#if UNITY_EDITOR
            //update ammos debug in editor
            SetAmmosDebug(ammoType);
#endif
        }

        /// <summary>
        /// Get ammo of type
        /// </summary>
        /// <param name="ammoType"></param>
        /// <returns></returns>
        public int GetCurrentAmmo(string ammoType)
        {
            if (currentAmmos.ContainsKey(ammoType))
                return currentAmmos[ammoType];

            return 0;
        }

        /// <summary>
        /// Check if reached limit of ammo for this type
        /// </summary>
        /// <param name="ammoType"></param>
        /// <returns></returns>
        public bool IsFullOfAmmo(string ammoType)
        {
            //if current ammos is equal to limit ammos
            if (currentAmmos.ContainsKey(ammoType) && currentLimitAmmos.ContainsKey(ammoType))
            {
                if (currentAmmos[ammoType] >= currentLimitAmmos[ammoType])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Set limit for this type of ammo
        /// </summary>
        /// <param name="ammoType"></param>
        public void SetLimitAmmo(string ammoType, int limitAmmo)
        {
            //create if not contains this ammo
            if (currentLimitAmmos.ContainsKey(ammoType) == false)
                currentLimitAmmos.Add(ammoType, 0);

            //set limit
            currentLimitAmmos[ammoType] = limitAmmo;

            //call event
            onSetLimitAmmo?.Invoke();

#if UNITY_EDITOR
            //update limit ammos debug in editor
            SetLimitAmmosDebug(ammoType);
#endif
        }

        /// <summary>
        /// Get limit for this type of ammo
        /// </summary>
        /// <param name="ammoType"></param>
        /// <returns></returns>
        public int GetCurrentLimitAmmo(string ammoType)
        {
            if (currentLimitAmmos.ContainsKey(ammoType))
                return currentLimitAmmos[ammoType];

            return 0;
        }

        #endregion
    }
}