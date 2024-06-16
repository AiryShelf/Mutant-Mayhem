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


    void Update()
    {
        grenadeCountText.text = player.stats.grenadeAmmo.ToString();
    }
}
