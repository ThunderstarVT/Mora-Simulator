using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float power = 5000f;

    private void Start()
    {
        foreach (PhysicsObject physicsObject in PhysicsObject.Instances)
        {
            physicsObject.AddExplosionForce(power, transform.position);
        }
        
        Destroy(gameObject);
    }
}
