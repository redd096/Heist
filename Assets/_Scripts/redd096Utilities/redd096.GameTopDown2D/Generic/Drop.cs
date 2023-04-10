using UnityEngine;
using redd096.Attributes;

namespace redd096.GameTopDown2D
{
    [System.Serializable]
    public struct DropStruct
    {
        public GameObject DropPrefab;
        [Range(0, 100)] public int PercentageDrop;
    }

    [AddComponentMenu("redd096/.GameTopDown2D/Generic/Drop")]
    public class Drop : MonoBehaviour
    {
        [Header("Drop on Die or when deactivated?")]
        [SerializeField] bool dropOnDie = true;
        [SerializeField] bool dropOnDeactivate = false;

        [Header("Drop")]
        [ReadOnly][SerializeField] int totalPercentage = 0;
        [SerializeField] DropStruct[] drops = default;

        SpawnableObject spawnableObject;
        HealthComponent healthComponent;

        void OnValidate()
        {
            //editor - show total percentage
            totalPercentage = 0;
            if (drops != null && drops.Length > 0)
            {
                foreach (DropStruct dropStruct in drops)
                    totalPercentage += dropStruct.PercentageDrop;
            }
        }

        void OnEnable()
        {
            //get references
            spawnableObject = GetComponent<SpawnableObject>();
            healthComponent = GetComponent<HealthComponent>();

            //add events
            if (spawnableObject)
            {
                spawnableObject.onDeactiveObject += OnDeactiveObject;
            }
            if (healthComponent)
            {
                healthComponent.onDie += OnDie;
            }
        }

        void OnDisable()
        {
            //remove events
            if (spawnableObject)
            {
                spawnableObject.onDeactiveObject -= OnDeactiveObject;
            }
            if (healthComponent)
            {
                healthComponent.onDie -= OnDie;
            }
        }

        #region events

        void OnDie(HealthComponent whoDied, Character whoHit)
        {
            //instantiate on Die
            if (dropOnDie)
            {
                InstantiateDrop();
            }
        }

        void OnDeactiveObject(SpawnableObject deactivatedObject)
        {
            //instantiate on deactivate
            if (dropOnDeactivate)
            {
                InstantiateDrop();
            }
        }

        #endregion

        void InstantiateDrop()
        {
            //get random value from 0 to 100
            int random = Mathf.RoundToInt(Random.value * 100);

            //cycle every drop
            int percentage = 0;
            for (int i = 0; i < drops.Length; i++)
            {
                //if reach percentage, instantiate
                percentage += drops[i].PercentageDrop;
                if (percentage >= random)
                {
                    if (drops[i].DropPrefab)
                    {
                        Instantiate(drops[i].DropPrefab, transform.position, transform.rotation);
                    }

                    break;
                }
            }
        }
    }
}