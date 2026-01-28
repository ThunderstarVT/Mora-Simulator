using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    [Header("Required Components (PhysicsObject)")]
    [SerializeField] protected Rigidbody rb;

    public virtual void AddExplosionForce(float force, Vector3 origin)
    {
        float distance = Vector3.Distance(origin, transform.position);
        float distScalar = 1 / (distance * distance);
        
        rb.linearVelocity += (rb.centerOfMass - origin).normalized * force * distScalar / rb.mass;
    }
}
