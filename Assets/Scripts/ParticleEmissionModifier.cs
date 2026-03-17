using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;

public class ParticleEmissionModifier : MonoBehaviour
{
    private Dictionary<ParticleSystem, float> particleSystemEmissionRates = new();
    private Dictionary<ParticleSystem, List<(float, float, float)>> particleSystemEmissionBursts = new();

    private void Start()
    {
        foreach (ParticleSystem p in GetComponents<ParticleSystem>())
        {
            particleSystemEmissionRates.Add(p, p.emission.rateOverTimeMultiplier);
            
            particleSystemEmissionBursts.Add(p, new List<(float, float, float)>());

            for (int i = 0; i < p.emission.burstCount; i++)
            {
                particleSystemEmissionBursts[p].Add((
                    p.emission.GetBurst(i).count.constantMin, 
                    p.emission.GetBurst(i).count.constantMax, 
                    p.emission.GetBurst(i).count.constant));
            }
        }

        OnApply();

        SettingsManager.OnApply += OnApply;
    }

    private void OnDestroy()
    {
        SettingsManager.OnApply -= OnApply;
    }

    private void OnApply()
    {
        particleSystemEmissionRates.Keys.ToList().ForEach(p =>
        {
            bool wasPlaying = p.isPlaying;
            p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
            ParticleSystem.EmissionModule emission = p.emission;
            emission.rateOverTimeMultiplier = particleSystemEmissionRates[p] * SettingsManager.Instance.ParticleRateModifier;
            
            for (int i = 0; i < particleSystemEmissionBursts[p].Count; i++)
            {
                ParticleSystem.Burst burst = emission.GetBurst(i);
                ParticleSystem.MinMaxCurve curve = burst.count;
                curve.constantMin = particleSystemEmissionBursts[p][i].Item1 * SettingsManager.Instance.ParticleRateModifier;
                curve.constantMax = particleSystemEmissionBursts[p][i].Item2 * SettingsManager.Instance.ParticleRateModifier;
                curve.constant = particleSystemEmissionBursts[p][i].Item3 * SettingsManager.Instance.ParticleRateModifier;
                burst.count = curve;
                emission.SetBurst(i, burst);
            }
            
            if (wasPlaying) p.Play();
        });
    }
}
