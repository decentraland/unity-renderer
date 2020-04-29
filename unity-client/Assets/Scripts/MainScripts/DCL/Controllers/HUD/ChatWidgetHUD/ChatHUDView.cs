using DCL.Helpers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
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


    public void AddEntry(ChatEntry.Model chatEntryModel)
    {
        var chatEntryGO = Instantiate(Resources.Load("Chat Entry") as GameObject, chatEntriesContainer);
        ChatEntry chatEntry = chatEntryGO.GetComponent<ChatEntry>();

        chatEntry.Populate(chatEntryModel);

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
