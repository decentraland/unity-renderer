using DCL.Interface;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IDimensionSelectorComponentView
{
    /// <summary>
    /// Set the current dimension label.
    /// </summary>
    /// <param name="dimension">Current dimension.</param>
    void SetCurrentDimension(string dimension);

    /// <summary>
    /// Set the available dimensions component with a list of dimensions.
    /// </summary>
    /// <param name="dimensions">List of dimensions (model) to be loaded.</param>
    void SetAvailableDimensions(List<DimensionRowComponentModel> dimensions);
}

public class DimensionSelectorComponentView : BaseComponentView, IDimensionSelectorComponentView, IComponentModelConfig
{
    [Header("Assets References")]
    [SerializeField] internal DimensionRowComponentView dimensionRowPrefab;

    [Header("Prefab References")]
    [SerializeField] internal TMP_Text currentDimensionText;
    [SerializeField] internal GridContainerComponentView availableDimensions;
    [SerializeField] internal Button modalBackgroundButton;
    [SerializeField] internal ButtonComponentView closeCardButton;
    [SerializeField] internal InputAction_Trigger closeAction;

    [Header("Configuration")]
    [SerializeField] internal DimensionSelectorComponentModel model;
    [SerializeField] internal Color colorForEvenRows;
    [SerializeField] internal Color colorForOddRows;

    public override void Awake()
    {
        base.Awake();

        if (closeCardButton != null)
            closeCardButton.onClick.AddListener(CloseModal);

        if (closeAction != null)
            closeAction.OnTriggered += OnCloseActionTriggered;

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.AddListener(CloseModal);
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (DimensionSelectorComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetCurrentDimension(model.currentDimensionName);
        availableDimensions.RefreshControl();
    }
    public override void Dispose()
    {
        base.Dispose();

        if (closeCardButton != null)
            closeCardButton.onClick.RemoveAllListeners();

        if (closeAction != null)
            closeAction.OnTriggered -= OnCloseActionTriggered;

        if (modalBackgroundButton != null)
            modalBackgroundButton.onClick.RemoveAllListeners();
    }

    public void SetCurrentDimension(string dimension)
    {
        model.currentDimensionName = dimension;

        // Set the current dimension in the modal header
        if (currentDimensionText != null)
            currentDimensionText.text = $"You are in <b>{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dimension.ToLower())}</b>";

        // Search the current dimension in the available ones and set it as connected
        var instantiatedDimensions = availableDimensions.GetItems();
        foreach (DimensionRowComponentView dimensionRow in instantiatedDimensions)
        {
            dimensionRow.SetAsConnected(dimensionRow.model.name == dimension);
        }
    }

    public void SetAvailableDimensions(List<DimensionRowComponentModel> dimensions)
    {
        List<BaseComponentView> dimensionsToAdd = new List<BaseComponentView>();
        bool isAnOddRow = true;
        foreach (DimensionRowComponentModel dimension in dimensions)
        {
            DimensionRowComponentView newDimensionRow = GameObject.Instantiate(dimensionRowPrefab);
            newDimensionRow.Configure(dimension);
            newDimensionRow.SetRowColor(isAnOddRow ? colorForOddRows : colorForEvenRows);
            newDimensionRow.onWarpInClick.AddListener(() =>
            {
                WebInterface.SendChatMessage(new ChatMessage
                {
                    messageType = ChatMessage.Type.NONE,
                    recipient = string.Empty,
                    body = $"/changerealm {dimension.name}"
                });
            });
            dimensionsToAdd.Add(newDimensionRow);
            isAnOddRow = !isAnOddRow;
        }

        availableDimensions.SetItems(dimensionsToAdd);
    }

    internal void CloseModal() { Hide(); }

    internal void OnCloseActionTriggered(DCLAction_Trigger action) { CloseModal(); }
}