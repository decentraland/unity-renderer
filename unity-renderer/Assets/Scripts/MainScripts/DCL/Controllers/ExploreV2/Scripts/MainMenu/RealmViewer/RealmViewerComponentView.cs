using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IRealmViewerComponentView
{
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

public class RealmViewerComponentView : BaseComponentView, IRealmViewerComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal HorizontalLayoutGroup horizontalLayoutGroup;
    [SerializeField] internal TMP_Text realm;
    [SerializeField] internal TMP_Text numberOfusers;

    [Header("Configuration")]
    [SerializeField] internal RealmViewerComponentModel model;

    public void Configure(BaseComponentModel newModel)
    {
        model = (RealmViewerComponentModel)newModel;
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

        float formattedUsersCount = newNumberOfUsers >= 1000 ? (newNumberOfUsers / 1000f) : newNumberOfUsers;
        numberOfusers.text = newNumberOfUsers >= 1000 ? $"{formattedUsersCount}k" : $"{formattedUsersCount}";

        RebuildLayouts();
    }

    internal void RebuildLayouts()
    {
        if (horizontalLayoutGroup != null)
            Utils.ForceRebuildLayoutImmediate(horizontalLayoutGroup.transform as RectTransform);
    }
}