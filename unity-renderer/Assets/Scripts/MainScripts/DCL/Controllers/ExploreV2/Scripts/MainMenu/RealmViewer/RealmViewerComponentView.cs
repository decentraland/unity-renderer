using TMPro;
using UnityEngine;

public interface IRealmViewerComponentView
{
    /// <summary>
    /// Fill the model and updates the realm viewer with this data.
    /// </summary>
    /// <param name="model">Data to configure the real viewer.</param>
    void Configure(RealmViewerComponentModel model);

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

public class RealmViewerComponentView : BaseComponentView, IRealmViewerComponentView
{
    [Header("Prefab References")]
    [SerializeField] internal TMP_Text realm;
    [SerializeField] internal TMP_Text numberOfusers;

    [Header("Configuration")]
    [SerializeField] internal RealmViewerComponentModel model;

    public override void PostInitialization() { Configure(model); }

    public virtual void Configure(RealmViewerComponentModel model)
    {
        this.model = model;
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
    }

    public void SetNumberOfUsers(int newNumberOfUsers)
    {
        model.numberOfUsers = newNumberOfUsers;

        if (numberOfusers == null)
            return;

        float formattedUsersCount = newNumberOfUsers >= 1000 ? (newNumberOfUsers / 1000f) : newNumberOfUsers;
        numberOfusers.text = newNumberOfUsers >= 1000 ? $"{formattedUsersCount}k" : $"{formattedUsersCount}";
    }
}