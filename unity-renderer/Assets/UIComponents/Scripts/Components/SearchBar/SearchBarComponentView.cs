using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ISearchBarComponentView
{
    string Text { get; }

    /// <summary>
    /// Event that will be triggered when a search is ordered in the search component.
    /// </summary>
    event Action<string> OnSearchText;

    /// <summary>
    /// Set the place holder text of the search component.
    /// </summary>
    /// <param name="value"></param>
    void SetPlaceHolderText(string value);

    /// <summary>
    /// Order a specific search.
    /// </summary>
    /// <param name="value">Text to search.</param>
    void SubmitSearch(string value, bool notify = true);

    /// <summary>
    /// Clear the search component.
    /// </summary>
    void ClearSearch(bool notify = true);

    /// <summary>
    /// Set the idle time to search.
    /// </summary>
    /// <param name="idleSearchTime">Time in seconds.</param>
    void SetIdleSearchTime(float idleSearchTime);

    /// <summary>
    /// Set the focus on the text input.
    /// </summary>
    void SetFocus();
}

public class SearchBarComponentView : BaseComponentView, ISearchBarComponentView, IComponentModelConfig<SearchBarComponentModel>
{
    [Header("Prefab References")]
    [SerializeField] internal TMP_InputField inputField;
    [SerializeField] internal TMP_Text placeHolderText;
    [SerializeField] internal GameObject searchSpinner;
    [SerializeField] internal Button clearSearchButton;

    [Header("Configuration")]
    [SerializeField] internal SearchBarComponentModel model;

    public event Action<string> OnSearchText;
    public event Action<string> OnSubmit;

    internal Coroutine searchWhileTypingRoutine;
    internal float lastValueChangeTime = 0;

    public string Text => inputField.text;

    public override void Awake()
    {
        base.Awake();

        inputField.onValueChanged.AddListener(OnValueChanged);
        inputField.onSubmit.AddListener(s => SubmitSearch(s));
        clearSearchButton.onClick.AddListener(() => ClearSearch());

        SetClearMode();
    }

    public void Configure(SearchBarComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetPlaceHolderText(model.placeHolderText);
    }

    public void SetPlaceHolderText(string value)
    {
        model.placeHolderText = value;

        if (placeHolderText == null)
            return;

        placeHolderText.text = value;
    }

    public void SubmitSearch(string value, bool notify = true)
    {
        StopSearchCoroutine();

        if (notify)
            inputField.text = value;
        else
            inputField.SetTextWithoutNotify(value);

        SetSearchMode();

        if (notify)
        {
            OnSearchText?.Invoke(value);
            OnSubmit?.Invoke(value);
        }
    }

    public void ClearSearch(bool notify = true)
    {
        StopSearchCoroutine();

        inputField.SetTextWithoutNotify(string.Empty);
        SetClearMode();

        if (notify)
            OnSearchText?.Invoke(string.Empty);
    }

    public void SetIdleSearchTime(float idleSearchTime) { model.idleTimeToTriggerSearch = idleSearchTime; }

    public void SetFocus()
    {
        inputField.Select();
    }

    public override void Dispose()
    {
        base.Dispose();

        inputField.onValueChanged.RemoveAllListeners();
        inputField.onSubmit.RemoveAllListeners();
        inputField.onSelect.RemoveAllListeners();
        inputField.onDeselect.RemoveAllListeners();
        clearSearchButton.onClick.RemoveAllListeners();

        StopSearchCoroutine();
    }

    internal void OnValueChanged(string value)
    {
        AudioScriptableObjects.input.Play(true);
        if (model.idleTimeToTriggerSearch < 0)
            return;

        lastValueChangeTime = Time.unscaledTime;

        if (searchWhileTypingRoutine == null)
            searchWhileTypingRoutine = StartCoroutine(SearchWhileTyping());
    }

    internal IEnumerator SearchWhileTyping()
    {
        SetTypingMode();

        while ((Time.unscaledTime - lastValueChangeTime) < model.idleTimeToTriggerSearch)
        {
            yield return null;
        }

        string value = inputField.text;
        if (string.IsNullOrEmpty(value))
        {
            SetClearMode();
        }
        else
        {
            SetSearchMode();
        }

        OnSearchText?.Invoke(value);
        searchWhileTypingRoutine = null;
    }

    internal void StopSearchCoroutine()
    {
        if (searchWhileTypingRoutine != null)
        {
            StopCoroutine(searchWhileTypingRoutine);
            searchWhileTypingRoutine = null;
        }
    }

    internal void SetTypingMode()
    {
        clearSearchButton.gameObject.SetActive(false);
        searchSpinner.SetActive(true);
    }

    internal void SetSearchMode()
    {
        clearSearchButton.gameObject.SetActive(!string.IsNullOrEmpty(inputField.text));
        searchSpinner.SetActive(false);
    }

    internal void SetClearMode()
    {
        clearSearchButton.gameObject.SetActive(false);
        searchSpinner.SetActive(false);
    }
}
