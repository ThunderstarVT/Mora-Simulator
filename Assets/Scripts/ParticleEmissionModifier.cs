using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;

public class ParticleEmissionModifier : MonoBehaviour
{
    private Dictionary<ParticleSystem, float> particleSystems = new();

    private void Start()
    {
        foreach (ParticleSystem p in GetComponents<ParticleSystem>())
        {
            particleSystems.Add(p, p.emission.rateOverTimeMultiplier);
        }

        particleSystems.Keys.ToList().ForEach(p =>
        {
            ParticleSystem.EmissionModule emission = p.emission;
            emission.rateOverTimeMultiplier = particleSystems[p] * SettingsManager.Instance.ParticleRateModifier;
        });

        SettingsManager.OnApply += () =>
        {
            particleSystems.Keys.ToList().ForEach(p =>
            {
                ParticleSystem.EmissionModule emission = p.emission;
                emission.rateOverTimeMultiplier = particleSystems[p] * SettingsManager.Instance.ParticleRateModifier;
            });
        };
    }
}
