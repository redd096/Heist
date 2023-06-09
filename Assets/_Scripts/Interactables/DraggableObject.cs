using System.Collections;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    [SerializeField] int score = 10;
    [SerializeField] PhysicMaterial phMaterialDefault = default;
    [SerializeField] PhysicMaterial phMaterialOnPick = default;
    [SerializeField] Outline outline = default;

    private Coroutine coroutine;

    public int Score => score;

    public System.Action onPick { get; set; }
    public System.Action onDrop { get; set; }
    public System.Action onThrow { get; set; }

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
            if(coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            previousParent = transform.parent;
            transform.parent = character.transform;
            foreach (Collider col in GetComponentsInChildren<Collider>()) col.material = phMaterialOnPick;
            DestroyRigidbody();                     //destroy rigidbody to move with character

            onPick?.Invoke();

            return true;
        }

        return false;
    }

    public bool Drop(bool isThrowed)
    {
        if (isPicked)
        {
            isPicked = false;
            transform.parent = previousParent;
            foreach (Collider col in GetComponentsInChildren<Collider>()) col.material = phMaterialDefault;
            var rb = RecreateRigidbody();
            rb.isKinematic = true;

            if (isThrowed) onThrow?.Invoke();
            else onDrop?.Invoke();

            return true;
        }
        return false;
    }

    public void WaitEndOfThrow()
    {
        coroutine = StartCoroutine(CheckIsMoving());
    }

    private IEnumerator CheckIsMoving()
    {
        var rb = GetComponent<Rigidbody>();

        while (rb.velocity.magnitude == 0)
            yield return null;

        while (!rb.IsSleeping())
            yield return null;

        rb.isKinematic = true;
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

    Rigidbody RecreateRigidbody()
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
        return rb;
    }

    #endregion
}
