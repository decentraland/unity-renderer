using System;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using UnityEngine;
using NSubstitute;
using NUnit.Framework;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class AvatarModifierAreaShould : IntegrationTestSuite_Legacy
{
    private const string MOCK_MODIFIER_KEY = "MockModifier";
    private AvatarModifierArea avatarModifierArea;
    private IAvatarModifier mockAvatarModifier;
    public ParcelScene scene;
    public CoreComponentsPlugin coreComponentsPlugin;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        coreComponentsPlugin = new CoreComponentsPlugin();
        scene = TestUtils.CreateTestScene();
        IDCLEntity entity = TestUtils.CreateSceneEntity(scene);

        AvatarModifierArea.Model model = new AvatarModifierArea.Model
        {
            area = new BoxTriggerArea { box = new Vector3(10, 10, 10) },
        };
        avatarModifierArea = TestUtils.EntityComponentCreate<AvatarModifierArea, AvatarModifierArea.Model>(scene, entity, model, CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA);
        yield return avatarModifierArea.routine;

        model.modifiers = new[] { MOCK_MODIFIER_KEY };
        mockAvatarModifier = Substitute.For<IAvatarModifier>();
        avatarModifierArea.modifiers.Add(MOCK_MODIFIER_KEY, mockAvatarModifier);

        //now that the modifier has been added we trigger the Update again so it gets taken into account
        yield return TestUtils.EntityComponentUpdate(avatarModifierArea, model);
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        yield return base.TearDown();
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
    
    [UnityTest]
    public IEnumerator ApplyExcludedListCorrectly()
    {
        var player1 = MockPlayer("player1");
        var player2 = MockPlayer("player2");
        var player3 = MockPlayer("player3");
        
        DataStore.i.player.ownPlayer.Set(player1.player);
        DataStore.i.player.otherPlayers.AddOrSet(player2.player.id, player2.player);
        DataStore.i.player.otherPlayers.AddOrSet(player3.player.id, player3.player);

        var model = (AvatarModifierArea.Model)avatarModifierArea.GetModel();
        model.excludeIds = new[] { player1.player.id };

        yield return TestUtils.EntityComponentUpdate(avatarModifierArea, model);
        yield return null;

        mockAvatarModifier.DidNotReceive().ApplyModifier(player1.gameObject);
        mockAvatarModifier.Received().ApplyModifier(player2.gameObject);
        mockAvatarModifier.Received().ApplyModifier(player3.gameObject);
        
        model.excludeIds = new[] { player2.player.id };

        yield return TestUtils.EntityComponentUpdate(avatarModifierArea, model);
        yield return null;

        mockAvatarModifier.Received().ApplyModifier(player1.gameObject);
        mockAvatarModifier.Received().RemoveModifier(player2.gameObject);

        model.excludeIds =  new[] { player2.player.id, "notLoadedAvatar" };

        yield return TestUtils.EntityComponentUpdate(avatarModifierArea, model);
        yield return null;
        
        var player4 = MockPlayer("notLoadedAvatar");
        DataStore.i.player.otherPlayers.AddOrSet(player4.player.id, player4.player);
        yield return null;
        
        mockAvatarModifier.DidNotReceive().ApplyModifier(player4.gameObject);

        model.excludeIds =  new string[0];
        yield return TestUtils.EntityComponentUpdate(avatarModifierArea, model);
        yield return null;
        
        mockAvatarModifier.Received().ApplyModifier(player2.gameObject);

        yield return null;
        
        DataStore.i.player.otherPlayers.Remove(player2.player.id);
        DataStore.i.player.otherPlayers.Remove(player3.player.id);
        DataStore.i.player.otherPlayers.Remove(player4.player.id);
        
        Object.Destroy(((PlayerName)player1.player.playerName).gameObject);
        Object.Destroy(((PlayerName)player2.player.playerName).gameObject);
        Object.Destroy(((PlayerName)player3.player.playerName).gameObject);
        Object.Destroy(((PlayerName)player4.player.playerName).gameObject);

        Object.Destroy(player1.gameObject);
        Object.Destroy(player2.gameObject);
        Object.Destroy(player3.gameObject);
        Object.Destroy(player4.gameObject);
        Object.Destroy(avatarModifierArea.gameObject);
    }
    
    [UnityTest]
    public IEnumerator NotRemoveModifierOnWhenModelChange()
    {
        var model = (AvatarModifierArea.Model)avatarModifierArea.GetModel();

        var fakeObject = PrepareGameObjectForModifierArea();
        yield return null;
        
        mockAvatarModifier.Received().ApplyModifier(fakeObject);
        mockAvatarModifier.ClearReceivedCalls();

        model.area = new BoxTriggerArea { box = new Vector3(11, 11, 11) };
        yield return TestUtils.EntityComponentUpdate(avatarModifierArea, model);
        yield return null;
        
        mockAvatarModifier.Received(1).RemoveModifier(fakeObject);
        mockAvatarModifier.Received().ApplyModifier(fakeObject);
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

    private (Player player, GameObject gameObject) MockPlayer(string playerId)
    {
        GameObject gameObject = PrepareGameObjectForModifierArea();
        Player player = new Player()
        {
            id = playerId,
            collider = gameObject.GetComponentInChildren<Collider>(),
            playerName = GameObject.Instantiate(Resources.Load<GameObject>("PlayerName")).GetComponent<PlayerName>()
        };
        return (player, gameObject);
    }
}