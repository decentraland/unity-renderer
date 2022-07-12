using System.Collections;
using DCL.Chat.Channels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Chat.HUD
{
    public class SearchChannelsWindowComponentViewShould
    {
        private SearchChannelsWindowComponentView view;

        [SetUp]
        public void SetUp()
        {
            view = SearchChannelsWindowComponentView.Create();
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
            view.Set(new Channel("bleh", 7, 4, false, false, "desc", 0));
            
            Assert.AreEqual(1, view.channelList.Count());
            Assert.AreEqual("Results (1)", view.resultsHeaderLabel.text);
            var entry = view.channelList.Get("bleh");
            Assert.AreEqual("#bleh", entry.nameLabel.text);
            Assert.AreEqual(false, entry.joinedContainer.activeSelf);
            Assert.AreEqual("4 members", entry.memberCountLabel.text);
        }
        
        [Test]
        public void ShowJoinedChannel()
        {
            view.Show();
            view.Set(new Channel("bleh", 7, 4, true, false, "desc", 0));
            
            Assert.AreEqual(1, view.channelList.Count());
            Assert.AreEqual("Results (1)", view.resultsHeaderLabel.text);
            var entry = view.channelList.Get("bleh");
            Assert.AreEqual("#bleh", entry.nameLabel.text);
            Assert.AreEqual(true, entry.joinedContainer.activeSelf);
            Assert.AreEqual("4 members", entry.memberCountLabel.text);
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
            view.Set(new Channel("bleh", 7, 4, false, false, "desc", 0));
            view.Set(new Channel("foo", 2, 9, false, false, "desc", 0));
            view.Set(new Channel("bar", 0, 5, false, false, "desc", 0));
            
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
        }
        
        [Test]
        public void HideLoading()
        {
            view.HideLoading();
            
            Assert.AreEqual(false, view.loadingContainer.activeSelf);
            Assert.AreEqual(true, view.channelList.gameObject.activeSelf);
        }

        [TestCase("bleh")]
        [TestCase("hey")]
        public void SearchAnything(string text)
        {
            var triggeredSearch = "";
            view.OnSearchUpdated += s => triggeredSearch = s;
            view.searchBar.SubmitSearch(text);
            view.Set(new Channel(text, 1, 42, false, false, "desc", 0));
            
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
            view.Set(new Channel("bleh", 1, 42, false, false, "desc", 0));
            
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
    }
}