using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestSceneHotAreaUsers : IMapRendererTestSceneElementProvider
    {
        private class SyncList : IList
        {
            private readonly BaseDictionary<string, Player> dict;
            public readonly List<Player> list = new ();

            private int idCounter;

            public SyncList(BaseDictionary<string, Player> dict)
            {
                this.dict = dict;
            }

            public IEnumerator GetEnumerator() =>
                ((IEnumerable)list).GetEnumerator();

            public void CopyTo(Array array, int index)
            {
                ((ICollection)list).CopyTo(array, index);
            }

            public int Count => list.Count;

            public bool IsSynchronized => ((ICollection)list).IsSynchronized;

            public object SyncRoot => ((ICollection)list).SyncRoot;

            public int Add(object value)
            {
                var player = value as Player ?? new Player {id = idCounter.ToString()};

                dict.Add(player.id, player);
                idCounter++;
                return ((IList)list).Add(player);
            }

            public void Clear()
            {
                list.Clear();
            }

            public bool Contains(object value) =>
                ((IList)list).Contains(value);

            public int IndexOf(object value) =>
                ((IList)list).IndexOf(value);

            public void Insert(int index, object value)
            {
                ((IList)list).Insert(index, value);
            }

            public void Remove(object value)
            {
                ((IList)list).Remove(value);

                if (value is Player p)
                    dict.Remove(p.id);
            }

            public void RemoveAt(int index)
            {
                var element = list.ElementAtOrDefault(index);

                if (element != null)
                    dict.Remove(element.id);

                list.RemoveAt(index);
            }

            public bool IsFixedSize => ((IList)list).IsFixedSize;

            public bool IsReadOnly => ((IList)list).IsReadOnly;

            public object this[int index]
            {
                get => ((IList)list)[index];

                set => ((IList)list)[index] = value;
            }
        }

        private readonly BaseDictionary<string, Player> otherPlayers;
        private readonly SyncList list;
        private readonly Dictionary<VisualElement, EventCallback<ChangeEvent<Vector3>>> callbacks = new ();

        public MapRendererTestSceneHotAreaUsers(BaseDictionary<string, Player> otherPlayers)
        {
            this.otherPlayers = otherPlayers;
            list = new SyncList(otherPlayers);
        }

        public VisualElement GetElement()
        {
            var root = new VisualElement();

            VisualElement MakeItem()
            {
                return new Vector3Field("Position");
            }

            void BindItem(VisualElement vs, int index)
            {
                var vf = (Vector3Field)vs;
                vf.SetValueWithoutNotify(list.list[index].worldPosition);
                EventCallback<ChangeEvent<Vector3>> callback = evt => list.list[index].worldPosition = evt.newValue;
                callbacks[vf] = callback;
                vf.RegisterValueChangedCallback(callback);
            }

            void UnbindItem(VisualElement vs, int index)
            {
                var vf = (Vector3Field)vs;

                if (callbacks.TryGetValue(vf, out var callback))
                {
                    vf.UnregisterCallback(callback);
                    callbacks.Remove(vf);
                }
            }

            var listView = new ListView(list, 16, MakeItem, BindItem)
                {
                    unbindItem = UnbindItem,
                    showAddRemoveFooter = true,
                };

            root.Add(listView);

            return root;
        }
    }
}
