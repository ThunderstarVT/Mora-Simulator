using UnityEngine;

[RequireComponent(typeof(NameHaver))]
public class NuclearTaco : MonoBehaviour, IEdible
{
    [SerializeField] private Transform explosionOrigin;
    [SerializeField] private float explosionPower;
    [SerializeField] private GameObject particlePrefab;
    
    public void Eat()
    {
        if (particlePrefab) Instantiate(particlePrefab, explosionOrigin.position, Quaternion.Euler(Vector3.zero));

        GameObject explosionObject = new GameObject();
        explosionObject.transform.SetPositionAndRotation(explosionOrigin.position, Quaternion.Euler(Vector3.zero));
        Explosion explosion = explosionObject.AddComponent<Explosion>();
        explosion.SetPower(explosionPower);
            
        Destroy(gameObject);
        
        ScoreTracker.Instance.AwardPoints(1000, "nuclear_taco", "Eat " + GetComponent<NameHaver>().Name);
    }
}
