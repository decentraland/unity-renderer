using System;

public interface IAvatarModifierAreaFeedbackView : IDisposable
{
    void SetUp(BaseRefCounter<AvatarModifierAreaID> avatarAreaWarnings);
    void SetWorldMode(bool isWorld);
}

