using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    public static ScorePopup Summon(long score, float scale, float duration, Vector2 velocity, Transform origin)
    {
        GameObject obj = new GameObject();
        obj.transform.SetParent(origin);
        obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(Vector3.zero));
        
        TextMeshProUGUI scoreText = obj.AddComponent<TextMeshProUGUI>();
        scoreText.text = score.ToString();
        scoreText.alignment = TextAlignmentOptions.Center;
        
        ScorePopup scorePopup = obj.AddComponent<ScorePopup>();
        scorePopup.initialScale = scale * Mathf.Sqrt(score);
        scorePopup.duration = duration;
        scorePopup.velocity = velocity;
        
        return scorePopup;
    }
    
    private float initialScale;
    private float duration;
    private Vector2 velocity;

    private float timer;

    private void Start()
    {
        timer = duration;
    }

    private void FixedUpdate()
    {
        transform.localScale = Vector3.one * Mathf.Lerp(0, initialScale, timer / duration);
        transform.localPosition += new Vector3(velocity.x, velocity.y) * Time.fixedDeltaTime;
        
        timer -= Time.fixedDeltaTime;
        
        if (timer <= 0f) Destroy(gameObject);
    }
}
