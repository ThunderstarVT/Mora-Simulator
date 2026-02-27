using UnityEngine;

[RequireComponent(typeof(DestructibleObject))]
public class ExplosiveCanister : MonoBehaviour
{
    [SerializeField] private DestructibleObject destructibleObject;
    
    [Space]
    [SerializeField] private Transform explosionOrigin;
    [SerializeField] private float explosionPower;
    [SerializeField] private GameObject particlePrefab;
    
    void Start()
    {
        destructibleObject.OnBreakEvent += () =>
        {
            if (particlePrefab) Instantiate(particlePrefab, explosionOrigin.position, explosionOrigin.rotation);

            GameObject explosionObject = new GameObject();
            explosionObject.transform.SetPositionAndRotation(explosionOrigin.position, explosionOrigin.rotation);
            Explosion explosion = explosionObject.AddComponent<Explosion>();
            explosion.SetPower(explosionPower);
            
            Destroy(gameObject);
        };
    }
}
