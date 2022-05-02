using System;
using UnityEngine;

public class WorldChatWindowViewMock : MonoBehaviour, IWorldChatWindowView
{
    public event Action OnClose;
    public event Action<string> OnOpenPrivateChat;
    public event Action<string> OnOpenPublicChannel;
    public event Action<string> OnUnfriend;
    public RectTransform Transform => (RectTransform) transform;
    public bool IsActive => gameObject.activeSelf;

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    public void Initialize(IChatController chatController, ILastReadMessagesService lastReadMessagesService)
    {
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    public void SetPrivateChat(PrivateChatModel model)
    {
    }

    public void RemovePrivateChat(string userId)
    {
    }

    public void SetPublicChannel(PublicChatChannelModel model)
    {
    }

    public void ShowPrivateChatsLoading()
    {
    }

    public void HidePrivateChatsLoading()
    {
    }

    public void Dispose() => Destroy(gameObject);
}