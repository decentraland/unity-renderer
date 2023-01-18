using UnityEngine;
using System;
using Variables.RealmsInfo;
using System.Collections.Generic;
using System.Linq;
using Decentraland.Bff;
using Google.Protobuf;

namespace DCL
{
    public class RealmsInfoHandler
    {
        private RealmsInfoModel model = new RealmsInfoModel();

        public CurrentRealmVariable playerRealm => DataStore.i.realm.playerRealm;
        public BaseCollection<RealmModel> realmsInfo => DataStore.i.realm.realmsInfo;
        private BaseVariable<AboutResponse.Types.AboutConfiguration> playerRealmAboutConfiguration => DataStore.i.realm.playerRealmAboutConfiguration;
        private BaseVariable<AboutResponse.Types.LambdasInfo> playerRealmAboutLambda => DataStore.i.realm.playerRealmAboutLambdas;
        private BaseVariable<AboutResponse.Types.ContentInfo> playerRealmAboutContent => DataStore.i.realm.playerRealmAboutContent;

        private BaseVariable<string> realmName => DataStore.i.realm.realmName;

        public void Set(string json)
        {
            JsonUtility.FromJsonOverwrite(json, model);
            Set(model);
        }

        internal void Set(RealmsInfoModel newModel)
        {
            model = newModel;

            if (!string.IsNullOrEmpty(model.current?.serverName))
            {
                DataStore.i.realm.playerRealm.Set(model.current.Clone());
                realmName.Set(DataStore.i.realm.playerRealm.Get().serverName);
            }

            DataStore.i.realm.realmsInfo.Set(newModel.realms != null ? newModel.realms.ToList() : new List<RealmModel>());
        }

        public void SetAbout(string json)
        {
            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            AboutResponse aboutResponse = jsonParser.Parse<AboutResponse>(json);
            playerRealmAboutConfiguration.Set(aboutResponse.Configurations);
            playerRealmAboutContent.Set(aboutResponse.Content);
            playerRealmAboutLambda.Set(aboutResponse.Lambdas);
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
