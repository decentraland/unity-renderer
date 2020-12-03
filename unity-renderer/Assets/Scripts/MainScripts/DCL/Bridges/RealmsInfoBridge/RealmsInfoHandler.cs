using UnityEngine;
using System;
using Variables.RealmsInfo;

public class RealmsInfoHandler
{
    private RealmsInfoModel model = new RealmsInfoModel();

    public CurrentRealmVariable playerRealm => DataStore.playerRealm;
    public RealmsVariable realmsInfo => DataStore.realmsInfo;

    public void Set(string json)
    {
        JsonUtility.FromJsonOverwrite(json, model);
        Set(model);
    }

    public void Set(RealmsInfoModel newModel)
    {
        model = newModel;
        DataStore.playerRealm.Set(model.current?.Clone());
        DataStore.realmsInfo.Set(model.realms);
    }
}

[Serializable]
public class RealmsInfoModel
{
    public CurrentRealmModel current;
    public RealmModel[] realms;
}
