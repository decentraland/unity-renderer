using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        Transform EmotesSectionTransform { get; }
        ICharacterPreviewController CharacterPreview { get; }
        bool isVisible { get; }

        void Initialize(ICharacterPreviewFactory characterPreviewFactory);
        void Dispose();
        void Show();
        void Hide();
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void PlayPreviewEmote(string emoteId);
        void ResetPreviewEmote();
        void UpdateAvatarPreview(AvatarModel avatarModel);
    }
}
