using System;
using System.Collections.Generic;
using DCL.Helpers;
using TMPro;
using UnityEngine;
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

    [Space]
    [SerializeField]
    internal Image avatarPicture;
    [SerializeField] internal Image blockedAvatarOverlay;
    [SerializeField] internal TextMeshProUGUI name;

    [Header("Passport")]
    [SerializeField]
    internal TextMeshProUGUI description;

    [Header("Trade")]
    [SerializeField]
    private RectTransform wearablesContainer;

    [Header("Block")]
    [SerializeField]
    internal Button reportPlayerButton;
    [SerializeField] internal Button blockPlayerButton;
    [SerializeField] internal Button unblockPlayerButton;

    internal readonly List<PlayerInfoCollectibleItem> playerInfoCollectibles = new List<PlayerInfoCollectibleItem>(10);
    internal UserProfile currentUserProfile;
    private UnityAction<bool> toggleChangedDelegate => (x) => UpdateTabs();
    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    public static PlayerInfoCardHUDView CreateView()
    {
        return Instantiate(Resources.Load<GameObject>(PREFAB_PATH)).GetComponent<PlayerInfoCardHUDView>();
    }

    public void Initialize(UnityAction cardClosedCallback, UnityAction reportPlayerCallback, UnityAction blockPlayerCallback, UnityAction unblockPlayerCallback)
    {
        hideCardButton.onClick.RemoveAllListeners();
        hideCardButton.onClick.AddListener(cardClosedCallback);

        reportPlayerButton.onClick.RemoveAllListeners();
        reportPlayerButton.onClick.AddListener(reportPlayerCallback);

        blockPlayerButton.onClick.RemoveAllListeners();
        blockPlayerButton.onClick.AddListener(blockPlayerCallback);

        unblockPlayerButton.onClick.RemoveAllListeners();
        unblockPlayerButton.onClick.AddListener(unblockPlayerCallback);

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
    }

    public void SetCardActive(bool active)
    {
        if (active)
        {
            Utils.UnlockCursor();
        }
        else
        {
            Utils.LockCursor();
        }

        cardCanvas.enabled = active;
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

            var playerInfoCollectible = collectiblesFactory.Instantiate<PlayerInfoCollectibleItem>(collectible.rarity, wearablesContainer.transform);
            if (playerInfoCollectible == null) continue;
            playerInfoCollectibles.Add(playerInfoCollectible);
            playerInfoCollectible.Initialize(collectible);
        }

        SetIsBlocked(IsBlocked(userProfile.userId));
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
        for (int i = 0; i < ownUserProfile.blocked.Count; i++)
        {
            if (ownUserProfile.blocked[i] == userId) return true;
        }
        return false;
    }
}