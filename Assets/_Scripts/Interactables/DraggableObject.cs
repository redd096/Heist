using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    [SerializeField] int score = 10;
    [SerializeField] PhysicMaterial phMaterialDefault = default;
    [SerializeField] PhysicMaterial phMaterialOnPick = default;
    [SerializeField] Outline outline = default;

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

    public bool Pick(DragComponent character)
    {
        if (isPicked == false)
        {
            isPicked = true;
            previousParent = transform.parent;
            transform.parent = character.transform;
            foreach (Collider col in GetComponentsInChildren<Collider>()) col.material = phMaterialOnPick;
            DestroyRigidbody();                     //destroy rigidbody to move with character
            return true;
        }

        return false;
    }

    public bool Drop()
    {
        if (isPicked)
        {
            isPicked = false;
            transform.parent = previousParent;
            foreach (Collider col in GetComponentsInChildren<Collider>()) col.material = phMaterialDefault;
            RecreateRigidbody();
            return true;
        }

        return false;
    }

    public void Highlight(bool isHighlighted, User user = null)
    {
        outline.enabled = isHighlighted;
        if(user != null)
            outline.OutlineColor = user.PlayerColor;
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
