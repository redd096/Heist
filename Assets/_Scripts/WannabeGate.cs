using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WannabeGate : MonoBehaviour
{
    [Header("Ignore objects with these tags or without it?")]
    [SerializeField] bool ignoreObjectsWithTheseTags = true;
    [SerializeField] List<string> tags = new List<string>() { "TriggerZone" };

    [Header("Open when something is in trigger, or close?")]
    [SerializeField] bool openWhenInTrigger = true;
    public GameObject gate;

    List<Collider> collidersInTrigger = new List<Collider>();
    List<WannabeGate> wannabeGates = new List<WannabeGate>();

    private void Awake()
    {
        //add to the list every other pressure plate that activate this gate
        foreach (WannabeGate wannabeGate in FindObjectsOfType<WannabeGate>())
        {
            if (wannabeGate.gate == gate)
                wannabeGates.Add(wannabeGate);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //add to the list and try activate gate
        if (IsCorrectObject(other))
        {
            if (collidersInTrigger.Contains(other) == false) collidersInTrigger.Add(other);
            TryActivate();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //remove from the list and try activate gate
        if (IsCorrectObject(other))
        {
            if (collidersInTrigger.Contains(other)) collidersInTrigger.Remove(other);
            TryActivate();
        }
    }

    bool IsCorrectObject(Collider other)
    {
        if (ignoreObjectsWithTheseTags)
            return tags.Contains(other.tag) == false;   //is correct if it's tag is NOT in the list
        else
            return tags.Contains(other.tag);            //is correct if it's tag is in the list
    }

    bool CheckThereIsSomethingInTrigger()
    {
        //check at least one pressure plate has something on it
        foreach (WannabeGate wannabeGate in wannabeGates)
            if (wannabeGate.collidersInTrigger.Count > 0)
                return true;

        return false;
    }

    private void TryActivate()
    {
        //check if something is on trigger. False to open, True to close
        if(CheckThereIsSomethingInTrigger())
            gate.SetActive(openWhenInTrigger == false);
        else
            gate.SetActive(openWhenInTrigger);
    }
}
