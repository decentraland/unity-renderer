using UnityEngine;
using System;
using Variables.RealmsInfo;

namespace DCL
{
    public class RealmsInfoHandler
    {
        private RealmsInfoModel model = new RealmsInfoModel();

        public CurrentRealmVariable playerRealm => DataStore.i.playerRealm;
        public RealmsVariable realmsInfo => DataStore.i.realmsInfo;

        public void Set(string json)
        {
            JsonUtility.FromJsonOverwrite(json, model);
            Set(model);
        }

        public void Set(RealmsInfoModel newModel)
        {
            model = newModel;
            DataStore.i.playerRealm.Set(model.current?.Clone());
            DataStore.i.realmsInfo.Set(model.realms);
        }
    }

    [Serializable]
    public class RealmsInfoModel
    {
        public CurrentRealmModel current;
        public RealmModel[] realms;
    }
}