using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Tests;
using UnityEngine;

public class BIWInputWrapperShould : IntegrationTestSuite
{
    private BIWInputWrapper inputWrapper;
    private BIWContext context;
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        inputWrapper = new BIWInputWrapper();
        context = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
            inputWrapper);

        inputWrapper.Init(context);
    }

    [Test]
    public void TestMouseClick() { }

    protected override IEnumerator TearDown()
    {
        inputWrapper.Dispose();
        context.Dispose();
        return base.TearDown();
    }
}