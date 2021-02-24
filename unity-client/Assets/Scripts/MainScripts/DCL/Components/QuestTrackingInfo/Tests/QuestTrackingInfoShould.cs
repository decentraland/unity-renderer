using DCL;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using DCL.QuestsController;
using NSubstitute;
using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine.TestTools;

public class QuestTrackingInfoShould : IntegrationTestSuite
{
    private ParcelScene scene;
    private IQuestsController mockQuestController;

    protected override WorldRuntimeContext CreateRuntimeContext()
    {
        return DCL.Tests.WorldRuntimeContextFactory.CreateWithCustomMocks
        (
            sceneController: new SceneController(),
            state: new WorldState(),
            componentFactory: RuntimeComponentFactory.Create()
        );
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = Environment.i.world.sceneController.CreateTestScene() as ParcelScene;
        mockQuestController = Substitute.For<IQuestsController>();
        QuestsController.i = mockQuestController;
    }

    [UnityTest]
    public IEnumerator InitializeEmpty()
    {
        DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestHelpers.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, null, CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        mockQuestController.DidNotReceiveWithAnyArgs().UpdateQuestProgress(new QuestModel());
        mockQuestController.DidNotReceiveWithAnyArgs().InitializeQuests(new List<QuestModel>());
    }

    [UnityTest]
    public IEnumerator AddQuests()
    {
        DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestHelpers.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, null, CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        QuestModel quest = new QuestModel
        {
            id = "questId"
        };
        questTrackingInfo.SetModel(quest);
        mockQuestController.Received().UpdateQuestProgress(quest);
    }

    [UnityTest]
    public IEnumerator RemovePreviousQuest()
    {
        DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestHelpers.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, null, CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        QuestModel quest = new QuestModel
        {
            id = "questId"
        };
        questTrackingInfo.SetModel(quest);

        QuestModel quest2 = new QuestModel
        {
            id = "questId2"
        };
        questTrackingInfo.SetModel(quest2);

        mockQuestController.Received().RemoveQuest(quest);
    }

    [UnityTest]
    public IEnumerator RemovePreviousQuestOnDestroy()
    {
        DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestHelpers.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, null, CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        QuestModel quest = new QuestModel
        {
            id = "questId"
        };
        questTrackingInfo.SetModel(quest);
        TestHelpers.RemoveSceneEntity(scene, entity);

        yield return null; // Needed for Unity to call OnDestroy

        mockQuestController.Received().RemoveQuest(quest);
    }
}