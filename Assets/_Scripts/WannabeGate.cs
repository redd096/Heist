using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WannabeGate : MonoBehaviour
{
    public GameObject gate;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag != "TriggerZone")
        {
            gate.SetActive(false);
        }
        else
            gate.SetActive(true);
    }
}
