using System;
using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    public static List<PhysicsObject> Instances { get; protected set; } = new();

    [Header("Required Components (PhysicsObject)")]
    [SerializeField] protected Rigidbody rb;
    
    [Header("Settings (PhysicsObject)")]
    [SerializeField] protected List<Collider> colliders;

    private void Start()
    {
        Instances.Add(this);
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }


    public virtual void AddExplosionForce(float power, Vector3 origin)
    {
        Vector3 com = transform.position + transform.rotation * rb.centerOfMass;
        
        Vector3 toObject = com - origin;
        float dist = toObject.magnitude;
        Vector3 direction = toObject.normalized;
        
        float falloff = 1 / (dist * dist + 1);
        
        rb.linearVelocity += power * direction * falloff / rb.mass;
    }

    /// <summary>
    /// Applies a buoyant force that is as accurate as I could make it.
    /// </summary>
    /// <param name="volume">the AABB of the fluid</param>
    /// <param name="density">the fluid's density</param>
    /// <param name="velocity">the fluid's velocity</param>
    public virtual void AddBuoyantForceAndDrag(Bounds volume, float density, Vector3 velocity)
    {
        // get all colliders whose bounds intersect with the fluid AABB
        List<Collider> interColliders = colliders.Where(c => c.bounds.Intersects(volume)).ToList();

        // if no colliders intersect, return early to save performance
        if (interColliders.Count == 0) return;

        // get the bounds around all colliders whose bounds intersect with the fluid AABB
        Bounds colliderBounds = interColliders.Aggregate(interColliders[0].bounds, (b, c) => { b.Encapsulate(c.bounds); return b; });

        // get the AABB of the intersection of the fluid and the colliders' bounds
        Bounds intersectBounds = new Bounds();
        intersectBounds.SetMinMax(
            new Vector3(
                Mathf.Max(volume.min.x, colliderBounds.min.x),
                Mathf.Max(volume.min.y, colliderBounds.min.y),
                Mathf.Max(volume.min.z, colliderBounds.min.z)),
            new Vector3(
                Mathf.Min(volume.max.x, colliderBounds.max.x),
                Mathf.Min(volume.max.y, colliderBounds.max.y),
                Mathf.Min(volume.max.z, colliderBounds.max.z)));
        
        // create sample points
        List<Vector3> samplePoints = Enumerable.Range(0, SettingsManager.Instance.BuoyancySamples).Select(_ => 
            new Vector3(
                Random.Range(intersectBounds.min.x, intersectBounds.max.x), 
                Random.Range(intersectBounds.min.y, intersectBounds.max.y), 
                Random.Range(intersectBounds.min.z, intersectBounds.max.z))
        ).ToList();
        
        // count how many are in any collider
        List<Vector3> samplesInside = samplePoints.Where(s => interColliders.Any(c => c.ClosestPoint(s) == s)).ToList();
        
        // calculate the overlapping volume
        float overlapVolume = BoundsVolume(intersectBounds) * samplesInside.Count / samplePoints.Count;
        
        // calculate buoyant force and centre of volume
        Vector3 buoyantForce = -overlapVolume * density * Physics.gravity;
        Vector3 cov = samplesInside.Aggregate(Vector3.zero, (sum, sample) => sum + sample) / samplesInside.Count; // center of volume (estimated as average of samples inside collider)
        
        // get the velocity relative to the fluid
        Vector3 relativeVelocity = rb.linearVelocity - velocity;
            
        // estimate the drag force
        Vector3 dragForce = relativeVelocity * (-density * relativeVelocity.magnitude * Mathf.Pow(overlapVolume, 2f/3f));
            
        rb.AddForceAtPosition(buoyantForce + dragForce, cov);
    }

    protected float BoundsVolume(Bounds volume)
    {
        return volume.size.x * volume.size.y * volume.size.z;
    }
}
