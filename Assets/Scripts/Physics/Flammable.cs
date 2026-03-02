using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Flammable : MonoBehaviour
{
    public static List<Flammable> Instances { get; } = new();

    [SerializeField] private int burnTicks = 10;
    [SerializeField] private float spreadRange = 1f;
    [SerializeField] private List<Collider> colliders;
    
    [Space]
    [SerializeField] private List<ParticleSystem> particles;
    
    private Coroutine burnCoroutine;
    
    public bool IsBurning => burnCoroutine != null;
    public bool CanBurn => burnTicks > 0;
    
    public List<Collider> Colliders => colliders;
    
    
    public event Action OnBurnEnd;


    private void Start()
    {
        Instances.Add(this);
        
        particles.ForEach(p =>
        {
            ParticleSystem.EmissionModule emission = p.emission;
            emission.enabled = false;
        });

        OnBurnEnd += () =>
        {
            particles.ForEach(p =>
            {
                ParticleSystem.EmissionModule emission = p.emission;
                emission.enabled = false;
            });
        };
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
        
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
    }


    public void StartBurn()
    {
        if (burnCoroutine == null && burnTicks > 0)
        {
            particles.ForEach(p =>
            {
                ParticleSystem.EmissionModule emission = p.emission;
                emission.enabled = true;
            });
            
            burnCoroutine = StartCoroutine(BurnCoroutine());
        }
    }

    private void TrySpread()
    {
        foreach (Flammable instance in Instances.Where(f => f != this && f.CanBurn && !f.IsBurning && 
                f.Colliders.Any(c1 => Colliders.Any(c2 => ColliderDistance(c1, c2) < spreadRange))))
        {
            instance.StartBurn();
        }
    }


    private IEnumerator BurnCoroutine()
    {
        burnTicks--;
        yield return new WaitForSeconds(Random.Range(0.25f, 0.75f));
        TrySpread();
        
        while (burnTicks > 0)
        {
            burnTicks--;
            yield return new WaitForSeconds(0.5f);
            TrySpread();
        }
        
        OnBurnEnd?.Invoke();
        burnCoroutine = null;
    }


    private static float ColliderDistance(Collider c1, Collider c2)
    {
        return Vector3.Distance(
                c1.ClosestPoint(c2.ClosestPoint(c1.bounds.center)), 
                c2.ClosestPoint(c1.ClosestPoint(c2.bounds.center)));
    }
}