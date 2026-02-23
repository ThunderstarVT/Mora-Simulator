using System;
using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class Ragdoll : PhysicsObject
{
    [Header("Required Components (Ragdoll)")]
    [SerializeField] private Animator anim;
    
    
    [Header("Settings (Ragdoll)")]
    [SerializeField] private bool ragdoll;
    public bool isRagdolling => ragdoll;
    
    [Space]
    [SerializeField] private Transform root;
    [SerializeField] private List<RagdollBone> bones;
    
    [Space]
    [SerializeField, Min(0f)] private float impulseThreshold = 20f;
    [SerializeField, Min(0f)] private float explosionThreshold = 2.5f;

    
    protected void Awake()
    {
        foreach (RagdollBone bone in bones)
        {
            bone.OnCollisionEnterEvent += OnCollisionEnter;
        }
    }


    public override Vector3 GetCenter()
    {
        return bones.Aggregate(Vector3.zero, (sum, bone) => sum + bone.transform.position + bone.transform.rotation * bone.RB.centerOfMass) / bones.Count;
    }

    public override Vector3 GetVelocity()
    {
        return bones.Aggregate(Vector3.zero, (sum, bone) => sum + bone.RB.linearVelocity) / bones.Count;
    }


    public override void AddExplosionForce(float power, Vector3 origin)
    {
        if (!ragdoll)
        {
            foreach (RagdollBone bone in bones)
            {
                Vector3 com = bone.transform.position + bone.transform.rotation * bone.RB.centerOfMass;
        
                Vector3 toObject = com - origin;
                float _dist = toObject.magnitude;
        
                float falloff = 1 / (_dist * _dist + 1);

                if (Mathf.Abs(power) * falloff > explosionThreshold) SetActive();

                if (ragdoll) break;
            }
        }

        float dist = (GetCenter() - origin).magnitude;
        float comFalloff = 1 / (dist * dist + 1);
        
        OnExplosionEventInvoke(power * comFalloff);

        if (!ragdoll) return;
        {
            foreach (RagdollBone bone in bones)
            {
                Vector3 com = bone.transform.position + bone.transform.rotation * bone.RB.centerOfMass;
        
                Vector3 toObject = com - origin;
                float _dist = toObject.magnitude;
                Vector3 direction = toObject.normalized;
        
                float falloff = 1 / (_dist * _dist);
        
                bone.RB.linearVelocity += power * direction * falloff / bone.RB.mass;
            }
        }
    }

    public override void AddAcceleration(Vector3 acceleration)
    {
        foreach (RagdollBone bone in bones)
        {
            bone.AddAcceleration(acceleration);
        }
    }

    public override void AddBuoyantForceAndDrag(Bounds volume, float density, Vector3 velocity)
    {
        // loop over every bone that intersects with the volume
        foreach (RagdollBone bone in bones.Where(bone => bone.Collider.bounds.Intersects(volume)))
        {
            // get the AABB of the intersection of the fluid and the collider's bounds
            Bounds intersectBounds = new Bounds();
            intersectBounds.SetMinMax(
                new Vector3(
                    Mathf.Max(volume.min.x, bone.Collider.bounds.min.x),
                    Mathf.Max(volume.min.y, bone.Collider.bounds.min.y),
                    Mathf.Max(volume.min.z, bone.Collider.bounds.min.z)),
                new Vector3(
                    Mathf.Min(volume.max.x, bone.Collider.bounds.max.x),
                    Mathf.Min(volume.max.y, bone.Collider.bounds.max.y),
                    Mathf.Min(volume.max.z, bone.Collider.bounds.max.z)));
            
            // create sample points
            List<Vector3> samplePoints = Enumerable.Range(0, SettingsManager.Instance.BuoyancySamples >> 3).Select(_ => 
                new Vector3(
                    Random.Range(intersectBounds.min.x, intersectBounds.max.x), 
                    Random.Range(intersectBounds.min.y, intersectBounds.max.y), 
                    Random.Range(intersectBounds.min.z, intersectBounds.max.z))
            ).ToList();
            
            // count how many are in the collider
            List<Vector3> samplesInside = samplePoints.Where(s => bone.Collider.ClosestPoint(s) == s).ToList();
        
            // return if no samples inside
            if (samplesInside.Count == 0) return;
            
            // set ragdoll active when touching fluid
            SetActive();
            
            // calculate the overlapping volume
            float overlapVolume = BoundsVolume(intersectBounds) * samplesInside.Count / samplePoints.Count;
            
            // calculate buoyant force and centre of volume
            Vector3 buoyantForce = -overlapVolume * density * Physics.gravity;
            Vector3 cov = samplesInside.Aggregate(Vector3.zero, (sum, sample) => sum + sample) / samplesInside.Count; // center of volume (estimated as average of samples inside collider)

            // calculate velocity at centre of volume
            Vector3 covVelocity = bone.RB.GetPointVelocity(cov);
        
            // get the velocity relative to the fluid
            Vector3 relativeVelocity = covVelocity - velocity;
            
            // estimate the drag force
            Vector3 dragForce = relativeVelocity * (-density * Mathf.Pow(overlapVolume, 2f/3f));
            
            if ((buoyantForce + dragForce).magnitude > 0) bone.RB.AddForceAtPosition(buoyantForce + dragForce, cov);
        }
    }


    public void SetActive()
    {
        if (ragdoll) return;
        
        ragdoll = true;
        
        rb.isKinematic = true;
        rb.useGravity = false;

        foreach (RagdollBone bone in bones)
        {
            bone.RB.isKinematic = false;
            bone.RB.useGravity = true;
        }
            
        anim.enabled = false;
    }
    
    public void SetInactive()
    {
        if (!ragdoll) return;
        
        ragdoll = false;
        
        rb.isKinematic = false;
        rb.useGravity = true;

        foreach (RagdollBone bone in bones)
        {
            bone.RB.isKinematic = true;
            bone.RB.useGravity = false;
        }
            
        anim.enabled = true;
            
        Vector3 sum = bones.Aggregate(Vector3.zero, (current, bone) => current + bone.RB.position);
        sum /= bones.Count;

        transform.position = sum;
        root.position = Vector3.zero;
    }


    protected override void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.impulse.magnitude);

        if (collision.impulse.magnitude > impulseThreshold)
        {
            SetActive();
        }
        
        base.OnCollisionEnter(collision);
    }


    private void OnValidate()
    {
        if (ragdoll)
        {
            rb.isKinematic = true;
            rb.useGravity = false;

            foreach (RagdollBone bone in bones)
            {
                bone.RB.isKinematic = false;
                bone.RB.useGravity = true;
            }
            
            anim.enabled = false;
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            foreach (RagdollBone bone in bones)
            {
                bone.RB.isKinematic = true;
                bone.RB.useGravity = false;
            }
            
            anim.enabled = true;
        }
    }
}
