using UnityEngine;

public class DeactiveInMultiplayer : MonoBehaviour
{
    [Header("Default use this gameObject")]
    [SerializeField] GameObject objectToDeactive = default;

    private void Start()
    {
        if (objectToDeactive == null)
            objectToDeactive = gameObject;

        //when online, deactive object
        if (NetworkManager.instance)
            objectToDeactive.SetActive(false);
    }
}
