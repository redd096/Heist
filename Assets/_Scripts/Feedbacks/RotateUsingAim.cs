using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateUsingAim : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 20;

    [Header("Necessary Components - default get in parent")]
    [SerializeField] AimComponent aimComponent = default;

    private void Update()
    {
        //start only if there are all necessary components
        if (CheckComponents() == false)
            return;

        Rotate();
    }


    void Rotate()
    {
        Quaternion lookRotation = Quaternion.LookRotation(aimComponent.AimDirectionInput, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    #region private API

    bool CheckComponents()
    {
        //check if have components
        if (aimComponent == null)
            aimComponent = GetComponentInParent<AimComponent>();

        //if movement mode is rigidbody, be sure to have a rigidbody
        if (aimComponent == null)
        {
            Debug.LogWarning($"Miss AimComponent on {name}");
            return false;
        }

        return true;
    }

    #endregion
}
