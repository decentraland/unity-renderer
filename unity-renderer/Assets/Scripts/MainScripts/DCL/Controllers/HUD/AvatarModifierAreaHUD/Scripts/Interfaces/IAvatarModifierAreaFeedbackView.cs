using System;

public interface IAvatarModifierAreaFeedbackView : IDisposable
{
    void SetUp(BaseRefCounter<AvatarAreaWarningID> avatarAreaWarnings);

}

public enum AvatarAreaWarningID { HIDE_AVATAR, DISABLE_PASSPORT }
