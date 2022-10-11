using System;

public interface IAvatarEditorHUDAnimationController : IDisposable
{
    void OnSelectWearable(string wearableId);
    void AvatarAppearFeedback(AvatarModel avatarModelToUpdate);
    void Initialize(AvatarEditorHUDView avatarEditorHUDView);
}
