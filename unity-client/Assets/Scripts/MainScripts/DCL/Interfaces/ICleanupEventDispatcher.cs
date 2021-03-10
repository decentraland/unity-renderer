
namespace DCL
{
    public interface ICleanableEventDispatcher
    {
        System.Action<ICleanableEventDispatcher> OnCleanupEvent { get; set; }
    }
}