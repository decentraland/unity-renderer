using System;

[Serializable]
public class VoiceChatBarComponentModel : BaseComponentModel
{
    public string message;
    public bool isSomeoneTalking;
    public bool isJoined;
}
