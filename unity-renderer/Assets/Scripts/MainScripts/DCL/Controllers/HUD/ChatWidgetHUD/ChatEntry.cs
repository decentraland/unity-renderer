using UnityEngine;

public abstract class ChatEntry : MonoBehaviour
{
    public abstract ChatEntryModel Model { get; }
    public abstract void Populate(ChatEntryModel model);
    public abstract void SetFadeout(bool enabled);
}