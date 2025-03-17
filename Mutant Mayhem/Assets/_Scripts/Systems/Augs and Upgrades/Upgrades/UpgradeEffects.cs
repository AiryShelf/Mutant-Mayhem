using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeEffects : MonoBehaviour
{
    public int FxDestroyTime = 4;
    public GameObject playerUpgAppliedPrefab;
    public GameObject uiUpgAppliedPrefab;
    public GameObject struturesUpgAppliedPrefab;

    // Can probably put these stragglers in the list
    ParticleSystem playerUpgAppliedFX;
    ParticleSystem uiButtonUpgAppliedFX;
    ParticleSystem uiToolbarUpgAppliedFX;
    List<ParticleSystem> structuresUpgAppliedFX = new List<ParticleSystem>();

    Player player;

    public void Initialize()
    {
        player = FindObjectOfType<Player>();

        // Reset particle systems
        if (playerUpgAppliedFX != null)
            Destroy(playerUpgAppliedFX.gameObject);
        if (uiButtonUpgAppliedFX != null)
            Destroy(uiButtonUpgAppliedFX.gameObject);
        if (uiToolbarUpgAppliedFX != null)
            Destroy(uiToolbarUpgAppliedFX.gameObject);

        playerUpgAppliedFX = Instantiate(playerUpgAppliedPrefab).GetComponent<ParticleSystem>();
        uiButtonUpgAppliedFX = Instantiate(uiUpgAppliedPrefab).GetComponent<ParticleSystem>();
        uiToolbarUpgAppliedFX = Instantiate(uiUpgAppliedPrefab).GetComponent<ParticleSystem>();
    }

    public void ToolbarUpgradeEffect(Vector3 pos)
    {
        uiToolbarUpgAppliedFX.transform.position = pos;
        uiToolbarUpgAppliedFX.Play();
    }

    public void PlayUpgradeButtonEffect()
    {
        Vector2 worldPos;
        if (InputManager.LastUsedDevice == Keyboard.current)
            worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        else
            worldPos = CursorManager.Instance.GetCustomCursorWorldPos();

        uiButtonUpgAppliedFX.transform.position = worldPos;
        uiButtonUpgAppliedFX.Play();

        PlayerUpgradeEffectAt(player.transform.position);
    }

    public void PlayUnlockEffect()
    {
        uiButtonUpgAppliedFX.transform.position = QCubeController.Instance.transform.position;
        uiButtonUpgAppliedFX.Play();
    }

    public void PlayerUpgradeEffectAt(Vector2 pos)
    {
        playerUpgAppliedFX.transform.position = pos;
        playerUpgAppliedFX.Play();
    }

    public void PlayStructureUpgradeEffectAt(Vector2 pos)
    {
        ParticleSystem newFX = Instantiate(struturesUpgAppliedPrefab).GetComponent<ParticleSystem>();
        structuresUpgAppliedFX.Add(newFX);

        newFX.transform.position = pos;
        newFX.Play();
        StartCoroutine(DestroyFXDelayed(newFX, FxDestroyTime));
    }

    IEnumerator DestroyFXDelayed(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ps != null)
        {
            structuresUpgAppliedFX.Remove(ps);
            Destroy(ps.gameObject);
        }
    }
}
