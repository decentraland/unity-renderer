using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class CallbackOnExternalClick : MonoBehaviour
{
    public UnityEvent callback;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                callback.Invoke();
            }
        }
    }
}