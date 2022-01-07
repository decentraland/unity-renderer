using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IDimensionRowComponentView
{
    /// <summary>
    /// Event that will be triggered when the Warp In button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onWarpInClick { get; }

    /// <summary>
    /// Set the name label.
    /// </summary>
    /// <param name="name">A string.</param>
    void SetName(string name);

    /// <summary>
    /// Set the number of players label.
    /// </summary>
    /// <param name="numberOfPlayers">Number of players.</param>
    void SetNumberOfPlayers(int numberOfPlayers);

    /// <summary>
    /// Show/hide the connected mark.
    /// </summary>
    /// <param name="isConnected">True for showing the connected mark.</param>
    void SetAsConnected(bool isConnected);
}

public class DimensionRowComponentView : BaseComponentView, IDimensionRowComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal TMP_Text nameText;
    [SerializeField] internal TMP_Text playersText;
    [SerializeField] internal ButtonComponentView warpInButton;
    [SerializeField] internal GameObject connectedMark;

    [Header("Configuration")]
    [SerializeField] internal DimensionRowComponentModel model;

    public Button.ButtonClickedEvent onWarpInClick => warpInButton?.onClick;

    public void Configure(BaseComponentModel newModel)
    {
        model = (DimensionRowComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetName(model.name);
        SetNumberOfPlayers(model.players);
        SetAsConnected(model.isConnected);
    }

    public void SetName(string name)
    {
        model.name = name;

        if (nameText == null)
            return;

        nameText.text = name;
    }

    public void SetNumberOfPlayers(int numberOfPlayers)
    {
        model.players = numberOfPlayers;

        if (playersText == null)
            return;

        float formattedPlayersCount = numberOfPlayers >= 1000 ? (numberOfPlayers / 1000f) : numberOfPlayers;
        playersText.text = numberOfPlayers >= 1000 ? $"{formattedPlayersCount}k" : $"{formattedPlayersCount}";
    }

    public void SetAsConnected(bool isConnected)
    {
        model.isConnected = isConnected;

        if (connectedMark == null)
            return;

        connectedMark.SetActive(isConnected);
        warpInButton.gameObject.SetActive(!isConnected);
    }
}