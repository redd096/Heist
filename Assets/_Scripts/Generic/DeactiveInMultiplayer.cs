using UnityEngine;

public class DeactiveInMultiplayer : MonoBehaviour
{
    [Header("Default use this gameObject")]
    [SerializeField] GameObject objectToDeactive = default;

    [Header("Deactive only if client or also server?")]
    [SerializeField] bool deactiveOnlyClient = false;

    private void Start()
    {
        if (objectToDeactive == null)
            objectToDeactive = gameObject;

        //check if deactive only client
        if (NetworkManager.instance && NetworkManager.instance.Runner && NetworkManager.instance.Runner.IsServer)
        {
            if (deactiveOnlyClient)
                return;
        }

        //when online, deactive object
        if (NetworkManager.instance)
            objectToDeactive.SetActive(false);
    }
}
