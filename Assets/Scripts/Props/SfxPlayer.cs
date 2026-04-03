using Singletons;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    
    private void Start()
    {
        if (TryGetComponent(out AudioSource audioSource))
        {
            audioSource.volume = SettingsManager.Instance.SfxVolume;
        
            audioSource.PlayOneShot(audioClip);
        
            Destroy(gameObject, audioClip.length);
        }
    }
}
