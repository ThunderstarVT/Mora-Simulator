using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RagdollBone : MonoBehaviour
{
    [Header("Required Components (RagdollBone)")]
    [SerializeField] private Rigidbody rb;
    
    public Rigidbody RB => rb;


    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterEvent?.Invoke(collision);
    }


    public event Action<Collision> OnCollisionEnterEvent;
}
