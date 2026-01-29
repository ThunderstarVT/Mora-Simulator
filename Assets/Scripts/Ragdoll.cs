using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
public class Ragdoll : PhysicsObject
{
    [Header("Required Components (Ragdoll)")]
    [SerializeField] private Animator anim;
    
    
    [Space][Header("Settings (Ragdoll)")]
    [SerializeField] private bool ragdoll;
    
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


    public override void AddExplosionForce(float power, Vector3 origin)
    {
        if (!ragdoll)
        {
            foreach (RagdollBone bone in bones)
            {
                Vector3 com = bone.transform.position + bone.transform.rotation * bone.RB.centerOfMass;
        
                Vector3 toObject = com - origin;
                float dist = toObject.magnitude;
        
                float falloff = 1 / (dist * dist + 1);

                if (Mathf.Abs(power) * falloff > explosionThreshold) SetActive();

                if (ragdoll) break;
            }
        }

        if (!ragdoll) return;
        {
            foreach (RagdollBone bone in bones)
            {
                Vector3 com = bone.transform.position + bone.transform.rotation * bone.RB.centerOfMass;
        
                Vector3 toObject = com - origin;
                float dist = toObject.magnitude;
                Vector3 direction = toObject.normalized;
        
                float falloff = 1 / (dist * dist);
        
                bone.RB.linearVelocity += power * direction * falloff / bone.RB.mass;
            }
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

        root.position = sum;
    }


    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.impulse.magnitude);

        if (collision.impulse.magnitude > impulseThreshold)
        {
            SetActive();
        }
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
