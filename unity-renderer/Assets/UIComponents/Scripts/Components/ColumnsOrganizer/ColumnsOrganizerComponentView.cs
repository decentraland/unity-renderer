using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public interface IColumnsOrganizerComponentView
{
    /// <summary>
    /// Recalculates the size of each columns following the configuration in the model.
    /// </summary>
    void RecalculateColumnsSize();
}

public class ColumnsOrganizerComponentView : BaseComponentView, IColumnsOrganizerComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] internal RectTransform columnsContainer;

    [Header("Configuration")]
    [SerializeField] internal ColumnsOrganizerComponentModel model;

    internal List<RectTransform> currentColumns = new List<RectTransform>();

    public override void RefreshControl()
    {
        if (model == null)
            return;

        RecalculateColumnsSize();
    }

    public override void OnScreenSizeChanged()
    {
        base.OnScreenSizeChanged();

        RecalculateColumnsSize();
    }

    public void RecalculateColumnsSize()
    {
        RegisterCurrentColumns();

        horizontalLayoutGroup.spacing = model.spaceBetweenColumns;

        float totalWidthOfFixedSizeColumns = model.columnsConfig.Where(x => !x.isPercentage).Sum(x => x.width);
        float totalWidthOfPercentageColumns = columnsContainer.rect.width - totalWidthOfFixedSizeColumns - (model.spaceBetweenColumns * (model.columnsConfig.Count - 1));

        for (int i = 0; i < currentColumns.Count; i++)
        {
            if (model.columnsConfig.Count - 1 >= i)
            {
                if (model.columnsConfig[i].isPercentage)
                {
                    float newWidth = totalWidthOfPercentageColumns * model.columnsConfig[i].width / 100f;
                    if (newWidth < 0)
                        newWidth = 0;

                    currentColumns[i].sizeDelta = new Vector2(
                        newWidth,
                        currentColumns[i].sizeDelta.y);
                }
                else
                {
                    currentColumns[i].sizeDelta = new Vector2(
                        model.columnsConfig[i].width,
                        currentColumns[i].sizeDelta.y);
                }
            }
        }

        StartCoroutine(RefreshAllChildUIComponentsSize());
    }

    internal void RegisterCurrentColumns()
    {
        currentColumns.Clear();

        foreach (RectTransform child in transform)
        {
            currentColumns.Add(child);
        }
    }

    internal IEnumerator RefreshAllChildUIComponentsSize()
    {
        yield return null;

        for (int i = 0; i < model.uiComponentsToRefreshSize.Count; i++)
        {
            model.uiComponentsToRefreshSize[i].OnScreenSizeChanged();
        }

        Utils.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
}