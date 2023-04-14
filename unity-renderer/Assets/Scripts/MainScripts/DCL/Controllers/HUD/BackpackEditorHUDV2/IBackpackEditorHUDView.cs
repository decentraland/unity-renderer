using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using UnityEngine;

namespace DCL.Backpack
{
    public interface IBackpackEditorHUDView
    {
        Transform EmotesSectionTransform { get; }
        bool isVisible { get; }

        void Initialize(ICharacterPreviewFactory characterPreviewFactory);
        void Dispose();
        void Show(bool instant = false);
        void Hide(bool instant = false);
        void SetAsFullScreenMenuMode(Transform parentTransform);
        void PlayPreviewEmote(string emoteId);
        void ResetPreviewEmote();
        void UpdateAvatarPreview(AvatarModel avatarModel);
    }
}
