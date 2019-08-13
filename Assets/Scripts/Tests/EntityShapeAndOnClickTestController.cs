using DCL;
using DCL.Components;
using DCL.Helpers;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class EntityShapeAndOnClickTestController : MonoBehaviour
{
    IEnumerator Start()
    {
        var sceneController = FindObjectOfType<SceneController>();
        var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

        sceneController.UnloadAllScenes();
        var scene = sceneController.CreateTestScene();
        yield return new WaitForAllMessagesProcessed();

        TestHelpers.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(-3, 1, 0));
        TestHelpers.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(0, 1, 0));
        TestHelpers.InstantiateEntityWithShape(scene, "3", DCL.Models.CLASS_ID.PLANE_SHAPE, new Vector3(2, 1, 0));
        TestHelpers.InstantiateEntityWithShape(scene, "4", DCL.Models.CLASS_ID.CONE_SHAPE, new Vector3(4, 1, 0));
        TestHelpers.InstantiateEntityWithShape(scene, "5", DCL.Models.CLASS_ID.CYLINDER_SHAPE, new Vector3(6, 1, 0));
        TestHelpers.InstantiateEntityWithShape(scene, "6", DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0, 1, 6),
            TestHelpers.GetTestsAssetsPath() + "/GLTF/Avatar_Idle.glb");
        TestHelpers.InstantiateEntityWithShape(scene, "7", DCL.Models.CLASS_ID.OBJ_SHAPE, new Vector3(10, 1, 0),
            TestHelpers.GetTestsAssetsPath() + "/OBJs/teapot.obj");
        TestHelpers.InstantiateEntityWithShape(scene, "8", DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0, 1, 12),
            TestHelpers.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb");

        for (int i = 0; i < 8; i++)
        {
            Assert.IsNotNull(scene.entities[(i + 1).ToString()]);
        }

        for (int i = 0; i < 8; i++)
        {
            string eventIndex = (i + 1).ToString();

            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = OnClickComponent.NAME,
                uuid = "event" + eventIndex
            };

            TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene,
                scene.entities[eventIndex], OnClickComponentModel);
        }

        var charController = FindObjectOfType<DCLCharacterController>();
        charController.SetPosition(JsonConvert.SerializeObject(new
        {
            x = 10,
            y = 0,
            z = 0
        }));

        string animJson = JsonConvert.SerializeObject(new DCLAnimator.Model
        {
            states = new DCLAnimator.Model.DCLAnimationState[]
            {
                new DCLAnimator.Model.DCLAnimationState
                {
                    name = "clip01",
                    clip = "animation:0",
                    playing = true,
                    looping = true,
                    weight = 1,
                    speed = 1
                }
            }
        });

        string entityId = "8";

        {
            scene.EntityComponentCreateOrUpdate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "animation",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.ANIMATOR,
                json = animJson
            }), out CleanableYieldInstruction routine);
        }

        entityId = "9";

        TestHelpers.CreateSceneEntity(scene, entityId);
        {
            scene.EntityComponentCreateOrUpdate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "text",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.TEXT_SHAPE,
                json = animJson
            }), out CleanableYieldInstruction routine);
        }

        var model = new TextShape.Model()
        { value = "Hello World!", width = 0.5f, height = 0.5f, hTextAlign = "center", vTextAlign = "center" };
        TestHelpers.InstantiateEntityWithTextShape(scene, new Vector3(5, 5, 5), model);

        yield return null;
    }
}
