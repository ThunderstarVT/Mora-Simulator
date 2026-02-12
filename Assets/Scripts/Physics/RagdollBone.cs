using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RagdollBone : MonoBehaviour
{
    [Header("Required Components (RagdollBone)")]
    [SerializeField] private Rigidbody rb;
    
    public Rigidbody RB => rb;
    
    [Header("")]
    [SerializeField] private new Collider collider;
    
    public Collider Collider => collider;

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterEvent?.Invoke(collision);
    }


    public void AddAcceleration(Vector3 acceleration)
    {
        rb.AddForce(acceleration * rb.mass);
    }


    public event Action<Collision> OnCollisionEnterEvent;
}
