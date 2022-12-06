using JetBrains.Annotations;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using System;

public interface IAvatarEditorHUDView : IDisposable
{
    ICharacterPreviewController CharacterPreview { get; }

    public event Action<AvatarModel>OnAvatarAppearFeedback;
    public event Action OnRandomize;
    public event Action<string> WearableSelectorClicked;
}
