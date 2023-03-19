using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    /// <summary>
    /// Work around for value types in <see cref="ListViewController"/>
    /// this.itemsSource.Add((object) null)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ListViewValueTypeContainer<T> where T: struct
    {
        private T Value { get; set; }

        public static implicit operator T(ListViewValueTypeContainer<T> container) =>
            container?.Value ?? default;

        public static implicit operator ListViewValueTypeContainer<T>(T value) =>
            new () { Value = value };
    }
}
