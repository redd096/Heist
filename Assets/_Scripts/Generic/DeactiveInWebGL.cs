using UnityEngine;

public class DeactiveInWebGL : MonoBehaviour
{
    [Header("Default use this gameObject")]
    [SerializeField] GameObject objectToDeactive = default;

    private void Start()
    {
        if (objectToDeactive == null)
            objectToDeactive = gameObject;

        //when in webgl, deactive object
#if UNITY_WEBGL
        objectToDeactive.SetActive(false);
#endif
    }
}
