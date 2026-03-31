using UnityEngine;
using UnityEngine.SceneManagement;

public class BackroomsTeleporter : MonoBehaviour
{
    [SerializeField] private string backroomsAchievementSet;
    [SerializeField] private int backroomsAchievementIndex;
    
    [Space]
    [SerializeField] private string backroomsSceneName;
    
    private void OnTriggerEnter(Collider other)
    {
        if (AchievementTracker.Instance.HasAchievement(backroomsAchievementSet, backroomsAchievementIndex)) return;
        
        AchievementTracker.Instance.AwardAchievement(backroomsAchievementSet, backroomsAchievementIndex);
        SceneManager.LoadScene(backroomsSceneName);
    }
}
