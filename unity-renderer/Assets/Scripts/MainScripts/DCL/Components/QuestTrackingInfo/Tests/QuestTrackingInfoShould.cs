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
    private CoreComponentsPlugin coreComponentsPlugin;

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<ISceneController>(() => new SceneController());
        serviceLocator.Register<IWorldState>(() => new WorldState());
        serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        coreComponentsPlugin = new CoreComponentsPlugin();
        scene = TestUtils.CreateTestScene();
        mockQuestController = Substitute.For<IQuestsController>();
        QuestsController.i = mockQuestController;
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator InitializeEmpty()
    {
        IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestUtils.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, null, CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        mockQuestController.DidNotReceiveWithAnyArgs().UpdateQuestProgress(new QuestModel());
        mockQuestController.DidNotReceiveWithAnyArgs().InitializeQuests(new List<QuestModel>());
    }

    [UnityTest]
    public IEnumerator AddQuests()
    {
        IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestUtils.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, new QuestModel(), CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
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
        IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestUtils.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, new QuestModel(), CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
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
        IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
        QuestTrackingInfo questTrackingInfo = TestUtils.EntityComponentCreate<QuestTrackingInfo, QuestModel>(scene, entity, new QuestModel(), CLASS_ID_COMPONENT.QUEST_TRACKING_INFORMATION);
        yield return questTrackingInfo.routine;
        QuestModel quest = new QuestModel
        {
            id = "questId"
        };
        questTrackingInfo.UpdateFromModel(quest);
        TestUtils.RemoveSceneEntity(scene, entity);

        yield return null; // Needed for Unity to call OnDestroy

        mockQuestController.Received().RemoveQuest(quest);
    }
}