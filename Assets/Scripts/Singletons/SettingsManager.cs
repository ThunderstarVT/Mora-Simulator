using System;
using UnityEngine;

namespace Singletons
{
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager instance;

        public static event Action OnApply;
        

        public static SettingsManager Instance
        {
            get
            {
                if (instance == null) Debug.LogError("[Settings Manager]: Settings manager does not exist.");
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null || instance == this)
            {
                instance = this;
            
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey("Settings_MouseSenseX")) mouseSenseX = PlayerPrefs.GetFloat("Settings_MouseSenseX");
            if (PlayerPrefs.HasKey("Settings_MouseInvertX"))
                mouseInvertX = PlayerPrefs.GetInt("Settings_MouseInvertX") > 0;
            if (PlayerPrefs.HasKey("Settings_MouseSenseY")) mouseSenseY = PlayerPrefs.GetFloat("Settings_MouseSenseY");
            if (PlayerPrefs.HasKey("Settings_MouseInvertY"))
                mouseInvertY = PlayerPrefs.GetInt("Settings_MouseInvertY") > 0;
            if (PlayerPrefs.HasKey("Settings_MouseSenseZ")) mouseSenseX = PlayerPrefs.GetFloat("Settings_MouseSenseZ");
            if (PlayerPrefs.HasKey("Settings_MouseInvertZ"))
                mouseInvertZ = PlayerPrefs.GetInt("Settings_MouseInvertZ") > 0;
            
            if (PlayerPrefs.HasKey("Settings_SfxVolume")) sfxVolume = PlayerPrefs.GetFloat("Settings_SfxVolume");
            if (PlayerPrefs.HasKey("Settings_MusicVolume")) musicVolume = PlayerPrefs.GetFloat("Settings_MusicVolume");
            if (PlayerPrefs.HasKey("Settings_VoiceVolume")) voiceVolume = PlayerPrefs.GetFloat("Settings_VoiceVolume");
            
            if (PlayerPrefs.HasKey("Settings_BuoyancyAccuracy")) 
                buoyancyAccuracy = (Options)PlayerPrefs.GetInt("Settings_BuoyancyAccuracy");
            if (PlayerPrefs.HasKey("Settings_ParticleCount"))
                particleCount = (Options)PlayerPrefs.GetInt("Settings_ParticleCount");

            OnApply += () =>
            {
                PlayerPrefs.SetFloat("Settings_MouseSenseX", mouseSenseX);
                PlayerPrefs.SetInt("Settings_MouseInvertX", mouseInvertX ? 1 : 0);
                PlayerPrefs.SetFloat("Settings_MouseSenseY", mouseSenseY);
                PlayerPrefs.SetInt("Settings_MouseInvertY", mouseInvertY ? 1 : 0);
                PlayerPrefs.SetFloat("Settings_MouseSenseZ", mouseSenseZ);
                PlayerPrefs.SetInt("Settings_MouseInvertZ", mouseInvertZ ? 1 : 0);
                
                PlayerPrefs.SetFloat("Settings_SfxVolume", sfxVolume);
                PlayerPrefs.SetFloat("Settings_MusicVolume", musicVolume);
                PlayerPrefs.SetFloat("Settings_VoiceVolume", voiceVolume);
                
                PlayerPrefs.SetInt("Settings_BuoyancyAccuracy", (int)buoyancyAccuracy);
                PlayerPrefs.SetInt("Settings_ParticleCount", (int)particleCount);
                
                PlayerPrefs.Save();
            };
            
            Apply();
        }


        public void Apply()
        {
            mouseSenseX_old = mouseSenseX;
            mouseInvertX_old = mouseInvertX;
            mouseSenseY_old = mouseSenseY;
            mouseInvertY_old = mouseInvertY;
            mouseSenseZ_old = mouseSenseZ;
            mouseInvertZ_old = mouseInvertZ;

            sfxVolume_old = sfxVolume;
            musicVolume_old = musicVolume;
            voiceVolume_old = voiceVolume;

            buoyancyAccuracy_old = buoyancyAccuracy;
            particleCount_old = particleCount;
            
            
            OnApply?.Invoke();
        }

        public void Revert()
        {
            mouseSenseX = mouseSenseX_old;
            mouseInvertX = mouseInvertX_old;
            mouseSenseY = mouseSenseY_old;
            mouseInvertY = mouseInvertY_old;
            mouseSenseZ = mouseSenseZ_old;
            mouseInvertZ = mouseInvertZ_old;

            sfxVolume = sfxVolume_old;
            musicVolume = musicVolume_old;
            voiceVolume = voiceVolume_old;

            buoyancyAccuracy = buoyancyAccuracy_old;
            particleCount = particleCount_old;
        }

