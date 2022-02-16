using UnityEngine;
using System;
using Variables.RealmsInfo;
using System.Collections.Generic;
using System.Linq;

namespace DCL
{
    public class RealmsInfoHandler
    {
        private RealmsInfoModel model = new RealmsInfoModel();

        public CurrentRealmVariable playerRealm => DataStore.i.realm.playerRealm;
        public BaseCollection<RealmModel> realmsInfo => DataStore.i.realm.realmsInfo;

        public void Set(string json)
        {
            JsonUtility.FromJsonOverwrite(json, model);
            Set(model);
        }

        public void Set(RealmsInfoModel newModel)
        {
            model = newModel;
            DataStore.i.realm.playerRealm.Set(model.current?.Clone());
            DataStore.i.realm.realmsInfo.Set(newModel.realms != null ? newModel.realms.ToList() : new List<RealmModel>());
        }
    }

    [Serializable]
    public class RealmsInfoModel
    {
        public CurrentRealmModel current;
        public RealmModel[] realms;
    }
}