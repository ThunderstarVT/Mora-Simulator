using System;
using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerActions : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;
    [SerializeField] private Ragdoll ragdoll;

    [Header("Kick Settings")] 
    [SerializeField] private Transform kickOrigin;
    [SerializeField, Min(0f)] private float kickRadius = 0.8f;
    [SerializeField, Min(0f)] private float kickImpulse = 1000f;
    [SerializeField] private float kickUpwardsModifier = 0.2f;
    
    [Header("Eat Settings")] 
    
    [Header("Fire Breath Settings")] 
    [SerializeField] private Transform fireOrigin;
    [SerializeField, Min(0f)] private float fireDistance = 1.5f;
    [SerializeField, Min(0f)] private float fireRadius = 0.4f;
    [SerializeField] private ParticleSystem fireParticles;
    
    private bool breathingFire;
    public bool BreathingFire => breathingFire;
    
    [Header("Scream Settings")]
    [SerializeField] private AudioSource screamAudioSource;
    [SerializeField] private List<AudioClip> screamAudioClips;

    
    private void Start()
    {
        if (fireParticles)
        {
            ParticleSystem.EmissionModule emission = fireParticles.emission;
            emission.enabled = breathingFire;
        }

        UpdateAudioVolume();
        
        SettingsManager.OnApply += UpdateAudioVolume;
        
        inputManager.OnKick += OnKick;
        inputManager.OnEat += OnEat;
        inputManager.OnBreatheFire += OnBreatheFire;
        inputManager.OnMakeSound += OnScream;
    }

    private void OnDestroy()
    {
        SettingsManager.OnApply -= UpdateAudioVolume;
    }

    private void UpdateAudioVolume()
    {
        screamAudioSource.volume = SettingsManager.Instance.VoiceVolume;
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

        if (kickOrigin)
        {
            Gizmos.color = Color.cyan;
            
            Gizmos.DrawWireSphere(kickOrigin.position, kickRadius);
            
#if UNITY_EDITOR
            Handles.color = Color.cyan;
            Handles.ArrowHandleCap(0, kickOrigin.position, 
                Quaternion.LookRotation(kickOrigin.forward + Vector3.up * kickUpwardsModifier), 
                Mathf.Log10(kickImpulse + 1f), EventType.Repaint);
#endif
        }
    }


    private void OnKick(InputAction.CallbackContext context)
    {
        if (!ragdoll.isRagdolling)
        {
            OnKickEvent?.Invoke();
            
            List<PhysicsObject> physicsObjectsInKick = PhysicsObject.Instances.Where(obj =>
            {
                if (obj == ragdoll) return false;

                try
                {
                    Ragdoll rd = (Ragdoll) obj;
                    
                    return rd.Bones.Any(b =>
                    {
                        bool wasEnabled = b.Collider.enabled;

                        b.Collider.enabled = true;
                        
                        float dist = Vector3.Distance(kickOrigin.position,
                            b.Collider.ClosestPoint(kickOrigin.position));
                        
                        b.Collider.enabled = wasEnabled;
                        
                        return dist < kickRadius;
                    });
                }
                catch (Exception)
                {
                    return obj.Colliders.Any(c =>
                        Vector3.Distance(kickOrigin.position, c.ClosestPoint(kickOrigin.position)) < kickRadius);
                }
            }).ToList();

            if (physicsObjectsInKick.Count < 1) return;

            PhysicsObject first = physicsObjectsInKick
                .OrderBy(obj => Vector3.Distance(kickOrigin.position, obj.GetCenter())).First();
            
            first.OnKickedEventInvoke(kickImpulse);
            first.AddImpulseAtPoint((kickOrigin.forward + Vector3.up * kickUpwardsModifier).normalized * kickImpulse, 
                kickOrigin.position);
        }
    }

    private void OnEat(InputAction.CallbackContext context)
    {
        if (!ragdoll.isRagdolling)
        {
            OnEatEvent?.Invoke();
            
            //TODO: eat edible object in sphere that is closest to center
            // will be added when edible object gets added
        }
    }

    private void OnBreatheFire(InputAction.CallbackContext context)
    {
        breathingFire = context.ReadValue<float>() > 0.5f;

        if (fireParticles)
        {
            ParticleSystem.EmissionModule emission = fireParticles.emission;
            emission.enabled = breathingFire;
        }
    }
    
    private AudioClip lastScreamClip;
    private void OnScream(InputAction.CallbackContext context)
    {
        OnScreamEvent?.Invoke();
        
        if (screamAudioClips.Count < 1) return;
        
        AudioClip clip = screamAudioClips.OrderBy(_ => Random.value).First(c => c != lastScreamClip);
        lastScreamClip = clip;
        screamAudioSource.PlayOneShot(clip);
    }
    
    
    // events for the animator
    public event Action OnKickEvent;
    public event Action OnEatEvent;
    public event Action OnScreamEvent;
}
