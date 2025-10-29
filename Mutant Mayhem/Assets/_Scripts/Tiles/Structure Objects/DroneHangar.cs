using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DroneHangar : MonoBehaviour, IPowerConsumer
{
    [SerializeField] StructureSO droneHangarSO;
    [SerializeField] SpriteRenderer spriteLightSprite;
    [SerializeField] SpriteRenderer radarDishSprite;
    [SerializeField] float radarDishRotateSpeed = 180f;
    [SerializeField] List<Light2D> lights;
    [SerializeField] DroneContainer droneContainer;

    Coroutine radarCoroutine;

    public void PowerOn()
    {
        spriteLightSprite.enabled = true;
        foreach (var light in lights)
            light.gameObject.SetActive(true);

        BuildingSystem.Instance.UnlockStructures(droneHangarSO, false);
        StopAllCoroutines();
        radarCoroutine = StartCoroutine(RotateRadarDish());
        droneContainer.hasPower = true;
    }

    public void PowerOff()
    {
        spriteLightSprite.enabled = false;
        foreach (var light in lights)
            light.gameObject.SetActive(false);

        BuildingSystem.Instance.LockStructures(droneHangarSO, false);
        StopAllCoroutines();
        droneContainer.hasPower = false;
    }

    IEnumerator RotateRadarDish()
    {
        Debug.Log("Starting radar dish rotation");
        while (true)
        {
            radarDishSprite.transform.Rotate(Vector3.forward, radarDishRotateSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
    }
}
