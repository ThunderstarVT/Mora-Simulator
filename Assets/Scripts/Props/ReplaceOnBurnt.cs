using System;
using UnityEngine;

[RequireComponent(typeof(Flammable))]
public class ReplaceOnBurnt : MonoBehaviour
{
    [SerializeField] private Flammable flammable;
    
    [Space]
    [SerializeField] private GameObject prefab;

    private void Awake()
    {
        flammable.OnBurnEnd += OnBurnEnd;
    }

    private void OnDestroy()
    {
        flammable.OnBurnEnd -= OnBurnEnd;
    }

    
    private void OnBurnEnd()
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        
        GameObject obj = Instantiate(prefab, position, rotation);

        if (TryGetComponent(out Rigidbody rb1) && obj.TryGetComponent(out Rigidbody rb2))
        {
            rb2.linearVelocity = rb1.linearVelocity;
            rb2.angularVelocity = rb1.angularVelocity;
        }
        
        Destroy(gameObject);
    }
}
