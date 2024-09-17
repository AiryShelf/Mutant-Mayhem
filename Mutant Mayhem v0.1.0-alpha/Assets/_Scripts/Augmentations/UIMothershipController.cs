using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMothershipController : MonoBehaviour
{
    [SerializeField] Transform bkgTransform;
    [SerializeField] Material bkgMaterial;
    [SerializeField] Vector2 bkgMatParalaxDir;
    [SerializeField] float bkgMatParalaxSpeed;
    Vector2 currentOffset;

    AugManager augManager;

    void Awake()
    {
        augManager = AugManager.Instance;
        if (augManager == null)
        {
            Debug.LogError("Could not find augManager");
        }

        augManager.Initialize();
    }

    public void OnLaunch()
    {
        SceneManager.LoadScene(2);
    }

    public void OnReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }
}
