// unset:none
using DCL;
using System;
using System.Collections.Generic;
using System.Linq;
using Variables.RealmsInfo;

public class ExploreV2ComponentRealmsController: IDisposable
{
    private readonly DataStore_Realm realmModel;
    private readonly IExploreV2MenuComponentView view;

    internal readonly List<RealmRowComponentModel> currentAvailableRealms = new ();

    public ExploreV2ComponentRealmsController(DataStore_Realm realmModel, IExploreV2MenuComponentView view)
    {
        this.realmModel = realmModel;
        this.view = view;
    }

    public void Initialize()
    {
        realmModel.realmName.OnChange += UpdateRealmInfo;
        realmModel.realmsInfo.OnSet += UpdateAvailableRealmsInfo;

        UpdateRealmInfo(realmModel.realmName.Get());
        UpdateAvailableRealmsInfo(realmModel.realmsInfo.Get());
    }

    public void Dispose()
    {
        realmModel.realmName.OnChange -= UpdateRealmInfo;
        realmModel.realmsInfo.OnSet -= UpdateAvailableRealmsInfo;
    }

    internal void UpdateRealmInfo(string realmName, string _ = "")
    {
        if (string.IsNullOrEmpty(realmName))
            return;

        // Get the name of the current realm
        view.currentRealmViewer.SetRealm(realmName);
        view.currentRealmSelectorModal.SetCurrentRealm(realmName);

        // Calculate number of users in the current realm
        var realmList = DataStore.i.realm.realmsInfo.Get()?.ToList();
        RealmModel currentRealmModel = realmList?.FirstOrDefault(r => r.serverName == realmName);
        var realmUsers = 0;

        if (currentRealmModel != null)
            realmUsers = currentRealmModel.usersCount;

        view.currentRealmViewer.SetNumberOfUsers(realmUsers);
    }

    internal void UpdateAvailableRealmsInfo(IEnumerable<RealmModel> currentRealmList)
    {
        var realmList = currentRealmList?.ToList();

        if (!NeedToRefreshRealms(realmList))
            return;

        currentAvailableRealms.Clear();

        if (realmList != null)
        {
            string serverName = ServerNameForCurrentRealm();

            foreach (RealmModel realm in realmList)
                currentAvailableRealms.Add(new RealmRowComponentModel
                {
                    name = realm.serverName,
                    players = realm.usersCount,
                    isConnected = realm.serverName == serverName,
                });
        }

        view.currentRealmSelectorModal.SetAvailableRealms(currentAvailableRealms);
    }

    private bool NeedToRefreshRealms(IEnumerable<RealmModel> newRealmList)
    {
        if (newRealmList == null)
            return true;

        var needToRefresh = false;

        IEnumerable<RealmModel> realmModels = newRealmList as RealmModel[] ?? newRealmList.ToArray();

        if (realmModels.Count() != currentAvailableRealms.Count)
            needToRefresh = true;
        else
            foreach (RealmModel realm in realmModels)
                if (!currentAvailableRealms.Exists(x => x.name == realm.serverName && x.players == realm.usersCount))
                {
                    needToRefresh = true;
                    break;
                }

        return needToRefresh;
    }

    private static string ServerNameForCurrentRealm()
    {
        if (DataStore.i.realm.playerRealm.Get() != null)
            return DataStore.i.realm.playerRealm.Get().serverName;

        if (DataStore.i.realm.playerRealmAboutConfiguration.Get() != null)
            return DataStore.i.realm.playerRealmAboutConfiguration.Get().RealmName;

        return "";
    }
}
