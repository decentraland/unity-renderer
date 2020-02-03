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
    [SerializeField] internal Image avatarPicture;
    [SerializeField] internal TextMeshProUGUI name;

    [Header("Passport")]
    [SerializeField] internal TextMeshProUGUI description;

    [Header("Trade")]
    [SerializeField] private RectTransform wearablesContainer;

    internal readonly List<PlayerInfoCollectibleItem> playerInfoCollectibles = new List<PlayerInfoCollectibleItem>(10);
    internal UserProfile currentUserProfile;
    private UnityAction<bool> toggleChangedDelegate => (x) => UpdateTabs();
    private UnityAction cardClosedCallback = () => { }; 

    public static PlayerInfoCardHUDView CreateView()
    {
        return Instantiate(Resources.Load<GameObject>(PREFAB_PATH)).GetComponent<PlayerInfoCardHUDView>();
    }

    private void Awake()
    {
        hideCardButton.onClick.AddListener(OnHideCardButton);
    }

    public void Initialize(UnityAction cardClosedCallback)
    {
        this.cardClosedCallback = cardClosedCallback;
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

    private void OnHideCardButton()
    {
        cardClosedCallback?.Invoke();
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
}