using DCL.Interface;
using System;
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

        public ulong timestamp;
    }

    [SerializeField] internal float timeToFade = 10;
    [SerializeField] internal float fadeDuration = 5;

    [SerializeField] internal TextMeshProUGUI username;
    [SerializeField] internal TextMeshProUGUI body;

    [SerializeField] internal Color worldMessageColor = Color.white;
    [SerializeField] internal Color privateMessageColor = Color.white;
    [SerializeField] internal Color systemColor = Color.white;

    public Model model { get; private set; }

    public void Populate(Model chatEntryModel)
    {
        this.model = chatEntryModel;

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

        if (this.enabled)
            group.alpha = 0;
    }

    [SerializeField] CanvasGroup group;

    public void SetFadeout(bool enabled)
    {
        if (!enabled)
        {
            group.alpha = 1;
            this.enabled = false;
            return;
        }

        this.enabled = true;
    }

    private void Update()
    {
        double fadeTime = (double)(model.timestamp / 1000.0) + timeToFade;
        double currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        if (currentTime > fadeTime)
        {
            double timeSinceFadeTime = currentTime - fadeTime;
            group.alpha = Mathf.Clamp01(1 - (float)(timeSinceFadeTime / fadeDuration));
        }
        else
        {
            group.alpha += (1 - group.alpha) * 0.05f;
        }
    }

    string RemoveTabs(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

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
