using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementTracker : MonoBehaviour
{
    private static AchievementTracker instance;

    public static AchievementTracker Instance
    {
        get
        {
            if (instance == null) Debug.LogError("[Achievement Tracker]: Achievement tracker does not exist.");
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
        achievementSets.ForEach(a => a.Load());
    }


    public void AwardAchievement(string set, int index)
    {
        foreach (AchievementSet achievementSet in achievementSets.Where(a => a.name == set))
        {
            achievementSet.Award(index);
        }
    }


    [SerializeField] private List<AchievementSet> achievementSets;
    
    
    [Serializable]
    private class AchievementSet
    {
        public string name;

        private int awarded;

        [Space] 
        [SerializeField] private string description1; 
        [SerializeField] private string description2, description3, description4, description5, description6, description7, description8;
        [Space] 
        [SerializeField] private Sprite icon1;
        [SerializeField] private Sprite icon2, icon3, icon4, icon5, icon6, icon7, icon8;

        public (string, Sprite) this[int index]
        {
            get
            {
                return index switch
                {
                    0 => (description1, icon1),
                    1 => (description2, icon2),
                    2 => (description3, icon3),
                    3 => (description4, icon4),
                    4 => (description5, icon5),
                    5 => (description6, icon6),
                    6 => (description7, icon7),
                    7 => (description8, icon8),
                    _ => (null, null)
                };
            }
        }


        public void Load()
        {
            awarded = PlayerPrefs.GetInt("AchievementSet_" + name);
        }

        public void Award(int index)
        {
            if (index < 0 || index >= 8) throw new IndexOutOfRangeException();

            if ((awarded & (1 << index)) != 0) return; // if already awarded, return
            
            awarded |= 1 << index;
            
            PlayerPrefs.SetInt("AchievementSet_" + name, awarded);
            PlayerPrefs.Save();
        }
    }
}
