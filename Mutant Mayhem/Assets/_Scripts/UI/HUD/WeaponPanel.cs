using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
    [SerializeField] PlayerShooter playerShooter;
    [SerializeField] Image currentGunImage;
    [SerializeField] Image ammoRadialImage;
    [SerializeField] float radialFillSpeed;
    [SerializeField] float radialAlpha = 0.2f;
    [SerializeField] TextMeshProUGUI ammoCountText;
    [SerializeField] int fontSize = 12;
    //[SerializeField] string infinityTextSymbol = "♾️";
    //[SerializeField] int infinityFontSize = 20;

    //"♾️" "∞"

    int ammoTotal;
    int ammoInClip;
    float currentAmmoFillAmount;

    Color textColor;
    Color textColorStart;

    void Awake()
    {
        textColorStart = ammoCountText.color;
    }

    void Update()
    {
        if (ammoCountText == null || currentGunImage == null)
        {
            Debug.Log("Missing Reference to UI components. Weapon Panel");
            return;
        }

        ammoTotal = playerShooter.gunsAmmo[playerShooter.currentGunIndex];
        ammoInClip = playerShooter.gunsAmmoInClips[playerShooter.currentGunIndex];

        if (ammoInClip + ammoTotal <= 0)
        {
            textColor = Color.red;
            currentGunImage.color = new Color(1, 0, 0, 0.7f);
        }
        else
        {
            textColor = textColorStart;
            currentGunImage.color = new Color(1, 1, 1, 1f);
        }

        ammoCountText.color = textColor;        
        ammoCountText.text =  (ammoTotal + ammoInClip).ToString();
        ammoCountText.fontSize = fontSize;

        currentGunImage.sprite = playerShooter.currentGunSO.uiSprite;

        DrawAmmoCircle();
    }

    void DrawAmmoCircle()
    {
        int maxAmmo = playerShooter.currentGunSO.clipSize;
        float targetFillAmount = (float)ammoInClip / maxAmmo;

        currentAmmoFillAmount = Mathf.Lerp(currentAmmoFillAmount, targetFillAmount, 
                                           Time.deltaTime * radialFillSpeed);

        ammoRadialImage.fillAmount = currentAmmoFillAmount;
        ammoRadialImage.color = Color.Lerp(new Color(1, 0, 0, radialAlpha), 
                                           new Color(0, 1, 0, radialAlpha), currentAmmoFillAmount);
    }
}
