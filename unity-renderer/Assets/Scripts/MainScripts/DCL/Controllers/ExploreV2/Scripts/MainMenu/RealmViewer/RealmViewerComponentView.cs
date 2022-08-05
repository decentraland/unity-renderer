using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IRealmViewerComponentView
{
    /// <summary>
    /// Event that will be triggered when the Logo button is clicked.
    /// </summary>
    Button.ButtonClickedEvent onLogoClick { get; }

    /// <summary>
    /// Set the realm label.
    /// </summary>
    /// <param name="newRealm">New realm.</param>
    void SetRealm(string newRealm);

    /// <summary>
    /// Set the number of users label.
    /// </summary>
    /// <param name="newNumberOfUsers">New number of users.</param>
    void SetNumberOfUsers(int newNumberOfUsers);
}

public class RealmViewerComponentView : BaseComponentView, IRealmViewerComponentView, IComponentModelConfig<RealmViewerComponentModel>
{
    [Header("Prefab References")]
    [SerializeField] internal HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] internal TMP_Text realm;
    [SerializeField] internal TMP_Text numberOfusers;
    [SerializeField] internal ButtonComponentView logoButton;

    [Header("Configuration")]
    [SerializeField] internal RealmViewerComponentModel model;

    public Button.ButtonClickedEvent onLogoClick => logoButton?.onClick;

    public void Configure(RealmViewerComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetRealm(model.realmName);
        SetNumberOfUsers(model.numberOfUsers);
    }

    public void SetRealm(string newRealm)
    {
        model.realmName = newRealm;

        if (realm == null)
            return;

        realm.text = newRealm;

        RebuildLayouts();
    }

    public void SetNumberOfUsers(int newNumberOfUsers)
    {
        model.numberOfUsers = newNumberOfUsers;

        if (numberOfusers == null)
            return;

        numberOfusers.text = ExploreV2CommonUtils.FormatNumberToString(newNumberOfUsers);

        RebuildLayouts();
    }

    internal void RebuildLayouts()
    {
        if (horizontalLayoutGroup != null)
            Utils.ForceRebuildLayoutImmediate(horizontalLayoutGroup.transform as RectTransform);
    }
}