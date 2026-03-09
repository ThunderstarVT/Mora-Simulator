using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float power;

    private void Start()
    {
        foreach (PhysicsObject physicsObject in PhysicsObject.Instances)
        {
            physicsObject.AddExplosionForce(power, transform.position);
        }
        
        Destroy(gameObject);
    }

    public void SetPower(float power)
    {
        this.power = power;
    }
}
