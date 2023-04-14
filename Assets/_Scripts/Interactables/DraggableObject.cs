using Fusion;
using System.Linq;
using UnityEngine;

public class DraggableObject : NetworkBehaviour
{
    [SerializeField] int score = 10;
    [SerializeField] PhysicMaterial phMaterialDefault = default;
    [SerializeField] PhysicMaterial phMaterialOnPick = default;

    public int Score => score;

    bool isPicked = false;
    Transform previousParent;

    //rigidbody vars
    float mass, drag, angularDrag;
    bool useGravity, isKinematic;
    RigidbodyInterpolation interpolation;
    CollisionDetectionMode collisionDetection;
    RigidbodyConstraints constraints;

    private void Awake()
    {
        //set default physics material
        foreach (Collider col in GetComponentsInChildren<Collider>()) col.material = phMaterialDefault;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPCPick(PlayerRef player)
    {
        Pick(GameManager.usersInScene.FirstOrDefault((x) => x.Object.InputAuthority == player).GetComponent<PlayerController>().CurrentPawn.transform);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPCDrop()
    {
        GetComponent<NetworkTransform>().enabled = false;
        transform.parent = previousParent;
        foreach (Collider col in GetComponentsInChildren<Collider>()) col.material = phMaterialDefault;
        RecreateRigidbody();
    }

    public bool TryPick(DragComponent character)
    {
        if (isPicked == false)
        {
            isPicked = true;
            if (NetworkManager.instance)
                RPCPick(character.GetComponent<PlayerPawn>().CurrentController.GetComponent<User>().Object.InputAuthority);
            else
                Pick(character.transform);

            return true;
        }

        return false;
    }

    public bool TryDrop()
    {
        if (isPicked)
        {
            isPicked = false;
            RPCDrop();

            return true;
        }

        return false;
    }

    void Pick(Transform character)
    {
        GetComponent<NetworkTransform>().enabled = false;
        previousParent = transform.parent;
        transform.parent = character;
        foreach (Collider col in GetComponentsInChildren<Collider>()) col.material = phMaterialOnPick;
        DestroyRigidbody();                     //destroy rigidbody to move with character
    }

    #region rigidbody

    void DestroyRigidbody()
    {
        //save vars
        Rigidbody rb = GetComponent<Rigidbody>();
        mass = rb.mass;
        drag = rb.drag;
        angularDrag = rb.angularDrag;
        useGravity = rb.useGravity;
        isKinematic = rb.isKinematic;
        interpolation = rb.interpolation;
        collisionDetection = rb.collisionDetectionMode;
        constraints = rb.constraints;

        //and remove rigidbody
        Destroy(rb);
    }

    void RecreateRigidbody()
    {
        //add rigidbody
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();

        //and set with previous vars
        rb.mass = mass;
        rb.drag = drag;
        rb.angularDrag = angularDrag;
        rb.useGravity = useGravity;
        rb.isKinematic = isKinematic;
        rb.interpolation = interpolation;
        rb.collisionDetectionMode = collisionDetection;
        rb.constraints = constraints;
    }

    #endregion
}
