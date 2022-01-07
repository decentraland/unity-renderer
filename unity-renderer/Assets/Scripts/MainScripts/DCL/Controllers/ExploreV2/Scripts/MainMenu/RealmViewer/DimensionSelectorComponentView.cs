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

        if (currentDimensionText == null)
            return;

        currentDimensionText.text = $"You are in <b>{dimension}</b>";
    }

    public void SetAvailableDimensions(List<DimensionRowComponentModel> dimensions)
    {
        List<BaseComponentView> dimensionsToAdd = new List<BaseComponentView>();
        foreach (DimensionRowComponentModel dimension in dimensions)
        {
            DimensionRowComponentView newDimensionRow = GameObject.Instantiate(dimensionRowPrefab);
            newDimensionRow.Configure(dimension);
            newDimensionRow.onWarpInClick.AddListener(() => Debug.Log($"{newDimensionRow.name} clicked!"));
            dimensionsToAdd.Add(newDimensionRow);
        }

        availableDimensions.SetItems(dimensionsToAdd);
    }

    internal void CloseModal() { Hide(); }

    internal void OnCloseActionTriggered(DCLAction_Trigger action) { CloseModal(); }
}