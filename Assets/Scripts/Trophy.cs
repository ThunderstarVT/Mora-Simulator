using UnityEngine;
using UnityEngine.SceneManagement;

public class Trophy : MonoBehaviour
{
    [SerializeField, Range(0, 31)] private int id;

    [Space] 
    [SerializeField] private string achievementSetName;

    [SerializeField, Range(0, 7)]
    private int fourAchievement, eightAchievement, sixteenAchievement, thirtyTwoAchievement;
    
    [Space]
    [SerializeField] private float spinSpeed;


    private void Start()
    {
        if ((PlayerPrefs.GetInt("Trophy_" + SceneManager.GetActiveScene().name) & (1 << id)) != 0) Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        int trophyPref = PlayerPrefs.GetInt("Trophy_" + SceneManager.GetActiveScene().name) | (1 << id);
        
        PlayerPrefs.SetInt("Trophy_" + SceneManager.GetActiveScene().name, trophyPref);
        PlayerPrefs.Save();
        
        int collected = 0;
        while (trophyPref != 0)
        {
            if ((trophyPref & 1) == 1) collected++;
            trophyPref >>= 1;
        }
        
        AchievementTracker.Instance.QueuePopup("Trophy Get (" + collected + "/32)");
        
        if (collected >= 4) AchievementTracker.Instance.AwardAchievement(achievementSetName, fourAchievement);
        if (collected >= 8) AchievementTracker.Instance.AwardAchievement(achievementSetName, eightAchievement);
        if (collected >= 16) AchievementTracker.Instance.AwardAchievement(achievementSetName, sixteenAchievement);
        if (collected >= 32) AchievementTracker.Instance.AwardAchievement(achievementSetName, thirtyTwoAchievement);
        
        Destroy(gameObject);
    }
}
