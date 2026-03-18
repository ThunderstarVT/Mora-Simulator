using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ScoreTracker))]
public class ScoreAchievementLink : MonoBehaviour
{
    [SerializeField] private List<ScoreThreshold> thresholds;
    
    private ScoreTracker scoreTracker;
    
    private void Start()
    {
        scoreTracker = GetComponent<ScoreTracker>();

        scoreTracker.OnScoreChanged += (_score, _toAdd, _added) =>
        {
            foreach (ScoreThreshold threshold in thresholds.Where(threshold => (_score >= threshold.score && threshold.score != -1) || (_score < 0 && threshold.score == -1)))
            {
                AchievementTracker.Instance.AwardAchievement(threshold.setName, threshold.achievementIndex);
            }
        };
    }

    [Serializable]
    struct ScoreThreshold
    {
        public long score;
        [Space]
        public string setName;
        public int achievementIndex;
    }
}
