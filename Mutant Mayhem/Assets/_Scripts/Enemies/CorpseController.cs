using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseController : MonoBehaviour
{
    [SerializeField] protected float timeToStartFade;
    [SerializeField] protected float timeForFade;

    [Header("Set During Runtime by Health")]
    public string corpsePoolName = "";

    protected Color startColor;

    void Start()
    {
        if (InputManager.IsMobile())
        {
            timeToStartFade /= 2;
            timeForFade /= 1.5f;
        }   
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
    }

    protected IEnumerator WaitToFade()
    {
        yield return new WaitForSeconds(timeToStartFade);
        StartCoroutine(FadeOut());
    }

    protected virtual IEnumerator FadeOut()
    {
        yield return null;
    }
}
