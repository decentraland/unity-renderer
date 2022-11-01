using UnityEngine;
using System;
using Variables.RealmsInfo;
using System.Collections.Generic;
using System.Linq;
using Decentraland.Bff;

namespace DCL
{
    public class RealmsInfoHandler
    {
        private RealmsInfoModel model = new RealmsInfoModel();

        public CurrentRealmVariable playerRealm => DataStore.i.realm.playerRealm;
        public BaseCollection<RealmModel> realmsInfo => DataStore.i.realm.realmsInfo;
        private BaseVariable<AboutResponse> playerRealmAbout => DataStore.i.realm.playerRealmAbout;
        private BaseVariable<string> realmName => DataStore.i.realm.realmName;

        public void Set(string json)
        {
            JsonUtility.FromJsonOverwrite(json, model);
            Set(model);
        }

        public void Set(RealmsInfoModel newModel)
        {
            model = newModel;
            if (model.current != null && !string.IsNullOrEmpty(model.current.serverName))
            {
                DataStore.i.realm.playerRealm.Set(model.current.Clone());
                realmName.Set(DataStore.i.realm.playerRealm.Get().serverName);
            }
            DataStore.i.realm.realmsInfo.Set(newModel.realms != null ? newModel.realms.ToList() : new List<RealmModel>());
        }
        
        public void SetAbout(string json)
        {
            AboutResponse aboutResponse = AboutResponse.Parser.ParseJson(json);
            playerRealmAbout.Set(aboutResponse);
            realmName.Set(aboutResponse.Configurations.RealmName);
        }
    }

    [Serializable]
    public class RealmsInfoModel
    {
        public CurrentRealmModel current;
        public RealmModel[] realms;
    }
}