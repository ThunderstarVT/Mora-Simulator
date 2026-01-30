using System;
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
            LOW,
            MID,
            HIGH
        }
        
        [SerializeField] private Options buoyancyAccuracy = Options.LOW;

        public int BuoyancySamples
        {
            get
            {
                return buoyancyAccuracy switch
                {
                    Options.LOW => 64,
                    Options.MID => 128,
                    Options.HIGH => 256,
                    _ => 0
                };
            }
        }
    }
}