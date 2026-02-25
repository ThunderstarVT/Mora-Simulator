using System;
using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerActions : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private Ragdoll ragdoll;

    [Header("Kick Settings")] 
    
    [Header("Eat Settings")] 
    
    [Header("Fire Breath Settings")] 
    [SerializeField] private Transform fireOrigin;
    [SerializeField, Min(0f)] private float fireDistance = 1.5f;
    [SerializeField, Min(0f)] private float fireRadius = 0.5f;
    [SerializeField] private ParticleSystem fireParticles;
    
    private bool breathingFire;
    
    [Header("Scream Settings")]
    [SerializeField] private AudioSource screamAudioSource;
    [SerializeField] private List<AudioClip> screamAudioClips;

    
    private void Start()
    {
        screamAudioSource.volume = SettingsManager.Instance.VoiceVolume;
        
        SettingsManager.OnApply += () => screamAudioSource.volume = SettingsManager.Instance.VoiceVolume;
        
        inputManager.OnKick += OnKick;
        inputManager.OnEat += OnEat;
        inputManager.OnBreatheFire += OnBreatheFire;
        inputManager.OnMakeSound += OnScream;
    }

    private void FixedUpdate()
    {
        if (breathingFire && fireOrigin)
        {
            Vector3 start = fireOrigin.position;
            Vector3 end = start + fireOrigin.forward * fireDistance;
            
            Collider[] hits = Physics.OverlapCapsule(start, end, fireRadius, 
                Physics.AllLayers, QueryTriggerInteraction.Collide);
            
            foreach (Flammable flammable in Flammable.Instances.Where(
                         f => f.CanBurn && !f.IsBurning && f.Colliders.Any(c => hits.Contains(c))))
            {
                flammable.StartBurn();
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (fireOrigin)
        {
            Gizmos.color = Color.red;
            
            Vector3 start = fireOrigin.position;
            Vector3 end = start + fireOrigin.forward * fireDistance;
            
            Gizmos.DrawWireSphere(start, fireRadius);
            Gizmos.DrawWireSphere(end, fireRadius);
            
            Vector3 up = fireOrigin.up * fireRadius;
            Vector3 right = fireOrigin.right * fireRadius;
            
            Gizmos.DrawLine(start + up, end + up);
            Gizmos.DrawLine(start - up, end - up);
            Gizmos.DrawLine(start + right, end + right);
            Gizmos.DrawLine(start - right, end - right);
        }
    }


    private void OnKick(InputAction.CallbackContext obj)
    {
        if (!ragdoll.isRagdolling)
        {
            
        }
    }

    private void OnEat(InputAction.CallbackContext obj)
    {
        if (!ragdoll.isRagdolling)
        {
            
        }
    }

    private void OnBreatheFire(InputAction.CallbackContext obj)
    {
        breathingFire = obj.ReadValue<float>() > 0.5f;

        if (fireParticles)
        {
            ParticleSystem.EmissionModule emission = fireParticles.emission;
            emission.enabled = breathingFire;
        }
    }

    private void OnScream(InputAction.CallbackContext obj)
    {
        if (screamAudioClips.Count < 1) return;
        
        AudioClip clip = screamAudioClips.OrderBy(c => Random.value).First();
        screamAudioSource.PlayOneShot(clip);
    }
}
