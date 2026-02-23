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
            if (collision.impulse.magnitude > impulseThreshold) OnBreakEvent?.Invoke();
        };

        physicsObject.OnExplosionEvent += power =>
        {
            if (power > explosionThreshold) OnBreakEvent?.Invoke();
        };
    }
}
