using UnityEngine;

public class RagdollCenterTracker : MonoBehaviour
{
    [SerializeField] private Ragdoll ragdoll;
    
    private void FixedUpdate()
    {
        transform.position = ragdoll.GetCenter();
    }
}
