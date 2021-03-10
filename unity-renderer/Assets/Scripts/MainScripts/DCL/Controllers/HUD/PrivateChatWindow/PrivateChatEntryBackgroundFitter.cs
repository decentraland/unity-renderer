using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class PrivateChatEntryBackgroundFitter : MonoBehaviour
{
    public RectTransform rectTransform;
    public RectTransform parentContainerRectTransform;
    public TextMeshProUGUI messageText;
    public bool leftMessage = true;

    void Update()
    {
        Vector2 textSize = new Vector2(messageText.bounds.size.x + messageText.margin.x + messageText.margin.z,
                                    messageText.bounds.size.y + messageText.margin.y * 2);

        messageText.transform.localPosition = new Vector3(leftMessage ? (messageText.margin.x + messageText.margin.z) / 2 : 0f, messageText.transform.localPosition.y, 0);

        if (parentContainerRectTransform)
        {
            parentContainerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textSize.x);
            parentContainerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textSize.y);
            parentContainerRectTransform.ForceUpdateRectTransforms();
        }

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textSize.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textSize.y);
        rectTransform.ForceUpdateRectTransforms();
    }
}
