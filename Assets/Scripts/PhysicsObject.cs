using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public virtual void AddBuoyantForce(Bounds volume, float density)
    {
        //TODO: remove once fully implemented
        Debug.LogException(new NotImplementedException(), this);

        if (colliders.Any(c => c.GetType() == typeof(MeshCollider)))
        {
            Debug.LogError("Buoyancy calculation doesn't support mesh colliders!");
        }
        
        List<Collider> interColliders = colliders.Where(c => c.bounds.Intersects(volume)).ToList();

        if (interColliders.Count == 0) return;

        for (int i = 0; i < interColliders.Count; i++)
        {
            // add intersect

            for (int j = i + 1; j < interColliders.Count; j++)
            {
                if (!interColliders[i].bounds.Intersects(interColliders[j].bounds)) continue;
                
                // remove double-counted
            }
        }
        
        Vector3 force = -0f * density * Physics.gravity; // volume * density * gravity (* scalar if needed)
        Vector3 cov = transform.position; // center of volume
        
        rb.AddForceAtPosition(force, cov);
    }

    protected float Overlap(Bounds volume, Collider collider)
    {
        Bounds unionBounds = new Bounds();
        unionBounds.SetMinMax(
            new Vector3(
                Mathf.Max(volume.min.x, collider.bounds.min.x),
                Mathf.Max(volume.min.y, collider.bounds.min.y),
                Mathf.Max(volume.min.z, collider.bounds.min.z)),
            new Vector3(
                Mathf.Min(volume.max.x, collider.bounds.max.x),
                Mathf.Min(volume.max.y, collider.bounds.max.y),
                Mathf.Min(volume.max.z, collider.bounds.max.z)));
        
        if (collider.GetType() == typeof(BoxCollider))
        {
            float f = BoundsVolume(new Bounds(Vector3.zero, ((BoxCollider) collider).size)) / BoundsVolume(collider.bounds);
            return BoundsVolume(unionBounds) * f;
        }

        if (collider.GetType() == typeof(CapsuleCollider))
        {
            CapsuleCollider capsule = (CapsuleCollider) collider;
            
            float capsuleVolume = MathF.PI * capsule.radius * capsule.radius * (capsule.height - capsule.radius * 2f/3f);
            
            float f = capsuleVolume / BoundsVolume(collider.bounds);
            return BoundsVolume(unionBounds) * f;
        }

        if (collider.GetType() == typeof(SphereCollider))
        {
            return BoundsVolume(unionBounds) * Mathf.PI/6f;
        }

        return 0;
    }

    private float BoundsVolume(Bounds volume)
    {
        return volume.size.x * volume.size.y * volume.size.z;
    }
}
