using TMPro;
using UnityEngine;

internal class SpawnPointIndicatorMonoBehaviour : MonoBehaviour
{
    internal const float LOOKAT_INDICATOR_HEIGHT = 2;
    internal const float TEXT_HEIGHT = 0.3f;

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
        areaIndicator.position = position;
        lookAtIndicator.position = new Vector3(position.x, position.y + LOOKAT_INDICATOR_HEIGHT, position.z);

        var textPosition = areaTextTransform.position;
        textPosition.Set(position.x, textPosition.y, position.z);
        areaTextTransform.position = textPosition;
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

        var textPosition = areaTextTransform.position;
        textPosition.Set(textPosition.x, size.y + TEXT_HEIGHT, textPosition.z);
        areaTextTransform.position = textPosition;
    }

    public void SetName(in string name)
    {
        areaText.text = name;
    }
}