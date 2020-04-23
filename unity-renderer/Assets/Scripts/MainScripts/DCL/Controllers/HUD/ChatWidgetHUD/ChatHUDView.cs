using DCL.Helpers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using ChatMessage = ChatController.ChatMessage;
public class ChatHUDView : MonoBehaviour
{
    static string VIEW_PATH = "Chat Widget";
    public TMP_InputField inputField;
    public RectTransform chatEntriesContainer;

    public ScrollRect scrollRect;
    public ChatHUDController controller;
    [NonSerialized] public List<ChatEntry> entries = new List<ChatEntry>();

    public static ChatHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ChatHUDView>();
        return view;
    }


    public void Initialize(ChatHUDController controller, UnityAction<string> onSendMessage)
    {
        this.controller = controller;
        inputField.onSubmit.AddListener(onSendMessage);
    }

    public void ResetInputField()
    {
        inputField.text = "";
        inputField.caretColor = Color.white;
    }

    public void FocusInputField()
    {
        inputField.Select();
        inputField.ActivateInputField();
    }


    public void AddEntry(ChatMessage message)
    {
        var chatEntryGO = Instantiate(Resources.Load("Chat Entry") as GameObject, chatEntriesContainer);
        ChatEntry chatEntry = chatEntryGO.GetComponent<ChatEntry>();
        chatEntry.Populate(message);
        entries.Add(chatEntry);
        ForceUpdateLayout();
    }

    public void CleanAllEntries()
    {
        foreach (var entry in entries)
        {
            Destroy(entry.gameObject);
        }

        entries.Clear();
    }

    public void RepopulateAllChatMessages(List<ChatMessage> entriesList)
    {
        CleanAllEntries();

        int entriesCount = entriesList.Count;

        for (int i = 0; i < entriesCount; i++)
        {
            AddEntry(entriesList[i]);
        }
    }

    [ContextMenu("Force Layout Update")]
    public void ForceUpdateLayout()
    {
        Utils.InverseTransformChildTraversal<RectTransform>(
        (x) =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(x);
        },
        chatEntriesContainer);

        LayoutRebuilder.ForceRebuildLayoutImmediate(chatEntriesContainer);
    }
}
