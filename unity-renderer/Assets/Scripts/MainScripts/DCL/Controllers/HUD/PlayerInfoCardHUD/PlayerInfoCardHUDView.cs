using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerInfoCardHUDView : MonoBehaviour
{
    private const string PREFAB_PATH = "PlayerInfoCardHUD";

    public enum Tabs
    {
        Passport,
        Trade,
        Block
    }

    [Serializable]
    internal class TabsMapping
    {
        public GameObject container;
        public Toggle toggle;
        public Tabs tab;
    }

    [SerializeField] internal GenericFactory collectiblesFactory;
    [SerializeField] internal Canvas cardCanvas;
    [SerializeField] internal TabsMapping[] tabsMapping;
    [SerializeField] internal Button hideCardButton;

    [Space] [SerializeField] internal RawImage avatarPicture;
    [SerializeField] internal Image blockedAvatarOverlay;
    [SerializeField] internal TextMeshProUGUI name;

    [Header("Friends")] [SerializeField] internal GameObject friendStatusContainer;
    [SerializeField] internal Button requestSentButton;
    [SerializeField] internal Button addFriendButton;
    [SerializeField] internal GameObject alreadyFriendsContainer;
    [SerializeField] internal GameObject requestReceivedContainer;
    [SerializeField] internal Button acceptRequestButton;
    [SerializeField] internal Button rejectRequestButton;

    [Header("Passport")] [SerializeField] internal TextMeshProUGUI description;

    [Header("Trade")] [SerializeField] private RectTransform wearablesContainer;
    [SerializeField] private GameObject emptyCollectiblesImage;

    [Header("Block")] [SerializeField] internal Button reportPlayerButton;
    [SerializeField] internal Button blockPlayerButton;
    [SerializeField] internal Button unblockPlayerButton;

    internal readonly List<PlayerInfoCollectibleItem> playerInfoCollectibles = new List<PlayerInfoCollectibleItem>(10);
    private UnityAction<bool> toggleChangedDelegate => (x) => UpdateTabs();

    private MouseCatcher mouseCatcher;
    private HUDCanvasCameraModeController hudCanvasCameraModeController;

    private void Awake() { hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera); }

    public static PlayerInfoCardHUDView CreateView()
    {
        return Instantiate(Resources.Load<GameObject>(PREFAB_PATH)).GetComponent<PlayerInfoCardHUDView>();
    }

    public void Initialize(UnityAction cardClosedCallback,
        UnityAction reportPlayerCallback,
        UnityAction blockPlayerCallback,
        UnityAction unblockPlayerCallback,
        UnityAction addFriendCallback,
        UnityAction cancelInvitation,
        UnityAction acceptFriendRequest,
        UnityAction rejectFriendRequest)
    {
        hideCardButton.onClick.RemoveAllListeners();
        hideCardButton.onClick.AddListener(cardClosedCallback);

        reportPlayerButton.onClick.RemoveAllListeners();
        reportPlayerButton.onClick.AddListener(reportPlayerCallback);

        blockPlayerButton.onClick.RemoveAllListeners();
        blockPlayerButton.onClick.AddListener(blockPlayerCallback);

        unblockPlayerButton.onClick.RemoveAllListeners();
        unblockPlayerButton.onClick.AddListener(unblockPlayerCallback);

        addFriendButton.onClick.RemoveAllListeners();
        addFriendButton.onClick.AddListener(addFriendCallback);

        requestSentButton.onClick.RemoveAllListeners();
        requestSentButton.onClick.AddListener(cancelInvitation);

        acceptRequestButton.onClick.RemoveAllListeners();
        acceptRequestButton.onClick.AddListener(acceptFriendRequest);

        rejectRequestButton.onClick.RemoveAllListeners();
        rejectRequestButton.onClick.AddListener(rejectFriendRequest);

        for (int index = 0; index < tabsMapping.Length; index++)
        {
            var tab = tabsMapping[index];
            tab.toggle.onValueChanged.RemoveListener(toggleChangedDelegate);
            tab.toggle.onValueChanged.AddListener(toggleChangedDelegate);
        }

        for (int index = 0; index < tabsMapping.Length; index++)
        {
            if (tabsMapping[index].tab == Tabs.Passport)
            {
                tabsMapping[index].toggle.isOn = true;
                break;
            }
        }

        if (SceneReferences.i != null)
        {
            var mouseCatcher = DCL.SceneReferences.i.mouseCatcher;

            if (mouseCatcher != null)
            {
                this.mouseCatcher = mouseCatcher;
                mouseCatcher.OnMouseDown += OnPointerDown;
            }
        }
    }

    public void SetCardActive(bool active)
    {
        if (active && mouseCatcher != null)
        {
            mouseCatcher.UnlockCursor();
        }

        cardCanvas.enabled = active;
        CommonScriptableObjects.playerInfoCardVisibleState.Set(active);
    }

    private void UpdateTabs()
    {
        for (int index = 0; index < tabsMapping.Length; index++)
        {
            tabsMapping[index].container.SetActive(tabsMapping[index].toggle.isOn);
        }
    }

    public void SetFaceSnapshot(Texture2D texture)
    {
        avatarPicture.texture = texture;
    }

    public void SetName(string name) => this.name.text = name;

    public void SetDescription(string description) => this.description.text = description;

    public void HideFriendshipInteraction()
    {
        requestReceivedContainer.SetActive(false);
        addFriendButton.gameObject.SetActive(false);
        alreadyFriendsContainer.SetActive(false);
        requestSentButton.gameObject.SetActive(false);
    }

    public void UpdateFriendshipInteraction(bool canUseFriendButton,
        UserStatus status)
    {
        if (status == null)
        {
            requestReceivedContainer.SetActive(false);
            addFriendButton.gameObject.SetActive(false);
            alreadyFriendsContainer.SetActive(false);
            requestSentButton.gameObject.SetActive(false);
            return;
        }

        friendStatusContainer.SetActive(canUseFriendButton);

        if (!canUseFriendButton)
        {
            addFriendButton.gameObject.SetActive(false);
            alreadyFriendsContainer.SetActive(false);
            requestSentButton.gameObject.SetActive(false);
            return;
        }

        addFriendButton.gameObject.SetActive(false);
        alreadyFriendsContainer.SetActive(false);
        requestReceivedContainer.SetActive(false);
        requestSentButton.gameObject.SetActive(false);

        switch (status.friendshipStatus)
        {
            case FriendshipStatus.NOT_FRIEND:
                addFriendButton.gameObject.SetActive(true);
                break;
            case FriendshipStatus.FRIEND:
                alreadyFriendsContainer.SetActive(true);
                break;
            case FriendshipStatus.REQUESTED_FROM:
                requestReceivedContainer.SetActive(true);
                break;
            case FriendshipStatus.REQUESTED_TO:
                requestSentButton.gameObject.SetActive(true);
                break;
        }
    }

    public void SetIsBlocked(bool isBlocked)
    {
        unblockPlayerButton.gameObject.SetActive(isBlocked);
        blockedAvatarOverlay.gameObject.SetActive(isBlocked);
    }

    public void SetVisibility(bool visible)
    {
        if (gameObject.activeSelf != visible)
        {
            if (visible)
            {
                AudioScriptableObjects.dialogOpen.Play(true);
            }
            else
            {
                AudioScriptableObjects.dialogClose.Play(true);
            }
        }

        gameObject.SetActive(visible);
    }

    public void ClearCollectibles()
    {
        for (var i = playerInfoCollectibles.Count - 1; i >= 0; i--)
        {
            var playerInfoCollectible = playerInfoCollectibles[i];
            playerInfoCollectibles.RemoveAt(i);
            Destroy(playerInfoCollectible.gameObject);
        }
    }

    public void SetWearables(IEnumerable<WearableItem> wearables)
    {
        var emptyWearables = true;

        foreach (var wearable in wearables)
        {
            emptyWearables = false;
            var playerInfoCollectible =
                collectiblesFactory.Instantiate<PlayerInfoCollectibleItem>(wearable.rarity,
                    wearablesContainer.transform);
            if (playerInfoCollectible == null) continue;
            playerInfoCollectibles.Add(playerInfoCollectible);
            playerInfoCollectible.Initialize(wearable);
        }

        emptyCollectiblesImage.SetActive(emptyWearables);
    }

    private void OnPointerDown()
    {
        hideCardButton.onClick.Invoke();
    }

    private void OnDestroy()
    {
        hudCanvasCameraModeController?.Dispose();
        if (mouseCatcher != null)
            mouseCatcher.OnMouseDown -= OnPointerDown;
    }
}