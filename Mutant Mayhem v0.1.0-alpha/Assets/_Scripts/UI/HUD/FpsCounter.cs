using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
public class FpsCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float hudRefreshRate = 1f;
 
    private float timer;
 
    private void Update()
    {
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            fpsText.text = "FPS: " + fps;
            timer = Time.unscaledTime + hudRefreshRate;
        }
    }
}