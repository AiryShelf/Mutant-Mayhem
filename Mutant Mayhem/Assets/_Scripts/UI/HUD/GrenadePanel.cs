using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrenadePanel : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] Image grenadeImage;
    [SerializeField] TextMeshProUGUI grenadeCountText;
    [SerializeField] int fontSize;

    Color textColor;
    Color textColorStart;

    void Awake()
    {
        textColorStart = grenadeCountText.color;
    }

    void Update()
    {
        if (player.stats.grenadeAmmo <= 0)
        {
            grenadeImage.color = new Color(1, 0, 0, 0.7f);
            textColor = Color.red;
        }
        else
        {
            grenadeImage.color = new Color(1, 1, 1, 1f);
            textColor = textColorStart;
        }
        grenadeCountText.color = textColor;
        grenadeCountText.text = player.stats.grenadeAmmo.ToString();
    }
}
