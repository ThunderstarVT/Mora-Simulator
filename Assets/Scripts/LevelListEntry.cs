using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelListEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;
    [SerializeField] private Image image;

    public void SetText(string text)
    {
        this.text.text = text;
    }
    
    public void SetButtonOnClick(UnityAction action)
    {
        button.onClick.AddListener(action);
    }

    public void SetImageAlpha(float alpha)
    {
        image.color = new Color(1, 1, 1, alpha);
    }
}
