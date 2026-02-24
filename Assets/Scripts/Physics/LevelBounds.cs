using System;
using System.Linq;
using UnityEngine;

public class LevelBounds : MonoBehaviour
{
    [SerializeField] private Bounds volume;
    
    [Space]
    [SerializeField] private float springConstant;
    [SerializeField] private float dampingCoefficient;

    private void FixedUpdate()
    {
        foreach (PhysicsObject physicsObject in PhysicsObject.Instances.Where(o => !volume.Contains(o.GetCenter())))
        {
            try
            {
                ((Ragdoll) physicsObject).SetActive();
            }
            catch (Exception)
            {
                // ignored
            }

            Vector3 position = physicsObject.GetCenter();
            Vector3 velocity = physicsObject.GetVelocity();
            
            physicsObject.AddAcceleration(-(springConstant * (position - volume.ClosestPoint(position) + (position - volume.ClosestPoint(position)).normalized)) -(dampingCoefficient * velocity));
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 1f, 1f);
        Gizmos.DrawWireCube(volume.center, -volume.size);
        
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.5f);
        Gizmos.DrawCube(volume.center, -volume.size);
    }
}
