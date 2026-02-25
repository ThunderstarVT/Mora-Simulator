using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;

public class ParticleEmissionModifier : MonoBehaviour
{
    private Dictionary<ParticleSystem.EmissionModule, float> particleSystems = new();

    private void Start()
    {
        foreach (ParticleSystem p in GetComponents<ParticleSystem>())
        {
            particleSystems.Add(p.emission, p.emission.rateOverTimeMultiplier);
        }
        
        particleSystems.Keys.ToList().ForEach(e => 
            e.rateOverTimeMultiplier = particleSystems[e] * SettingsManager.Instance.ParticleRateModifier);

        SettingsManager.OnApply += () =>
        {
            particleSystems.Keys.ToList().ForEach(e =>
                e.rateOverTimeMultiplier = particleSystems[e] * SettingsManager.Instance.ParticleRateModifier);
        };
    }
}
