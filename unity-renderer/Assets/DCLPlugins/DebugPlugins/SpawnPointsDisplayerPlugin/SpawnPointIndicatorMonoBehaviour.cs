using TMPro;
using UnityEngine;

internal class SpawnPointIndicatorMonoBehaviour : MonoBehaviour
{
    [SerializeField] internal Transform lookAtIndicator;
    [SerializeField] internal Transform areaIndicator;
    [SerializeField] internal TextMeshPro areaText;

    internal Transform areaTextTransform;

    public bool isDestroyed { private set; get; }

    private void Awake()
    {
        areaTextTransform = areaText.transform;
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    private void LateUpdate()
    {
        Vector3 lookAtDir = areaTextTransform.position - CommonScriptableObjects.cameraPosition;
        areaTextTransform.forward = lookAtDir.normalized;
    }

    public void SetPosition(in Vector3 position)
    {
        transform.position = position;
        areaIndicator.localPosition = Vector3.zero;

        lookAtIndicator.localPosition = new Vector3(0, lookAtIndicator.localPosition.y, 0);
        areaTextTransform.localPosition = new Vector3(0, areaTextTransform.localPosition.y, 0);
    }

    public void SetRotation(in Quaternion? rotation)
    {
        if (rotation.HasValue)
        {
            lookAtIndicator.gameObject.SetActive(true);
            lookAtIndicator.rotation = rotation.Value;
            return;
        }
        lookAtIndicator.gameObject.SetActive(false);
    }

    public void SetSize(in Vector3 size)
    {
        areaIndicator.localScale = size;
        var lookAtPosition = lookAtIndicator.localPosition;
        var textPosition = areaTextTransform.localPosition;
        lookAtPosition.y = textPosition.y = size.y * 0.5f;

        lookAtIndicator.localPosition = lookAtPosition;
        areaTextTransform.localPosition = textPosition;
    }

    public void SetName(in string name)
    {
        areaText.text = name;
    }
}