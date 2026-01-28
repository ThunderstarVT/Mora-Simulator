using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    [Header("Required Components (PhysicsObject)")]
    [SerializeField] protected Rigidbody rb;

    public virtual void AddExplosionForce(float power, Vector3 origin)
    {
        Vector3 com = transform.position + transform.rotation * rb.centerOfMass;
        
        Vector3 toObject = com - origin;
        float dist = toObject.magnitude;
        Vector3 direction = toObject.normalized;
        
        float falloff = 1 / (dist * dist);
        
        rb.linearVelocity += power * direction * falloff / rb.mass;
    }
}
