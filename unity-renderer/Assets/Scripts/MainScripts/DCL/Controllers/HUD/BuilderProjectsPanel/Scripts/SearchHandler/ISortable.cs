public interface ISortable<in T>
{
    int Compare(string sortType, bool isDescendingOrder, T other);
}
