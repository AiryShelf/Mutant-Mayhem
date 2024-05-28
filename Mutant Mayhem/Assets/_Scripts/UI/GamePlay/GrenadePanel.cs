using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class GrenadePanel : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] Image grenadeImage;
    [SerializeField] TextMeshProUGUI grenadeCountText;
    [SerializeField] int fontSize;


    void Update()
    {
        grenadeCountText.text = player.grenadeAmmo.ToString();
    }
}
