using NUnit.Framework;
using UnityEngine;
using KernelConfigurationTypes;

public class KernelConfigurationShould
{
    [Test]
    public void TriggerInitializePromiseCorrectly()
    {
        const float testValue = 1234;
        KernelConfig.i.initialized = false;

        var promiseFromController = KernelConfig.i.EnsureConfigInitialized();
        Assert.IsTrue(promiseFromController.keepWaiting, "Promise shouldn't be resolved until first value is set");

        KernelConfig.i.Set(new KernelConfigModel() { comms = new Comms() { commRadius = testValue } });

        Assert.IsFalse(promiseFromController.keepWaiting, "Promise should be resolved");
        Assert.AreEqual(testValue, promiseFromController.value?.comms.commRadius, "Promise value should match configs value");

        bool promiseFromControllerPass = false;

        promiseFromController.Then((config) => promiseFromControllerPass = config.comms.commRadius == testValue);

        Assert.IsTrue(promiseFromControllerPass);
    }

    [Test]
    public void TriggerOnChangeCorrectly()
    {
        const float testValue = 1234;
        const float testValue2 = 5678;

        bool onChangeCalled = false;
        bool onChangePass = false;

        KernelConfig.i.Set(new KernelConfigModel());

        KernelConfigModel model = new KernelConfigModel() { comms = new Comms() { commRadius = testValue } };

        KernelConfig.OnKernelConfigChanged onConfigChange = (current, prev) =>
        {
            onChangeCalled = true;
            onChangePass = current.comms.commRadius == testValue;
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
            onChangePass = current.comms.commRadius == testValue2 && prev.comms.commRadius == testValue;
        };

        KernelConfig.i.OnChange += onConfigChange;

        KernelConfig.i.Set(new KernelConfigModel() { comms = new Comms() { commRadius = testValue2 } });
        Assert.IsTrue(onChangePass);

        KernelConfig.i.OnChange -= onConfigChange;
    }

    [Test]
    public void ParseJsonCorrectly()
    {
        KernelConfigModel model = new KernelConfigModel();
        string json = JsonUtility.ToJson(model);
        KernelConfig.i.Set(json);

        Assert.IsTrue(model.Equals(KernelConfig.i.Get()));
    }
}
