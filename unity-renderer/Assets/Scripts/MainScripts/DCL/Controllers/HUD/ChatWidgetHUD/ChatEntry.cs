using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatEntry : MonoBehaviour
{
    public struct Model
    {
        public enum SubType
        {
            NONE,
            PRIVATE_FROM,
            PRIVATE_TO
        }

        public ChatMessage.Type messageType;
        public string bodyText;
        public string senderName;
        public string recipientName;
        public SubType subType;
    }

    [SerializeField] internal TextMeshProUGUI username;
    [SerializeField] internal TextMeshProUGUI body;

    public Color worldMessageColor = Color.white;
    public Color privateMessageColor = Color.white;
    public Color systemColor = Color.white;

    public Model message;

    public void Populate(Model chatEntryModel)
    {
        this.message = chatEntryModel;

        string userString = GetDefaultSenderString(chatEntryModel.senderName);

        if (chatEntryModel.subType == Model.SubType.PRIVATE_FROM)
        {
            userString = $"<b>[From {chatEntryModel.senderName}]:</b>";
        }
        else
        if (chatEntryModel.subType == Model.SubType.PRIVATE_TO)
        {
            userString = $"<b>[To {chatEntryModel.recipientName}]:</b>";
        }

        switch (chatEntryModel.messageType)
        {
            case ChatMessage.Type.PUBLIC:
                this.body.color = username.color = worldMessageColor;
                break;
            case ChatMessage.Type.PRIVATE:
                this.body.color = username.color = privateMessageColor;
                break;
            case ChatMessage.Type.SYSTEM:
                this.body.color = username.color = systemColor;
                break;
        }

        chatEntryModel.bodyText = RemoveTabs(chatEntryModel.bodyText);

        username.text = userString;
        body.text = $"{userString} {chatEntryModel.bodyText}";

        LayoutRebuilder.ForceRebuildLayoutImmediate(body.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(username.transform as RectTransform);
    }

    string RemoveTabs(string text)
    {
        //NOTE(Brian): ContentSizeFitter doesn't fare well with tabs, so i'm replacing these
        //             with spaces.
        return text.Replace("\t", "    ");
    }

    string GetDefaultSenderString(string sender)
    {
        if (!string.IsNullOrEmpty(sender))
            return $"<b>{sender}:</b>";

        return "";
    }

}
