using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ISectionToggle
{
    /// <summary>
    /// Set the toggle info.
    /// </summary>
    /// <param name="model">Model with all the toggle info.</param>
    void SetInfo(SectionToggleModel model);

    /// <summary>
    /// Set an action on the toggle onClick.
    /// </summary>
    /// <param name="action">Action to call after clicking the toggle.</param>
    void SetOnClickAction(Action action);

    /// <summary>
    /// Invoke the action of selecting the toggle.
    /// </summary>
    void SelectToggle();
}

public class SectionToggle : MonoBehaviour, ISectionToggle
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private TMP_Text sectionText;
    [SerializeField] private Image sectionImage;

    public void SetInfo(SectionToggleModel model)
    {
        if (model == null)
            return;

        if (sectionText != null)
            sectionText.text = model.title;

        if (sectionImage != null)
        {
            sectionImage.enabled = model.icon != null;
            sectionImage.sprite = model.icon;
        }

        SetOnClickAction(model.onClickAction);
    }

    public void SetOnClickAction(Action action)
    {
        if (toggle == null)
            return;

        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
                action?.Invoke();
        });
    }

    public void SelectToggle()
    {
        if (toggle == null)
            return;

        toggle.Select();
    }

    private void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveAllListeners();
    }
}