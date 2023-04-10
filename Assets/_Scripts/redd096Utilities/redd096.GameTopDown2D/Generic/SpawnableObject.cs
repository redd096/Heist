using UnityEngine;

namespace redd096.GameTopDown2D
{
    /// <summary>
    /// This script will be added by ExitInteractable to know when someone is dead
    /// </summary>
    [AddComponentMenu("redd096/.GameTopDown2D/Generic/Spawnable Object")]
    public class SpawnableObject : MonoBehaviour
    {
        public System.Action<SpawnableObject> onDeactiveObject { get; set; }

        void OnDisable()
        {
            onDeactiveObject?.Invoke(this);
        }
    }
}