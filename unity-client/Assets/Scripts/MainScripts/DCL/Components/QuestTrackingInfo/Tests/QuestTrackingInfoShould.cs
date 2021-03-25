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
            componentFactory: new RuntimeComponentFactory()
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
        IDCLEntity entity = TestHelpers.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestHelpers.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, null, CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        mockQuestController.DidNotReceiveWithAnyArgs().UpdateQuestProgress(new QuestModel());
        mockQuestController.DidNotReceiveWithAnyArgs().InitializeQuests(new List<QuestModel>());
    }

    [UnityTest]
    public IEnumerator AddQuests()
    {
        IDCLEntity entity = TestHelpers.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestHelpers.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, new QuestModel(), CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        QuestModel quest = new QuestModel
        {
            id = "questId"
        };
        questTrackingInfo.UpdateFromModel(quest);
        mockQuestController.Received().UpdateQuestProgress(quest);
    }

    [UnityTest]
    public IEnumerator RemovePreviousQuest()
    {
        IDCLEntity entity = TestHelpers.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestHelpers.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, new QuestModel(), CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        QuestModel quest = new QuestModel
        {
            id = "questId"
        };
        questTrackingInfo.UpdateFromModel(quest);

        QuestModel quest2 = new QuestModel
        {
            id = "questId2"
        };
        questTrackingInfo.UpdateFromModel(quest2);

        mockQuestController.Received().RemoveQuest(quest);
    }

    [UnityTest]
    public IEnumerator RemovePreviousQuestOnDestroy()
    {
        IDCLEntity entity = TestHelpers.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestHelpers.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, new QuestModel(), CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        QuestModel quest = new QuestModel
        {
            id = "questId"
        };
        questTrackingInfo.UpdateFromModel(quest);
        TestHelpers.RemoveSceneEntity(scene, entity);

        yield return null; // Needed for Unity to call OnDestroy

        mockQuestController.Received().RemoveQuest(quest);
    }
}