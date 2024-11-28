using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMothershipController : MonoBehaviour
{
    void Awake()
    {
        AugManager.Instance.Initialize();
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
