namespace AvatarSystem
{
    public interface IVisibility
    {
        bool composedVisibility { get; }
        void SetVisible(bool visible);
    }
}