using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
    [SerializeField] PlayerShooter playerShooter;
    [SerializeField] UnityEngine.UI.Image currentGunImage;
    [SerializeField] UnityEngine.UI.Image ammoRadialImage;
    [SerializeField] float radialFillSpeed;
    [SerializeField] float radialAlpha = 0.2f;
    [SerializeField] TextMeshProUGUI ammoCountText;
    [SerializeField] int fontSize = 12;
    [SerializeField] string infinityTextSymbol = "♾️";
    [SerializeField] int infinityFontSize = 20;

    //"♾️" "∞"

    int ammoTotal;
    int ammoInClip;
    float currentAmmoFillAmount;

    void Start()
    {
        
    }

    void Update()
    {
        if (ammoCountText == null || currentGunImage == null)
        {
            Debug.Log("Missing Reference to UI components. Weapon Panel");
            return;
        }

        ammoTotal = playerShooter.gunAmmoTotals[playerShooter.currentGunIndex];
        ammoInClip = playerShooter.gunAmmoInClips[playerShooter.currentGunIndex];
        
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
