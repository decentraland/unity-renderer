using UnityEngine;

internal interface IUnpublishListener
{
    void OnUnpublishResult(PublishSceneResultPayload result);
}