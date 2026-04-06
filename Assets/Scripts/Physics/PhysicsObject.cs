using System;
using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(NameHaver))]
public class PhysicsObject : MonoBehaviour
{
    public static List<PhysicsObject> Instances { get; } = new();

    [Header("Required Components (PhysicsObject)")]
    public Rigidbody rb;
    [SerializeField] private AudioSource collisionAudioSource;
    [SerializeField] private AudioClip collisionAudioClip;
    [SerializeField] private float preLogVolumeMultiplier = 0.005f;
    
    [Header("Settings (PhysicsObject)")]
    [SerializeField] protected List<Collider> colliders;
    public List<Collider> Colliders => colliders;

    private void Start()
    {
        Instances.Add(this);

        if (collisionAudioSource)
        {
            OnCollisionEnterEvent += collision =>
            {
                float reducedMass = rb.mass;

                if (collision.rigidbody)
                {
                    reducedMass = (rb.mass * collision.rigidbody.mass) /
                                  (rb.mass + collision.rigidbody.mass);
                }
            
                float impulse = reducedMass * Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal);

                float volume = Mathf.Log(1.0f + impulse * preLogVolumeMultiplier);

                if (collisionAudioClip)
                {
                    collisionAudioSource.PlayOneShot(collisionAudioClip, volume);
                    Debug.Log(volume);
                }
            };
        }
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }


    public virtual Vector3 GetCenter()
    {
        return rb.worldCenterOfMass;
    }

    public virtual Vector3 GetVelocity()
    {
        return rb.linearVelocity;
    }


    public virtual void AddExplosionForce(float power, Vector3 origin)
    {
        Vector3 com = rb.worldCenterOfMass;
        
        Vector3 toObject = com - origin;
        float dist = toObject.magnitude;
        Vector3 direction = toObject.normalized;
        
        float falloff = 1 / (dist * dist + 1);
        
        rb.linearVelocity += power * direction * falloff / rb.mass;
        
        OnExplosionEvent?.Invoke(power * falloff);
    }

    public virtual void AddAcceleration(Vector3 acceleration)
    {
        rb.AddForce(acceleration * rb.mass);
    }
    
    public virtual void AddImpulseAtPoint(Vector3 impulse, Vector3 point)
    {
        Vector3 closestPoint = colliders.Aggregate(Vector3.positiveInfinity, (current, c) => 
            Vector3.Distance(point, c.ClosestPoint(point)) < Vector3.Distance(point, current) ? c.ClosestPoint(point) : current);
        
        
        
        rb.AddForceAtPosition(impulse, closestPoint, ForceMode.Impulse);
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
        
        // return if no samples inside
        if (samplesInside.Count == 0) return;
        
        // calculate the overlapping volume
        float overlapVolume = BoundsVolume(intersectBounds) * samplesInside.Count / samplePoints.Count;
        
        // calculate buoyant force and centre of volume
        Vector3 buoyantForce = -overlapVolume * density * Physics.gravity;
        Vector3 cov = samplesInside.Aggregate(Vector3.zero, (sum, sample) => sum + sample) / samplesInside.Count; // center of volume (estimated as average of samples inside collider)
        
        // calculate velocity at centre of volume
        Vector3 covVelocity = rb.GetPointVelocity(cov);
        
        // get the velocity relative to the fluid
        Vector3 relativeVelocity = covVelocity - velocity;
            
        // estimate the drag force
        Vector3 dragForce = relativeVelocity * (-density * relativeVelocity.magnitude * Mathf.Pow(overlapVolume, 2f/3f));
            
        rb.AddForceAtPosition(buoyantForce + dragForce, cov);
    }

    protected float BoundsVolume(Bounds volume)
    {
        return volume.size.x * volume.size.y * volume.size.z;
    }


    protected virtual void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterEvent?.Invoke(collision);
    }


    public event Action<Collision> OnCollisionEnterEvent;

    public event Action<float> OnKickedEvent;
    public void OnKickedEventInvoke(float impulse) => OnKickedEvent?.Invoke(impulse);
    
    public event Action<float> OnExplosionEvent;
    protected void OnExplosionEventInvoke(float power) => OnExplosionEvent?.Invoke(power);
}
