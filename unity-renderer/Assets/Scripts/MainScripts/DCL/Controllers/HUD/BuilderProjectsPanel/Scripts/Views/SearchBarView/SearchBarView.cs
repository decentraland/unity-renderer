using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SearchBarConfig
{
    public bool showFilterOwner;
    public bool showFilterOperator;
    public bool showFilterContributor;
    public bool showResultLabel;
}

internal class SearchBarView : MonoBehaviour
{
    private const string RESULT_FORMAT = "Results ({0})";

    [SerializeField] internal SearchInputField inputField;
    [SerializeField] internal Button sortButton;
    [SerializeField] internal TextMeshProUGUI sortTypeLabel;
    [SerializeField] internal Toggle ownerToggle;
    [SerializeField] internal Toggle operatorToggle;
    [SerializeField] internal Toggle contributorToggle;
    [SerializeField] private TextMeshProUGUI resultLabel;
    [SerializeField] internal SortDropdownView sortDropdown;

    private bool filterOwner = false;
    private bool filterOperator = false;
    private bool filterContributor = false;

    private ISectionSearchHandler searchHandler;

    private void Awake()
    {
        sortButton.onClick.AddListener(OnSortButtonPressed);

        ownerToggle.onValueChanged.AddListener(OnToggleOwner);
        operatorToggle.onValueChanged.AddListener(OnToggleOperator);
        contributorToggle.onValueChanged.AddListener(OnToggleContributor);

        filterOwner = ownerToggle.isOn;
        filterOperator = operatorToggle.isOn;
        filterContributor = contributorToggle.isOn;

        inputField.OnSearchText += text => searchHandler?.SetSearchString(text);
        sortDropdown.OnSortTypeSelected += OnSortTypeSelected;
    }

    public void SetResultCount(int count)
    {
        resultLabel.text = string.Format(RESULT_FORMAT, count);
    }

    public void ShowFilters(bool filterOwner, bool filterOperator, bool filterContributor)
    {
        ownerToggle.gameObject.SetActive(filterOwner);
        operatorToggle.gameObject.SetActive(filterOperator);
        contributorToggle.gameObject.SetActive(filterContributor);
    }

    public void SetSortTypes(string[] types)
    {
        sortDropdown.AddSortType(types);
    }

    private void OnSortButtonPressed()
    {
        if (sortDropdown.GetSortTypesCount() > 1)
        {
            sortDropdown.Show();
        }
    }

    private void OnSortTypeSelected(string type)
    {
        sortTypeLabel.text = type;
        searchHandler?.SetSortType(type);
    }

    private void OnToggleOwner(bool isOn)
    {
        filterOwner = isOn;
        ReportFilter();
    }

    private void OnToggleOperator(bool isOn)
    {
        filterOperator = isOn;
        ReportFilter();
    }

    private void OnToggleContributor(bool isOn)
    {
        filterContributor = isOn;
        ReportFilter();
    }

    private void ReportFilter()
    {
        searchHandler?.SetFilter(filterOwner, filterOperator, filterContributor);
    }

    public void SetSearchBar(ISectionSearchHandler handler, SearchBarConfig config)
    {
        if (searchHandler != null)
        {
            searchHandler.OnUpdated -= OnUpdateResultCount;
        }

        searchHandler = handler;

        if (searchHandler == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);

        sortDropdown.Clear();
        sortDropdown.AddSortType(handler.sortTypes);

        if (config != null)
        {
            ShowFilters(config.showFilterOwner, config.showFilterOperator, config.showFilterContributor);
            resultLabel.gameObject.SetActive(config.showResultLabel);
        }

        ownerToggle.SetIsOnWithoutNotify(handler.filterOwner);
        operatorToggle.SetIsOnWithoutNotify(handler.filterOperator);
        contributorToggle.SetIsOnWithoutNotify(handler.filterContributor);
        filterOwner = handler.filterOwner;
        filterOperator = handler.filterOperator;
        filterContributor = handler.filterContributor;

        sortTypeLabel.text = handler.sortType;
        inputField.inputField.SetTextWithoutNotify(handler.searchString);
        SetResultCount(handler.resultCount);

        searchHandler.OnUpdated += OnUpdateResultCount;
    }

    private void OnUpdateResultCount()
    {
        if (searchHandler == null)
            return;
        SetResultCount(searchHandler.resultCount);
    }
}
