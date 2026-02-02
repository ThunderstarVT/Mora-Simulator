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
        
        [SerializeField, Range(0f, 1f)] private float mouseSense = 0.5f;
        [Space]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.5f;
        [Space]
        [SerializeField] private Options buoyancyAccuracy = Options.MID;

        
        public float MouseSense => Mathf.Pow(10f, Mathf.Lerp(-1f, 1f, mouseSense));

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
    }
}