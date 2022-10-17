using System;

public interface IAvatarEditorHUDView : IDisposable
{
    public event Action<AvatarModel>OnAvatarAppearFeedback;
    public event Action OnRandomize;
    public event Action<string> WearableSelectorClicked;
}
