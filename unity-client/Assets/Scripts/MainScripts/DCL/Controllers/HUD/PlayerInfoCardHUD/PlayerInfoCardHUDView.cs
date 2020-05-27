using System;
using DCL.Helpers;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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

    [System.Serializable]
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

    [Space] [SerializeField] internal Image avatarPicture;
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
    internal UserProfile currentUserProfile;
    private UnityAction<bool> toggleChangedDelegate => (x) => UpdateTabs();
    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    private MouseCatcher mouseCatcher;

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


        FriendsController.i.OnUpdateFriendship -= OnFriendStatusUpdated;
        FriendsController.i.OnUpdateFriendship += OnFriendStatusUpdated;

        if (InitialSceneReferences.i != null)
        {
            var mouseCatcher = DCL.InitialSceneReferences.i.mouseCatcher;

            if (mouseCatcher != null)
            {
                this.mouseCatcher = mouseCatcher;
                mouseCatcher.OnMouseDown += OnPointerDown;
            }
        }
    }

    private void OnFriendStatusUpdated(string userId, FriendshipAction action)
    {
        if (currentUserProfile == null)
            return;

        UpdateFriendButton();
    }

    public void SetCardActive(bool active)
    {
        if (active && mouseCatcher != null)
            mouseCatcher.UnlockCursor();

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

    public void SetUserProfile(UserProfile userProfile)
    {
        Assert.IsTrue(userProfile != null, "userProfile can't be null");

        currentUserProfile = userProfile;
        name.text = currentUserProfile.userName;
        description.text = currentUserProfile.description;
        avatarPicture.sprite = currentUserProfile.faceSnapshot;

        ClearCollectibles();
        var collectiblesIds = currentUserProfile.GetInventoryItemsIds();
        for (int index = 0; index < collectiblesIds.Length; index++)
        {
            string collectibleId = collectiblesIds[index];
            WearableItem collectible = CatalogController.wearableCatalog.Get(collectibleId);
            if (collectible == null) continue;

            var playerInfoCollectible =
                collectiblesFactory.Instantiate<PlayerInfoCollectibleItem>(collectible.rarity,
                    wearablesContainer.transform);
            if (playerInfoCollectible == null) continue;
            playerInfoCollectibles.Add(playerInfoCollectible);
            playerInfoCollectible.Initialize(collectible);
        }

        emptyCollectiblesImage.SetActive(collectiblesIds.Length == 0);

        SetIsBlocked(IsBlocked(userProfile.userId));

        UpdateFriendButton();
    }

    private void UpdateFriendButton()
    {
        bool canUseFriendButton = FriendsController.i != null && FriendsController.i.isInitialized && currentUserProfile.hasConnectedWeb3;

        friendStatusContainer.SetActive(canUseFriendButton);
        if (!canUseFriendButton)
        {
            addFriendButton.gameObject.SetActive(false);
            alreadyFriendsContainer.SetActive(false);
            requestSentButton.gameObject.SetActive(false);
            return;
        }

        if (currentUserProfile == null)
            return;

        var status = FriendsController.i.GetUserStatus(currentUserProfile.userId);

        addFriendButton.gameObject.SetActive(false);
        alreadyFriendsContainer.SetActive(false);
        requestReceivedContainer.SetActive(false);
        requestSentButton.gameObject.SetActive(false);

        switch (status.friendshipStatus)
        {
            case FriendshipStatus.NONE:
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
        gameObject.SetActive(visible);
    }

    private void ClearCollectibles()
    {
        for (var i = playerInfoCollectibles.Count - 1; i >= 0; i--)
        {
            var playerInfoCollectible = playerInfoCollectibles[i];
            playerInfoCollectibles.RemoveAt(i);
            Destroy(playerInfoCollectible.gameObject);
        }
    }

    internal bool IsBlocked(string userId)
    {
        if (ownUserProfile == null || ownUserProfile.blocked == null)
            return false;

        for (int i = 0; i < ownUserProfile.blocked.Count; i++)
        {
            if (ownUserProfile.blocked[i] == userId) return true;
        }

        return false;
    }

    private void OnPointerDown()
    {
        hideCardButton.onClick.Invoke();
    }


    private void OnDestroy()
    {
        if (mouseCatcher != null)
            mouseCatcher.OnMouseDown -= OnPointerDown;
    }
}