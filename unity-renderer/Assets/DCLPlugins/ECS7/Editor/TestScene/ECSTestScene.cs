using System;
using System.Collections;
using System.Reflection;
using DCL;
using DCL.Camera;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Interface;
using DCL.Models;
using UnityEngine;
using Environment = DCL.Environment;

public class ECSTestScene : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LoadScene(SceneScript));
    }

    private static void SceneScript(string sceneId, IECSComponentWriter componentWriter)
    {
        componentWriter.PutComponent(sceneId, 1, 1,
            new ECSTransform() { position = new Vector3(8, 1, 8) });
        AddGLTFShapeComponent(sceneId, componentWriter);
        AddBoxComponent(sceneId, componentWriter);
    }

    private static void AddGLTFShapeComponent(string sceneId, IECSComponentWriter componentWriter)
    {
        Environment.i.world.state.scenesSortedByDistance[0].contentProvider.baseUrl = "https://peer-lb.decentraland.org/content/contents/";
        Environment.i.world.state.scenesSortedByDistance[0].contentProvider.fileToHash.Add("models/SCENE.glb".ToLower(), "QmQgQtuAg9qsdrmLwnFiLRAYZ6Du4Dp7Yh7bw7ELn7AqkD");
            
        componentWriter.PutComponent(sceneId, 2, 1050,
            new PBGLTFShape() { Src = "models/SCENE.glb", Visible = true});
    }

    private static void AddBoxComponent(string sceneId, IECSComponentWriter componentWriter)
    {
        PBBoxShape model = new PBBoxShape();
        model.Visible = true;
        model.WithCollisions = true;
        componentWriter.PutComponent(sceneId,2,ComponentID.BOX_SHAPE,
            model );
    }
    
    private static void AddNFTComponent(string sceneId, IECSComponentWriter componentWriter)
    {
        PBNFTShape model = new PBNFTShape();
        model.Src = "ethereum://0x06012c8cf97bead5deae237070f9587f8e7a266d/1540722";
        model.Visible = true;
        model.WithCollisions = true;
        model.Color = new Color3();
        model.Style = 6;
        model.Color.R = 0.5f;
        model.Color.G = 0.5f;
        model.Color.B = 1f;
        componentWriter.PutComponent(sceneId,0,ComponentID.NFT_SHAPE,
            model );
    }

    private static IEnumerator LoadScene(Action<string, IECSComponentWriter> sceneScript)
    {
        LoadParcelScenesMessage.UnityParcelScene scene = new LoadParcelScenesMessage.UnityParcelScene()
        {
            basePosition = new Vector2Int(0, 0),
            parcels = new Vector2Int[] { new Vector2Int(0, 0) },
            id = "temptation"
        };

        bool started = false;

        WebInterface.OnMessageFromEngine += (type, s1) =>
        {
            if (type == "SystemInfoReport")
            {
                started = true;
            }
        };

        yield return new WaitUntil(() => started);
        yield return new WaitForSeconds(2);

        var mainGO = GameObject.Find("Main");
        mainGO.SendMessage("SetDebug");

        Environment.i.world.sceneController.LoadParcelScenes(JsonUtility.ToJson(scene));
        var playerAvatarController = FindObjectOfType<PlayerAvatarController>();
        CommonScriptableObjects.rendererState.RemoveLock(playerAvatarController);

        yield return new WaitForSeconds(1);

        ECS7Plugin ecs7Plugin = new ECS7Plugin();
        IECSComponentWriter componentWriter = typeof(ECS7Plugin)
                                              .GetField("componentWriter", BindingFlags.NonPublic | BindingFlags.Instance)
                                              .GetValue(ecs7Plugin) as IECSComponentWriter;

        sceneScript.Invoke(scene.id, componentWriter);

        mainGO.SendMessage("ActivateRendering");

        var message = new QueuedSceneMessage_Scene
        {
            sceneId = scene.id,
            tag = "",
            type = QueuedSceneMessage.Type.SCENE_MESSAGE,
            method = MessagingTypes.INIT_DONE,
            payload = new Protocol.SceneReady()
        };
        Environment.i.world.sceneController.EnqueueSceneMessage(message);

        var cameraConfig = new CameraController.SetRotationPayload()
        {
            x = 0,
            y = 0,
            z = 0,
            cameraTarget = new Vector3(0, 0, 1)
        };
        CommonScriptableObjects.cameraMode.Set(CameraMode.ModeId.FirstPerson);
        var cameraController = GameObject.Find("CameraController");
        cameraController.SendMessage("SetRotation", JsonUtility.ToJson(cameraConfig));
    }
}