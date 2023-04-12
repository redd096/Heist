using UnityEngine;
using redd096.StateMachine.StateMachineRedd096;

public class RotateByInput : ActionTask
{
    public float speed;
    public bool doJiggle;

    public RotateUsingAim stronzo;
    [SerializeField] AimComponent aimComponent = default;

    [Header("Necessary Components - default get in parent")]
    [SerializeField] Rigidbody movementComponent = default;
    [SerializeField] PlayerPawn pawn = default;

    [Header("Rotation")]
    [SerializeField] string inputName = "Rotate";

    protected override void OnInitTask()
    {
        base.OnInitTask();

        //set references
        if (movementComponent == null) movementComponent = GetComponentInParent<Rigidbody>();
        if (pawn == null) pawn = GetStateMachineComponent<PlayerPawn>();
    }

    public void FixedUpdate()
    {
        if (!IsTaskActive)
            return;

        if (movementComponent == null || pawn == null || pawn.CurrentController == null)
            return;

        var value = Vector3.SignedAngle(-pawn.transform.right, aimComponent.AimDirectionInput, Vector3.up);
        
        if(doJiggle)
            movementComponent.angularVelocity = Vector3.zero;

        movementComponent.AddTorque(speed * value * Vector3.up);        
    }

    protected override void OnEnterTask()
    {
        base.OnEnterTask();

        movementComponent.centerOfMass = Vector3.zero;
        stronzo.enabled = false;
    }

    protected override void OnExitTask()
    {
        base.OnEnterTask();

        stronzo.enabled = true;
    }
}
