using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Let to set a dynamic sensitivity value depending on the height of the content container.
/// Call to RecalculateSensitivity() function to calculate and apply the new sensitivity value.
/// </summary>
public class DynamicScrollSensitivity : MonoBehaviour
{
    [Tooltip("Scroll Rect component that will be modified.")]
    public ScrollRect mainScrollRect;
    [Tooltip("Min value for the calculated scroll sensitivity.")]
    public float minSensitivity = 5f;
    [Tooltip("Max value for the calculated scroll sensitivity.")]
    public float maxSensitivity = 50f;
    [Tooltip("Multiplier applied to the sensitivity value (sensitivityMultiplier = 1 for not applying).")]
    public float sensitivityMultiplier = 0.5f;
    [Tooltip("True to recalculate each time the game object is enabled.")]
    public bool recalculateOnEnable = true;

    private Coroutine recalculateCoroutine = null;

    private void OnEnable()
    {
        if (recalculateOnEnable)
            RecalculateSensitivity();
    }

    private void OnDestroy() { KillCurrentCoroutine(); }

    /// <summary>
    /// Recalculate the scroll sensitivity value depending on the current height of the content container.
    /// </summary>
    public void RecalculateSensitivity()
    {
        KillCurrentCoroutine();
        recalculateCoroutine = CoroutineStarter.Start(RecalculateCoroutine());
    }

    private IEnumerator RecalculateCoroutine()
    {
        // We need to wait for a frame for having available the correct height of the contentContainer after the OnEnable event
        yield return null;

        if (mainScrollRect.content != null)
        {
            float newSensitivity = (mainScrollRect.content.rect.height * minSensitivity / mainScrollRect.viewport.rect.height) * sensitivityMultiplier;
            mainScrollRect.scrollSensitivity = Mathf.Clamp(newSensitivity, minSensitivity, maxSensitivity);
        }
    }

    private void KillCurrentCoroutine()
    {
        if (recalculateCoroutine == null)
            return;

        CoroutineStarter.Stop(recalculateCoroutine);
        recalculateCoroutine = null;
    }
}