using System.Collections.Generic;
using System.Linq;
using Singletons;
using UnityEngine;

public class AudioVolume : MonoBehaviour
{
    private enum AudioType
    {
        VOICE,
        MUSIC,
        SFX
    }
    
    [SerializeField] private AudioType audioType;

    private List<AudioSource> audioSources => GetComponents<AudioSource>().ToList();
    
    private void Start()
    {
        OnApply();
        
        SettingsManager.OnApply += OnApply;
    }

    private void OnDestroy()
    {
        SettingsManager.OnApply -= OnApply;
    }

    private void OnApply()
    {
        switch (audioType)
        {
            case AudioType.VOICE:
                audioSources.ForEach(audioSource => audioSource.volume = SettingsManager.Instance.VoiceVolume);
                break;
            case AudioType.MUSIC:
                audioSources.ForEach(audioSource => audioSource.volume = SettingsManager.Instance.MusicVolume);
                break;
            case AudioType.SFX:
                audioSources.ForEach(audioSource => audioSource.volume = SettingsManager.Instance.SfxVolume);
                break;
        }
    }
}
