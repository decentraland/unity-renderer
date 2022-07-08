using System;
using DCL.Chat.Channels;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class SearchChannelsWindowComponentView : BaseComponentView, ISearchChannelsWindowView
    {
        [SerializeField] internal CollapsablePublicChannelListComponentView channelList;
        [SerializeField] internal TMP_Text resultsHeaderLabel;
        [SerializeField] internal GameObject loadingContainer;
        [SerializeField] internal ScrollRect scroll;
        [SerializeField] internal SearchBarComponentView searchBar;

        private bool isLayoutDirty;
        
        public event Action<string> OnSearchUpdated;
        public event Action OnRequestMoreChannels;

        public RectTransform Transform => (RectTransform) transform;
        public int EntryCount => channelList.Count();
        public bool IsActive => gameObject.activeInHierarchy;

        public override void Awake()
        {
            base.Awake();
            searchBar.OnSearchText += s => OnSearchUpdated?.Invoke(s);
        }

        public override void Update()
        {
            base.Update();
            
            if (isLayoutDirty)
                ((RectTransform) scroll.transform).ForceUpdateLayout();
            isLayoutDirty = false;
        }

        public static SearchChannelsWindowComponentView Create()
        {
            return Instantiate(Resources.Load<SearchChannelsWindowComponentView>("SocialBarV1/ChannelSearchHUD"));
        }
        
        public void ClearAllEntries()
        {
            channelList.Clear(true);
            UpdateLayout();
            UpdateHeaders();
        }

        public void ShowLoading() => loadingContainer.SetActive(true);

        public void Set(Channel channel)
        {
            channelList.Set(channel.ChannelId,
                new PublicChatEntry.PublicChatEntryModel(channel.ChannelId, channel.Name, channel.LastMessageTimestamp));
            
            UpdateLayout();
            UpdateHeaders();
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public void ClearSearchInput() => searchBar.ClearSearch();

        public void HideLoading() => loadingContainer.SetActive(false);
        
        public override void RefreshControl()
        {
            throw new NotImplementedException();
        }

        private void UpdateLayout() => isLayoutDirty = true;
        
        private void UpdateHeaders()
        {
            resultsHeaderLabel.text = $"Results ({channelList.Count()})";
        }
    }
}