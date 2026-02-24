using UnityEngine;

namespace Singletons
{
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager instance;

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


        private enum Options
        {
            POTATO = -4,
            LOW = -1,
            MID = 0,
            HIGH = 1
        }
        
        [SerializeField, Range(0f, 1f)] private float mouseSenseX = 0.5f;
        [SerializeField] private bool mouseInvertX = false; // horizontal
        [SerializeField, Range(0f, 1f)] private float mouseSenseY = 0.5f;
        [SerializeField] private bool mouseInvertY = false; // vertical
        [SerializeField, Range(0f, 1f)] private float mouseSenseZ = 0.5f;
        [SerializeField] private bool mouseInvertZ = false; // scroll
        [Space]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.5f;
        [Space]
        [SerializeField] private Options buoyancyAccuracy = Options.MID;
        [SerializeField] private Options particleCount = Options.MID;

        
        public Vector3 MouseSense => new(
            (mouseInvertX ? -1 : 1) * Mathf.Pow(10f, Mathf.Lerp(-1f, 1f, mouseSenseX)), 
            (mouseInvertY ? -1 : 1) * Mathf.Pow(10f, Mathf.Lerp(-1f, 1f, mouseSenseY)),
            (mouseInvertZ ? -1 : 1) * Mathf.Pow(10f, Mathf.Lerp(-1f, 1f, mouseSenseZ)));

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