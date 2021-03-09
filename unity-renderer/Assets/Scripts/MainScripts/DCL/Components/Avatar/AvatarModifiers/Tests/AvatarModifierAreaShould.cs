using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;
using NSubstitute;
using UnityEngine.TestTools;

public class AvatarModifierAreaShould : IntegrationTestSuite_Legacy
{
    private const string MOCK_MODIFIER_KEY = "MockModifier";
    private AvatarModifierArea avatarModifierArea;
    private AvatarModifier mockAvatarModifier;
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
        AvatarModifierArea.Model model = new AvatarModifierArea.Model
        {
            area = new BoxTriggerArea { box = new Vector3(10, 10, 10) },
        };
        avatarModifierArea = TestHelpers.EntityComponentCreate<AvatarModifierArea, AvatarModifierArea.Model>(scene, entity, model, CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA);
        yield return avatarModifierArea.routine;

        model.modifiers = new [] { MOCK_MODIFIER_KEY };
        mockAvatarModifier = Substitute.For<AvatarModifier>();
        avatarModifierArea.modifiers.Add(MOCK_MODIFIER_KEY, mockAvatarModifier);

        //now that the modifier has been added we trigger the Update again so it gets taken into account
        yield return TestHelpers.EntityComponentUpdate(avatarModifierArea, model);
    }

    [UnityTest]
    public IEnumerator NotApplyModifierIfNoAvatarDetected()
    {
        yield return null;

        mockAvatarModifier.DidNotReceiveWithAnyArgs().ApplyModifier(null);
    }

    [UnityTest]
    public IEnumerator ApplyModifierWhenDetectingAvatar()
    {
        var fakeObject = PrepareGameObjectForModifierArea();
        yield return null;

        mockAvatarModifier.Received().ApplyModifier(fakeObject);
    }

    [UnityTest]
    public IEnumerator BehaveWhenTheModifiedItemIsRemoved()
    {
        var fakeObject = PrepareGameObjectForModifierArea();
        yield return null;

        Object.Destroy(fakeObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RemoveModifiersWhenMovingAway()
    {
        var fakeObject = PrepareGameObjectForModifierArea();
        yield return null;

        fakeObject.transform.position += Vector3.one * 100;
        yield return null;

        mockAvatarModifier.Received().RemoveModifier(fakeObject);
    }

    [UnityTest]
    public IEnumerator RemoveModifiersWhenBeingDestroyed()
    {
        var fakeObject = PrepareGameObjectForModifierArea();
        yield return null;

        mockAvatarModifier.Received().ApplyModifier(fakeObject);
        fakeObject.SetActive(false);
        Object.Destroy(avatarModifierArea.gameObject);
        yield return null;
        mockAvatarModifier.Received().RemoveModifier(fakeObject);
    }

    private GameObject PrepareGameObjectForModifierArea()
    {
        GameObject fakeObject = CreateTestGameObject("_FakeObject");

        // ModifierArea presumes the object has a parent
        GameObject fakeParent = CreateTestGameObject("_FakeParent");
        fakeObject.transform.parent = fakeParent.transform;

        var boxCollider = fakeObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(3, 3, 3);
        fakeObject.layer = LayerMask.NameToLayer(BoxTriggerArea.AVATAR_TRIGGER_LAYER);
        fakeObject.transform.position = avatarModifierArea.transform.position;

        return fakeParent;
    }
}
