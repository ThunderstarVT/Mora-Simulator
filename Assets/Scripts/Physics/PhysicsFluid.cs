using System.Collections.Generic;
using UnityEngine;

public class PhysicsFluid : MonoBehaviour
{
    public static List<PhysicsFluid> Instances { get; } = new();
    
    [SerializeField] private Bounds volume;
    public Bounds Volume => volume;
    
    [SerializeField] private float density;
    [SerializeField] private Vector3 velocity;

    private void Start()
    {
        Instances.Add(this);
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }
    
    private void FixedUpdate()
    {
        foreach (PhysicsObject physicsObject in PhysicsObject.Instances)
        {
            physicsObject.AddBuoyantForceAndDrag(volume, density, velocity);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 1f);
        Gizmos.DrawWireCube(volume.center, volume.size);
        
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
        Gizmos.DrawCube(volume.center, volume.size);
    }
}
