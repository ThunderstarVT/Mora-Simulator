using UnityEngine;

public class PhysicsFluid : MonoBehaviour
{
    [SerializeField] private Bounds volume;
    [SerializeField] private float density;
    [SerializeField] private Vector3 velocity;

    private void FixedUpdate()
    {
        foreach (PhysicsObject physicsObject in PhysicsObject.Instances)
        {
            physicsObject.AddBuoyantForceAndDrag(volume, density, velocity);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 1f);
        Gizmos.DrawWireCube(volume.center, volume.size);
        
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
        Gizmos.DrawCube(volume.center, volume.size);
    }
}
