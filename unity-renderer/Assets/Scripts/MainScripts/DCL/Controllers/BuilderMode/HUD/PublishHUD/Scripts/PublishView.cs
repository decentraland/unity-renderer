using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Builder
{
    //TODO: this class is in process to be made
    public class PublishView : MonoBehaviour //BaseComponentView
    {

    public event Action OnCancel;
    public event Action<string, string> OnPublish;
    public event Action<string> OnSceneNameChange;

    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Button publishButton;
    [SerializeField] internal TMP_Text publishButtonText;
    [SerializeField] internal TMP_InputField sceneNameInput;
    [SerializeField] internal TMP_Text sceneNameValidationText;
    [SerializeField] internal TMP_InputField sceneDescriptionInput;
    [SerializeField] internal Image sceneScreenshot;
    [SerializeField] internal TMP_Text sceneNameCharCounterText;
    [SerializeField] internal int sceneNameCharLimit = 30;
    [SerializeField] internal TMP_Text sceneDescriptionCharCounterText;
    [SerializeField] internal int sceneDescriptionCharLimit = 140;

    private const string VIEW_PATH = "Common/PublicationDetailsView";

    public string currentSceneName => sceneNameInput.text;

    // public override void RefreshControl() { }
    }
}