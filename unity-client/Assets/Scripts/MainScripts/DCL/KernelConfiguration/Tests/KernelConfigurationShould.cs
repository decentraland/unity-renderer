using NUnit.Framework;
using UnityEngine;
using KernelConfigurationTypes;

public class KernelConfigurationShould
{
    [Test]
    public void TriggerInitializePromiseCorrectly()
    {
        const float commRadiusTestValue = 1234;
        const bool voiceChatEnabledTestValue = false;
        const bool builderInWorldEnabledTestValue = false;
        const string regexTestValue = "1234";
        KernelConfig.i.initialized = false;

        var promiseFromController = KernelConfig.i.EnsureConfigInitialized();
        Assert.IsTrue(promiseFromController.keepWaiting, "Promise shouldn't be resolved until first value is set");

        KernelConfig.i.Set(new KernelConfigModel()
        {
            comms = new Comms()
            {
                commRadius = commRadiusTestValue,
                voiceChatEnabled = voiceChatEnabledTestValue
            },
            profiles = new Profiles()
            {
                nameValidCharacterRegex = regexTestValue,
                nameValidRegex = regexTestValue
            },
            features = new Features()
            {
                enableBuilderInWorld = builderInWorldEnabledTestValue
            },
        });

        Assert.IsFalse(promiseFromController.keepWaiting, "Promise should be resolved");
        Assert.AreEqual(commRadiusTestValue, promiseFromController.value?.comms.commRadius, "Promise value should match configs value");
        Assert.AreEqual(voiceChatEnabledTestValue, promiseFromController.value?.comms.voiceChatEnabled, "Promise value should match configs value");
        Assert.AreEqual(regexTestValue, promiseFromController.value?.profiles.nameValidCharacterRegex, "Promise value should match configs value");
        Assert.AreEqual(regexTestValue, promiseFromController.value?.profiles.nameValidRegex, "Promise value should match configs value");

        bool promiseFromControllerPass = false;

        promiseFromController.Then((config) =>
        {
            promiseFromControllerPass =
                config.comms.commRadius == commRadiusTestValue &&
                config.comms.voiceChatEnabled == voiceChatEnabledTestValue &&
                config.profiles.nameValidCharacterRegex == regexTestValue &&
                config.profiles.nameValidRegex == regexTestValue;
        });

        Assert.IsTrue(promiseFromControllerPass);
    }

    [Test]
    public void TriggerOnChangeCorrectly()
    {
        const float commRadiusTestValue = 1234;
        const float commRadiusTestValue2 = 5678;
        const bool voiceChatEnabledTestValue = false;
        const bool voiceChatEnabledTestValue2 = true;
        const bool builderInWorldEnabledTestValue = false;
        const bool builderInWorldEnabledTestValue2 = false;
        const string regexTestValue = "1234";
        const string regexTestValue2 = "5678";

        bool onChangeCalled = false;
        bool onChangePass = false;

        KernelConfig.i.Set(new KernelConfigModel());

        KernelConfigModel model = new KernelConfigModel()
        {
            comms = new Comms()
            {
                commRadius = commRadiusTestValue,
                voiceChatEnabled = voiceChatEnabledTestValue
            },
            profiles = new Profiles()
            {
                nameValidCharacterRegex = regexTestValue,
                nameValidRegex = regexTestValue
            },
            features = new Features()
            {
                enableBuilderInWorld = builderInWorldEnabledTestValue
            },
        };

        KernelConfig.OnKernelConfigChanged onConfigChange = (current, prev) =>
        {
            onChangeCalled = true;
            onChangePass =
                current.comms.commRadius == commRadiusTestValue &&
                current.comms.voiceChatEnabled == voiceChatEnabledTestValue &&
                current.profiles.nameValidCharacterRegex == regexTestValue &&
                current.profiles.nameValidRegex == regexTestValue;
        };

        KernelConfig.i.OnChange += onConfigChange;

        KernelConfig.i.Set(model);
        Assert.IsTrue(onChangePass);

        onChangeCalled = false;
        onChangePass = false;

        KernelConfigModel modelUpdateWithSameValues = model.Clone();
        KernelConfig.i.Set(modelUpdateWithSameValues); // this shouldn't trigger onChange cause it has the same values
        Assert.IsFalse(onChangeCalled, "OnChange was called even if the new value is equal to the new one");

        KernelConfig.i.OnChange -= onConfigChange;

        onConfigChange = (current, prev) =>
        {
            onChangeCalled = true;
            onChangePass =
                current.comms.commRadius == commRadiusTestValue2 && prev.comms.commRadius == commRadiusTestValue &&
                current.comms.voiceChatEnabled == voiceChatEnabledTestValue2 && prev.comms.voiceChatEnabled == voiceChatEnabledTestValue &&
                current.profiles.nameValidCharacterRegex == regexTestValue2 && prev.profiles.nameValidRegex == regexTestValue &&
                current.profiles.nameValidRegex == regexTestValue2 && prev.profiles.nameValidRegex == regexTestValue;
        };

        KernelConfig.i.OnChange += onConfigChange;

        KernelConfig.i.Set(new KernelConfigModel()
        {
            comms = new Comms()
            {
                commRadius = commRadiusTestValue2,
                voiceChatEnabled = voiceChatEnabledTestValue2
            },
            profiles = new Profiles()
            {
                nameValidCharacterRegex = regexTestValue2,
                nameValidRegex = regexTestValue2
            },
            features = new Features()
            {
                enableBuilderInWorld = builderInWorldEnabledTestValue2
            },
        });
        Assert.IsTrue(onChangePass);

        KernelConfig.i.OnChange -= onConfigChange;
    }

    [Test]
    public void ParseJsonCorrectly()
    {
        KernelConfigModel model = new KernelConfigModel();

        var worldRange = new WorldRange(-150, -150, 150, 150);
        model.validWorldRanges.Add(worldRange);

        string json = JsonUtility.ToJson(model);
        KernelConfig.i.Set(json);

        Assert.IsTrue(model.Equals(KernelConfig.i.Get()));
    }
}
