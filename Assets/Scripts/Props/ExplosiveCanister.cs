using System;
using UnityEngine;

[RequireComponent(typeof(DestructibleObject)), RequireComponent(typeof(Flammable))]
public class ExplosiveCanister : MonoBehaviour
{
    [SerializeField] private DestructibleObject destructibleObject;
    [SerializeField] private Flammable flammable;
    
    [Space]
    [SerializeField] private Transform explosionOrigin;
    [SerializeField] private float explosionPower;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private GameObject sfxPrefab;
    
    [Space]
    [SerializeField] private Vector3 positionOffset;
    
    private bool exploded = false;
    
    void Start()
    {
        destructibleObject.OnBreakEvent += Explode;
        
        flammable.OnBurnEnd += Explode;
    }

    private void Explode()
    {
        if (!exploded)
        {
            Vector3 offset = transform.TransformPoint(positionOffset);
            
            if (particlePrefab) Instantiate(particlePrefab, offset, Quaternion.Euler(Vector3.zero));
            if (sfxPrefab) Instantiate(sfxPrefab, offset, Quaternion.Euler(Vector3.zero));

            GameObject explosionObject = new GameObject();
            explosionObject.transform.SetPositionAndRotation(offset, Quaternion.Euler(Vector3.zero));
            Explosion explosion = explosionObject.AddComponent<Explosion>();
            explosion.SetPower(explosionPower);
            
            Destroy(gameObject);
        
            ScoreTracker.Instance.AwardPoints(150, "explode", "Blow up " + GetComponent<NameHaver>().Name);
        
            exploded = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 offset = transform.TransformPoint(positionOffset);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(offset, 0.1f);
    }
}
