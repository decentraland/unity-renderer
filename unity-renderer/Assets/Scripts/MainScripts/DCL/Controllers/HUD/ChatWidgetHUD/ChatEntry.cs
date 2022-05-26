using UnityEngine;

public abstract class ChatEntry : MonoBehaviour
{
    public abstract ChatEntryModel Model { get; }
    public abstract void Populate(ChatEntryModel model);
    public abstract void SetFadeout(bool enabled);
    public abstract void DeactivatePreview();
    public abstract void ActivatePreview();
    public abstract void ActivatePreviewInstantly();
    public abstract void DeactivatePreviewInstantly();
}