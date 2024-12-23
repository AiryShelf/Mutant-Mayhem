using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeRenderers : MonoBehaviour
{
    public List<GameObject> targetGameObjects;
    public float fadeDuration = 1f;
    [SerializeField] bool startFaded;
    [SerializeField] bool deactivateWithFade = true;

    private List<Renderer> _renderers = new List<Renderer>();
    private List<Color> _originalColors = new List<Color>();

    void Start()
    {
        // Populate the list of renderers
        InitializeRenderers();

        if (startFaded)
            StartFaded();
    }

    public void InitializeRenderers()
    {
        _renderers.Clear();
        _originalColors.Clear();

        foreach (var targetGameObject in targetGameObjects)
        {
            if (targetGameObject == null) continue;

            // Find all renderers on the GameObject and its children
            Renderer[] renderers = targetGameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (!_renderers.Contains(renderer))
                {
                    _renderers.Add(renderer);
                    _originalColors.Add(renderer.material.color);
                }
            }
        }
    }

    void StartFaded()
    {
        foreach(Renderer renderer in _renderers)
        {
            Color color = renderer.material.color;
            color.a = 0;
            renderer.material.color = color;
        }
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0f));
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f));
    }

    IEnumerator Fade(float targetAlpha)
    {
        float fadeTime = 0f;

        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            float t = fadeTime / fadeDuration;

            for (int i = 0; i < _renderers.Count; i++)
            {
                if (_renderers[i] == null) continue;

                if (targetAlpha > 0)
                    _renderers[i].gameObject.SetActive(true);

                Color color = _originalColors[i];
                if (targetAlpha == 0f)
                {
                    color.a = Mathf.Lerp(color.a, 0f, t);
                }
                else
                {
                    float originalAlpha = _originalColors[i].a;
                    color.a = Mathf.Lerp(0f, originalAlpha, t);
                }

                _renderers[i].material.color = color;
            }

            yield return null;
        }

        // Ensure the final alpha is set
        for (int i = 0; i < _renderers.Count; i++)
        {
            if (_renderers[i] == null) continue;

            Color color = _originalColors[i];
            color.a = targetAlpha == 0f ? 0f : _originalColors[i].a;
            _renderers[i].material.color = color;

            if (deactivateWithFade)
            {
                if (targetAlpha <= 0)
                    _renderers[i].gameObject.SetActive(false);
            }
        }
    }
}
