using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SearchInputField : MonoBehaviour
{
    internal const float IDLE_TYPE_TIME_TRIGGER_SEARCH = 1f;

    public event Action<string> OnSearchText;

    [SerializeField] internal TMP_InputField inputField;
    [SerializeField] internal GameObject searchSpinner;
    [SerializeField] internal Button clearSearchButton;

    private Coroutine searchWhileTypingRoutine;
    private float lastValueChangeTime = 0;

    private void Awake()
    {
        inputField.onValueChanged.AddListener(OnValueChanged);
        inputField.onSubmit.AddListener(OnSubmit);
        clearSearchButton.onClick.AddListener(OnClear);

        SetClearMode();
    }

    private void OnValueChanged(string value)
    {
        lastValueChangeTime = Time.unscaledTime;
        StartSearchWhileTyping();
    }

    private void OnSubmit(string value)
    {
        StopSearchWhileTyping();
        SetSearchMode();
        OnSearchText?.Invoke(value);
    }

    private void OnClear()
    {
        inputField.SetTextWithoutNotify(string.Empty);
        SetClearMode();
        OnSearchText?.Invoke(string.Empty);
    }

    private void StartSearchWhileTyping()
    {
        if (searchWhileTypingRoutine != null)
            return;

        searchWhileTypingRoutine = StartCoroutine(SearchWhileTypingRoutine());
    }

    private void StopSearchWhileTyping()
    {
        if (searchWhileTypingRoutine == null)
            return;

        StopCoroutine(searchWhileTypingRoutine);
        searchWhileTypingRoutine = null;
    }

    private IEnumerator SearchWhileTypingRoutine()
    {
        SetTypingMode();

        while ((Time.unscaledTime - lastValueChangeTime) < IDLE_TYPE_TIME_TRIGGER_SEARCH)
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

    private void SetTypingMode()
    {
        clearSearchButton.gameObject.SetActive(false);
        searchSpinner.SetActive(true);
    }

    private void SetSearchMode()
    {
        clearSearchButton.gameObject.SetActive(true);
        searchSpinner.SetActive(false);
    }

    private void SetClearMode()
    {
        clearSearchButton.gameObject.SetActive(false);
        searchSpinner.SetActive(false);
    }
}
