using DCL;
using DCL.Interface;
using UnityEngine;
using UnityGLTF;

public class RenderingController : MonoBehaviour
{
    public static RenderingController i { get; private set; }

    public void Awake()
    {
        i = this;
    }

    [ContextMenu("Disable Rendering")]
    public void DeactivateRendering()
    {
        DCLCharacterController.i.initialPositionAlreadySet = false;
        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
        MessagingBus.renderingIsDisabled = true;
        GLTFSceneImporter.renderingIsDisabled = true;
        DCLCharacterController.i.gameObject.SetActive(false);

    }

    [ContextMenu("Enable Rendering")]
    public void ActivateRendering()
    {
        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = true;
        MessagingBus.renderingIsDisabled = false;
        GLTFSceneImporter.renderingIsDisabled = false;
        DCLCharacterController.i.gameObject.SetActive(true);
        WebInterface.ReportControlEvent(new WebInterface.ActivateRenderingACK());
    }
}
