using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Ragdoll)), RequireComponent(typeof(Flammable)), RequireComponent(typeof(NameHaver))]
public class NPC : MonoBehaviour, IEdible
{
    [SerializeField] private Ragdoll ragdoll;
    [SerializeField] private Flammable flammable;
    [SerializeField] private NameHaver nameHaver;

    private Ragdoll playerRagdoll => GameObject.FindGameObjectWithTag("Player").GetComponent<Ragdoll>();
    
    [Space] 
    [SerializeField] private List<string> npcNames = new();

    [Space] 
    [SerializeField] private NPCBehavior behavior;
    [SerializeField, Min(0f)] private float movementUpdateTime = 2.0f;
    [SerializeField, Min(0f)] private float unragdollTime = 5.0f;
    private float movementUpdateTimer = 0f;
    
    [Space]
    [SerializeField, Min(0f)] private float walkSpeed = 2.5f;
    [SerializeField, Min(0f)] private float sprintSpeed = 5f;
    [SerializeField, Min(0f)] private float velocitySmoothing = 10f;
    [SerializeField, Min(0f)] private float turnSpeed = 5.0f;
    
    [Space]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField, Min(0f)] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckOffset = 0.2f;
    
    [Space]
    [SerializeField] private string eatNpcAchievementSet;
    [SerializeField] private int eatNpcAchievementIndex;

    [Space] 
    [SerializeField] private Transform eggSpawnPos;
    [SerializeField] private GameObject eggPrefab;

    private Vector3 movement = Vector3.zero;
    private bool scared = false;
    private bool dead = false;
    private float ragdollTime = 0f;
    
    public bool IsGrounded => Physics.CheckSphere(
        ragdoll.isRagdolling ? ragdoll.GetCenter() : transform.position + Vector3.up * groundCheckOffset, 
        groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        
    
    private enum NPCBehavior
    {
        IDLE,
        DANCE,
        WANDER
    }
        

    private void Start()
    {
        int randomIndex = Random.Range(0, npcNames.Count);
        nameHaver.SetName(npcNames[randomIndex]);

        flammable.OnBurnEnd += Kill;
    }


    private void FixedUpdate()
    {
        if (ragdoll.isRagdolling)
        {
            ragdollTime += Time.fixedDeltaTime;

            movementUpdateTimer = 0;

            if (ragdollTime >= unragdollTime && !dead && IsGrounded)
            {
                ragdoll.SetInactive();
            }
        }
        else
        {
            if (dead)
            {
                ragdoll.SetActive();
                return;
            }

            ragdollTime = 0f;
            
            movementUpdateTimer -= Time.fixedDeltaTime;
            if (movementUpdateTimer <= 0f)
            {
                movementUpdateTimer = movementUpdateTime;
                
                if (scared)
                {
                    Vector3 direction = (ragdoll.GetCenter() - playerRagdoll.GetCenter()).normalized;
                    direction += Quaternion.Euler(0, Random.Range(0f, 360f), 0) * Vector3.forward;
                    direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
                    movement = direction * sprintSpeed;
                }
                else
                {
                    switch (behavior)
                    {
                        case NPCBehavior.IDLE:
                        case NPCBehavior.DANCE:
                            movement = Vector3.zero;
                            break;
                        case NPCBehavior.WANDER:
                            Vector3 direction = Quaternion.Euler(0, Random.Range(0f, 360f), 0) * Vector3.forward;
                            direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
                            movement = direction * (Random.value < 0.5f ? walkSpeed : 0);
                            break;
                    }
                }
            }
            
            Vector3 newLinearVelocity = Vector3.Lerp(ragdoll.rb.linearVelocity, movement, velocitySmoothing * Time.fixedDeltaTime);
                
            ragdoll.rb.linearVelocity = newLinearVelocity;
            
            ragdoll.rb.rotation = Quaternion.Lerp(ragdoll.rb.rotation, Quaternion.Euler(0, Vector2.SignedAngle(Vector2.up, new Vector2(-newLinearVelocity.x, newLinearVelocity.z)), 0), 
                turnSpeed * Time.fixedDeltaTime);
        }
    }
    
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        
        if (ragdoll.isRagdolling)
        {
            Gizmos.DrawWireSphere(ragdoll.GetCenter(), groundCheckRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + Vector3.up * groundCheckOffset, groundCheckRadius);
        }
    }


    public void Scare()
    {
        scared = true;
        
        movementUpdateTimer = 0f;
    }

    public void Kill()
    {
        dead = true;
        
        ScoreTracker.Instance.AwardPoints(100, "kill", "Kill " + nameHaver.Name);
    }
    

    public void Eat()
    {
        GameObject egg = Instantiate(eggPrefab, eggSpawnPos.position, eggSpawnPos.rotation);
        Material material = egg.GetComponent<Renderer>().material;
        material.SetColor("_BaseColor", new Color(Random.value, Random.value, Random.value));
        
        Destroy(gameObject);
        
        ScoreTracker.Instance.AwardPoints(25, "eat_npc", "Eat " + nameHaver.Name);
        
        AchievementTracker.Instance.AwardAchievement(eatNpcAchievementSet, eatNpcAchievementIndex);
    }
    
    public bool CanEat()
    {
        return dead;
    }
}
