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
            if (particlePrefab) Instantiate(particlePrefab, explosionOrigin.position, explosionOrigin.rotation);

            GameObject explosionObject = new GameObject();
            explosionObject.transform.SetPositionAndRotation(explosionOrigin.position, explosionOrigin.rotation);
            Explosion explosion = explosionObject.AddComponent<Explosion>();
            explosion.SetPower(explosionPower);
            
            Destroy(gameObject);
        
            ScoreTracker.Instance.AwardPoints(150, "explode", "Blow up " + GetComponent<NameHaver>().Name);
        
            exploded = true;
        }
    }
}
