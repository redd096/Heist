using System.Collections.Generic;
using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [AddComponentMenu("redd096/.GameTopDown2D/Interactables/Exit Interactable")]
    public class ExitInteractable : MonoBehaviour, IInteractable
    {
        [Header("Rules to Open")]
        [Tooltip("Check there are no enemies in scene")][SerializeField] bool checkNoEnemiesInScene = true;
        [Tooltip("Check every player has weapon")][SerializeField] bool checkEveryPlayerHasWeapon = true;

        [Header("On Interact")]
        [SerializeField][Scene] string sceneToLoad = default;

        [Header("DEBUG")]
        /*[ShowNonSerializedField]*/[SerializeField][ReadOnly] bool isOpen;

        [Button("Force Exit", ButtonAttribute.EEnableType.PlayMode)] public void ForceExit() => ChangeExitState();

        public bool IsOpen => isOpen;
        public string SceneToLoad => sceneToLoad;

        //events
        public System.Action onOpen { get; set; }
        public System.Action onClose { get; set; }
        public System.Action<ExitInteractable> onInteract { get; set; }

        //necessary for checks
        List<SpawnableObject> enemies = new List<SpawnableObject>();
        List<Character> players = new List<Character>();

        void OnEnable()
        {
            ActiveExit();
        }

        void OnDisable()
        {
            DeactiveExit();
        }

        #region public API

        /// <summary>
        /// Level Manager call it to activate (become interactable and start check if can open)
        /// </summary>
        public void ActiveExit()
        {
            Character[] charactersInScene = FindObjectsOfType<Character>();

            //register to every enemy death
            SpawnableObject spawnableObject;
            foreach (Character enemy in charactersInScene)
            {
                if (enemy.CharacterType == Character.ECharacterType.AI)
                {
                    spawnableObject = enemy.gameObject.AddComponent<SpawnableObject>();
                    spawnableObject.onDeactiveObject += OnEnemyDie;
                    enemies.Add(spawnableObject);   //and add to the list
                }
            }

            //register to every player change weapon
            foreach (Character player in charactersInScene)
            {
                if (player.CharacterType == Character.ECharacterType.Player)
                {
                    if (player.GetSavedComponent<WeaponComponent>())
                    {
                        player.GetSavedComponent<WeaponComponent>().onChangeWeapon += OnPlayerChangeWeapon;
                        players.Add(player);        //and add to the list
                    }
                }
            }

            //do checks
            DoCheck();
        }

        /// <summary>
        /// Level Manager call it to deactive (is not interactable and doesn't do checks)
        /// </summary>
        public void DeactiveExit()
        {
            //unregister from every enemy
            foreach (SpawnableObject enemy in enemies)
            {
                if (enemy)
                    enemy.onDeactiveObject -= OnEnemyDie;
            }
            enemies.Clear();

            //unregister from every player
            foreach (Character player in players)
            {
                if (player && player.GetSavedComponent<WeaponComponent>())
                    player.GetSavedComponent<WeaponComponent>().onChangeWeapon -= OnPlayerChangeWeapon;
            }
            players.Clear();
        }

        /// <summary>
        /// When someone interact with this object
        /// </summary>
        /// <param name="whoInteract"></param>
        public void Interact(InteractComponent whoInteract)
        {
            //only if is open
            if (isOpen)
            {
                //stop this script
                DeactiveExit();     //stop check open/close
                isOpen = false;     //can't interact anymore
                enabled = false;    //to be sure

                //call event
                onInteract?.Invoke(this);

                //change scene
                SceneLoader.instance.LoadScene(sceneToLoad);
            }
        }

        #endregion

        #region events

        void OnEnemyDie(SpawnableObject enemy)
        {
            //when an enemy die, remove from the list
            if (enemies.Contains(enemy))
                enemies.Remove(enemy);

            //do checks
            DoCheck();
        }

        void OnPlayerChangeWeapon()
        {
            //when a player change weapon, do checks
            DoCheck();
        }

        #endregion

        #region private API

        void DoCheck()
        {
            //check if can open, and if necessary change state
            if (isOpen != CheckCanOpen())
                ChangeExitState();
        }

        bool CheckCanOpen()
        {
            bool canOpen = true;

            //check there are not enemies in scene
            if (canOpen && checkNoEnemiesInScene)
            {
                canOpen = enemies.Count <= 0;
            }

            //check every player has weapon equipped
            if (canOpen && checkEveryPlayerHasWeapon)
            {
                foreach (Character player in players)
                {
                    //if someone has no weapon, can't open
                    if (player.GetSavedComponent<WeaponComponent>() == null || player.GetSavedComponent<WeaponComponent>().CurrentWeapon == null)
                    {
                        canOpen = false;
                        break;
                    }
                }
            }

            return canOpen;
        }

        void ChangeExitState()
        {
            //open or close
            isOpen = !isOpen;

            //call event
            if (isOpen)
                onOpen?.Invoke();
            else
                onClose?.Invoke();
        }

        #endregion
    }
}