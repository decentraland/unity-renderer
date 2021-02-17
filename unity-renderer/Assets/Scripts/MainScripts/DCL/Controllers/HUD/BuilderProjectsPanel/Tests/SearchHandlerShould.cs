using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tests
{
    public class SearchHandlerShould
    {
        [Test]
        public void SearchCorrectly()
        {
            List<SearchItem_Mock> list = new List<SearchItem_Mock>
            {
                new SearchItem_Mock(new[] {"temptation", "good"}),
                new SearchItem_Mock(new[] {"temporal", "variable"}),
                new SearchItem_Mock(new[] {"20 degrees", "temperature"}),
                new SearchItem_Mock(new[] {"I dont", "know"}),
                new SearchItem_Mock(new[] {"empty"})
            };

            const string searchText = "temp";
            var result = SearchHelper.Search(searchText, list);

            Assert.IsTrue(result.Contains(list[0]),$"keywords {list[0]} should appear while searching {searchText}");
            Assert.IsTrue(result.Contains(list[1]),$"keywords {list[1]} should appear while searching {searchText}");
            Assert.IsTrue(result.Contains(list[2]),$"keywords {list[2]} should appear while searching {searchText}");
            Assert.IsFalse(result.Contains(list[3]),$"keywords {list[3]} shouldn't appear while searching {searchText}");
            Assert.IsFalse(result.Contains(list[4]),$"keywords {list[4]} shouldn't appear while searching {searchText}");
        }

        [Test]
        public void SortCorrectly()
        {
            List<SortItem_Mock> list = new List<SortItem_Mock>
            {
                new SortItem_Mock(5),
                new SortItem_Mock(23),
                new SortItem_Mock(-1),
                new SortItem_Mock(0),
                new SortItem_Mock(-23),
                new SortItem_Mock(5),
                new SortItem_Mock(2)
            };

            SearchHelper.Sort("",list,true);

            var prev = list[0].value;
            for (int i = 1; i < list.Count; i++)
            {
                Assert.LessOrEqual(list[i].value, prev);
                prev = list[i].value;
            }

            SearchHelper.Sort("",list,false);

            prev = list[0].value;
            for (int i = 1; i < list.Count; i++)
            {
                Assert.GreaterOrEqual(list[i].value, prev);
                prev = list[i].value;
            }
        }

        [Test]
        public void ApplyFilterCorrectly()
        {
            List<ItemForHandler_Mock> list = new List<ItemForHandler_Mock>
            {
                new ItemForHandler_Mock("ABC", -1),
                new ItemForHandler_Mock("DEF", -10),
                new ItemForHandler_Mock("GHI", 30),
                new ItemForHandler_Mock("JKL", 5),
            };

            bool filterByOddValue = false;

            var searchHandler = new SearchHandler<ItemForHandler_Mock>(
                sortingTypes: new []{"NAME","VALUE"},
                filterPredicate: (item =>
                {
                    int expected = filterByOddValue ? 0 : 1;
                    return item.value % 2 == expected;
                }));

            searchHandler.SetSearchableList(list);

            bool filteredByOddCalled = false;
            void ResultFilterOdd(List<ItemForHandler_Mock> oddFiltered)
            {
                filteredByOddCalled = true;
                foreach (var item in oddFiltered)
                {
                    Assert.AreEqual(0, item.value % 2);
                }
            }

            searchHandler.OnSearchChanged += ResultFilterOdd;
            filterByOddValue = true;
            searchHandler.NotifyFilterChanged();
            Assert.IsTrue(filteredByOddCalled);
        }

        [Test]
        public void ApplySortCorrectly()
        {
            List<ItemForHandler_Mock> list = new List<ItemForHandler_Mock>
            {
                new ItemForHandler_Mock("ABC", -1),
                new ItemForHandler_Mock("DEF", -10),
                new ItemForHandler_Mock("GHI", 30),
                new ItemForHandler_Mock("JKL", 5),
            };

            var searchHandler = new SearchHandler<ItemForHandler_Mock>(
                sortingTypes: new []{"NAME","VALUE"},filterPredicate: null);

            searchHandler.SetSearchableList(list);

            bool sortCalled = false;
            void ResultFilterOdd(List<ItemForHandler_Mock> sortedList)
            {
                sortCalled = true;
                Assert.AreEqual("JKL", sortedList[0].name);
                Assert.AreEqual("GHI", sortedList[1].name);
                Assert.AreEqual("DEF", sortedList[2].name);
                Assert.AreEqual("ABC", sortedList[3].name);
            }

            searchHandler.OnSearchChanged += ResultFilterOdd;
            searchHandler.NotifySortOrderChanged(false);
            Assert.IsTrue(sortCalled);
        }

        class SearchItem_Mock : ISearchable
        {
            public string[] keywords { get; }

            public SearchItem_Mock(string[] keywords)
            {
                this.keywords = keywords;
            }

            public override string ToString()
            {
                return String.Join(",", keywords);
            }
        }

        class SortItem_Mock : ISortable<SortItem_Mock>
        {
            public int value;

            public SortItem_Mock(int someValue)
            {
                value = someValue;
            }

            public int Compare(string sortType, bool isDescendingOrder, SortItem_Mock other)
            {
                if (isDescendingOrder)
                {
                    return other.value - value;
                }

                return value - other.value;
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }

        class ItemForHandler_Mock : ISearchable, ISortable<ItemForHandler_Mock>
        {
            public string[] keywords { get; }
            public int value;
            public string name;

            public ItemForHandler_Mock(string name, int someValue)
            {
                keywords = new[] {name};
                this.value = someValue;
                this.name = name;
            }

            public int Compare(string sortType, bool isDescendingOrder, ItemForHandler_Mock other)
            {
                switch (sortType)
                {
                    case "NAME":
                        return isDescendingOrder ?
                            String.Compare(keywords[0], other.keywords[0], StringComparison.Ordinal)
                            : String.Compare(other.keywords[0], keywords[0], StringComparison.Ordinal);
                    case "VALUE":
                        return isDescendingOrder ? value.CompareTo(other.value) : other.value.CompareTo(other);
                }

                return 0;
            }
        }
    }
}