using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine;

public class BIWGodModeShould : IntegrationTestSuite
{

    private BiwGodMode godMode;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        godMode = new BiwGodMode();
        var context = BIWTestHelper.CreateReferencesControllerWithGenericMocks(
        );
        godMode.Init(context);
    }
}