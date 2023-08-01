using System.Collections;
using DCL.Chat.Channels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Social.Chat
{
    public class SearchChannelsWindowComponentViewShould
    {
        private SearchChannelsWindowComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = Object.Instantiate(
                AssetDatabase.LoadAssetAtPath<SearchChannelsWindowComponentView>(
                    "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Addressables/ChannelSearchHUD.prefab"));
        }

        [TearDown]
        public void TearDown()
        {
            view.Dispose();
        }

        [Test]
        public void Show()
        {
            view.Show();

            Assert.AreEqual(true, view.gameObject.activeSelf);
        }

        [Test]
        public void Hide()
        {
            view.Hide();

            Assert.AreEqual(false, view.gameObject.activeSelf);
        }

        [Test]
        public void ShowChannel()
        {
            view.Show();
            view.Set(new Channel("bleh", "blehName", 7, 4, false, false, "desc"));

            Assert.AreEqual(1, view.channelList.Count());
            Assert.AreEqual("Results (1)", view.resultsHeaderLabel.text);
            var entry = view.channelList.Get("bleh");
            Assert.AreEqual("#blehName", entry.nameLabel.text);
            Assert.AreEqual(false, entry.joinedContainer.activeSelf);
            Assert.AreEqual("4 members joined", entry.memberCountLabel.text);
        }

        [Test]
        public void ShowJoinedChannel()
        {
            view.Show();
            view.Set(new Channel("bleh", "blehName", 7, 4, true, false, "desc"));

            Assert.AreEqual(1, view.channelList.Count());
            Assert.AreEqual("Results (1)", view.resultsHeaderLabel.text);
            var entry = view.channelList.Get("bleh");
            Assert.AreEqual("#blehName", entry.nameLabel.text);
            Assert.AreEqual(true, entry.joinedContainer.activeSelf);
            Assert.AreEqual("4 members joined", entry.memberCountLabel.text);
        }

        [Test]
        public void ReplaceChannel()
        {
            ShowChannel();
            ShowChannel();
        }

        [Test]
        public void ShowManyChannels()
        {
            view.Show();
            view.Set(new Channel("bleh", "blehName", 7, 4, false, false, "desc"));
            view.Set(new Channel("foo", "fooName", 2, 9, false, false, "desc"));
            view.Set(new Channel("bar", "barName", 0, 5, false, false, "desc"));

            Assert.AreEqual(3, view.channelList.Count());
            Assert.AreEqual("Results (3)", view.resultsHeaderLabel.text);
        }

        [Test]
        public void ClearAllEntries()
        {
            ShowChannel();

            view.ClearAllEntries();

            Assert.AreEqual(0, view.channelList.Count());
            Assert.AreEqual("Results (0)", view.resultsHeaderLabel.text);
        }

        [Test]
        public void ShowLoading()
        {
            view.ShowLoading();

            Assert.AreEqual(true, view.loadingContainer.activeSelf);
            Assert.AreEqual(false, view.channelList.gameObject.activeSelf);
            Assert.AreEqual(false, view.resultsHeaderLabel.gameObject.activeSelf);
            Assert.AreEqual(false, view.createChannelOnSearchContent.activeSelf);
            Assert.AreEqual(false, view.loadMoreContent.activeSelf);
        }

        [Test]
        public void HideLoading()
        {
            view.SetCreateChannelButtonsActive(true);
            view.HideLoading();

            Assert.AreEqual(false, view.loadingContainer.activeSelf);
            Assert.AreEqual(true, view.channelList.gameObject.activeSelf);
            Assert.AreEqual(true, view.resultsHeaderLabel.gameObject.activeSelf);
            Assert.AreEqual(true, view.createChannelOnSearchContent.activeSelf);
            Assert.AreEqual(true, view.loadMoreContent.activeSelf);
        }

        [TestCase("bleh")]
        [TestCase("hey")]
        public void SearchAnything(string text)
        {
            var triggeredSearch = "";
            view.OnSearchUpdated += s => triggeredSearch = s;
            view.searchBar.SubmitSearch(text);
            view.Set(new Channel(text, text, 1, 42, false, false, "desc"));

            Assert.AreEqual(text, triggeredSearch);
            Assert.AreEqual(text, view.searchBar.Text);
            Assert.AreEqual("Did you mean?", view.resultsHeaderLabel.text);
        }

        [Test]
        public void ClearSearch()
        {
            SearchAnything("wo");
            view.ClearAllEntries();
            var triggeredSearch = "wo";
            view.OnSearchUpdated += s => triggeredSearch = s;

            view.ClearSearchInput();
            view.Set(new Channel("bleh", "blehName", 1, 42, false, false, "desc"));

            Assert.AreEqual("", triggeredSearch);
            Assert.AreEqual("", view.searchBar.Text);
            Assert.AreEqual("Results (1)", view.resultsHeaderLabel.text);
        }

        [UnityTest]
        public IEnumerator RequestMoreEntriesWhenScrollingDown()
        {
            var called = false;
            view.OnRequestMoreChannels += () => called = true;
            view.scroll.onValueChanged.Invoke(Vector2.one);
            view.scroll.onValueChanged.Invoke(Vector2.zero);

            yield return new WaitForSeconds(2);

            Assert.AreEqual(true, called);
        }

        [Test]
        public void ShowLoadMoreContainer()
        {
            view.ShowLoadingMore();

            Assert.IsTrue(view.loadMoreContainer.activeSelf);
        }

        [Test]
        public void HideLoadMoreContainer()
        {
            view.HideLoadingMore();

            Assert.IsFalse(view.loadMoreContainer.activeSelf);
        }

        [Test]
        public void ShowResultsHeaderContainer()
        {
            view.ShowResultsHeader();

            Assert.IsTrue(view.resultsHeaderLabelContainer.activeSelf);
        }

        [Test]
        public void HideResultsHeaderContainer()
        {
            view.HideResultsHeader();

            Assert.IsFalse(view.resultsHeaderLabelContainer.activeSelf);
        }

        [Test]
        public void ShowCreateChannelOnSearchContainer()
        {
            view.ShowCreateChannelOnSearch();

            Assert.IsTrue(view.createChannelOnSearchContainer.activeSelf);
        }

        [Test]
        public void HideCreateChannelOnSearchContainer()
        {
            view.HideCreateChannelOnSearch();

            Assert.IsFalse(view.createChannelOnSearchContainer.activeSelf);
        }

        [Test]
        public void TriggerLeaveChannel()
        {
            var leaveChannelId = "";
            view.OnLeaveChannel += s => leaveChannelId = s;
            view.Set(new Channel("bleh", "blehName", 7, 4, true, false, "desc"));

            view.channelList.Get("bleh").leaveButton.onClick.Invoke();

            Assert.AreEqual("bleh", leaveChannelId);
        }
    }
}