        public void FactoryReset()
        {
            mouseSenseX = mouseSenseX_old = 0.6f;
            mouseInvertX = mouseInvertX_old = false;
            mouseSenseY = mouseSenseY_old = 0.6f;
            mouseInvertY = mouseInvertY_old = true;
            mouseSenseZ = mouseSenseZ_old = 0.5f;
            mouseInvertZ = mouseInvertZ_old = true;

            sfxVolume = sfxVolume_old = 0.5f;
            musicVolume = musicVolume_old = 0.5f;
            voiceVolume = voiceVolume_old = 0.5f;

            buoyancyAccuracy = buoyancyAccuracy_old = Options.MID;
            particleCount = particleCount_old = Options.MID;
            
            OnApply?.Invoke();
        }

        
        [SerializeField] private bool apply;
        [SerializeField] private bool revert;
        private void OnValidate()
        {
            if (revert) Revert();
            revert = false;
            
            if (apply) Apply();
            apply = false;
        }
        
        
        public bool UnsavedChanges => MouseSenseXChanged || MouseInvertXChanged || 
                                      MouseSenseYChanged || MouseInvertYChanged || 
                                      MouseSenseZChanged || MouseInvertZChanged ||
                                      SfxVolumeChanged || MusicVolumeChanged || VoiceVolumeChanged ||
                                      BuoyancyAccuracyChanged || ParticleCountChanged;


        public enum Options
        {
            POTATO = -4,
            LOW = -1,
            MID = 0,
            HIGH = 1
        }
        
        [Space]
        [Range(0f, 1f)] public float mouseSenseX = 0.6f;
        public bool mouseInvertX = false; // horizontal
        [Range(0f, 1f)] public float mouseSenseY = 0.6f;
        public bool mouseInvertY = true; // vertical
        [Range(0f, 1f)] public float mouseSenseZ = 0.5f;
        public bool mouseInvertZ = true; // scroll
        [Space]
        [Range(0f, 1f)] public float sfxVolume = 0.5f;
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        [Range(0f, 1f)] public float voiceVolume = 0.5f;
        [Space]
        public Options buoyancyAccuracy = Options.MID;
        public Options particleCount = Options.MID;

        
        private float mouseSenseX_old;
        private bool mouseInvertX_old;
        private float mouseSenseY_old;
        private bool mouseInvertY_old;
        private float mouseSenseZ_old;
        private bool mouseInvertZ_old;

        private float sfxVolume_old;
        private float musicVolume_old;
        private float voiceVolume_old;

        private Options buoyancyAccuracy_old;
        private Options particleCount_old;
        
        
        public bool MouseSenseXChanged => mouseSenseX != mouseSenseX_old;
        public bool MouseInvertXChanged => mouseInvertX != mouseInvertX_old;
        public bool MouseSenseYChanged => mouseSenseY != mouseSenseY_old;
        public bool MouseInvertYChanged => mouseInvertY != mouseInvertY_old;
        public bool MouseSenseZChanged => mouseSenseZ != mouseSenseZ_old;
        public bool MouseInvertZChanged => mouseInvertZ != mouseInvertZ_old;
        
        public bool SfxVolumeChanged => sfxVolume != sfxVolume_old;
        public bool MusicVolumeChanged => musicVolume != musicVolume_old;
        public bool VoiceVolumeChanged => voiceVolume != voiceVolume_old;
        
        public bool BuoyancyAccuracyChanged => buoyancyAccuracy != buoyancyAccuracy_old;
        public bool ParticleCountChanged => particleCount != particleCount_old;

        
        public Vector3 MouseSense => new(
            (mouseInvertX ? -1 : 1) * Mathf.Pow(10f, Mathf.Lerp(0f, 2f, mouseSenseX)), 
            (mouseInvertY ? -1 : 1) * Mathf.Pow(10f, Mathf.Lerp(0f, 2f, mouseSenseY)),
            (mouseInvertZ ? -1 : 1) * Mathf.Pow(10f, Mathf.Lerp(0f, 2f, mouseSenseZ)));

        public float SfxVolume => sfxVolume;
        public float MusicVolume => musicVolume;
        public float VoiceVolume => voiceVolume;
        
        public int BuoyancySamples => buoyancyAccuracy switch
        {
            Options.POTATO => 8,
            Options.LOW => 64,
            Options.MID => 128,
            Options.HIGH => 256,
            _ => 0
        };

        public float ParticleRateModifier => particleCount switch
        {
            Options.POTATO => 0.0f,
            Options.LOW => 0.5f,
            Options.MID => 1.0f,
            Options.HIGH => 2.0f,
            _ => 0.0f
        };
    }
}