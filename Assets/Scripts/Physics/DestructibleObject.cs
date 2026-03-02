using System;
using UnityEngine;

[RequireComponent(typeof(PhysicsObject))]
public class DestructibleObject : MonoBehaviour
{
    [SerializeField] private PhysicsObject physicsObject;
    
    [Space]
    [SerializeField, Min(0f)] private float impulseThreshold = 20f;
    [SerializeField, Min(0f)] private float explosionThreshold = 2.5f;
    
    public event Action OnBreakEvent;

    private void Start()
    {
        physicsObject.OnCollisionEnterEvent += collision =>
        {
            float reducedMass = physicsObject.rb.mass;

            if (collision.rigidbody)
            {
                reducedMass = (physicsObject.rb.mass * collision.rigidbody.mass) /
                              (physicsObject.rb.mass + collision.rigidbody.mass);
            }
            
            float impulse = reducedMass * Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal);

            if (impulse > impulseThreshold)
            {
                OnBreakEvent?.Invoke();
                Debug.Log("Impulse: " + impulse);
            }
        };

        physicsObject.OnExplosionEvent += power =>
        {
            if (power > explosionThreshold) OnBreakEvent?.Invoke();
        };

        physicsObject.OnKickedEvent += impulse =>
        {
            if (impulse > impulseThreshold) OnBreakEvent?.Invoke();
        };
    }
}
