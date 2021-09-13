using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ISectionToggle
{
    /// <summary>
    /// Set the toggle info.
    /// </summary>
    /// <param name="model">Model with all the toggle info.</param>
    void SetSectionInfo(SectionToggleModel model);

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

    public void SetSectionInfo(SectionToggleModel model)
    {
        sectionText.text = model.title;
        sectionImage.sprite = model.icon;
    }

    public void SelectToggle() { toggle.Select(); }
}