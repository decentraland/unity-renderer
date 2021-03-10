using NUnit.Framework;
using UnityEngine;
using Variables.RealmsInfo;
using DCL;

public class RealmsInfoTests
{
    [Test]
    public void TriggerCurrentRealmOnChangeCorrectly()
    {
        const string SERVER_NAME = "temptation";
        const string LAYER = "red";

        var handler = new RealmsInfoHandler();

        var testModel = new RealmsInfoModel()
        {
            current = new CurrentRealmModel()
            {
                serverName = SERVER_NAME,
                layer = LAYER
            }
        };

        bool onChangeTriggered = false;
        CurrentRealmModel onChangeCurrentValue = null;

        handler.playerRealm.OnChange += (current, prev) =>
        {
            onChangeTriggered = true;
            onChangeCurrentValue = current;
        };

        handler.Set(testModel);
        Assert.IsTrue(onChangeTriggered, "OnChange not triggered");
        Assert.IsTrue(testModel.current.Equals(onChangeCurrentValue), "Values are not the same");
        Assert.IsTrue(testModel.current.serverName == SERVER_NAME, "Values are not the same");
        Assert.IsTrue(testModel.current.layer == LAYER, "Values are not the same");

        onChangeTriggered = false;
        handler.Set(JsonUtility.ToJson(testModel));
        Assert.IsFalse(onChangeTriggered, "OnChange shouldn't be triggered");

        const string NEW_LAYER = "blue";
        testModel.current.layer = NEW_LAYER;

        handler.Set(JsonUtility.ToJson(testModel));
        Assert.IsTrue(testModel.current.Equals(onChangeCurrentValue), "Values are not the same");
        Assert.IsTrue(testModel.current.serverName == SERVER_NAME, "Values are not the same");
        Assert.IsTrue(testModel.current.layer == NEW_LAYER, "Values are not the same");

        onChangeTriggered = false;
        handler.Set(testModel);
        Assert.IsFalse(onChangeTriggered, "OnChange shouldn't be triggered");
    }

    [Test]
    public void TriggerRealmsInfoOnChangeCorrectly()
    {
        const string SERVER_NAME_1 = "temptation";
        const string LAYER_1 = "red";
        const string SERVER_NAME_2 = "temptation";
        const string LAYER_2 = "blue";


        var realm1 = new RealmModel()
        {
            serverName = SERVER_NAME_1,
            layer = LAYER_1
        };

        var realm2 = new RealmModel()
        {
            serverName = SERVER_NAME_2,
            layer = LAYER_2
        };

        var testModel = new RealmsInfoModel()
        {
            realms = new RealmModel[] {realm1, realm2}
        };

        var handler = new RealmsInfoHandler();

        bool onChangeTriggered = false;
        RealmModel[] onChangeCurrentValue = null;

        handler.realmsInfo.OnChange += (current, prev) =>
        {
            onChangeTriggered = true;
            onChangeCurrentValue = current;
        };

        handler.Set(testModel);
        Assert.IsTrue(onChangeTriggered, "OnChange not triggered");
        Assert.IsTrue(testModel.realms.Length == 2, "Values are not the same");
        Assert.IsTrue(testModel.realms.Equals(onChangeCurrentValue), "Values are not the same");
        Assert.IsTrue(testModel.realms[0].serverName == SERVER_NAME_1, "Values are not the same");
        Assert.IsTrue(testModel.realms[0].layer == LAYER_1, "Values are not the same");
        Assert.IsTrue(testModel.realms[1].serverName == SERVER_NAME_2, "Values are not the same");
        Assert.IsTrue(testModel.realms[1].layer == LAYER_2, "Values are not the same");

        onChangeTriggered = false;
        handler.Set(JsonUtility.ToJson(testModel));
        Assert.IsFalse(onChangeTriggered, "OnChange shouldn't be triggered");
    }
}