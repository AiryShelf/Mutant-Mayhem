using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DroneHangar : MonoBehaviour, IPowerConsumer, ITileObjectExplodable
{
    [SerializeField] StructureSO droneHangarSO;
    [SerializeField] SpriteRenderer spriteLightSprite;
    [SerializeField] SpriteRenderer radarDishSprite;
    [SerializeField] float radarDishRotateSpeed = 180f;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            explosion.transform.position = transform.position;
        }
    }

    [SerializeField] List<Light2D> lights;
    public DroneContainer droneContainer;

    float healthRatio;

    public void Start()
    {
        DroneManager.Instance.droneHangars.Add(this);
        BuildingSystem.Instance.droneHangarsBuilt++;
    }
    
    public void OnDestroy()
    {
        DroneManager.Instance.droneHangars.Remove(this);
        BuildingSystem.Instance.droneHangarsBuilt--;
        if (UpgradePanelManager.Instance != null)
        {
            // Close upgrade panel if it's open for this hangar
            if (UpgradePanelManager.Instance.currentPanel is UiUpgradePanel_DroneHangar dronePanel &&
                dronePanel.droneContainer == this.droneContainer)
            {
                UpgradePanelManager.Instance.ClosePanel(StructureType.DroneHangar);
            }
        }
    }

    public void PowerOn()
    {
        //Debug.Log("Drone Hangar Power On");
        spriteLightSprite.enabled = true;
        foreach (var light in lights)
            light.gameObject.SetActive(true);

        BuildingSystem.Instance.UnlockStructures(droneHangarSO, false);
        StopAllCoroutines();
        StartCoroutine(RotateRadarDish());
        droneContainer.hasPower = true;
    }

    public void PowerOff()
    {
        //Debug.Log("Drone Hangar Power Off");
        spriteLightSprite.enabled = false;
        foreach (var light in lights)
            light.gameObject.SetActive(false);

        BuildingSystem.Instance.LockStructures(droneHangarSO, false);
        StopAllCoroutines();
        droneContainer.hasPower = false;
    }

    IEnumerator RotateRadarDish()
    {
        //Debug.Log("Starting radar dish rotation");
        while (true)
        {
            radarDishSprite.transform.Rotate(Vector3.forward, radarDishRotateSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
    }
}
