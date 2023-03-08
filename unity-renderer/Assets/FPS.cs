using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour
{
    [SerializeField] private float fpsRefreshTime = 0.1f;
    [SerializeField] private Rect location = new (5, 5, 85, 45);

    private WaitForSecondsRealtime waitForSecondsRealtime;
    private float fps;

    private void Awake() => waitForSecondsRealtime = new WaitForSecondsRealtime(fpsRefreshTime);

    private IEnumerator Start()
    {
        GUI.depth = 2;


        while (Application.isPlaying)
        {
            fps = 1f / Time.unscaledDeltaTime;
            yield return waitForSecondsRealtime;
        }
    }

    private void OnGUI()
    {
        var text = $"FPS: {Mathf.Round(fps)}";

        GUI.DrawTexture(location, Texture2D.linearGrayTexture, ScaleMode.StretchToFill);
        GUI.color = Color.black;
        GUI.skin.label.fontSize = 18;

        GUI.Label(location, text);
    }
}
