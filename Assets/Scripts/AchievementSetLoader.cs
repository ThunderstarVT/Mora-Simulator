using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementSetLoader : MonoBehaviour
{
    [SerializeField] private List<Image> images;
    [SerializeField] private Color lockedColor;
    [Space]
    [SerializeField] private RectTransform descriptionBox;
    [SerializeField] private TextMeshProUGUI descriptionBoxText;

    public void Load(AchievementTracker.AchievementSet achievementSet)
    {
        descriptionBox.gameObject.SetActive(false);
        
        if (achievementSet == null)
        {
            foreach (Image image in images) image.color = Color.clear;
            return;
        }
        
        for (int i = 0; i < 8; i++)
        {
            images[i].sprite = achievementSet[i].Item2;
            images[i].color = achievementSet[i].Item3 ? Color.white : lockedColor;

            if (achievementSet[i].Item1 == "")
            {
                images[i].color = Color.clear; // if achievement unnamed, hide it
            }
            else
            {
                // on hover, show name in descriptionBox
                AchievementHoverHandler hoverHandler = images[i].GetComponent<AchievementHoverHandler>();
                if (hoverHandler != null)
                {
                    string str = achievementSet[i].Item1;
                    hoverHandler.OnPointerEnterEvent += () =>
                    {
                        descriptionBox.gameObject.SetActive(true);
                        descriptionBoxText.text = str;
                    };

                    hoverHandler.OnPointerExitEvent += () =>
                    {
                        descriptionBox.gameObject.SetActive(false);
                    };
                }
            }
        }
    }
}
