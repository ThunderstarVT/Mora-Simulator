using System;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float power = 5000f;

    private void Start()
    {
        foreach (PhysicsObject physicsObject in FindObjectsByType<PhysicsObject>(FindObjectsSortMode.None))
        {
            physicsObject.AddExplosionForce(power, transform.position);
        }
        
        Destroy(gameObject);
    }
}
