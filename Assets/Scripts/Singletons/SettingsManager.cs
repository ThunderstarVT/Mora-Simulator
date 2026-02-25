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
            //TODO: load from player prefs
            
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

        
        [SerializeField] private bool apply;
        [SerializeField] private bool revert;
        private void OnValidate()
        {
            if (revert) Revert();
            revert = false;
            
            if (apply) Apply();
            apply = false;
        }
        
        
        public bool UnsavedChanges => mouseSenseX != mouseSenseX_old || 
                                      mouseInvertX != mouseInvertX_old ||
                                      mouseSenseY != mouseSenseY_old || 
                                      mouseInvertY != mouseInvertY_old || 
                                      mouseSenseZ != mouseSenseZ_old || 
                                      mouseInvertZ != mouseInvertZ_old ||
                                      sfxVolume != sfxVolume_old || 
                                      musicVolume != musicVolume_old || 
                                      voiceVolume != voiceVolume_old ||
                                      buoyancyAccuracy != buoyancyAccuracy_old || 
                                      particleCount != particleCount_old;


        private enum Options
        {
            POTATO = -4,
            LOW = -1,
            MID = 0,
            HIGH = 1
        }
        
        [SerializeField, Range(0f, 1f)] private float mouseSenseX = 0.6f;
        [SerializeField] private bool mouseInvertX = false; // horizontal
        [SerializeField, Range(0f, 1f)] private float mouseSenseY = 0.6f;
        [SerializeField] private bool mouseInvertY = true; // vertical
        [SerializeField, Range(0f, 1f)] private float mouseSenseZ = 0.5f;
        [SerializeField] private bool mouseInvertZ = true; // scroll
        [Space]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.5f;
        [Space]
        [SerializeField] private Options buoyancyAccuracy = Options.MID;
        [SerializeField] private Options particleCount = Options.MID;

        
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