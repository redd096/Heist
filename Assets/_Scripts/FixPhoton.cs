using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixPhoton : MonoBehaviour
{
    [SerializeField] NetworkPrefabRef selfPrefab = default;

    private void Awake()
    {
        //online
        if (NetworkManager.instance)
        {
            NetworkManager.instance.OnSceneLoadStartCallback += OnSceneLoadStartCallback;
        }
    }

    private void OnDestroy()
    {
        //online
        if (NetworkManager.instance)
        {
            NetworkManager.instance.OnSceneLoadStartCallback -= OnSceneLoadStartCallback;
        }
    }

    void OnSceneLoadStartCallback(NetworkRunner runner)
    {
        if (NetworkManager.instance.Runner.IsServer)
        {
            NetworkManager.instance.Runner.Spawn(selfPrefab, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}
