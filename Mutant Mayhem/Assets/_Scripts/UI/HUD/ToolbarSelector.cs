using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolbarSelector : MonoBehaviour
{
    [SerializeField] List<Image> boxImages;
    [SerializeField] List<Image> gunImages;
    [SerializeField] List<Image> unlockGlowImages;
    Image currentBox;
    Player player;
    Color unselectedColor;
    [SerializeField] Color selectedColor;

    List<Image> unlockedGunImages = new List<Image>();

    void Start()
    {
        player = FindObjectOfType<Player>();

        currentBox = boxImages[0];
        unselectedColor = currentBox.color;
        SwitchBoxes(0);

        player.playerShooter.onPlayerGunSwitched += SwitchBoxes;
        UpdateTootips();
    }

    void OnDestroy()
    {
        player.playerShooter.onPlayerGunSwitched -= SwitchBoxes;
    }

    public void SwitchBoxes(int i)
    {
        if (player.playerShooter.gunList[i] != null)
        {
            if (player.playerShooter.gunsUnlocked[i])
            {
                currentBox.color = unselectedColor;
                currentBox = boxImages[i];
                currentBox.color = selectedColor;
            }
        }
    }

    public void UnlockBoxImage(int i)
    {
        Image image = gunImages[i];
        image.color = new Color(1,1,1,1);
        //Debug.Log("Toolbarselector played upgEffect");
        UpgradeManager.Instance.upgradeEffects.ToolbarUpgradeEffect((Vector2)image.transform.position);

        Image glow = unlockGlowImages[i];
        StartCoroutine(PlayUnlockAnimation(glow, new Vector2(0, 100), 4f, 1.2f));
    }

    public void ResetBoxImage(int i, GunSO newGun)
    {
        gunImages[i].sprite = newGun.uiSprite;
        gunImages[i].color = new Color(1, 1, 1, 1);
        UpdateTootips();
    }

    void UpdateTootips()
    {
        for (int i = 0; i < boxImages.Count; i++)
        {
            TooltipTrigger trigger = boxImages[i].GetComponent<TooltipTrigger>();
            if (trigger != null)
            {
                GunSO gun = player.playerShooter.gunList[i];
                if (gun != null)
                {
                    trigger.header = gun.uiName;
                    trigger.content = gun.uiDescription;
                }
            }
        }
    }

    public void LockBoxImage(int i)
    {
        Image image = gunImages[i];
        image.color = new Color(0, 0, 0, 0.4f);
        //Debug.Log("Toolbarselector played upgEffect");
        //UpgradeManager.Instance.upgradeEffects.ToolbarUpgradeEffect((Vector2)image.transform.position);
    }

    // Coroutine to animate an Image moving and scaling to a center position and back
    public IEnumerator PlayUnlockAnimation(Image backGlowImage, Vector2 centerPos, float scale, float duration)
    {
        if (backGlowImage == null)
        {
            Debug.Log("BackGlowImage is null!");
            yield break;
        }

        if (unlockedGunImages.Contains(backGlowImage))
            yield break;

        backGlowImage.enabled = true;
        unlockedGunImages.Add(backGlowImage);
        RectTransform rectTransform = backGlowImage.rectTransform;
        Vector2 originalPos = rectTransform.anchoredPosition;
        Vector3 originalScale = rectTransform.localScale;

        // Move and scale to center position
        float halfDuration = duration / 2f;
        float timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / halfDuration));
            rectTransform.anchoredPosition = Vector2.Lerp(originalPos, centerPos, t);
            rectTransform.localScale = Vector3.Lerp(originalScale, Vector3.one * scale, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.6f);

        timer = 0f;
        float returnDuration = halfDuration * 0.75f;
        // Move and scale back to original position and scale
        while (timer < returnDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(timer / returnDuration));
            rectTransform.anchoredPosition = Vector2.Lerp(centerPos, originalPos, t);
            rectTransform.localScale = Vector3.Lerp(Vector3.one * scale, originalScale, t);
            yield return null;
        }

        // Ensure final position and scale are reset exactly
        rectTransform.anchoredPosition = originalPos;
        rectTransform.localScale = originalScale;

        StartCoroutine(FadeGlowEffect(backGlowImage, 5f));
    }

    IEnumerator FadeGlowEffect(Image glow, float delay)
    {
        float timer = 0f;
        while (timer < delay)
        {
            // Fade the Image's alpha over time
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / delay);
            Color color = glow.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            glow.color = color;
            yield return null;
        }

        glow.enabled = false;
    }
}
